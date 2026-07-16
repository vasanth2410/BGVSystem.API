using BGVSystem.Application.DTOs.Reviewer;
using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BGVSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Reviewer")]
public class ReviewerController : ControllerBase
{
    private readonly IReviewerService _reviewerService;

    public ReviewerController(
        IReviewerService reviewerService)
    {
        _reviewerService = reviewerService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var result =
            await _reviewerService
                .GetDashboardAsync(email!);

        return Ok(result);
    }

    [HttpGet("pending")]
    public async Task<IActionResult>
        GetPending()
    {
        var result =
            await _reviewerService
                .GetPendingVerificationsAsync();

        return Ok(result);
    }

    [HttpGet("approved")]
    public async Task<IActionResult>
        GetApproved()
    {
        var result =
            await _reviewerService
                .GetApprovedVerificationsAsync();

        return Ok(result);
    }

    [HttpGet("rejected")]
    public async Task<IActionResult>
        GetRejected()
    {
        var result =
            await _reviewerService
                .GetRejectedVerificationsAsync();

        return Ok(result);
    }

    [HttpGet("assigned-candidates")]
    public async Task<IActionResult>
    AssignedCandidates()
    {
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var result =
            await _reviewerService
                .GetAssignedCandidatesAsync(
                    email!);

        return Ok(result);
    }

    [HttpGet("candidate/{candidateId}")]
    public async Task<IActionResult>
GetCandidate(int candidateId)
    {
        var email =
    User.FindFirstValue(
        ClaimTypes.Email);

        var result =
            await _reviewerService
                .GetCandidateAsync(
                    candidateId,
                    email!);

        return Ok(result);
    }

    [HttpGet("candidate/{candidateId}/documents")]
    public async Task<IActionResult>
GetDocuments(int candidateId)
    {
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var result =
            await _reviewerService
                .GetCandidateDocumentsAsync(
                    candidateId,
                    email!);

        return Ok(result);
    }

    [HttpGet("candidate/{candidateId}/verifications")]
    public async Task<IActionResult>
 GetVerifications(int candidateId)
    {
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var result =
            await _reviewerService
                .GetCandidateVerificationsAsync(
                    candidateId,
                    email!);

        return Ok(result);
    }

    [HttpGet("document/{documentId}")]
    public async Task<IActionResult>
GetDocument(
    int documentId)
    {
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var result =
            await _reviewerService
                .GetDocumentAsync(
                    documentId,
                    email!);

        return Ok(result);
    }

    [HttpGet("document/download/{documentId}")]
    public async Task<IActionResult>
DownloadDocument(
    int documentId)
    {
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var result =
            await _reviewerService
                .DownloadDocumentAsync(
                    documentId,
                    email!);

        return File(
            result.Content,
            result.ContentType,
            result.FileName);
    }

    [HttpPut("document/{documentId}/review")]
    public async Task<IActionResult> ReviewDocument(
    int documentId,
    ReviewDocumentDto dto)
    {
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var result =
            await _reviewerService
                .ReviewDocumentAsync(
                    documentId,
                    email!,
                    dto);

        return Ok(result);
    }

    [HttpGet("documents")]
    public async Task<IActionResult> GetDocuments()
    {
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var result =
            await _reviewerService
                .GetReviewerDocumentsAsync(
                    email!);

        return Ok(result);
    }

    [HttpGet("verifications")]
    public async Task<IActionResult>
GetReviewerVerifications()
    {
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var result =
            await _reviewerService
                .GetReviewerVerificationsAsync(
                    email!);

        return Ok(result);
    }
}