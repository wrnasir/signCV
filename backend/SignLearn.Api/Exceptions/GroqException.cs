namespace SignLearn.Api.Exceptions
{
    public class GroqException : Exception
    {
        public int StatusCode { get; }
        public string ResponseBody { get; }

        public GroqException(int statusCode, string responseBody)
            : base($"Groq API error ({statusCode}): {responseBody}")
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}