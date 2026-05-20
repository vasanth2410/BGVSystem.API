using BGVSystem.Application.DTOs.Verifications;
using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPut("approve/{id}")]
    public async Task<IActionResult> Approve(
        int id,
        [FromQuery] string remarks)
    {
        var result = await _verificationService.ApproveAsync(id, remarks);

        return Ok(result);
    }

    [HttpPut("reject/{id}")]
    public async Task<IActionResult> Reject(
        int id,
        [FromQuery] string remarks)
    {
        var result = await _verificationService.RejectAsync(id, remarks);

        return Ok(result);
    }
}