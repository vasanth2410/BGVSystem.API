using BGVSystem.Application.DTOs.Auth;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Application.Exceptions;

namespace BGVSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICandidateRepository _candidateRepository;
    private readonly IJwtService _jwtService;
    private readonly IAuditService _auditService;

    public AuthService(
        IUserRepository userRepository,
        ICandidateRepository candidateRepository,
        IJwtService jwtService,
        IAuditService auditService)
    {
        _userRepository = userRepository;
        _candidateRepository = candidateRepository;
        _jwtService = jwtService;
        _auditService = auditService;
    }

    public async Task<string> RegisterAsync(RegisterRequestDto dto)
    {
        var cleanEmail = dto.Email.Trim().ToLower();

        var existingUser = await _userRepository
            .GetByEmailAsync(cleanEmail);

        if (existingUser != null)
        {
            throw new Exception("Email already exists");
        }

        var hashedPassword =
            BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Public registration is strictly restricted to Candidates (RoleId = 3)
        int targetRoleId = 3;

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = cleanEmail,
            PasswordHash = hashedPassword,
            RoleId = targetRoleId
        };

        await _userRepository.AddAsync(user);

        // If user is a Candidate (RoleId == 3), also create a matching Candidate record in Candidates table
        if (targetRoleId == 3)
        {
            var existingCandidate = await _candidateRepository.GetByEmailAsync(cleanEmail);
            if (existingCandidate == null)
            {
                var candidate = new Candidate
                {
                    FullName = dto.FullName.Trim(),
                    Email = cleanEmail,
                    Status = "Pending",
                    CreatedDate = DateTime.UtcNow
                };
                await _candidateRepository.AddAsync(candidate);
            }
        }

        await _userRepository.SaveChangesAsync();

        return "User registered successfully";
    }

    public async Task<AuthResponseDto> LoginAsync(
     LoginRequestDto dto)
    {
        var cleanEmail = dto.Email.Trim().ToLower();

        var user = await _userRepository
            .GetByEmailAsync(cleanEmail);

        if (user == null)
        {
            throw new UnauthorizedException(
     "Invalid email or password");
        }

        var isPasswordValid =
            BCrypt.Net.BCrypt.Verify(
                dto.Password,
                user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new UnauthorizedException(
    "Invalid email or password");
        }

        var token = _jwtService.GenerateToken(user);

        // Audit Log

        await _auditService.AddLogAsync(
            "User Logged In",
            user.Email,
            user.Role.Name);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            Role = user.Role.Name,
            FullName = user.FullName,
            MustChangePassword = user.MustChangePassword
        };
    }

    public async Task<string> ChangePasswordAsync(ChangePasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            throw new Exception("Email and new password are required.");
        }

        var cleanEmail = dto.Email.Trim().ToLower();
        var user = await _userRepository.GetByEmailAsync(cleanEmail);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        if (!string.IsNullOrWhiteSpace(dto.CurrentPassword))
        {
            var isCurrentValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
            if (!isCurrentValid)
            {
                throw new UnauthorizedException("Invalid current password.");
            }
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.MustChangePassword = false;

        await _userRepository.SaveChangesAsync();

        await _auditService.AddLogAsync(
            "User Changed Password",
            user.Email,
            user.Role?.Name ?? "User");

        return "Password changed successfully.";
    }
}