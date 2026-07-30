using BGVSystem.Application.DTOs.Verifications;

namespace BGVSystem.Application.Interfaces;

public interface IThirdPartyVerificationService
{
    Task<LiveVerificationResultDto> RunLiveVerificationAsync(int candidateId);
}
