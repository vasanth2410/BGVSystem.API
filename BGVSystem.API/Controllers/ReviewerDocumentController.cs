using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGVSystem.API.Controllers;

[ApiController]
[Route("api/reviewer-documents")]
[Authorize(Roles = "Reviewer")]
public class ReviewerDocumentController
    : ControllerBase
{
    private readonly IReviewerDocumentService
        _service;

    public ReviewerDocumentController(
        IReviewerDocumentService service)
    {
        _service = service;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult>
        Dashboard()
    {
        return Ok(
            await _service.GetDashboardAsync());
    }

    [HttpGet("pending")]
    public async Task<IActionResult>
        Pending()
    {
        return Ok(
            await _service
                .GetPendingDocumentsAsync());
    }

    [HttpGet("approved")]
    public async Task<IActionResult>
        Approved()
    {
        return Ok(
            await _service
                .GetApprovedDocumentsAsync());
    }

    [HttpGet("rejected")]
    public async Task<IActionResult>
        Rejected()
    {
        return Ok(
            await _service
                .GetRejectedDocumentsAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult>
        Get(int id)
    {
        return Ok(
            await _service
                .GetDocumentAsync(id));
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult>
        Approve(int id)
    {
        return Ok(
            await _service
                .ApproveDocumentAsync(id));
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult>
        Reject(int id)
    {
        return Ok(
            await _service
                .RejectDocumentAsync(id));
    }
}