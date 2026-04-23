namespace SignLearn.Api.DTOs
{
    public class Challenge
    {
        public required string TargetSign { get; set; }
        public required string Hint { get; set; }
        public required string Difficulty { get; set; }
        public required string FunFact { get; set; }
    }
}