using BGVSystem.Application.DTOs.Reviewer;
using BGVSystem.Application.DTOs.ReviewerAssignments;
using BGVSystem.Application.DTOs.Verifications;

namespace BGVSystem.Application.Interfaces;

public interface IReviewerService
{
    Task<ReviewerDashboardDto> GetDashboardAsync();

    Task<List<VerificationResponseDto>>
        GetPendingVerificationsAsync();

    Task<List<VerificationResponseDto>>
        GetApprovedVerificationsAsync();

    Task<List<VerificationResponseDto>>
        GetRejectedVerificationsAsync();

    Task<List<AssignedCandidateDto>>
    GetAssignedCandidatesAsync(
        string reviewerEmail);
}