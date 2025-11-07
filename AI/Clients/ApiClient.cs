using System;
using System.Collections.Generic;
using PromptRunner.AI.Models;

namespace PromptRunner.AI.Clients
{
    public class ApiClient
    {
        private readonly ApiSettings _apiConfig;

        public ApiClient(ApiSettings apiConfig)
        {
            _apiConfig = apiConfig;
        }

        public PromptResult ProcessPrompt(PromptConfig prompt)
        {
            try
            {
                // Determine provider configuration
                var providerName = prompt.Provider ?? _apiConfig.DefaultProvider;
                var providerConfig = _apiConfig.Providers.TryGetValue(providerName, out var config)
                    ? config
                    : throw new InvalidOperationException($"Provider {providerName} not configured");

                // Create chat options with global and prompt-specific settings
                var options = new ChatOptions
                {
                    Temperature = prompt.TemperatureOverride.HasValue
                        ? (float?)Convert.ToSingle(prompt.TemperatureOverride.Value)
                        : (float?)_apiConfig.GlobalSettings.DefaultTemperature,
                    MaxOutputTokens = prompt.MaxTokensOverride ?? _apiConfig.GlobalSettings.DefaultMaxOutputTokens,
                };

                // Create appropriate chat client based on provider
                dynamic client;
                switch (providerName)
                {
                    case "AzureOpenAI":
                        var azureApiKey = providerConfig.ApiKey;
                        var azureEndpoint = providerConfig.Uri;
                        var azureModel = prompt.ModelOverride.Length > 0 ? prompt.ModelOverride : providerConfig.DefaultModel;
                        client = new AzureOpenAIClient(azureApiKey, azureEndpoint, azureModel, providerConfig.ApiVersion);
                        break;
                    case "Anthropic":
                        var anthropicApiKey = providerConfig.ApiKey;
                        var anthropicEndpoint = providerConfig.Uri;
                        var anthropicModel = prompt.ModelOverride.Length > 0 ? prompt.ModelOverride : providerConfig.DefaultModel;
                        client = new AnthropicClient(anthropicApiKey, anthropicEndpoint, anthropicModel, providerConfig.ApiVersion);
                        break;
                    case "OpenAI":
                        var openaiApiKey = providerConfig.ApiKey;
                        var openaiEndpoint = providerConfig.Uri;
                        var openaiModel = prompt.ModelOverride.Length > 0 ? prompt.ModelOverride : providerConfig.DefaultModel;
                        client = new OpenAIClient(openaiApiKey, openaiEndpoint, openaiModel);
                        break;
                    case "Gemini":
                        var geminiApiKey = providerConfig.ApiKey;
                        var geminiEndpoint = providerConfig.Uri;
                        var geminiModel = prompt.ModelOverride.Length > 0 ? prompt.ModelOverride : providerConfig.DefaultModel;
                        client = new GeminiClient(geminiApiKey, geminiEndpoint, geminiModel);
                        break;
                    case "Ollama":
                        var ollamaApiKey = providerConfig.ApiKey;
                        var ollamaEndpoint = providerConfig.Uri;
                        var ollamaModel = prompt.ModelOverride.Length > 0 ? prompt.ModelOverride : providerConfig.DefaultModel;
                        client = new OllamaClient(ollamaApiKey, ollamaEndpoint, ollamaModel);
                        break;
                    default:
                        throw new NotSupportedException($"Provider {providerName} not supported");
                }

                // Create conversation from prompt messages or fallback to default
                List<ChatMessage> conversation;
                if (prompt.Messages?.Count > 0)
                {
                    conversation = new List<ChatMessage>(prompt.Messages);
                }
                else
                {
                    throw new InvalidOperationException($"No messages found for prompt {prompt.Id}");
                }

                // Process prompt
                var response = client.CompleteAsync(conversation, options).GetAwaiter().GetResult();

                if (response.IsError)
                {
                    return new PromptResult
                    {
                        PromptId = prompt.Id,
                        PromptName = prompt.Name,
                        Provider = providerName,
                        Error = response.ErrorMessage,
                        Response = response.Message 
                    };
                }

                return new PromptResult
                {
                    PromptId = prompt.Id,
                    PromptName = prompt.Name,
                    Provider = providerName,
                    Model = response.ModelID,
                    Response = response.Message
                };
            }
            catch (Exception ex)
            {
                return new PromptResult
                {
                    PromptId = prompt.Id,
                    PromptName = prompt.Name,
                    Provider = prompt.Provider,
                    Error = $"Configuration error: {ex.Message}",
                    Response = new ChatMessage("assistant", "Configuration error occurred")  // Add this line
                };
            }
        }
    }
}