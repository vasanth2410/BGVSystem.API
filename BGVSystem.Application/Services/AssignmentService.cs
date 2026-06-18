using BGVSystem.Application.DTOs.Assignments;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGVSystem.Application.Services
{
    public class AssignmentService
    : IAssignmentService
    {
        private readonly IAssignmentRepository
            _assignmentRepository;

        private readonly ICandidateRepository
    _candidateRepository;

        private readonly IUserRepository
            _userRepository;

        public AssignmentService(
            IAssignmentRepository
                assignmentRepository, ICandidateRepository
    candidateRepository, IUserRepository
            userRepository)
        {
            _assignmentRepository =
                assignmentRepository;

            _candidateRepository = candidateRepository;

            _userRepository = userRepository;
        }

        public async Task<string>
      CreateAsync(
          CreateAssignmentDto dto)
        {
            var candidate =
                await _candidateRepository
                    .GetByIdAsync(
                        dto.CandidateId);

            if (candidate == null)
            {
                throw new Exception(
                    "Candidate not found");
            }

            var reviewer =
                await _userRepository
                    .GetByIdAsync(
                        dto.ReviewerId);

            if (reviewer == null)
            {
                throw new Exception(
                    "Reviewer not found");
            }

            if (reviewer.RoleId != 2)
            {
                throw new Exception(
                    "Selected user is not a reviewer");
            }

            var assignment =
                new CandidateAssignment
                {
                    CandidateId =
                        dto.CandidateId,

                    ReviewerId =
                        dto.ReviewerId,

                    AssignedDate =
                        DateTime.UtcNow
                };

            await _assignmentRepository
                .AddAsync(assignment);

            await _assignmentRepository
                .SaveChangesAsync();

            return "Candidate assigned successfully";
        }

        public async Task<
            List<AssignmentResponseDto>>
            GetAllAsync()
        {
            var assignments =
                await _assignmentRepository
                    .GetAllAsync();

            return assignments
    .Select(x =>
        new AssignmentResponseDto
        {
            Id = x.Id,

            CandidateId =
                x.CandidateId,

            CandidateName =
                x.Candidate.FullName,

            ReviewerId =
                x.ReviewerId,

            ReviewerName =
                x.Reviewer.FullName,

            AssignedDate =
                x.AssignedDate
        })
    .ToList();

        }

        public async Task<List<AssignmentResponseDto>>
    GetByReviewerIdAsync(int reviewerId)
        {
            var assignments =
                await _assignmentRepository
                    .GetByReviewerIdAsync(reviewerId);

            return assignments
                .Select(x => new AssignmentResponseDto
                {
                    Id = x.Id,
                    CandidateId = x.CandidateId,
                    CandidateName = x.Candidate.FullName,
                    ReviewerId = x.ReviewerId,
                    ReviewerName = x.Reviewer.FullName,
                    AssignedDate = x.AssignedDate
                })
                .ToList();
        }
    }
}
