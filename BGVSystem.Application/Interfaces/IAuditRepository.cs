using BGVSystem.Domain.Entities;

namespace BGVSystem.Application.Interfaces;

public interface IAuditRepository
{
    Task AddAsync(AuditLog auditLog);

    Task<List<AuditLog>> GetAllAsync();

    Task SaveChangesAsync();

    Task<int> GetCountAsync();
}