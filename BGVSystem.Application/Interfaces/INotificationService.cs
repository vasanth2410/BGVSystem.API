using BGVSystem.Application.DTOs;
using BGVSystem.Domain.Entities;

namespace BGVSystem.Application.Interfaces;

public interface INotificationService
{
    Task<List<NotificationResponseDto>> GetAllAsync();

    Task<NotificationResponseDto> GetByIdAsync(int id);

    Task<List<NotificationResponseDto>> GetPendingAsync();
    Task<List<NotificationResponseDto>>
    GetDeadLettersAsync();
}