using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace BGVSystem.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Document document)
    {
        await _context.Documents.AddAsync(document);
    }

    public async Task<List<Document>> GetByCandidateIdAsync(int candidateId)
    {
        return await _context.Documents
            .Where(x => x.CandidateId == candidateId)
            .ToListAsync();
    }

    public async Task<Document?> GetByIdAsync(int id)
    {
        return await _context.Documents
            .FirstOrDefaultAsync(x => x.Id == id);
    }   

    public async Task DeleteAsync(Document document)
    {
        _context.Documents.Remove(document);

        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Documents.CountAsync();
    }

    public async Task<List<Document>> GetAllAsync()
    {
        return await _context.Documents.ToListAsync();
    }


}