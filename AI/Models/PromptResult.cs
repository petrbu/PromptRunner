using PromptRunner.AI.Models;

namespace PromptRunner.AI.Models
{
    public class PromptResult
    {
        public string PromptId { get; set; } = string.Empty;
        public string PromptName { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public ChatMessage Response { get; set; }
        public string Error { get; set; } = string.Empty;
        public bool HasError => !string.IsNullOrEmpty(Error);

        public override string ToString()
        {
            var result = $"Prompt ID: {PromptId}\n" +
                   $"Prompt Name: {PromptName}\n" +
                   $"Provider: {Provider}\n" +
                   $"Model: {Model}\n" +
                   $"Response: {Response.Content}";

            if (HasError)
            {
                result += $"\nError: {Error}";
            }

            return result;
        }
    }
}
