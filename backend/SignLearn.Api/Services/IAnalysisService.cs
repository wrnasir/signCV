using SignLearn.Api.DTOs;

namespace SignLearn.Api.Services
{
    public interface IAnalysisService
    {
        AnalysisResponse Analyze(AnalysisRequest req);
    }
}