using System;
using System.Collections.Generic;
using System.Linq;
using PromptRunner.AI.Models;

namespace PromptRunner.AI
{
    public class PromptManager
    {
        private readonly PromptsConfig _promptsConfig;

        public PromptManager(PromptsConfig promptsConfig)
        {
            _promptsConfig = promptsConfig;
        }

        public IEnumerable<PromptConfig> GetPrompts(string specificPromptName = null)
        {
            return specificPromptName != null
                ? _promptsConfig.Prompts.Where(p =>
                    string.Equals(p.Name, specificPromptName, StringComparison.OrdinalIgnoreCase))
                : _promptsConfig.Prompts;
        }
    }
}