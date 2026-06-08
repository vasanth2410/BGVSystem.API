using BGVSystem.Application.DTOs.CandidatePortal;
using BGVSystem.Application.Interfaces;

namespace BGVSystem.Application.Services;

public class CandidatePortalService
    : ICandidatePortalService
{
    private readonly ICandidateRepository
        _candidateRepository;

    private readonly IDocumentRepository
        _documentRepository;

    public CandidatePortalService(
        ICandidateRepository candidateRepository,
        IDocumentRepository documentRepository)
    {
        _candidateRepository =
            candidateRepository;

        _documentRepository =
            documentRepository;
    }

    public async Task<CandidateProfileDto?>
        GetProfileAsync(string email)
    {
        var candidate =
            await _candidateRepository
                .GetByEmailAsync(email);

        if (candidate == null)
        {
            return null;
        }

        return new CandidateProfileDto
        {
            Id = candidate.Id,
            FullName = candidate.FullName,
            Email = candidate.Email,
            PhoneNumber = candidate.PhoneNumber,
            AppliedRole = candidate.AppliedRole,
            Status = candidate.Status
        };
    }

    public async Task<CandidateDashboardDto?>
        GetDashboardAsync(string email)
    {
        var candidate =
            await _candidateRepository
                .GetByEmailAsync(email);

        if (candidate == null)
        {
            return null;
        }

        var documents =
            await _documentRepository
                .GetByCandidateIdAsync(
                    candidate.Id);

        return new CandidateDashboardDto
        {
            CandidateName =
                candidate.FullName,

            Status =
                candidate.Status,

            UploadedDocuments =
                documents.Count,

            RequiredDocuments = 4
        };
    }
}