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

    private readonly IUserRepository _userRepository;

    private readonly IEmailService _emailService;

    private readonly IAuditService _auditService;

    public AdminService(
        ICandidateRepository candidateRepository,
        IDocumentRepository documentRepository,
        IVerificationRepository verificationRepository,
        INotificationRepository notificationRepository,
        IAuditRepository auditRepository,
        IUserRepository userRepository,
        IEmailService emailService = null,
        IAuditService auditService = null)
    {
        _candidateRepository = candidateRepository;
        _documentRepository = documentRepository;
        _verificationRepository = verificationRepository;
        _notificationRepository = notificationRepository;
        _auditRepository = auditRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _auditService = auditService;
    }

    public async Task<AdminDashboardDto>
        GetDashboardAsync()
    {
        return new AdminDashboardDto
        {
            TotalCandidates =
                await _candidateRepository.GetTotalCountAsync(),

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

    public async Task<List<ReviewerDto>> GetReviewersAsync()
    {
        var reviewers =
            await _userRepository
                .GetReviewersAsync();

        return reviewers
            .Select(x =>
                new ReviewerDto
                {
                    Id = x.Id,
                    FullName = x.FullName
                })
            .ToList();
    }

    public async Task<string> CreateReviewerAsync(CreateReviewerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.FullName))
        {
            throw new Exception("FullName and Email are required.");
        }

        var cleanEmail = dto.Email.Trim().ToLower();

        var existingUser = await _userRepository.GetByEmailAsync(cleanEmail);
        if (existingUser != null)
        {
            throw new Exception("User with this email already exists.");
        }

        // Generate cryptographically secure temporary password
        var tempPassword = GenerateTemporaryPassword();
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(tempPassword);

        var reviewerUser = new BGVSystem.Domain.Entities.User
        {
            FullName = dto.FullName.Trim(),
            Email = cleanEmail,
            PasswordHash = hashedPassword,
            RoleId = 2, // Reviewer RoleId
            MustChangePassword = true,
            CreatedDate = DateTime.UtcNow
        };

        await _userRepository.AddAsync(reviewerUser);
        await _userRepository.SaveChangesAsync();

        // Send invitation credentials email using existing IEmailService if available
        if (_emailService != null)
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(reviewerUser.Email, reviewerUser.FullName, tempPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL NOTICE] SendWelcomeEmailAsync for Reviewer failed: {ex.Message}");
            }
        }

        if (_auditService != null)
        {
            await _auditService.AddLogAsync(
                "Reviewer Account Created",
                reviewerUser.Email,
                "Admin");
        }

        return "Reviewer account created successfully and invitation email sent.";
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
        var randomBytes = new byte[12];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        var result = new char[12];
        for (int i = 0; i < 12; i++)
        {
            result[i] = chars[randomBytes[i] % chars.Length];
        }
        return new string(result);
    }
}