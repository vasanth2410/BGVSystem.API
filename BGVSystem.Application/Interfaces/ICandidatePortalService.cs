using BGVSystem.Application.DTOs.CandidatePortal;

namespace BGVSystem.Application.Interfaces;

public interface ICandidatePortalService
{
    Task<CandidateProfileDto?> GetProfileAsync(
        string email);

    Task<CandidateDashboardDto?> GetDashboardAsync(
        string email);
}