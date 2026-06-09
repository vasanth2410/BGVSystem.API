namespace BGVSystem.Application.DTOs.CandidatePortal;

public class CandidateDashboardDto
{
    public string CandidateName { get; set; } = string.Empty;

    public int DocumentsUploaded { get; set; }

    public int ApprovedDocuments { get; set; }

    public int PendingDocuments { get; set; }

    public int RejectedDocuments { get; set; }

    public string OverallStatus { get; set; } = string.Empty;
}