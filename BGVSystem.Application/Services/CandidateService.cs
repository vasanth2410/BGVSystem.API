using BGVSystem.Application.DTOs;
//using BGVSystem.Application.DTOs.Candidates;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Application.Exceptions;
namespace BGVSystem.Application.Services;
using BGVSystem.Application.DTOs.Notifications;
using BGVSystem.Application.DTOs.Candidates;

public class CandidateService : ICandidateService
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IAuditService _auditService;
    private readonly IEmailService _emailService;
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IUserRepository _userRepository;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IDocumentRepository _documentRepository;

    public CandidateService(
        ICandidateRepository candidateRepository,
        IAuditService auditService,
        INotificationRepository notificationRepository,
        IEmailTemplateService emailTemplateService,
        IUserRepository userRepository,
        IVerificationRepository verificationRepository,
        IDocumentRepository documentRepository,
        IEmailService emailService = null)
    {
        _candidateRepository = candidateRepository;
        _auditService = auditService;
        _notificationRepository = notificationRepository;
        _emailTemplateService = emailTemplateService;
        _userRepository = userRepository;
        _verificationRepository = verificationRepository;
        _documentRepository = documentRepository;
        _emailService = emailService;
    }

    private string GenerateTemporaryPassword()
    {
        return $"Temp@{Random.Shared.Next(1000, 9999)}";
    }

    public async Task<List<CandidateResponseDto>> GetAllAsync()
    {
        var candidates = await _candidateRepository.GetAllAsync();

        foreach (var candidate in candidates)
        {
            await SyncCandidateStatusAsync(candidate);
        }

        return candidates.Select(x => new CandidateResponseDto
        {
            Id = x.Id,
            FullName = x.FullName,
            Email = x.Email,
            PhoneNumber = x.PhoneNumber,
            Status = x.Status
        }).ToList();
    }

    private async Task SyncCandidateStatusAsync(Candidate candidate)
    {
        var verifications = _verificationRepository != null
            ? await _verificationRepository.GetByCandidateIdAsync(candidate.Id)
            : new List<Verification>();

        var documents = _documentRepository != null
            ? await _documentRepository.GetByCandidateIdAsync(candidate.Id)
            : new List<Document>();

        if (!verifications.Any() && !documents.Any()) return;

        bool hasRejections = verifications.Any(v => (v.Status ?? "").Equals("Rejected", StringComparison.OrdinalIgnoreCase)) ||
                             documents.Any(d => (d.Status ?? "").Equals("Rejected", StringComparison.OrdinalIgnoreCase));

        bool allUploadedApproved = documents.Any() && documents.All(d =>
            (d.Status ?? "").Equals("Approved", StringComparison.OrdinalIgnoreCase) ||
            verifications.Any(v => (v.Status ?? "").Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(v.VerificationType) &&
                (v.VerificationType.Equals(d.FileName, StringComparison.OrdinalIgnoreCase) || v.VerificationType.Equals(d.OriginalFileName, StringComparison.OrdinalIgnoreCase)))
        );

        bool allVerifsApproved = verifications.Any() && verifications.All(v => (v.Status ?? "").Equals("Approved", StringComparison.OrdinalIgnoreCase));

        if (hasRejections)
        {
            if (candidate.Status != "Rejected")
            {
                candidate.Status = "Rejected";
                await _candidateRepository.SaveChangesAsync();
            }
        }
        else if (allUploadedApproved || allVerifsApproved)
        {
            if (candidate.Status != "Approved")
            {
                candidate.Status = "Approved";
                await _candidateRepository.SaveChangesAsync();
            }
        }
    }

    public async Task<CandidateResponseDto?> GetByIdAsync(int id)
    {
        var candidate =
            await _candidateRepository
                .GetByIdAsync(id);

        if (candidate == null)
        {
            throw new NotFoundException(
     "Candidate not found");
        }

        return new CandidateResponseDto
        {
            Id = candidate.Id,
            FullName = candidate.FullName,
            Email = candidate.Email,
            PhoneNumber = candidate.PhoneNumber,
            Status = candidate.Status
        };
    }

    public async Task<string> CreateAsync(
    CreateCandidateDto dto)
    {
        var existingUser =
    await _userRepository
        .GetByEmailAsync(dto.Email);

        if (existingUser != null)
        {
            var existingCandidate = await _candidateRepository.GetByEmailAsync(dto.Email);
            if (existingCandidate == null)
            {
                // Auto-clean orphan user record left behind by past candidate deletion
                await _userRepository.DeleteAsync(existingUser);
                await _userRepository.SaveChangesAsync();
                existingUser = null;
            }
        }

        if (existingUser != null)
        {
            throw new ValidationException(
                "Email already exists");
        }
        var candidate = new Candidate
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            PANNumber = dto.PANNumber,
            AadhaarNumber = dto.AadhaarNumber,
            AppliedRole = dto.AppliedRole,
            DateOfJoining = dto.DateOfJoining,
            Status = "Pending"
        };

        await _candidateRepository.AddAsync(candidate);

        await _candidateRepository.SaveChangesAsync();

        var temporaryPassword =
    GenerateTemporaryPassword();

        var hashedPassword =
            BCrypt.Net.BCrypt.HashPassword(
                temporaryPassword);

        var user = new User
        {
            FullName = candidate.FullName,

            Email = candidate.Email,

            PasswordHash = hashedPassword,

            RoleId = 3
        };

        await _userRepository
            .AddAsync(user);

        await _userRepository
            .SaveChangesAsync();

        if (_emailService != null)
        {
            try
            {
                await _emailService.SendWelcomeEmailAsync(candidate.Email, candidate.FullName, temporaryPassword);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CANDIDATE WELCOME EMAIL ERROR] {ex.Message}");
            }
        }
        else if (_emailTemplateService != null && _notificationRepository != null)
        {
            var body = await _emailTemplateService.GetWelcomeTemplateAsync(
                candidate.FullName,
                candidate.Email,
                temporaryPassword);

            await _notificationRepository.AddAsync(
                new Notification
                {
                    ToEmail = candidate.Email,
                    Subject = "Welcome to BGV Portal",
                    Body = body,
                    Status = "Pending",
                    RetryCount = 0,
                    MaxRetryCount = 3,
                    CreatedAt = DateTime.UtcNow
                });

            await _notificationRepository.SaveChangesAsync();
        }

        return "Candidate created successfully";
    }

    public async Task<string> UpdateAsync(int id, UpdateCandidateDto dto)
    {
        var candidate = await _candidateRepository.GetByIdAsync(id);

        if (candidate == null)
        {
            throw new NotFoundException(
    "Candidate not found");
        }

        candidate.FullName = dto.FullName;
        candidate.Email = dto.Email;
        candidate.PhoneNumber = dto.PhoneNumber;
        candidate.Address = dto.Address;
        candidate.DateOfBirth = dto.DateOfBirth;
        candidate.Gender = dto.Gender;
        candidate.PANNumber = dto.PANNumber;
        candidate.AadhaarNumber = dto.AadhaarNumber;
        candidate.AppliedRole = dto.AppliedRole;
        candidate.DateOfJoining = dto.DateOfJoining;
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            candidate.Status = dto.Status;
        }

        await _candidateRepository.UpdateAsync(candidate);

        await _candidateRepository.SaveChangesAsync();

        return "Candidate updated successfully";
    }

    public async Task<string> DeleteAsync(int id)
    {
        var candidate =
            await _candidateRepository
                .GetByIdAsync(id);

        if (candidate == null)
        {
            throw new NotFoundException(
    "Candidate not found");
        }

        await _candidateRepository
            .DeleteAsync(candidate);

        await _candidateRepository
            .SaveChangesAsync();

        // Audit Log

        await _auditService.AddLogAsync(
            "Candidate Deleted",
            "admin@test.com",
            "Admin");

        return "Candidate deleted successfully";
    }

    public async Task<
    List<CandidateResponseDto>>
