using BGVSystem.Application.DTOs.Reviewer;
using BGVSystem.Application.DTOs.Verifications;
using BGVSystem.Application.Interfaces;

namespace BGVSystem.Application.Services;

public class ReviewerService : IReviewerService
{
    private readonly ICandidateRepository _candidateRepository;

    private readonly IDocumentRepository _documentRepository;

    private readonly IVerificationRepository _verificationRepository;

    public ReviewerService(
        ICandidateRepository candidateRepository,
        IDocumentRepository documentRepository,
        IVerificationRepository verificationRepository)
    {
        _candidateRepository = candidateRepository;
        _documentRepository = documentRepository;
        _verificationRepository = verificationRepository;
    }

    public async Task<ReviewerDashboardDto>
        GetDashboardAsync()
    {
        var candidates =
            await _candidateRepository.GetAllAsync();

        var pending =
            await _verificationRepository
                .GetByStatusAsync("Pending");

        var approved =
            await _verificationRepository
                .GetByStatusAsync("Approved");

        var rejected =
            await _verificationRepository
                .GetByStatusAsync("Rejected");

        return new ReviewerDashboardDto
        {
            TotalCandidates = candidates.Count,

            TotalDocuments = candidates
                .Sum(x => x.Documents?.Count ?? 0),

            PendingVerifications =
                pending.Count,

            ApprovedVerifications =
                approved.Count,

            RejectedVerifications =
                rejected.Count
        };
    }

    public async Task<List<VerificationResponseDto>>
        GetPendingVerificationsAsync()
    {
        var items =
            await _verificationRepository
                .GetByStatusAsync("Pending");

        return items.Select(x =>
            new VerificationResponseDto
            {
                Id = x.Id,
                CandidateId = x.CandidateId,
                VerificationType =
                    x.VerificationType,
                Status = x.Status,
                ReviewerRemarks =
                    x.ReviewerRemarks
            }).ToList();
    }

    public async Task<List<VerificationResponseDto>>
        GetApprovedVerificationsAsync()
    {
        var items =
            await _verificationRepository
                .GetByStatusAsync("Approved");

        return items.Select(x =>
            new VerificationResponseDto
            {
                Id = x.Id,
                CandidateId = x.CandidateId,
                VerificationType =
                    x.VerificationType,
                Status = x.Status,
                ReviewerRemarks =
                    x.ReviewerRemarks
            }).ToList();
    }

    public async Task<List<VerificationResponseDto>>
        GetRejectedVerificationsAsync()
    {
        var items =
            await _verificationRepository
                .GetByStatusAsync("Rejected");

        return items.Select(x =>
            new VerificationResponseDto
            {
                Id = x.Id,
                CandidateId = x.CandidateId,
                VerificationType =
                    x.VerificationType,
                Status = x.Status,
                ReviewerRemarks =
                    x.ReviewerRemarks
            }).ToList();
    }
}