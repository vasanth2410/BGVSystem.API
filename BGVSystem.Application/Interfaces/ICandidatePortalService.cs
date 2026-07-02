using BGVSystem.Application.DTOs.CandidatePortal;
using Microsoft.AspNetCore.Http;

namespace BGVSystem.Application.Interfaces;

public interface ICandidatePortalService
{
    Task<CandidateProfileDto?> GetProfileAsync(
        string email);

    Task<CandidateDashboardDto?> GetDashboardAsync(
        string email);

    Task<List<CandidateVerificationDto>>
     GetVerificationStatusAsync(string email);

    Task<string> UploadDocumentAsync(
    string email,
    IFormFile file);

    Task<List<DocumentListDto>> GetDocumentsAsync(string email);

    Task<DocumentDownloadDto?> DownloadDocumentAsync(
    string email,
    int documentId);

}