using BGVSystem.Application.DTOs.Reviewer;
using BGVSystem.Application.DTOs.ReviewerAssignments;
using BGVSystem.Application.DTOs.Verifications;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using DocumentFormat.OpenXml.Spreadsheet;

namespace BGVSystem.Application.Services;

public class ReviewerService : IReviewerService
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly INotificationRepository _notificationRepository;

    public ReviewerService(
        ICandidateRepository candidateRepository,
        IDocumentRepository documentRepository,
        IVerificationRepository verificationRepository,
        IAssignmentRepository assignmentRepository,
        IUserRepository userRepository,
        IEmailTemplateService emailTemplateService = null,
        INotificationRepository notificationRepository = null
        )
    {
        _candidateRepository = candidateRepository;
        _documentRepository = documentRepository;
        _verificationRepository = verificationRepository;
        _assignmentRepository = assignmentRepository;
        _userRepository = userRepository;
        _emailTemplateService = emailTemplateService;
        _notificationRepository = notificationRepository;
    }

    public async Task<ReviewerDashboardDto>
        GetDashboardAsync(string reviewerEmail)
    {
        var reviewer = await _userRepository.GetByEmailAsync(reviewerEmail);
        if (reviewer == null)
        {
            throw new Exception("Reviewer not found");
        }

        var assignments = await _assignmentRepository.GetByReviewerIdAsync(reviewer.Id);
        var candidateIds = assignments.Select(x => x.CandidateId).ToList();

        var verifications = await _verificationRepository.GetByCandidateIdsAsync(candidateIds);

        // Group by CandidateId and VerificationType, selecting the non-pending status if available (e.g. if one duplicate is Approved/Rejected, it wins)
        var uniqueVerifications = verifications
            .GroupBy(x => new { x.CandidateId, x.VerificationType })
            .Select(g => g.OrderBy(x => x.Status == "Pending" ? 1 : 0).First())
            .ToList();

        var pending = uniqueVerifications.Where(x => x.Status == "Pending").ToList();
        var approved = uniqueVerifications.Where(x => x.Status == "Approved").ToList();
        var rejected = uniqueVerifications.Where(x => x.Status == "Rejected").ToList();

        double completionPercentage = 0;

        if (uniqueVerifications.Count > 0)
        {
            completionPercentage =
                Math.Round(
                    (double)approved.Count /
                    uniqueVerifications.Count * 100,
                    2
                );
        }

        return new ReviewerDashboardDto
        {
            Assigned = uniqueVerifications.Count,

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
                .GetByEmailAsync(reviewerEmail);

        if (reviewer == null)
        {
            throw new Exception("Reviewer not found");
        }

        var assignments =
            await _assignmentRepository
                .GetByReviewerIdAsync(reviewer.Id);

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
                    AssignmentId = assignment.Id,

                    CandidateId = candidate.Id,

                    CandidateName = candidate.FullName,

                    ReviewerId = reviewer.Id,

                    ReviewerName = reviewer.FullName,

                    AssignedDate = assignment.AssignedDate,

                    Status = candidate.Status
                });
        }

        return result;
    }

    public async Task<CandidateWorkQueueDto?>
 GetCandidateAsync(
     int candidateId,
     string reviewerEmail)
    {
        var assigned =
            await IsAssignedReviewerAsync(
                candidateId,
                reviewerEmail);

        if (!assigned)
        {
            throw new Exception("Access denied");
        }

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
    int candidateId,
    string reviewerEmail)
    {
        var assigned =
            await IsAssignedReviewerAsync(
                candidateId,
                reviewerEmail);

        if (!assigned)
        {
            throw new Exception("Access denied");
        }

        var documents =
            await _documentRepository
                .GetByCandidateIdAsync(candidateId);

        return documents
            .Select(x => new CandidateDocumentDto
            {
                Id = x.Id,
                FileName = x.OriginalFileName,
                Status = x.Status,
                FileType = x.FileType
            })
            .ToList();
    }

    public async Task<List<VerificationResponseDto>>
 GetCandidateVerificationsAsync(
     int candidateId,
     string reviewerEmail)
    {
        var assigned =
            await IsAssignedReviewerAsync(
                candidateId,
                reviewerEmail);

        if (!assigned)
        {
            throw new Exception("Access denied");
        }

        var items =
            await _verificationRepository
                .GetByCandidateIdAsync(candidateId);

        return items
            .Select(x => new VerificationResponseDto
            {
                Id = x.Id,
                CandidateId = x.CandidateId,
                VerificationType = x.VerificationType,
                Status = x.Status,
                ReviewerRemarks = x.ReviewerRemarks
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
            var oldStatus = candidate.Status;
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

            // 📧 Automatic Email Notifications for Document Request & Status Change
            if (_emailTemplateService != null && _notificationRepository != null)
            {
                try
                {
                    if (dto.Status == "Rejected")
                    {
                        var docBody = await _emailTemplateService.GetDocumentRequestTemplateAsync(
                            candidate.FullName,
                            document.OriginalFileName,
                            dto.ReviewerRemarks ?? "Resubmission requested by reviewer");

                        await _notificationRepository.AddAsync(new Notification
                        {
                            ToEmail = candidate.Email,
                            Subject = $"Action Required: Resubmit {document.OriginalFileName}",
                            Body = docBody,
                            Status = "Pending",
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    var statusBody = await _emailTemplateService.GetStatusUpdateTemplateAsync(
                        candidate.FullName,
                        candidate.Status,
                        dto.ReviewerRemarks ?? $"Document {document.OriginalFileName} marked as {dto.Status}");

                    await _notificationRepository.AddAsync(new Notification
                    {
                        ToEmail = candidate.Email,
                        Subject = $"BGV Status Alert: {candidate.Status}",
                        Body = statusBody,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow
                    });

                    await _notificationRepository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EMAIL TRIGGER ERROR] {ex.Message}");
                }
            }
        }

        await _verificationRepository.SaveChangesAsync();

        return "Document reviewed successfully.";
    }

    public async Task<List<ReviewerDocumentDto>>
GetReviewerDocumentsAsync(
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
            new List<ReviewerDocumentDto>();

        foreach (var assignment in assignments)
        {
            var candidate =
                await _candidateRepository
                    .GetByIdAsync(
                        assignment.CandidateId);

            if (candidate == null)
                continue;

            var documents =
                await _documentRepository
                    .GetByCandidateIdAsync(
                        assignment.CandidateId);

            result.AddRange(
                documents.Select(d =>
                    new ReviewerDocumentDto
                    {
                        Id = d.Id,

                        FileName =
                            d.OriginalFileName,

                        OriginalFileName =
                            d.OriginalFileName,

                        FileType =
                            d.FileType,

                        Status =
                            d.Status
                    }));
        }

        return result;
    }

    public async Task<List<VerificationResponseDto>>
GetReviewerVerificationsAsync(
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

        var candidateIds =
            assignments
                .Select(x => x.CandidateId)
                .ToList();

        var verifications =
            await _verificationRepository
                .GetByCandidateIdsAsync(
                    candidateIds);

        return verifications
            .Select(x =>
                new VerificationResponseDto
                {
                    Id = x.Id,
                    CandidateId = x.CandidateId,
                    VerificationType =
                        x.VerificationType,
                    Status = x.Status,
                    ReviewerRemarks =
                        x.ReviewerRemarks
                })
            .ToList();
    }
}