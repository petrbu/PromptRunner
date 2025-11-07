using System;
using PromptRunner.AI.Models;
using System.Net.Http;

namespace PromptRunner.AI.Clients
{
    public interface IErrorHandler
    {
        ChatResponse HandleError(Exception ex, string modelId);
        ChatResponse HandleError(HttpRequestException ex, string modelId);
    }

    // Create a static class for error handling functionality
    public static class ErrorHandlerHelper
    {
        public static AIException HandleHttpException(HttpRequestException ex)
        {
            AIException result;
            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                result = new AIException(AIErrorType.Authentication, "Authentication failed", ex.Message);
            }
            else if (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                result = new AIException(AIErrorType.RateLimit, "Rate limit exceeded", ex.Message);
            }
            else if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                result = new AIException(AIErrorType.ModelNotFound, "Model or endpoint not found", ex.Message);
            }
            else if (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                result = new AIException(AIErrorType.ServiceUnavailable, "Service is unavailable", ex.Message);
            }
            else
            {
                result = new AIException(AIErrorType.Unknown, "An unexpected error occurred", ex.Message);
            }
            return result;
        }
    }
}
