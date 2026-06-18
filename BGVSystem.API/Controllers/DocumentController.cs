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
public async Task<IActionResult> DownloadDocument(
    int id)
{
    var document =
        await _documentService
            .GetDocumentByIdAsync(id);

    if (document == null)
    {
        return NotFound();
    }

        var filePath = document.FilePath;
        if (!System.IO.File.Exists(filePath))
    {
        return NotFound(
            "File not found");
    }

    var fileBytes =
        await System.IO.File
            .ReadAllBytesAsync(filePath);

        return File(
        fileBytes,
        "application/octet-stream",
        document.OriginalFileName);
    }
}