using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BGVSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Candidate")]
public class CandidatePortalController : ControllerBase
{
    private readonly
        ICandidatePortalService
        _candidatePortalService;

    public CandidatePortalController(
        ICandidatePortalService
            candidatePortalService)
    {
        _candidatePortalService =
            candidatePortalService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult>
        GetProfile()
    {
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var result =
            await _candidatePortalService
                .GetProfileAsync(email!);

        return Ok(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult>
        GetDashboard()
    {
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var result =
            await _candidatePortalService
                .GetDashboardAsync(email!);

        return Ok(result);
    }

    [HttpGet("verifications")]
    public async Task<IActionResult>
GetVerifications()
    {
        var email =
            User.FindFirstValue(
                ClaimTypes.Email);

        var result =
            await _candidatePortalService
                .GetVerificationStatusAsync(email!);

        return Ok(result);
    }
}