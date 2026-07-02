using BGVSystem.Application.DTOs.Reviewer;
using BGVSystem.Application.DTOs.ReviewerAssignments;
using BGVSystem.Application.DTOs.Verifications;
using BGVSystem.Application.Interfaces;

namespace BGVSystem.Application.Services;

public class ReviewerService : IReviewerService
{
    private readonly ICandidateRepository _candidateRepository;

    private readonly IDocumentRepository _documentRepository;

    private readonly IVerificationRepository _verificationRepository;
    private readonly IAssignmentRepository
    _assignmentRepository;

    private readonly IUserRepository
        _userRepository;
    public ReviewerService(
        ICandidateRepository candidateRepository,
        IDocumentRepository documentRepository,
        IVerificationRepository verificationRepository,
        IAssignmentRepository assignmentRepository,
        IUserRepository userRepository
        )
    {
        _candidateRepository = candidateRepository;
        _documentRepository = documentRepository;
        _verificationRepository = verificationRepository;
        _assignmentRepository = assignmentRepository;
        _userRepository = userRepository;
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

        var totalDocuments =
     await _documentRepository.GetCountAsync();

        double completionPercentage = 0;

        if (candidates.Count > 0)
        {
            completionPercentage =
                Math.Round(
                    (double)approved.Count /
                    candidates.Count * 100,
                    2
                );
        }

        return new ReviewerDashboardDto
        {
            Assigned = candidates.Count,

            Pending = pending.Count,

            Approved = approved.Count,

            Rejected = rejected.Count,

            CompletionPercentage = completionPercentage
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

    public async Task<List<AssignedCandidateDto>>
    GetAssignedCandidatesAsync(
        string reviewerEmail)
    {
        var reviewer =
            await _userRepository
                .GetByEmailAsync(
                    reviewerEmail);

        if (reviewer == null)
        {
            throw new Exception(
                "Reviewer not found");
        }

        var assignments =
            await _assignmentRepository
                .GetByReviewerIdAsync(
                    reviewer.Id);

        var result =
            new List<AssignedCandidateDto>();

        foreach (var assignment in assignments)
        {
            var candidate =
                await _candidateRepository
                    .GetByIdAsync(
                        assignment.CandidateId);

            if (candidate == null)
                continue;

            result.Add(
                new AssignedCandidateDto
                {
                    CandidateId =
                        candidate.Id,

                    FullName =
                        candidate.FullName,

                    Email =
                        candidate.Email,

                    Status =
                        candidate.Status
                });
        }

        return result;
    }

    public async Task<CandidateWorkQueueDto?>
GetCandidateAsync(
    int candidateId)
    {
        var candidate =
            await _candidateRepository
                .GetByIdAsync(candidateId);

        if (candidate == null)
        {
            return null;
        }

        return new CandidateWorkQueueDto
        {
            CandidateId = candidate.Id,
            FullName = candidate.FullName,
            Email = candidate.Email,
            Status = candidate.Status
        };
    }

    public async Task<List<CandidateDocumentDto>>
GetCandidateDocumentsAsync(
    int candidateId)
    {
        var documents =
            await _documentRepository
                .GetByCandidateIdAsync(
                    candidateId);

        return documents
            .Select(x =>
                new CandidateDocumentDto
                {
                    Id = x.Id,
                    FileName =
                        x.OriginalFileName,
                    Status = x.Status,
                    FileType = x.FileType
                })
            .ToList();
    }

    public async Task<List<VerificationResponseDto>>
GetCandidateVerificationsAsync(
    int candidateId)
    {
        var items =
            await _verificationRepository
                .GetByCandidateIdAsync(
                    candidateId);

        return items
            .Select(x =>
                new VerificationResponseDto
                {
                    Id = x.Id,
                    CandidateId =
                        x.CandidateId,
                    VerificationType =
                        x.VerificationType,
                    Status = x.Status,
                    ReviewerRemarks =
                        x.ReviewerRemarks
                })
            .ToList();
    }
    private async Task<bool>
IsAssignedReviewerAsync(
    int candidateId,
    string reviewerEmail)
    {
        var reviewer =
            await _userRepository
                .GetByEmailAsync(
                    reviewerEmail);

        if (reviewer == null)
            return false;

        var assignments =
            await _assignmentRepository
                .GetByReviewerIdAsync(
                    reviewer.Id);

        return assignments.Any(x =>
            x.CandidateId ==
            candidateId);
    }

    public async Task<ReviewerDocumentDto?>
GetDocumentAsync(
    int documentId,
    string reviewerEmail)
    {
        var document =
            await _documentRepository
                .GetByIdAsync(
                    documentId);

        if (document == null)
        {
            return null;
        }

        var assigned =
            await IsAssignedReviewerAsync(
                document.CandidateId,
                reviewerEmail);

        if (!assigned)
        {
            throw new Exception(
                "Access denied");
        }

        return new ReviewerDocumentDto
        {
            Id = document.Id,

            FileName =
                document.FileName,

            OriginalFileName =
                document.OriginalFileName,

            Status =
                document.Status,

            FileType =
                document.FileType
        };
    }

    public async Task<
(
byte[] Content,
string FileName,
string ContentType
)>
DownloadDocumentAsync(
    int documentId,
    string reviewerEmail)
    {
        var document =
            await _documentRepository
                .GetByIdAsync(
                    documentId);

        if (document == null)
        {
            throw new Exception(
                "Document not found");
        }

        var assigned =
            await IsAssignedReviewerAsync(
                document.CandidateId,
                reviewerEmail);

        if (!assigned)
        {
            throw new Exception(
                "Access denied");
        }

        var bytes =
            await File.ReadAllBytesAsync(
                document.FilePath);

        return (
            bytes,
            document.OriginalFileName,
            "application/octet-stream"
        );
    }

    public async Task<string> ReviewDocumentAsync(
    int documentId,
    string reviewerEmail,
    ReviewDocumentDto dto)
    {
        var document =
            await _documentRepository
                .GetByIdAsync(documentId);

        if (document == null)
        {
            throw new Exception("Document not found");
        }

        var assigned =
            await IsAssignedReviewerAsync(
                document.CandidateId,
                reviewerEmail);

        if (!assigned)
        {
            throw new Exception("Access denied");
        }

        if (dto.Status != "Approved" &&
            dto.Status != "Rejected")
        {
            throw new Exception(
                "Status must be Approved or Rejected");
        }

        document.Status = dto.Status;

        var verification =
            await _verificationRepository
                .GetByCandidateAndTypeAsync(
                    document.CandidateId,
                    document.OriginalFileName);

        if (verification != null)
        {
            verification.Status = dto.Status;

            verification.ReviewerRemarks =
                dto.ReviewerRemarks;
        }

        var candidate =
            await _candidateRepository
                .GetByIdAsync(document.CandidateId);

        if (candidate != null)
        {
            var documents =
                await _documentRepository
                    .GetByCandidateIdAsync(candidate.Id);

            if (documents.All(x => x.Status == "Approved"))
            {
                candidate.Status = "Completed";
            }
            else if (documents.Any(x => x.Status == "Rejected"))
            {
                candidate.Status = "Rejected";
            }
            else
            {
                candidate.Status = "In Progress";
            }

            await _candidateRepository.UpdateAsync(candidate);
            await _candidateRepository.SaveChangesAsync();
        }

        await _verificationRepository.SaveChangesAsync();

        return "Document reviewed successfully.";
    }
}