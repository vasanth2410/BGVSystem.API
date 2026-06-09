using BGVSystem.Application.DTOs.CandidatePortal;

namespace BGVSystem.Application.Interfaces;

public interface ICandidatePortalService
{
    Task<CandidateProfileDto?> GetProfileAsync(
        string email);

    Task<CandidateDashboardDto?> GetDashboardAsync(
        string email);

    Task<List<CandidateVerificationDto>>
     GetVerificationStatusAsync(string email);
}