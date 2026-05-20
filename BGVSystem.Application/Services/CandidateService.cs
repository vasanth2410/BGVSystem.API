using BGVSystem.Application.DTOs;
//using BGVSystem.Application.DTOs.Candidates;
using BGVSystem.Application.Interfaces;
using BGVSystem.Domain.Entities;
using BGVSystem.Application.DTOs;

namespace BGVSystem.Application.Services;

public class CandidateService : ICandidateService
{
    private readonly ICandidateRepository _candidateRepository;

    public CandidateService(ICandidateRepository candidateRepository)
    {
        _candidateRepository = candidateRepository;
    }

    public async Task<List<CandidateResponseDto>> GetAllAsync()
    {
        var candidates = await _candidateRepository.GetAllAsync();

        return candidates.Select(x => new CandidateResponseDto
        {
            Id = x.Id,
            FullName = x.FullName,
            Email = x.Email,
            Status = x.Status
        }).ToList();
    }

    public async Task<CandidateResponseDto?> GetByIdAsync(int id)
    {
        var candidate = await _candidateRepository.GetByIdAsync(id);

        if (candidate == null)
        {
            return null;
        }

        return new CandidateResponseDto
        {
            Id = candidate.Id,
            FullName = candidate.FullName,
            Email = candidate.Email,
            Status = candidate.Status
        };
    }

    public async Task<string> CreateAsync(CreateCandidateDto dto)
    {
        var candidate = new Candidate
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            PANNumber = dto.PANNumber,
            AadhaarNumber = dto.AadhaarNumber,
            AppliedRole = dto.AppliedRole,
            DateOfJoining = dto.DateOfJoining,
            Status = "Pending"
        };

        await _candidateRepository.AddAsync(candidate);

        await _candidateRepository.SaveChangesAsync();

        return "Candidate created successfully";
    }

    public async Task<string> UpdateAsync(int id, UpdateCandidateDto dto)
    {
        var candidate = await _candidateRepository.GetByIdAsync(id);

        if (candidate == null)
        {
            throw new Exception("Candidate not found");
        }

        candidate.FullName = dto.FullName;
        candidate.Email = dto.Email;
        candidate.PhoneNumber = dto.PhoneNumber;
        candidate.Address = dto.Address;
        candidate.DateOfBirth = dto.DateOfBirth;
        candidate.Gender = dto.Gender;
        candidate.PANNumber = dto.PANNumber;
        candidate.AadhaarNumber = dto.AadhaarNumber;
        candidate.AppliedRole = dto.AppliedRole;
        candidate.DateOfJoining = dto.DateOfJoining;

        await _candidateRepository.UpdateAsync(candidate);

        await _candidateRepository.SaveChangesAsync();

        return "Candidate updated successfully";
    }

    public async Task<string> DeleteAsync(int id)
    {
        var candidate = await _candidateRepository.GetByIdAsync(id);

        if (candidate == null)
        {
            throw new Exception("Candidate not found");
        }

        await _candidateRepository.DeleteAsync(candidate);

        await _candidateRepository.SaveChangesAsync();

        return "Candidate deleted successfully";
    }
}