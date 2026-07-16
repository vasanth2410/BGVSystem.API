using BGVSystem.Application.DTOs.Reviewer;
using BGVSystem.Application.DTOs.ReviewerAssignments;
using BGVSystem.Application.DTOs.Verifications;

namespace BGVSystem.Application.Interfaces;

public interface IReviewerService
{
    Task<ReviewerDashboardDto> GetDashboardAsync(string reviewerEmail);

    Task<List<VerificationResponseDto>>GetPendingVerificationsAsync();

    Task<List<VerificationResponseDto>>GetApprovedVerificationsAsync();

    Task<List<VerificationResponseDto>>GetRejectedVerificationsAsync();

    Task<List<AssignedCandidateDto>>GetAssignedCandidatesAsync(string reviewerEmail);

    Task<CandidateWorkQueueDto?>
GetCandidateAsync(
    int candidateId,
    string reviewerEmail);

    Task<List<CandidateDocumentDto>>
    GetCandidateDocumentsAsync(
        int candidateId,
        string reviewerEmail);

    Task<List<VerificationResponseDto>>
    GetCandidateVerificationsAsync(
        int candidateId,
        string reviewerEmail);

    Task<ReviewerDocumentDto?>
GetDocumentAsync(
    int documentId,
    string reviewerEmail);

    Task<(byte[] Content,
          string FileName,
          string ContentType)>
    DownloadDocumentAsync(
        int documentId,
        string reviewerEmail);

    Task<string> ReviewDocumentAsync(
    int documentId,
    string reviewerEmail,
    ReviewDocumentDto dto);

    Task<List<ReviewerDocumentDto>>
GetReviewerDocumentsAsync(
    string reviewerEmail);

    Task<List<VerificationResponseDto>>
GetReviewerVerificationsAsync(
    string reviewerEmail);

}