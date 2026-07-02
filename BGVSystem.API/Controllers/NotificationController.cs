using BGVSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BGVSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Reviewer")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result =
            await _notificationService.GetAllAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result =
            await _notificationService.GetByIdAsync(id);

        return Ok(result);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var result =
            await _notificationService.GetPendingAsync();

        return Ok(result);
    }

    [HttpGet("deadletters")]
    public async Task<IActionResult>
    GetDeadLetters()
    {
        var result =
            await _notificationService
                .GetDeadLettersAsync();

        return Ok(result);
    }
}