namespace PromptRunner.AI.Models
{
    public class ChatResponse
    {
        public string ModelID { get; set; }
        public ChatMessage Message { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }
}