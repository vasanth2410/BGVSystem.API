namespace BGVSystem.Application.DTOs.ReviewerAssignments;

public class AssignedCandidateDto
{
    public int AssignmentId { get; set; }

    public int CandidateId { get; set; }

    public string CandidateName { get; set; } = string.Empty;

    public int ReviewerId { get; set; }

    public string ReviewerName { get; set; } = string.Empty;

    public DateTime AssignedDate { get; set; }

    public string Status { get; set; } = string.Empty;
}