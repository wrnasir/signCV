using SignLearn.Api.DTOs;

namespace SignLearn.Api.Services
{
    public interface IChallengeService
    {
        Task<ChallengeResponse> GenerateChallenge(ChallengeRequest request);
    }
}