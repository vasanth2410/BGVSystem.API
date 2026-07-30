namespace BGVSystem.Application.DTOs.Verifications;

public class OcrResultDto
{
    public int DocumentId { get; set; }
    public string DocumentType { get; set; } = "Unknown";
    public string ExtractedDocumentNumber { get; set; } = string.Empty;
    public string ExtractedName { get; set; } = string.Empty;
    public string ExtractedDob { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string RawText { get; set; } = string.Empty;
    public string Status { get; set; } = "Extracted";
}

public class PanMatchResultDto
{
    public string Status { get; set; } = "Pending"; // Verified, Mismatch, Invalid
    public string PanNumber { get; set; } = string.Empty;
    public string MatchedName { get; set; } = string.Empty;
    public double NameMatchScore { get; set; }
    public string IssuedBy { get; set; } = "Income Tax Department (NSDL Gateway)";
    public DateTime VerificationTime { get; set; } = DateTime.UtcNow;
}

public class AadhaarMatchResultDto
{
    public string Status { get; set; } = "Pending"; // Verified, Mismatch, Invalid
    public string MaskedAadhaar { get; set; } = string.Empty;
    public string MatchedName { get; set; } = string.Empty;
    public double NameMatchScore { get; set; }
    public bool AddressMatched { get; set; } = true;
    public string IssuedBy { get; set; } = "UIDAI Aadhaar Verification Vault";
    public DateTime VerificationTime { get; set; } = DateTime.UtcNow;
}

public class CriminalMatchResultDto
{
    public string Status { get; set; } = "Clean"; // Clean, Flagged, Under Investigation
    public int RecordsFound { get; set; } = 0;
    public int CourtCaseCount { get; set; } = 0;
    public string Summary { get; set; } = "No criminal records or pending court warrants found.";
    public string DatabaseSearched { get; set; } = "CCTNS & National Judicial Data Grid";
    public DateTime VerificationTime { get; set; } = DateTime.UtcNow;
}

public class LiveVerificationResultDto
{
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public PanMatchResultDto PanCheck { get; set; } = new();
    public AadhaarMatchResultDto AadhaarCheck { get; set; } = new();
    public CriminalMatchResultDto CriminalCheck { get; set; } = new();
    public string OverallStatus { get; set; } = "Verified";
    public double OverallConfidenceScore { get; set; } = 95.0;
    public DateTime VerifiedAt { get; set; } = DateTime.UtcNow;
}
