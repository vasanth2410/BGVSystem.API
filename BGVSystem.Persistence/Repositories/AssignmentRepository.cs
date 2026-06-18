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
    }
}
