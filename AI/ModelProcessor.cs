using System;
using System.Collections.Generic;
using System.Linq;
using PromptRunner.AI.Clients;
using PromptRunner.AI.Models;

namespace PromptRunner.AI
{
    public class ModelProcessor
    {
        private readonly ApiClient _apiClient;
        private readonly PromptManager _promptManager;

        public ModelProcessor(ApiClient apiClient, PromptManager promptManager)
        {
            _apiClient = apiClient;
            _promptManager = promptManager;
        }

        public List<PromptResult> ProcessPrompts()
        {
            var results = new List<PromptResult>();
            var promptsToProcess = _promptManager.GetPrompts();

            foreach (var prompt in promptsToProcess)
            {
                var result = _apiClient.ProcessPrompt(prompt);
                results.Add(result);
            }

            return results;
        }
    }
}


