namespace SignLearn.Api.Models
{
    public class SignAnalysisResponse
    {
        public required string RecognizedSign { get; set; }
        public required float Confidence { get; set; }
    }
}