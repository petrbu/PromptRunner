using System.Collections.Generic;

namespace PromptRunner.AI.Models
{
    public class ApiSettings
    {
        public string DefaultProvider { get; set; }
        public Dictionary<string, ProviderSettings> Providers { get; set; }
        public GlobalSettings GlobalSettings { get; set; }
    }
}
