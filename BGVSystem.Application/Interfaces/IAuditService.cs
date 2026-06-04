using BGVSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.Interfaces
{
    public interface IAuditService
    {
        Task AddLogAsync(
            string action,
            string performedBy,
            string role);

        Task<List<AuditLog>> GetAllAsync();
    }
}
