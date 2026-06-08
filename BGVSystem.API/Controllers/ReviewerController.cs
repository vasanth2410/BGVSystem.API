using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult>
        GetDashboard()
    {
        var result =
            await _reviewerService
                .GetDashboardAsync();

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
}