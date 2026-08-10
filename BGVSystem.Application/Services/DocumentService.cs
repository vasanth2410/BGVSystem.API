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
    private readonly IFileStorageService _fileStorageService;

    public DocumentService(
        IDocumentRepository documentRepository,
        IAuditService auditService,
        IFileStorageService fileStorageService)
    {
        _documentRepository = documentRepository;
        _auditService = auditService;
        _fileStorageService = fileStorageService;
    }

    public async Task<string> UploadAsync(UploadDocumentDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
        {
            throw new System.ComponentModel.DataAnnotations.ValidationException("File is required");
        }

        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(dto.File.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
        {
            throw new System.ComponentModel.DataAnnotations.ValidationException("Invalid file type");
        }

        // Infer document type from OriginalFileName (e.g. PAN, Aadhaar, Passport, Resume, Degree, PCC)
        string docType = InferDocumentType(dto.File.FileName);

        // Upload to Supabase Storage
        var storageResult = await _fileStorageService.UploadAsync(dto.File, dto.CandidateId, docType);

        // Check if a document of the same exact file name already exists for this candidate (Replacement workflow)
        var existingDocs = await _documentRepository.GetByCandidateIdAsync(dto.CandidateId);
        var existingSameDoc = existingDocs?.FirstOrDefault(d => 
            d.OriginalFileName.Equals(dto.File.FileName, StringComparison.OrdinalIgnoreCase));

        string? oldObjectPathToDelete = null;

        Document document;

        if (existingSameDoc != null)
        {
            // Replacement: Keep track of old file path to delete after successful DB update
            oldObjectPathToDelete = existingSameDoc.FilePath;

            existingSameDoc.FileName = storageResult.FileName;
            existingSameDoc.OriginalFileName = dto.File.FileName;
            existingSameDoc.FilePath = storageResult.ObjectPath;
            existingSameDoc.FileType = storageResult.FileType;
            existingSameDoc.FileSize = storageResult.FileSize;
            existingSameDoc.Status = "Uploaded";
            existingSameDoc.UploadedDate = DateTime.UtcNow;

            document = existingSameDoc;
        }
        else
        {
            document = new Document
            {
                CandidateId = dto.CandidateId,
                FileName = storageResult.FileName,
                OriginalFileName = dto.File.FileName,
                FilePath = storageResult.ObjectPath,
                FileType = storageResult.FileType,
                FileSize = storageResult.FileSize,
                Status = "Uploaded",
                UploadedDate = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document);
        }

        try
        {
            await _documentRepository.SaveChangesAsync();
        }
        catch
        {
            // Transaction Safety: Rollback uploaded file from Supabase if DB save fails
            await _fileStorageService.DeleteAsync(storageResult.ObjectPath);
            throw;
        }

        // Delete old file from storage after successful database save
        if (!string.IsNullOrWhiteSpace(oldObjectPathToDelete))
        {
            try
            {
                if (File.Exists(oldObjectPathToDelete))
                {
                    File.Delete(oldObjectPathToDelete); // Local fallback cleanup
                }
                else
                {
                    await _fileStorageService.DeleteAsync(oldObjectPathToDelete);
                }
            }
            catch
            {
                // Silent fallback for old file deletion
            }
        }

        // Audit Log
        await _auditService.AddLogAsync(
            "Document Uploaded",
            "candidate@test.com",
            "Candidate");

        return "Document uploaded successfully";
    }

    public async Task<List<DocumentResponseDto>> GetByCandidateIdAsync(int candidateId)
    {
        var documents = await _documentRepository.GetByCandidateIdAsync(candidateId);
        if (documents == null || !documents.Any())
        {
            throw new NotFoundException("No documents found for this candidate");
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

        // Delete from physical storage (Local fallback or Supabase)
        if (!string.IsNullOrWhiteSpace(document.FilePath))
        {
            if (File.Exists(document.FilePath))
            {
                File.Delete(document.FilePath);
            }
            else
            {
                await _fileStorageService.DeleteAsync(document.FilePath);
            }
        }

        await _documentRepository.DeleteAsync(document);
        await _documentRepository.SaveChangesAsync();

        return document.FileName;
    }

    public async Task<Document?> GetDocumentByIdAsync(int id)
    {
        return await _documentRepository.GetByIdAsync(id);
    }

    public async Task<List<DocumentResponseDto>> GetAllAsync()
    {
        var documents = await _documentRepository.GetAllAsync();

        return documents.Select(x => new DocumentResponseDto
        {
            Id = x.Id,
            CandidateId = x.CandidateId,
            FileName = x.OriginalFileName,
            FileType = x.FileType,
            FileSize = x.FileSize,
            Status = x.Status
        }).ToList();
    }

    private static string InferDocumentType(string fileName)
    {
        var lower = fileName.ToLowerInvariant();
        if (lower.Contains("pan")) return "PAN";
        if (lower.Contains("aadhaar") || lower.Contains("aadhar")) return "Aadhaar";
        if (lower.Contains("passport")) return "Passport";
        if (lower.Contains("resume")) return "Resume";
        if (lower.Contains("degree") || lower.Contains("mark") || lower.Contains("certificate")) return "Degree";
        if (lower.Contains("pcc") || lower.Contains("police")) return "PCC";
        return "Document";
    }
}