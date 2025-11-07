using System;
using System.IO;
using PromptRunner.AI.Models;
using System.Text.Json;
using System.Linq;

namespace PromptRunner.Utilities.Configuration
{
    public class PromptsConfigLoader
    {
        private const string DEFAULT_CONFIG_FILENAME = "prompts_config - providers.json";
        private const string PROMPT_CONFIGS_FOLDER = "PromptConfigs";

        public (PromptsConfig Config, string ConfigDirectory) LoadPromptsConfig(string promptsConfigFilename = null)
        {
            string configFilename = promptsConfigFilename ?? DEFAULT_CONFIG_FILENAME;
            string[] promptsConfigPaths = GetConfigPaths(configFilename);

            string promptsConfigPath = promptsConfigPaths.FirstOrDefault(File.Exists);
            if (promptsConfigPath == null)
            {
                throw new FileNotFoundException("No Prompts configuration file found");
            }

            string configDirectory = Path.GetDirectoryName(promptsConfigPath);
            if (configDirectory == null)
            {
                throw new DirectoryNotFoundException("Could not determine configuration directory");
            }

            var promptsConfig = JsonSerializer.Deserialize<PromptsConfig>(File.ReadAllText(promptsConfigPath));
            return (promptsConfig ?? new PromptsConfig(), configDirectory);
        }

        private string[] GetConfigPaths(string filename)
        {
            return new[]
            {
                    Path.Combine(AppContext.BaseDirectory, PROMPT_CONFIGS_FOLDER, filename),
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Utilities", "Configuration", PROMPT_CONFIGS_FOLDER, filename),
                    Path.Combine(Directory.GetCurrentDirectory(), "Utilities", "Configuration", PROMPT_CONFIGS_FOLDER, filename)
                };
        }
    }
}