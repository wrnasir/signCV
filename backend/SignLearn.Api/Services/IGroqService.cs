namespace SignLearn.Api.Services
{
    public interface IGroqService
    {
        Task<string> SendPrompt(string prompt, string systemPrompt = "You are a helpful assistant.");
    }
}