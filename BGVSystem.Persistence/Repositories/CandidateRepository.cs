using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using BGVSystem.Application.DTOs.Candidates;

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
        return await _context.Candidates
    .ToListAsync();
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

    public async Task DeleteAsync(
     Candidate candidate)
    {
        candidate.IsDeleted = true;

        candidate.DeletedAt =
            DateTime.UtcNow;

        _context.Candidates.Update(
            candidate);

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

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Candidates
    .CountAsync(
        x => !x.IsDeleted);
    }

    public async Task<List<Candidate>>
    GetByStatusAsync(string status)
    {
        return await _context.Candidates
     .Where(x => x.Status == status)
     .ToListAsync();
    }

    public async Task<List<Candidate>>
SearchAsync(
    CandidateSearchDto dto)
    {
        var query =
     _context.Candidates
         .Where(x => !x.IsDeleted)
         .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            query = query.Where(x =>
                x.FullName.Contains(dto.Name));
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            query = query.Where(x =>
                x.Email.Contains(dto.Email));
        }

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            query = query.Where(x =>
                x.Status == dto.Status);
        }

        query = query
            .Skip((dto.PageNumber - 1)
                * dto.PageSize)
            .Take(dto.PageSize);

        return await query.ToListAsync();
    }

    public async Task<List<Candidate>> GetDeletedCandidatesAsync()
    {
        return await _context.Candidates
            .IgnoreQueryFilters()
            .Where(x => x.IsDeleted)
            .ToListAsync();
    }

    public async Task RestoreAsync(int id)
    {
        var candidate =
            await _context.Candidates
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

        if (candidate == null)
        {
            throw new Exception(
                "Candidate not found");
        }

        candidate.IsDeleted = false;

        candidate.DeletedAt = null;

        await _context.SaveChangesAsync();
    }

    public async Task PermanentDeleteAsync(int id)
    {
        var candidate =
            await _context.Candidates
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

        if (candidate == null)
        {
            throw new Exception(
                "Candidate not found");
        }

        _context.Candidates.Remove(candidate);

        await _context.SaveChangesAsync();
    }
}