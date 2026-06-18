using BGVSystem.Application.DTOs.Document;
using BGVSystem.Application.Exceptions;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace BGVSystem.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IAuditService _auditService;
    public DocumentService(
     IDocumentRepository documentRepository,
     IAuditService auditService)
    {
        _documentRepository = documentRepository;

        _auditService = auditService;
    }

    public async Task<string> UploadAsync(UploadDocumentDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
        {
            throw new System.ComponentModel.DataAnnotations.ValidationException(
    "File is required");
        }

        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };

        var extension = Path.GetExtension(dto.File.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
        {
            throw new System.ComponentModel.DataAnnotations.ValidationException(
     "Invalid file type");
        }

        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        var filePath = Path.Combine(
            uploadsFolder,
            uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        var document = new Document
        {
            CandidateId = dto.CandidateId,
            FileName = uniqueFileName,
            OriginalFileName = dto.File.FileName,
            FilePath = filePath,
            FileType = extension,
            FileSize = dto.File.Length,
            Status = "Uploaded"
        };

        await _documentRepository.AddAsync(document);

        await _documentRepository.SaveChangesAsync();
        // Audit Log

        await _auditService.AddLogAsync(
            "Document Uploaded",
            "candidate@test.com",
            "Candidate");
        return "Document uploaded successfully";
    }

    public async Task<List<DocumentResponseDto>> GetByCandidateIdAsync(int candidateId)
    {
        var documents = await _documentRepository
            .GetByCandidateIdAsync(candidateId);
        if (documents == null || !documents.Any())
        {
            throw new NotFoundException(
                "No documents found for this candidate");
        }

        return documents.Select(x => new DocumentResponseDto
        {
            Id = x.Id,
            FileName = x.OriginalFileName,
            FileType = x.FileType,
            FileSize = x.FileSize,
            Status = x.Status
        }).ToList();
    }

    public async Task<string> DeleteAsync(int id)
    {
        var document = await _documentRepository.GetByIdAsync(id);

        if (document == null)
        {
            throw new Exception("Document not found");
        }

        if (File.Exists(document.FilePath))
        {
            File.Delete(document.FilePath);
        }

        await _documentRepository.DeleteAsync(document);

        await _documentRepository.SaveChangesAsync();

        return document.FileName;
    }

    public async Task<Document?>
GetDocumentByIdAsync(int id)
{
    return await
        _documentRepository
            .GetByIdAsync(id);
}
}