using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Persistence.Repositories
{
    public class AssignmentRepository
    : IAssignmentRepository
    {
        private readonly ApplicationDbContext
            _context;

        public AssignmentRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            CandidateAssignment assignment)
        {
            await _context.CandidateAssignments
                .AddAsync(assignment);
        }

        public async Task<List<CandidateAssignment>>
    GetAllAsync()
        {
            return await _context
                .CandidateAssignments
                .Include(x => x.Candidate)
                .Include(x => x.Reviewer)
                .ToListAsync();
        }

        public async Task<List<CandidateAssignment>>
    GetByReviewerIdAsync(
        int reviewerId)
        {
            return await _context
                .CandidateAssignments
                .Include(x => x.Candidate)
                .Include(x => x.Reviewer)
                .Where(x =>
                    x.ReviewerId ==
                    reviewerId)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsCandidateAssignedToReviewerAsync(
            int candidateId,
            int reviewerId)
        {
            return await _context
                .CandidateAssignments
                .AnyAsync(x =>
                    x.CandidateId == candidateId &&
                    x.ReviewerId == reviewerId);
        }

        public async Task DeleteAsync(int id)
        {
            var assignment = await _context.CandidateAssignments.FindAsync(id);
            if (assignment != null)
            {
                _context.CandidateAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> CleanupDuplicatesAsync()
        {
            var allAssignments = await _context.CandidateAssignments
                .OrderBy(a => a.AssignedDate)
                .ToListAsync();

            var duplicatesToRemove = new List<CandidateAssignment>();
            var seen = new HashSet<string>();

            foreach (var a in allAssignments)
            {
                var key = $"{a.CandidateId}_{a.ReviewerId}";
                if (seen.Contains(key))
                {
                    duplicatesToRemove.Add(a);
                }
                else
                {
                    seen.Add(key);
                }
            }

            if (duplicatesToRemove.Any())
            {
                _context.CandidateAssignments.RemoveRange(duplicatesToRemove);
                await _context.SaveChangesAsync();
            }

            return duplicatesToRemove.Count;
        }
    }
}
