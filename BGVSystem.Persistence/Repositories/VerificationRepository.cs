using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BGVSystem.Persistence.Repositories;

public class VerificationRepository : IVerificationRepository
{
    private readonly ApplicationDbContext _context;

    public VerificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Verification verification)
    {
        await _context.Verifications.AddAsync(verification);
    }

    public async Task<List<Verification>> GetAllAsync()
    {
        return await _context.Verifications.ToListAsync();
    }

    public async Task<Verification?> GetByIdAsync(int id)
    {
        return await _context.Verifications.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Verification>>
    GetByStatusAsync(string status)
    {
        return await _context.Verifications
            .Where(x => x.Status == status)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Verifications.CountAsync();
    }

    public async Task<int> GetCountByStatusAsync(
        string status)
    {
        return await _context.Verifications
            .CountAsync(x => x.Status == status);
    }
}