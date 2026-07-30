using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGVSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VerificationEngineController : ControllerBase
{
    private readonly IOcrService _ocrService;
    private readonly IThirdPartyVerificationService _thirdPartyVerificationService;
    private readonly IDocumentRepository _documentRepository;

    public VerificationEngineController(
        IOcrService ocrService,
        IThirdPartyVerificationService thirdPartyVerificationService,
        IDocumentRepository documentRepository)
    {
        _ocrService = ocrService;
        _thirdPartyVerificationService = thirdPartyVerificationService;
        _documentRepository = documentRepository;
    }

    /// <summary>
    /// OCR Engine: Read uploaded document and extract identity metadata
    /// </summary>
    [HttpPost("ocr-scan/{documentId}")]
    public async Task<IActionResult> ScanDocumentOcr(int documentId)
    {
        var doc = await _documentRepository.GetByIdAsync(documentId);
        if (doc == null)
        {
            return NotFound(new { message = $"Document with ID {documentId} not found." });
        }

        var result = await _ocrService.ProcessDocumentOcrAsync(doc.Id, doc.OriginalFileName, doc.FilePath);
        return Ok(result);
    }

    /// <summary>
    /// Third-Party Engine: Run simulated live verification match for PAN, Aadhaar & Criminal Records
    /// </summary>
    [HttpPost("live-verify/{candidateId}")]
    [Authorize]
    public async Task<IActionResult> RunLiveVerification(int candidateId)
    {
        var result = await _thirdPartyVerificationService.RunLiveVerificationAsync(candidateId);
        return Ok(result);
    }

    /// <summary>
    /// Get verification status summary for candidate
    /// </summary>
    [HttpGet("live-status/{candidateId}")]
    [Authorize]
    public async Task<IActionResult> GetLiveStatus(int candidateId)
    {
        var result = await _thirdPartyVerificationService.RunLiveVerificationAsync(candidateId);
        return Ok(result);
    }
}
