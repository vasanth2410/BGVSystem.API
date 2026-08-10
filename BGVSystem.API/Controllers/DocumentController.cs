using BGVSystem.Application.DTOs.Document;
//using BGVSystem.Application.DTOs.Documents;
using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BGVSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IFileStorageService _fileStorageService;

    public DocumentsController(
        IDocumentService documentService,
        IFileStorageService fileStorageService)
    {
        _documentService = documentService;
        _fileStorageService = fileStorageService;
    }

    //[Authorize(Roles = "Candidate")]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
     [FromForm] UploadDocumentDto dto)
    {
        var result =
            await _documentService.UploadAsync(dto);

        return Ok(result);
    }

    [HttpGet("candidate/{candidateId}")]
    public async Task<IActionResult> GetByCandidate(int candidateId)
    {
        var result = await _documentService
            .GetByCandidateIdAsync(candidateId);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _documentService.DeleteAsync(id);

        return Ok("Document deleted successfully");
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadDocument(int id)
    {
        var document = await _documentService.GetDocumentByIdAsync(id);

        if (document == null)
        {
            return NotFound();
        }

        var filePath = document.FilePath;
        
        // 1. Try local disk fallback first for backward compatibility
        if (System.IO.File.Exists(filePath))
        {
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/octet-stream", document.OriginalFileName);
        }

        var fileName = Path.GetFileName(filePath);
        var fallbackPath1 = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", fileName);
        if (System.IO.File.Exists(fallbackPath1))
        {
            var fileBytes = await System.IO.File.ReadAllBytesAsync(fallbackPath1);
            return File(fileBytes, "application/octet-stream", document.OriginalFileName);
        }

        // 2. Stream from Supabase Storage using ObjectPath
        try
        {
            var stream = await _fileStorageService.DownloadAsync(document.FilePath);
            return File(stream, "application/octet-stream", document.OriginalFileName);
        }
        catch (Exception ex)
        {
            return NotFound($"File not found in local disk or Supabase Storage. Error: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<IActionResult>
GetAll()
    {
        var result =
            await _documentService
                .GetAllAsync();

        return Ok(result);
    }
}