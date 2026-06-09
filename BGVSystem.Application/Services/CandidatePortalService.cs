using BGVSystem.Application.DTOs.CandidatePortal;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using Microsoft.AspNetCore.Http;

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

        var approved =
            documents.Count(x =>
                x.Status == "Approved");

        var pending =
            documents.Count(x =>
                x.Status == "Pending");

        var rejected =
            documents.Count(x =>
                x.Status == "Rejected");

        string overallStatus;

        if (rejected > 0)
        {
            overallStatus = "Rejected";
        }
        else if (pending == 0 && approved > 0)
        {
            overallStatus = "Completed";
        }
        else
        {
            overallStatus = "In Progress";
        }

        return new CandidateDashboardDto
        {
            CandidateName = candidate.FullName,
            DocumentsUploaded = documents.Count,
            ApprovedDocuments = approved,
            PendingDocuments = pending,
            RejectedDocuments = rejected,
            OverallStatus = overallStatus
        };
    }
    public async Task<List<CandidateVerificationDto>>
    GetVerificationStatusAsync(string email)
    {
        var candidate =
            await _candidateRepository
                .GetByEmailAsync(email);

        if (candidate == null)
        {
            throw new Exception(
                "Candidate not found");
        }

        var documents =
            await _documentRepository
                .GetByCandidateIdAsync(
                    candidate.Id);

        return documents
            .Select(x =>
                new CandidateVerificationDto
                {
                    DocumentId = x.Id,
                    FileName = x.OriginalFileName,
                    Status = x.Status
                })
            .ToList();
    }
}