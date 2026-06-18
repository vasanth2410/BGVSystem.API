using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BGVSystem.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<User>>
    GetReviewersAsync()
    {
        return await _context.Users
            .Where(x => x.RoleId == 2)
            .ToListAsync();
    }

}