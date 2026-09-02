using BGVSystem.Application.DTOs;
using BGVSystem.Application.Exceptions;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BGVSystem.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(
        INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<NotificationResponseDto>> GetAllAsync()
    {
        var notifications =
            await _notificationRepository.GetAllAsync();

        return notifications.Select(x => new NotificationResponseDto
        {
            Id = x.Id,
            ToEmail = x.ToEmail,
            Subject = x.Subject,
            Body = x.Body,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            SentAt = x.SentAt
        }).ToList();
    }

    public async Task<NotificationResponseDto> GetByIdAsync(int id)
    {
        var notification =
            await _notificationRepository.GetByIdAsync(id);

        if (notification == null)
        {
            throw new NotFoundException(
                "Notification not found");
        }

        return new NotificationResponseDto
        {
            Id = notification.Id,
            ToEmail = notification.ToEmail,
            Subject = notification.Subject,
            Body = notification.Body,
            Status = notification.Status,
            CreatedAt = notification.CreatedAt,
            SentAt = notification.SentAt
        };
    }

    public async Task<List<NotificationResponseDto>> GetPendingAsync()
    {
        var notifications =
            await _notificationRepository.GetPendingAsync();

        return notifications.Select(x => new NotificationResponseDto
        {
            Id = x.Id,
            ToEmail = x.ToEmail,
            Subject = x.Subject,
            Body = x.Body,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            SentAt = x.SentAt
        }).ToList();
    }

    public async Task<List<NotificationResponseDto>>
    GetDeadLettersAsync()
    {
        var notifications =
            await _notificationRepository
                .GetDeadLettersAsync();

        return notifications
            .Select(x => new NotificationResponseDto
            {
                Id = x.Id,
                ToEmail = x.ToEmail,
                Subject = x.Subject,
                Body = x.Body,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                SentAt = x.SentAt
            })
            .ToList();
    }

}