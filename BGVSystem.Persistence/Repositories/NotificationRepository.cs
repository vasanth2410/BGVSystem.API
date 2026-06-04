using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BGVSystem.Persistence.Repositories;

public class NotificationRepository
    : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Notification notification)
    {
        await _context.Notifications
            .AddAsync(notification);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetAllAsync()
    {
        return await _context.Notifications
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Notification>> GetPendingAsync()
    {
        return await _context.Notifications
            .Where(x => x.Status == "Pending")
            .ToListAsync();
    }

    public async Task UpdateAsync(
    Notification notification)
    {
        _context.Notifications.Update(notification);

        await Task.CompletedTask;
    }

    public async Task<List<Notification>>
    GetDeadLettersAsync()
    {
        return await _context.Notifications
            .Where(x =>
                x.Status == "DeadLetter")
            .ToListAsync();
    }
}