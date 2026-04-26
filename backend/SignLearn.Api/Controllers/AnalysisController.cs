using Microsoft.AspNetCore.Mvc;

using SignLearn.Api.DTOs;
using SignLearn.Api.Services;

namespace SignLearn.Api.Controllers
{
    [Route("api/analysis")]
    [ApiController]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisService _analysisService;

        public AnalysisController(IAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }
        
       [HttpPost]
       public IActionResult Analysis([FromBody] AnalysisRequest req)
        {
            try
            {
                AnalysisResponse result = _analysisService.Analyze(req);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}