namespace SignLearn.Api.DTOs
{
    public class ChallengeRequest
    {
        public int SkillLevel { get; set; } = 5;
        public List<string> MasteredSigns { get; set; } = new();
        public int Streak { get; set; } = 0;
        public List<string> UsedWords { get; set; } = new();
    }
}