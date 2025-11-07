using System.Collections.Generic;

namespace PromptRunner.AI.Models
{
    public class PromptConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string ModelOverride { get; set; } = string.Empty;
        public double? TemperatureOverride { get; set; }
        public int? MaxTokensOverride { get; set; }
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
