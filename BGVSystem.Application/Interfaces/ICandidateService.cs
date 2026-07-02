using BGVSystem.Application.DTOs;
using BGVSystem.Application.DTOs.Candidates;
//using BGVSystem.Application.DTOs.Candidates;

namespace BGVSystem.Application.Interfaces;

public interface ICandidateService
{
    Task<List<CandidateResponseDto>> GetAllAsync();

    Task<CandidateResponseDto?> GetByIdAsync(int id);

    Task<string> CreateAsync(CreateCandidateDto dto);

    Task<string> UpdateAsync(int id, UpdateCandidateDto dto);

    Task<string> DeleteAsync(int id);

    Task<List<CandidateResponseDto>> SearchAsync(CandidateSearchDto dto);

    Task<List<CandidateResponseDto>> GetDeletedCandidatesAsync();

    Task<string> RestoreAsync(int id);

    Task<string> PermanentDeleteAsync(int id);

    Task<CandidateDetailsDto?> GetDetailsAsync(int id);
}