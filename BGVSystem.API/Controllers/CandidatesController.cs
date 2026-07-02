using BGVSystem.Application.DTOs;
using BGVSystem.Application.DTOs.Candidates;
//using BGVSystem.Application.DTOs.Candidates;
using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BGVSystem.Application.DTOs.Candidates;

namespace BGVSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidateService;

    public CandidatesController(ICandidateService candidateService)
    {
        _candidateService = candidateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _candidateService.GetAllAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _candidateService.GetByIdAsync(id);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCandidateDto dto)
    {
        var result =
            await _candidateService
                .CreateAsync(dto);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateCandidateDto dto)
    {
        var result = await _candidateService.UpdateAsync(id, dto);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result =
            await _candidateService.DeleteAsync(id);

        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult>
Search(
    [FromQuery]
    CandidateSearchDto dto)
    {
        var result =
            await _candidateService
                .SearchAsync(dto);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeletedCandidates()
    {
        var result =
            await _candidateService
                .GetDeletedCandidatesAsync();

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("restore/{id}")]
    public async Task<IActionResult> Restore(int id)
    {
        var result =
            await _candidateService
                .RestoreAsync(id);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("permanent/{id}")]
    public async Task<IActionResult> PermanentDelete(int id)
    {
        var result =
            await _candidateService
                .PermanentDeleteAsync(id);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("details/{id}")]
    public async Task<IActionResult> GetDetails(int id)
    {
        var result =
            await _candidateService.GetDetailsAsync(id);

        return Ok(result);
    }
}