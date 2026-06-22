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
    public CandidateService(
    ICandidateRepository candidateRepository,
    IAuditService auditService,
    INotificationRepository notificationRepository,
    IEmailTemplateService emailTemplateService,
    IUserRepository userRepository)
    {
        _candidateRepository = candidateRepository;

        _auditService = auditService;

        _notificationRepository = notificationRepository;

        _emailTemplateService = emailTemplateService;

        _userRepository = userRepository;
    }

    private string GenerateTemporaryPassword()
    {
        return $"Temp@{Random.Shared.Next(1000, 9999)}";
    }

    public async Task<List<CandidateResponseDto>> GetAllAsync()
    {
        var candidates = await _candidateRepository.GetAllAsync();

        return candidates.Select(x => new CandidateResponseDto
        {
            Id = x.Id,
            FullName = x.FullName,
            Email = x.Email,
            PhoneNumber = x.PhoneNumber,
            Status = x.Status
        }).ToList();
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

        var body =
     await _emailTemplateService
         .GetWelcomeTemplateAsync(
             candidate.FullName,
             candidate.Email,
             temporaryPassword);

        await _notificationRepository.AddAsync(
            new Notification
            {
                ToEmail = candidate.Email,

                Subject = "Welcome to BGV System",

                Body = body,

                Status = "Pending",

                RetryCount = 0,

                MaxRetryCount = 3,

                CreatedAt = DateTime.UtcNow
            });

        await _notificationRepository.SaveChangesAsync();

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
        await _candidateRepository
            .PermanentDeleteAsync(id);

        return "Candidate permanently deleted";
    }
}