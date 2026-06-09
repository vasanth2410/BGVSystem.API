namespace BGVSystem.Application.DTOs.CandidatePortal;

public class CandidateVerificationDto
{
    public int DocumentId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}