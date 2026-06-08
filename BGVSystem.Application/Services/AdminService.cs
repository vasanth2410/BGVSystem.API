using BGVSystem.Application.DTOs.Admin;
using BGVSystem.Application.Interfaces;

namespace BGVSystem.Application.Services;

public class AdminService : IAdminService
{
    private readonly ICandidateRepository _candidateRepository;

    private readonly IDocumentRepository _documentRepository;

    private readonly IVerificationRepository _verificationRepository;

    private readonly INotificationRepository _notificationRepository;

    private readonly IAuditRepository _auditRepository;

    public AdminService(
        ICandidateRepository candidateRepository,
        IDocumentRepository documentRepository,
        IVerificationRepository verificationRepository,
        INotificationRepository notificationRepository,
        IAuditRepository auditRepository)
    {
        _candidateRepository = candidateRepository;
        _documentRepository = documentRepository;
        _verificationRepository = verificationRepository;
        _notificationRepository = notificationRepository;
        _auditRepository = auditRepository;
    }

    public async Task<AdminDashboardDto>
        GetDashboardAsync()
    {
        return new AdminDashboardDto
        {
            TotalCandidates =
                await _candidateRepository.GetCountAsync(),

            TotalDocuments =
                await _documentRepository.GetCountAsync(),

            TotalVerifications =
                await _verificationRepository.GetCountAsync(),

            PendingVerifications =
                await _verificationRepository
                    .GetCountByStatusAsync("Pending"),

            ApprovedVerifications =
                await _verificationRepository
                    .GetCountByStatusAsync("Approved"),

            RejectedVerifications =
                await _verificationRepository
                    .GetCountByStatusAsync("Rejected"),

            NotificationsSent =
                await _notificationRepository
                    .GetSentCountAsync(),

            AuditLogsCount =
                await _auditRepository.GetCountAsync()
        };
    }
}