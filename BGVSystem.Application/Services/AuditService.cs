using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;

namespace BGVSystem.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;

    public AuditService(
        IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task AddLogAsync(
        string action,
        string performedBy,
        string role)
    {
        var auditLog = new AuditLog
        {
            Action = action,
            PerformedBy = performedBy,
            Role = role,
            PerformedAt = DateTime.UtcNow
        };

        await _auditRepository
            .AddAsync(auditLog);

        await _auditRepository
            .SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetAllAsync()
    {
        return await _auditRepository
            .GetAllAsync();
    }
}