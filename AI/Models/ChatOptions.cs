namespace PromptRunner.AI.Models
{
    public class ChatOptions
    {
        public float? Temperature { get; set; }
        public int? MaxOutputTokens { get; set; }

        public ChatOptions(float? temperature = null, int? maxOutputTokens = null)
        {
            Temperature = temperature;
            MaxOutputTokens = maxOutputTokens;
        }
    }
}