using Microsoft.AspNetCore.Mvc;
using SignLearn.Api.DTOs;
using SignLearn.Api.Services;

namespace SignLearn.Api.Controllers
{
    [Route("api/challenge")]
    [ApiController]
    public class ChallengeController : ControllerBase
    {
        private readonly IChallengeService _challengeService;

        public ChallengeController(IChallengeService challengeService)
        {
            _challengeService = challengeService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] ChallengeRequest request)
        {
            try
            {
                ChallengeResponse response = await _challengeService.GenerateChallenge(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}