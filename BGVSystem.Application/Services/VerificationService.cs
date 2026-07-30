using BGVSystem.Application.DTOs.Verifications;
using BGVSystem.Application.Exceptions;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Application.DTOs.Reviewer;
namespace BGVSystem.Application.Services;

public class VerificationService : IVerificationService
{
    private readonly IVerificationRepository _verificationRepository;
    private readonly ICandidateRepository _candidateRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IAuditService _auditService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly INotificationRepository _notificationRepository;

    public VerificationService(
        IVerificationRepository verificationRepository,
        ICandidateRepository candidateRepository,
        IAssignmentRepository assignmentRepository,
        IAuditService auditService,
        IEmailTemplateService emailTemplateService = null,
        INotificationRepository notificationRepository = null)
    {
        _verificationRepository = verificationRepository;
        _candidateRepository = candidateRepository;
        _auditService = auditService;
        _assignmentRepository = assignmentRepository;
        _emailTemplateService = emailTemplateService;
        _notificationRepository = notificationRepository;
    }

    public async Task<string> CreateAsync(CreateVerificationDto dto)
    {
        var candidate = await _candidateRepository
            .GetByIdAsync(dto.CandidateId);

        if (candidate == null)
        {
            throw new Exception("Candidate not found");
        }

        var existing = await _verificationRepository
            .GetByCandidateAndTypeAsync(dto.CandidateId, dto.VerificationType);

        if (existing != null)
        {
            return "Verification already exists";
        }

        var verification = new Verification
        {
            CandidateId = dto.CandidateId,
            VerificationType = dto.VerificationType,
            Status = "Pending"
        };

        await _verificationRepository.AddAsync(verification);

        await _verificationRepository.SaveChangesAsync();

        return "Verification created successfully";
    }

    public async Task<List<VerificationResponseDto>> GetAllAsync()
    {
        var verifications = await _verificationRepository.GetAllAsync();

        return verifications.Select(x => new VerificationResponseDto
        {
            Id = x.Id,
            CandidateId = x.CandidateId,
            VerificationType = x.VerificationType,
            Status = x.Status,
            ReviewerRemarks = x.ReviewerRemarks
        }).ToList();
    }

    public async Task<VerificationResponseDto?> GetByIdAsync(int id)
    {
        var verification =
            await _verificationRepository.GetByIdAsync(id);

        if (verification == null)
        {
            throw new NotFoundException(
                $"Verification with Id {id} was not found.");
        }

        return new VerificationResponseDto
        {
            Id = verification.Id,
            CandidateId = verification.CandidateId,
            VerificationType = verification.VerificationType,
            Status = verification.Status,
            ReviewerRemarks = verification.ReviewerRemarks
        };
    }

