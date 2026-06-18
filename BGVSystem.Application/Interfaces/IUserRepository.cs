using BGVSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);

        Task AddAsync(User user);

        Task SaveChangesAsync();

        Task<User?> GetByIdAsync(int id);

        Task<List<User>> GetReviewersAsync();
    }
}
