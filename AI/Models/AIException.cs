using System;

namespace PromptRunner.AI.Models
{
    public enum AIErrorType
    {
        Authentication,
        RateLimit,
        InvalidRequest,
        ModelNotFound,
        ServiceUnavailable,
        Unknown
    }

    public class AIException : Exception
    {
        public AIErrorType ErrorType { get; }
        public string Details { get; }

        public AIException(AIErrorType type, string message, string details = null, Exception inner = null) 
            : base(message, inner)
        {
            ErrorType = type;
            Details = details;
        }

        public AIException(string message) : base(message)
        {
        }

        public AIException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