    public async Task<string> ApproveAsync(
     int id,
     string remarks)
    {
        var verification =
            await _verificationRepository
                .GetByIdAsync(id);

        if (verification == null)
        {
            throw new NotFoundException(
                "Verification not found");
        }

        verification.Status = "Approved";

        verification.ReviewerRemarks =
            remarks;

        var candidate =
            await _candidateRepository
                .GetByIdAsync(
                    verification.CandidateId);

        if (candidate == null)
        {
            throw new NotFoundException(
                "Candidate not found");
        }

        candidate.Status = "Approved";

        await _verificationRepository
            .SaveChangesAsync();

        await UpdateCandidateStatusAsync(
    verification.CandidateId);

        await _candidateRepository
            .SaveChangesAsync();

        await _auditService.AddLogAsync(
            "Verification Approved",
            "reviewer@test.com",
            "Reviewer");

        // 📧 Dispatch Status Email Notification
        if (_emailTemplateService != null && _notificationRepository != null && candidate != null)
        {
            try
            {
                var body = await _emailTemplateService.GetStatusUpdateTemplateAsync(
                    candidate.FullName,
                    "Approved",
                    remarks ?? $"{verification.VerificationType} Check Approved");

                await _notificationRepository.AddAsync(new Notification
                {
                    ToEmail = candidate.Email,
                    Subject = $"BGV Status Alert: {verification.VerificationType} Approved",
                    Body = body,
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

        return "Verification approved successfully";
    }

    public async Task<string> RejectAsync(
     int id,
     string remarks)
    {
        var verification =
            await _verificationRepository
                .GetByIdAsync(id);

        if (verification == null)
        {
            throw new NotFoundException(
                "Verification not found");
        }

        verification.Status = "Rejected";

        verification.ReviewerRemarks =
            remarks;

        var candidate =
            await _candidateRepository
                .GetByIdAsync(
                    verification.CandidateId);

        if (candidate == null)
        {
            throw new NotFoundException(
                "Candidate not found");
        }

        candidate.Status = "Rejected";

        await _verificationRepository
            .SaveChangesAsync();

        await UpdateCandidateStatusAsync(
    verification.CandidateId);

        await _candidateRepository
            .SaveChangesAsync();

        await _auditService.AddLogAsync(
            "Verification Rejected",
            "reviewer@test.com",
            "Reviewer");

        // 📧 Dispatch Status Email Notification
        if (_emailTemplateService != null && _notificationRepository != null && candidate != null)
        {
            try
            {
                var body = await _emailTemplateService.GetStatusUpdateTemplateAsync(
                    candidate.FullName,
                    "Rejected",
                    remarks ?? $"{verification.VerificationType} Check Rejected");

                await _notificationRepository.AddAsync(new Notification
                {
                    ToEmail = candidate.Email,
                    Subject = $"BGV Status Alert: {verification.VerificationType} Rejected",
                    Body = body,
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

        return "Verification rejected successfully";
    }

    public async Task UpdateCandidateStatusAsync(
     int candidateId)
    {
        var candidate =
            await _candidateRepository
                .GetByIdAsync(candidateId);

        if (candidate == null)
        {
            return;
        }

        var verifications =
            await _verificationRepository
                .GetByCandidateIdAsync(candidateId);

        if (!verifications.Any())
        {
            return;
        }

        if (verifications.Any(x =>
            x.Status == "Rejected"))
        {
            candidate.Status = "Rejected";
        }
        else if (verifications.All(x =>
            x.Status == "Approved"))
        {
            candidate.Status = "Approved";
        }
        else
        {
            candidate.Status = "In Progress";
        }

        await _candidateRepository
            .SaveChangesAsync();
    }

    public async Task<string> ReReviewAsync(int id)
    {
        var verification =
            await _verificationRepository
                .GetByIdAsync(id);

        if (verification == null)
        {
            throw new NotFoundException(
                "Verification not found");
        }

        verification.Status = "Pending";

        verification.ReviewerRemarks =
            string.Empty;

        await _verificationRepository
            .SaveChangesAsync();

        await UpdateCandidateStatusAsync(
            verification.CandidateId);

        await _auditService.AddLogAsync(
            "Verification Reopened",
            "reviewer@test.com",
            "Reviewer");

        return "Verification moved back to Pending.";
    }
    public async Task<ReviewerDashboardDto> GetDashboardStatisticsAsync()
    {
        var assigned =
            await _verificationRepository
                .GetAssignedCountAsync();

        var pending =
            await _verificationRepository
                .GetPendingCountAsync();

        var approved =
            await _verificationRepository
                .GetApprovedCountAsync();

        var rejected =
            await _verificationRepository
                .GetRejectedCountAsync();

        double completionPercentage = 0;

        if (assigned > 0)
        {
            completionPercentage =
                Math.Round(
                    (double)approved / assigned * 100,
                    2
                );
        }

        return new ReviewerDashboardDto
        {
            Assigned = assigned,
            Pending = pending,
            Approved = approved,
            Rejected = rejected,
            CompletionPercentage = completionPercentage
        };
    }

    public async Task<List<VerificationResponseDto>>
GetByCandidateIdAsync(int candidateId)
    {
        var verifications =
            await _verificationRepository
                .GetByCandidateIdAsync(candidateId);

        var unique = verifications
            .GroupBy(x => x.VerificationType)
            .Select(g => g.OrderBy(x => x.Status == "Pending" ? 1 : 0).ThenByDescending(x => x.Id).First())
            .ToList();

        return unique
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

    public async Task<List<VerificationResponseDto>>
GetReviewerCandidateVerificationsAsync(
    int candidateId,
    int reviewerId)
    {
        var isAssigned =
            await _assignmentRepository
                .IsCandidateAssignedToReviewerAsync(
                    candidateId,
                    reviewerId);

        if (!isAssigned)
        {
            throw new UnauthorizedAccessException(
                "This candidate is not assigned to you.");
        }

        var verifications =
            await _verificationRepository
                .GetByCandidateIdAsync(candidateId);

        var unique = verifications
            .GroupBy(x => x.VerificationType)
            .Select(g => g.OrderBy(x => x.Status == "Pending" ? 1 : 0).ThenByDescending(x => x.Id).First())
            .ToList();

        return unique
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


}