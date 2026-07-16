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

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
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
        
        if (!System.IO.File.Exists(filePath))
        {
            var fileName = Path.GetFileName(filePath);
            
            // Try relative fallbacks
            var fallbackPath1 = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", fileName);
            var fallbackPath2 = Path.Combine(Directory.GetCurrentDirectory(), "api", "Uploads", fileName);
            var fallbackPath3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", fileName);
            var fallbackPath4 = Path.Combine(Directory.GetCurrentDirectory(), "..", "BGVSystem.API", "Uploads", fileName);

            if (System.IO.File.Exists(fallbackPath1))
            {
                filePath = fallbackPath1;
            }
            else if (System.IO.File.Exists(fallbackPath2))
            {
                filePath = fallbackPath2;
            }
            else if (System.IO.File.Exists(fallbackPath3))
            {
                filePath = fallbackPath3;
            }
            else if (System.IO.File.Exists(fallbackPath4))
            {
                filePath = fallbackPath4;
            }
            else
            {
                return NotFound($"File not found. Checked DB path: {document.FilePath}");
            }
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

        return File(
            fileBytes,
            "application/octet-stream",
            document.OriginalFileName);
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