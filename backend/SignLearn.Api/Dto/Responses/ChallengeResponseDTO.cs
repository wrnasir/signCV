namespace SignLearn.Api.DTOs
{
    public class ChallengeResponse
    {
        public required string TargetWord { get; set; }
        public required string Hint { get; set; }
        public required string Difficulty { get; set; }
    }
}