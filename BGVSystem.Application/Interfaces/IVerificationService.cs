using BGVSystem.Application.DTOs.Verifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace BGVSystem.Application.Interfaces
{
    public interface IVerificationService
    {
        Task<string> CreateAsync(CreateVerificationDto dto);

        Task<List<VerificationResponseDto>> GetAllAsync();

        Task<VerificationResponseDto?> GetByIdAsync(int id);

        Task<string> ApproveAsync(int id, string remarks);

        Task<string> RejectAsync(int id, string remarks);
    }
}
