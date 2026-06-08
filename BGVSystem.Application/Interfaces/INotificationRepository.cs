using BGVSystem.Domain.Entities;

namespace BGVSystem.Application.Interfaces;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);

    Task UpdateAsync(Notification notification);

    Task<List<Notification>> GetAllAsync();

    Task<Notification?> GetByIdAsync(int id);

    Task<List<Notification>> GetPendingAsync();

    Task SaveChangesAsync();
    Task<List<Notification>> GetDeadLettersAsync();

    Task<int> GetSentCountAsync();

}