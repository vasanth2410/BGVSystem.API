using BGVSystem.Application.DTOs;
//using BGVSystem.Application.DTOs.Candidates;

namespace BGVSystem.Application.Interfaces;

public interface ICandidateService
{
    Task<List<CandidateResponseDto>> GetAllAsync();

    Task<CandidateResponseDto?> GetByIdAsync(int id);

    Task<string> CreateAsync(CreateCandidateDto dto);

    Task<string> UpdateAsync(int id, UpdateCandidateDto dto);

    Task<string> DeleteAsync(int id);
}