using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BGVSystem.Persistence.Repositories;

public class CandidateRepository : ICandidateRepository
{
    private readonly ApplicationDbContext _context;

    public CandidateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Candidate>> GetAllAsync()
    {
        return await _context.Candidates.ToListAsync();
    }

    public async Task<Candidate?> GetByIdAsync(int id)
    {
        return await _context.Candidates
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Candidate candidate)
    {
        await _context.Candidates.AddAsync(candidate);
    }

    public async Task UpdateAsync(Candidate candidate)
    {
        _context.Candidates.Update(candidate);

        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Candidate candidate)
    {
        _context.Candidates.Remove(candidate);

        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<Candidate?> GetByEmailAsync(
    string email)
    {
        return await _context.Candidates
            .FirstOrDefaultAsync(x =>
                x.Email == email);
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Candidates.CountAsync();
    }

    public async Task<List<Candidate>>
    GetByStatusAsync(string status)
    {
        return await _context.Candidates
            .Where(x => x.Status == status)
            .ToListAsync();
    }
}