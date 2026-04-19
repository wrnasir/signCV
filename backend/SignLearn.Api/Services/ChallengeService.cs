using System.Text.Json;
using SignLearn.Api.DTOs;

namespace SignLearn.Api.Services
{
    /// <summary>
    /// Generates ASL spelling challenges by building prompts
    /// and delegating LLM calls to GroqService.
    /// </summary>
    public class ChallengeService
    {
        private readonly GroqService _groqService;
        private const int MIN_SKILL = 1;
        private const int MAX_SKILL = 10;
        private const int DEFAULT_SKILL = 3;

        private const string SYSTEM_PROMPT =
        @"You are an ASL (American Sign Language) teaching assistant. 
        You generate words for students to practice spelling through sign language.
        You must respond with ONLY valid JSON, no markdown, no backticks, no explanation.";

        public ChallengeService(GroqService groqService)
        {
            _groqService = groqService;
        }

        /// <summary>
        /// Generates a word challenge scaled to the user's skill level.
        /// </summary>
        public async Task<ChallengeResponse> GenerateChallenge(ChallengeRequest request)
        {
            int skillLevel = ClampSkillLevel(request.SkillLevel);
            string prompt = BuildPrompt(skillLevel, request.MasteredSigns, request.Streak);
            string rawResponse = await _groqService.SendPrompt(prompt, SYSTEM_PROMPT);
            ChallengeResponse response = ParseResponse(rawResponse);
            return response;
        }

        /// <summary>
        /// Clamps skill level to valid range, defaults if out of bounds.
        /// </summary>
        private int ClampSkillLevel(int skillLevel)
        {
            if (skillLevel < MIN_SKILL || skillLevel > MAX_SKILL)
            {
                return DEFAULT_SKILL;
            }
            return skillLevel;
        }

        /// <summary>
        /// Builds the prompt string for Groq based on user context.
        /// </summary>
        private string BuildPrompt(int skillLevel, List<string> masteredSigns, int streak)
        {
            string masteredSection = masteredSigns.Count > 0
                ? $"The student has mastered these signs: {string.Join(", ", masteredSigns)}. Incorporate these letters more often."
                : "The student has not mastered any signs yet. Start with letters that have distinct hand shapes (e.g., L, O, B, C, W).";

            string streakSection = streak switch
            {
                >= 7 => "The student is on a hot streak. Push the difficulty higher than the skill level suggests.",
                >= 4 => "The student is doing well. Slightly increase the challenge.",
                0 => "The student just started or recently failed. Keep it encouraging.",
                _ => ""
            };

            string difficultyGuide = skillLevel switch
            {
                <= 3 => "Use short words (3-4 letters) with visually distinct signs. Avoid letters M, N, S, T which look similar.",
                <= 6 => "Use medium words (4-6 letters). Can include some similar-looking signs but not too many in one word.",
                <= 8 => "Use longer words (5-7 letters). Include challenging signs like M, N, S, T.",
                _ => "Use complex words (6-8 letters). Include multiple difficult signs. Challenge the student."
            };

            return $@"Generate a single word for an ASL spelling challenge.
                    Skill level: {skillLevel}/{MAX_SKILL}
                    Current streak: {streak} correct in a row
                    {masteredSection}
                    {streakSection}
                    {difficultyGuide}

                    Respond with ONLY this JSON format:
                    {{
                        ""targetWord"": ""the word in uppercase"",
                        ""hint"": ""a short hint about the word's meaning"",
                        ""difficulty"": ""easy/medium/hard""
                    }}";
        }

        /// <summary>
        /// Parses the raw LLM response string into a ChallengeResponse.
        /// Handles cases where the LLM wraps JSON in backticks.
        /// </summary>
        private ChallengeResponse ParseResponse(string rawResponse)
        {
            // Strip markdown backticks if the LLM added them
            string cleaned = rawResponse.Trim();
            if (cleaned.StartsWith("```"))
            {
                int firstNewline = cleaned.IndexOf('\n');
                int lastBackticks = cleaned.LastIndexOf("```");
                if (firstNewline != -1 && lastBackticks > firstNewline)
                {
                    cleaned = cleaned.Substring(firstNewline + 1, lastBackticks - firstNewline - 1).Trim();
                }
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            ChallengeResponse response = JsonSerializer.Deserialize<ChallengeResponse>(cleaned, options)
                ?? throw new Exception("Failed to parse challenge from LLM response");

            // Ensure the word is uppercase
            response.TargetWord = response.TargetWord.ToUpper();

            return response;
        }
    }
}