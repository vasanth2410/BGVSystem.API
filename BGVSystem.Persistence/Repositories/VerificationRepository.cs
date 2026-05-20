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
}