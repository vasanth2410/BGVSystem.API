using BGVSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Application.Interfaces
{
    public interface IVerificationRepository
    {
        Task AddAsync(Verification verification);

        Task<List<Verification>> GetAllAsync();

        Task<Verification?> GetByIdAsync(int id);

        Task SaveChangesAsync();
    }
}
