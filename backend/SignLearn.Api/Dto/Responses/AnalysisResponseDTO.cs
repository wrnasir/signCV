namespace SignLearn.Api.DTOs
{
    public class AnalysisResponse
    {
        public required string RecognizedSign { get; set; }
        public required float Confidence { get; set; }
    }
}