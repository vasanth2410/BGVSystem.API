using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGVSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(
        IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var result =
            await _adminService
                .GetDashboardAsync();

        return Ok(result);
    }

    [HttpGet("reviewers")]
    public async Task<IActionResult>
    GetReviewers()
    {
        var result =
            await _adminService
                .GetReviewersAsync();

        return Ok(result);
    }

    [HttpPost("reviewers")]
    public async Task<IActionResult> CreateReviewer([FromBody] BGVSystem.Application.DTOs.Admin.CreateReviewerDto dto)
    {
        var result = await _adminService.CreateReviewerAsync(dto);
        return Ok(result);
    }
}