SearchAsync(
    CandidateSearchDto dto)
    {
        var candidates =
            await _candidateRepository
                .SearchAsync(dto);

        return candidates
            .Select(x =>
                new CandidateResponseDto
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    Status = x.Status
                })
            .ToList();
    }

    public async Task<List<CandidateResponseDto>>
GetDeletedCandidatesAsync()
    {
        var candidates =
            await _candidateRepository
                .GetDeletedCandidatesAsync();

        return candidates
            .Select(x => new CandidateResponseDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Status = x.Status
            })
            .ToList();
    }

    public async Task<string>
RestoreAsync(int id)
    {
        await _candidateRepository
            .RestoreAsync(id);

        return "Candidate restored successfully";
    }

    public async Task<string>
PermanentDeleteAsync(int id)
    {
        var candidate = await _candidateRepository.GetByIdAsync(id);
        if (candidate != null)
        {
            var user = await _userRepository.GetByEmailAsync(candidate.Email);
            if (user != null)
            {
                await _userRepository.DeleteAsync(user);
                await _userRepository.SaveChangesAsync();
            }
        }

        await _candidateRepository
            .PermanentDeleteAsync(id);

        return "Candidate permanently deleted";
    }

    public async Task<CandidateDetailsDto?> GetDetailsAsync(int id)
    {
        var candidate =
            await _candidateRepository.GetByIdAsync(id);

        if (candidate == null)
        {
            throw new NotFoundException("Candidate not found");
        }

        return new CandidateDetailsDto
        {
            Id = candidate.Id,
            FullName = candidate.FullName,
            Email = candidate.Email,
            PhoneNumber = candidate.PhoneNumber,
            Address = candidate.Address,
            DateOfBirth = candidate.DateOfBirth,
            Gender = candidate.Gender,
            PANNumber = candidate.PANNumber,
            AadhaarNumber = candidate.AadhaarNumber,
            AppliedRole = candidate.AppliedRole,
            DateOfJoining = candidate.DateOfJoining,
            Status = candidate.Status
        };
    }
}