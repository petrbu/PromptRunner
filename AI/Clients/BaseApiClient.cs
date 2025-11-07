using PromptRunner.AI.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;


namespace PromptRunner.AI.Clients
{
    public abstract class BaseApiClient
    {
        protected readonly string ApiKey;
        protected readonly string Endpoint;
        protected readonly string Model;

        protected BaseApiClient(string apiKey, string endpoint, string model)
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentException("Endpoint must be provided", nameof(endpoint));
            }

            ApiKey = apiKey;
            Endpoint = endpoint.TrimEnd('/');
            Model = model;
        }

        protected ChatResponse HandleError(HttpRequestException ex, string modelId)
        {
            ChatResponse response = new ChatResponse 
            {
                ModelID = modelId,
                Message = new ChatMessage("assistant", "Error occurred"),
                IsError = true
            };

            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                response.ErrorMessage = "Authentication failed";
            }
            else if (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                response.ErrorMessage = "Rate limit exceeded";
            }
            else
            {
                response.ErrorMessage = $"Request failed: {ex.Message}";
            }

            return response;
        }

        protected ChatResponse HandleError(Exception ex, string modelId)
        {
            return new ChatResponse
            {
                ModelID = modelId,
                Message = new ChatMessage("assistant", "Error occurred"),
                IsError = true,
                ErrorMessage = $"Unexpected error: {ex.Message}"
            };
        }

        public abstract Task<ChatResponse> CompleteAsync(List<ChatMessage> messages, ChatOptions options);
    }
}