using BGVSystem.Application.DTOs.CandidatePortal;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace BGVSystem.Application.Services;

public class CandidatePortalService
    : ICandidatePortalService
{
    private readonly ICandidateRepository
        _candidateRepository;

    private readonly IDocumentRepository _documentRepository;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IEmailService _emailService;

    public CandidatePortalService(
        ICandidateRepository candidateRepository,
        IDocumentRepository documentRepository,
        IVerificationRepository verificationRepository,
        IWebHostEnvironment environment,
        IEmailService emailService = null)
    {
        _candidateRepository = candidateRepository;
        _documentRepository = documentRepository;
        _verificationRepository = verificationRepository;
        _environment = environment;
        _emailService = emailService;
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

    public async Task<string> UploadDocumentAsync(
    string email,
    IFormFile file)
    {
        var candidate =
            await _candidateRepository
                .GetByEmailAsync(email);

        if (candidate == null)
        {
            throw new Exception("Candidate not found");
        }

        if (file == null || file.Length == 0)
        {
            throw new Exception("Please select a file.");
        }

        var uploadsFolder =
            Path.Combine(_environment.ContentRootPath, "Uploads");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName =
            Guid.NewGuid().ToString() +
            Path.GetExtension(file.FileName);

        var filePath =
            Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream =
            new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var document = new Document
        {
            CandidateId = candidate.Id,
            FileName = uniqueFileName,
            OriginalFileName = file.FileName,
            FilePath = filePath,
            FileType = Path.GetExtension(file.FileName),
            FileSize = file.Length,
            Status = "Uploaded"
        };

        await _documentRepository.AddAsync(document);
        await _documentRepository.SaveChangesAsync();

        var existingVerification = await _verificationRepository
            .GetByCandidateAndTypeAsync(candidate.Id, file.FileName);

        if (existingVerification == null)
        {
            var verification = new Verification
            {
                CandidateId = candidate.Id,
                VerificationType = file.FileName,
                Status = "Pending",
                ReviewerRemarks = string.Empty,
                CreatedDate = DateTime.UtcNow
            };
            await _verificationRepository.AddAsync(verification);
            await _verificationRepository.SaveChangesAsync();
        }

        if (_emailService != null && candidate != null)
        {
            try
            {
                await _emailService.SendDocumentsUploadedEmailAsync(candidate.Email, candidate.FullName);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[EMAIL NOTICE] SendDocumentsUploadedEmailAsync failed: {ex.Message}");
            }
        }

        return "Document uploaded successfully.";


    }

    public async Task<List<DocumentListDto>>
GetDocumentsAsync(string email)
    {
        var candidate =
            await _candidateRepository
                .GetByEmailAsync(email);

        if (candidate == null)
        {
            throw new Exception("Candidate not found");
        }

        var documents =
            await _documentRepository
                .GetByCandidateIdAsync(candidate.Id);

        return documents.Select(x =>
            new DocumentListDto
            {
                Id = x.Id,
                FileName = x.OriginalFileName,
                FileType = x.FileType,
                Status = x.Status,
                UploadedDate = x.UploadedDate
            }).ToList();
    }

    public async Task<DocumentDownloadDto?> DownloadDocumentAsync(
    string email,
    int documentId)
    {
        var candidate =
            await _candidateRepository
                .GetByEmailAsync(email);

        if (candidate == null)
            throw new Exception("Candidate not found");

        var document =
            await _documentRepository
                .GetByIdAsync(documentId);

        if (document == null)
            throw new Exception("Document not found");

        if (document.CandidateId != candidate.Id)
            throw new UnauthorizedAccessException();

        if (!File.Exists(document.FilePath))
            throw new FileNotFoundException();

        var bytes =
            await File.ReadAllBytesAsync(document.FilePath);

        return new DocumentDownloadDto
        {
            FileBytes = bytes,
            FileName = document.OriginalFileName,
            ContentType = "application/octet-stream"
        };
    }


}