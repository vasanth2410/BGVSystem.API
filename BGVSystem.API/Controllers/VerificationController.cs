using BGVSystem.Application.DTOs.Verifications;
using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BGVSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VerificationController : ControllerBase
{
    private readonly IVerificationService _verificationService;

    public VerificationController(
        IVerificationService verificationService)
    {
        _verificationService = verificationService;
    }

    private int GetReviewerId()
    {
        return int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier
            )!
        );
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVerificationDto dto)
    {
        var result = await _verificationService.CreateAsync(dto);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _verificationService.GetAllAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _verificationService.GetByIdAsync(id);

        return Ok(result);
    }

    [Authorize(Roles = "Reviewer")]
    [HttpPut("approve/{id}")]
    public async Task<IActionResult> Approve(
        int id,
        string remarks)
    {
        var result =
            await _verificationService
                .ApproveAsync(id, remarks);

        return Ok(result);
    }

    [Authorize(Roles = "Reviewer")]
    [HttpPut("reject/{id}")]
    public async Task<IActionResult> Reject(
     int id,
     string remarks)
    {
        var result =
            await _verificationService
                .RejectAsync(id, remarks);

        return Ok(result);
    }

    [Authorize(Roles = "Reviewer")]
    [HttpPut("rereview/{id}")]
    public async Task<IActionResult> ReReview(int id)
    {
        var result =
            await _verificationService
                .ReReviewAsync(id);

        return Ok(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStatistics()
    {
        var result =
            await _verificationService
                .GetDashboardStatisticsAsync();

        return Ok(result);
    }

    [Authorize(Roles = "Reviewer")]
    [HttpGet("reviewer/candidate/{candidateId}")]
    public async Task<IActionResult> GetReviewerCandidateVerifications(
    int candidateId)
    {
        var reviewerId = GetReviewerId();

        var result =
            await _verificationService
                .GetReviewerCandidateVerificationsAsync(
                    candidateId,
                    reviewerId);

        return Ok(result);
    }
}