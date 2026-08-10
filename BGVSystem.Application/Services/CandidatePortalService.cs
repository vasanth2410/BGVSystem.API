using BGVSystem.Application.DTOs.CandidatePortal;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace BGVSystem.Application.Services;

public class CandidatePortalService : ICandidatePortalService
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _fileStorageService;

    public CandidatePortalService(
        ICandidateRepository candidateRepository,
        IDocumentRepository documentRepository,
        IVerificationRepository verificationRepository,
        IWebHostEnvironment environment,
        IFileStorageService fileStorageService,
        IEmailService emailService = null)
    {
        _candidateRepository = candidateRepository;
        _documentRepository = documentRepository;
        _verificationRepository = verificationRepository;
        _environment = environment;
        _fileStorageService = fileStorageService;
        _emailService = emailService;
    }

    public async Task<CandidateProfileDto?> GetProfileAsync(string email)
    {
        var candidate = await _candidateRepository.GetByEmailAsync(email);

        if (candidate == null)
        {
            return null;
        }

        return new CandidateProfileDto
        {
            Id = candidate.Id,
            FullName = candidate.FullName,
            Email = candidate.Email,
            PhoneNumber = candidate.PhoneNumber,
            AppliedRole = candidate.AppliedRole,
            Status = candidate.Status
        };
    }

    private static string GetEffectiveStatus(Document doc, IEnumerable<Verification> verifications)
    {
        var match = verifications.FirstOrDefault(v =>
            !string.IsNullOrEmpty(v.VerificationType) && (
                v.VerificationType.Equals(doc.OriginalFileName, StringComparison.OrdinalIgnoreCase) ||
                v.VerificationType.Equals(doc.FileName, StringComparison.OrdinalIgnoreCase)
            )
        );

        if (match != null && !string.IsNullOrEmpty(match.Status))
        {
            if (match.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ||
                match.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                return match.Status;
            }
        }

        if (!string.IsNullOrEmpty(doc.Status) && (
            doc.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ||
            doc.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase)))
        {
            return doc.Status;
        }

        return match?.Status ?? doc.Status ?? "Uploaded";
    }

    public async Task<CandidateDashboardDto?> GetDashboardAsync(string email)
    {
        var candidate = await _candidateRepository.GetByEmailAsync(email);

        if (candidate == null)
        {
            return null;
        }

        var documents = await _documentRepository.GetByCandidateIdAsync(candidate.Id);
        var verifications = await _verificationRepository.GetByCandidateIdAsync(candidate.Id);

        int approved = 0;
        int pending = 0;
        int rejected = 0;

        foreach (var doc in documents)
        {
            var effectiveStatus = GetEffectiveStatus(doc, verifications);

            if (effectiveStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                approved++;
            }
            else if (effectiveStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase) ||
                     effectiveStatus.Equals("Needs Action", StringComparison.OrdinalIgnoreCase))
            {
                rejected++;
            }
            else
            {
                pending++;
            }
        }

        string overallStatus;

        if (rejected > 0)
        {
            overallStatus = "Rejected";
        }
        else if (pending == 0 && approved > 0)
        {
            overallStatus = "Completed";
        }
        else
        {
            overallStatus = "In Progress";
        }

        return new CandidateDashboardDto
        {
            CandidateName = candidate.FullName,
            DocumentsUploaded = documents.Count,
            ApprovedDocuments = approved,
            PendingDocuments = pending,
            RejectedDocuments = rejected,
            OverallStatus = overallStatus
        };
    }

    public async Task<List<CandidateVerificationDto>> GetVerificationStatusAsync(string email)
    {
        var candidate = await _candidateRepository.GetByEmailAsync(email);

        if (candidate == null)
        {
            throw new Exception("Candidate not found");
        }

        var documents = await _documentRepository.GetByCandidateIdAsync(candidate.Id);
        var verifications = await _verificationRepository.GetByCandidateIdAsync(candidate.Id);

        return documents
            .Select(x => new CandidateVerificationDto
            {
                DocumentId = x.Id,
                FileName = x.OriginalFileName,
                Status = GetEffectiveStatus(x, verifications)
            })
            .ToList();
    }

    public async Task<string> UploadDocumentAsync(string email, IFormFile file)
    {
        var candidate = await _candidateRepository.GetByEmailAsync(email);

        if (candidate == null)
        {
            throw new Exception("Candidate not found");
        }

        if (file == null || file.Length == 0)
        {
            throw new Exception("Please select a file.");
        }

        string docType = InferDocumentType(file.FileName);

        // Upload to Supabase Storage
        var storageResult = await _fileStorageService.UploadAsync(file, candidate.Id, docType);

        // Document Replacement Check (Only replace if exact same original file name is re-uploaded)
        var existingDocs = await _documentRepository.GetByCandidateIdAsync(candidate.Id);
        var existingSameDoc = existingDocs?.FirstOrDefault(d =>
            d.OriginalFileName.Equals(file.FileName, StringComparison.OrdinalIgnoreCase));

        string? oldFilePathToDelete = null;

        if (existingSameDoc != null)
        {
            oldFilePathToDelete = existingSameDoc.FilePath;

            existingSameDoc.FileName = storageResult.FileName;
            existingSameDoc.OriginalFileName = file.FileName;
            existingSameDoc.FilePath = storageResult.ObjectPath;
            existingSameDoc.FileType = storageResult.FileType;
            existingSameDoc.FileSize = storageResult.FileSize;
            existingSameDoc.Status = "Uploaded";
            existingSameDoc.UploadedDate = DateTime.UtcNow;
        }
        else
        {
            var document = new Document
            {
                CandidateId = candidate.Id,
                FileName = storageResult.FileName,
                OriginalFileName = file.FileName,
                FilePath = storageResult.ObjectPath,
                FileType = storageResult.FileType,
                FileSize = storageResult.FileSize,
                Status = "Uploaded",
                UploadedDate = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document);
        }

        try
        {
            await _documentRepository.SaveChangesAsync();
        }
        catch
        {
            // Rollback Supabase upload if DB save fails
            await _fileStorageService.DeleteAsync(storageResult.ObjectPath);
            throw;
        }

        // Cleanup old file after successful save
        if (!string.IsNullOrWhiteSpace(oldFilePathToDelete))
        {
            try
            {
                if (File.Exists(oldFilePathToDelete))
                {
                    File.Delete(oldFilePathToDelete);
                }
                else
                {
                    await _fileStorageService.DeleteAsync(oldFilePathToDelete);
                }
            }
            catch
            {
                // Silent fallback
            }
        }

        var existingVerification = await _verificationRepository.GetByCandidateAndTypeAsync(candidate.Id, file.FileName);

        if (existingVerification == null)
        {
            var verification = new Verification
            {
                CandidateId = candidate.Id,
                VerificationType = file.FileName,
                Status = "Pending",
                ReviewerRemarks = string.Empty,
                CreatedDate = DateTime.UtcNow
            };
            await _verificationRepository.AddAsync(verification);
            await _verificationRepository.SaveChangesAsync();
        }

        if (_emailService != null && candidate != null)
        {
            try
            {
                await _emailService.SendDocumentsUploadedEmailAsync(candidate.Email, candidate.FullName);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[EMAIL NOTICE] SendDocumentsUploadedEmailAsync failed: {ex.Message}");
            }
        }

        return "Document uploaded successfully.";
    }

    public async Task<List<DocumentListDto>> GetDocumentsAsync(string email)
    {
        var candidate = await _candidateRepository.GetByEmailAsync(email);

        if (candidate == null)
        {
            throw new Exception("Candidate not found");
        }

        var documents = await _documentRepository.GetByCandidateIdAsync(candidate.Id);
        var verifications = await _verificationRepository.GetByCandidateIdAsync(candidate.Id);

        return documents.Select(x => new DocumentListDto
        {
            Id = x.Id,
            FileName = x.OriginalFileName,
            FileType = x.FileType,
            Status = GetEffectiveStatus(x, verifications),
            UploadedDate = x.UploadedDate
        }).ToList();
    }

    public async Task<DocumentDownloadDto?> DownloadDocumentAsync(string email, int documentId)
    {
        var candidate = await _candidateRepository.GetByEmailAsync(email);

        if (candidate == null)
            throw new Exception("Candidate not found");

        var document = await _documentRepository.GetByIdAsync(documentId);

        if (document == null)
            throw new Exception("Document not found");

        if (document.CandidateId != candidate.Id)
            throw new UnauthorizedAccessException();

        byte[] bytes;

        // Legacy Local File check
        if (File.Exists(document.FilePath))
        {
            bytes = await File.ReadAllBytesAsync(document.FilePath);
        }
        else
        {
            using var stream = await _fileStorageService.DownloadAsync(document.FilePath);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            bytes = ms.ToArray();
        }

        return new DocumentDownloadDto
        {
            FileBytes = bytes,
            FileName = document.OriginalFileName,
            ContentType = "application/octet-stream"
        };
    }

    private static string InferDocumentType(string fileName)
    {
        var lower = fileName.ToLowerInvariant();
        if (lower.Contains("pan")) return "PAN";
        if (lower.Contains("aadhaar") || lower.Contains("aadhar")) return "Aadhaar";
        if (lower.Contains("passport")) return "Passport";
        if (lower.Contains("resume")) return "Resume";
        if (lower.Contains("degree") || lower.Contains("mark") || lower.Contains("certificate")) return "Degree";
        if (lower.Contains("pcc") || lower.Contains("police")) return "PCC";
        return "Document";
    }
}