namespace SignLearn.Api.Models
{
    public class SignAnalysisRequest
    {
        public required string RecognizedSign { get; set; }
        public required float Confidence { get; set; }
    }
}