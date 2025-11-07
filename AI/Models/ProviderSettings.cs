namespace PromptRunner.AI.Models
{
    public class ProviderSettings
    {
        public string Uri { get; set; }
        public string DefaultModel { get; set; }
        public string ApiKey { get; set; }
        public string Endpoint { get; set; } // Optional for Azure

        public string ApiVersion { get; set; }
    }
}
