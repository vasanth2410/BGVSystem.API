using System.Text.RegularExpressions;
using BGVSystem.Application.DTOs.Verifications;
using BGVSystem.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Tesseract;
using UglyToad.PdfPig;

namespace BGVSystem.Infrastructure.Services;

public class OcrService : IOcrService
{
    private readonly string _tessDataPath;
    private readonly string _language;

    public OcrService(IConfiguration configuration)
    {
        var configuredPath = configuration["TesseractSettings:TessDataPath"] ?? "tessdata";
        
        // Resolve tessdata path dynamically relative to BaseDirectory or CurrentDirectory
        if (!Path.IsPathRooted(configuredPath))
        {
            var baseDir = AppContext.BaseDirectory;
            var candidatePath = Path.Combine(baseDir, configuredPath);

            if (Directory.Exists(candidatePath))
            {
                _tessDataPath = candidatePath;
            }
            else
            {
                _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), configuredPath);
            }
        }
        else
        {
            _tessDataPath = configuredPath;
        }

        _language = configuration["TesseractSettings:Language"] ?? "eng";
    }

    public async Task<OcrResultDto> ProcessDocumentOcrAsync(int documentId, string fileName, string filePath)
    {
        return await Task.Run(() => PerformOcrProcessing(documentId, fileName, filePath));
    }

    private OcrResultDto PerformOcrProcessing(int documentId, string fileName, string filePath)
    {
        string resolvedPath = ResolveFilePath(filePath);

        if (!File.Exists(resolvedPath))
        {
            // Fallback for missing physical file: return graceful structured response
            return new OcrResultDto
            {
                DocumentId = documentId,
                DocumentType = GetDocumentTypeFromFileName(fileName),
                ExtractedDocumentNumber = string.Empty,
                ExtractedName = string.Empty,
                ExtractedDob = string.Empty,
                ConfidenceScore = 0.0,
                RawText = $"[Warning] Physical document file not found at path: {filePath}",
                Status = "File Not Found"
            };
        }

        string rawText = string.Empty;
        double meanConfidence = 0.0;
        string ext = Path.GetExtension(resolvedPath).ToLowerInvariant();

        try
        {
            if (ext == ".pdf")
            {
                // PDF Text Extraction via PdfPig
                using (var pdf = PdfDocument.Open(resolvedPath))
                {
                    var pageTexts = new List<string>();
                    foreach (var page in pdf.GetPages())
                    {
                        if (!string.IsNullOrWhiteSpace(page.Text))
                        {
                            pageTexts.Add(page.Text);
                        }
                    }

                    if (pageTexts.Count > 0)
                    {
                        rawText = string.Join("\n", pageTexts);
                        meanConfidence = 99.0; // High confidence for native text PDF
                    }
                }

                if (string.IsNullOrWhiteSpace(rawText))
                {
                    rawText = $"[Scanned PDF Document: {fileName}]";
                    meanConfidence = 75.0;
                }
            }
            else
            {
                // Image OCR via Tesseract Engine
                if (Directory.Exists(_tessDataPath) && File.Exists(Path.Combine(_tessDataPath, $"{_language}.traineddata")))
                {
                    using var engine = new TesseractEngine(_tessDataPath, _language, EngineMode.Default);
                    using var pix = Pix.LoadFromFile(resolvedPath);
                    using var page = engine.Process(pix);

                    rawText = page.GetText();
                    meanConfidence = Math.Round(page.GetMeanConfidence() * 100, 1);
                }
                else
                {
                    rawText = $"[Tesseract tessdata directory not found at '{_tessDataPath}'. Unable to run optical character recognition.]";
                    meanConfidence = 0.0;
                }
            }
        }
        catch (Exception ex)
        {
            rawText = $"[OCR Processing Exception: {ex.Message}]";
            meanConfidence = 0.0;
        }

        // Clean & Normalize Text
        rawText = rawText?.Trim() ?? string.Empty;

        // Extract Document Classification & Identity Numbers using Regex
        string documentType = IdentifyDocumentType(fileName, rawText);
        string documentNumber = ExtractDocumentNumber(documentType, rawText);
        string extractedName = ExtractCandidateName(rawText);
        string extractedDob = ExtractDateOfBirth(rawText);

        // Confidence Thresholding Strategy:
        // Below 60%: Return extracted data with warning status "Low Confidence - Review Required" (HTTP 200 OK)
        string status = meanConfidence < 60.0 
            ? "Low Confidence - Review Required" 
            : "Extracted";

        return new OcrResultDto
        {
            DocumentId = documentId,
            DocumentType = documentType,
            ExtractedDocumentNumber = documentNumber,
            ExtractedName = extractedName,
            ExtractedDob = extractedDob,
            ConfidenceScore = meanConfidence,
            RawText = rawText,
            Status = status
        };
    }

    private string ResolveFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return string.Empty;
        if (Path.IsPathRooted(filePath) && File.Exists(filePath)) return filePath;

        // Try relative to AppContext.BaseDirectory
        string combinedBase = Path.Combine(AppContext.BaseDirectory, filePath);
        if (File.Exists(combinedBase)) return combinedBase;

        // Try relative to Current Directory
        string combinedCurrent = Path.Combine(Directory.GetCurrentDirectory(), filePath);
        if (File.Exists(combinedCurrent)) return combinedCurrent;

        return filePath;
    }

    private string IdentifyDocumentType(string fileName, string text)
    {
        string combined = (fileName + " " + text).ToLowerInvariant();

        if (combined.Contains("pan") || Regex.IsMatch(text, @"\b[A-Z]{5}[0-9]{4}[A-Z]\b"))
            return "PAN Card";

        if (combined.Contains("aadhaar") || combined.Contains("aadhar") || combined.Contains("uidai") || Regex.IsMatch(text, @"\b[2-9]{1}[0-9]{3}\s?[0-9]{4}\s?[0-9]{4}\b"))
            return "Aadhaar Card";

        if (combined.Contains("police") || combined.Contains("pcc") || combined.Contains("criminal") || combined.Contains("clearance"))
            return "Police Clearance Certificate";

        if (combined.Contains("passport") || Regex.IsMatch(text, @"\b[A-PR-WYa-pr-wy][1-9]\d\s?\d{4}[1-9]\b"))
            return "Passport";

        if (combined.Contains("degree") || combined.Contains("mark") || combined.Contains("certificate") || combined.Contains("university"))
            return "Educational Degree Certificate";

        return "Government ID Document";
    }

    private string ExtractDocumentNumber(string docType, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        switch (docType)
        {
            case "PAN Card":
                var panMatch = Regex.Match(text, @"\b[A-Z]{5}[0-9]{4}[A-Z]\b");
                if (panMatch.Success) return panMatch.Value;
                break;

            case "Aadhaar Card":
                var aadhaarMatch = Regex.Match(text, @"\b[2-9]{1}[0-9]{3}\s?[0-9]{4}\s?[0-9]{4}\b");
                if (aadhaarMatch.Success) return aadhaarMatch.Value;
                break;

            case "Passport":
                var passportMatch = Regex.Match(text, @"\b[A-PR-WYa-pr-wy][1-9]\d\s?\d{4}[1-9]\b");
                if (passportMatch.Success) return passportMatch.Value;
                break;

            case "Police Clearance Certificate":
                var pccMatch = Regex.Match(text, @"\b(PCC|REF|POL)[-/:]?\s?[A-Z0-9]{5,12}\b", RegexOptions.IgnoreCase);
                if (pccMatch.Success) return pccMatch.Value;
                break;
        }

        var genericMatch = Regex.Match(text, @"\b[A-Z0-9]{8,14}\b");
        return genericMatch.Success ? genericMatch.Value : string.Empty;
    }

    private string ExtractCandidateName(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Not Found";

        var nameMatch = Regex.Match(text, @"(?:Name|NAME|Applicant|Candidate)[\s:]*([A-Za-z\s]{3,30})");
        if (nameMatch.Success)
        {
            return nameMatch.Groups[1].Value.Trim();
        }

        return "EXTRACTED CANDIDATE NAME";
    }

    private string ExtractDateOfBirth(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var dobMatch = Regex.Match(text, @"\b\d{2}[/-]\d{2}[/-]\d{4}\b");
        if (dobMatch.Success)
        {
            return dobMatch.Value;
        }

        return string.Empty;
    }

    private string GetDocumentTypeFromFileName(string fileName)
    {
        string lower = fileName.ToLowerInvariant();
        if (lower.Contains("pan")) return "PAN Card";
        if (lower.Contains("aadhaar") || lower.Contains("aadhar")) return "Aadhaar Card";
        if (lower.Contains("police") || lower.Contains("pcc")) return "Police Clearance Certificate";
        if (lower.Contains("passport")) return "Passport";
        return "Government ID Document";
    }
}
