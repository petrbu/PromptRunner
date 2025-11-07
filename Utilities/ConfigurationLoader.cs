using System;
using System.IO;
using System.Linq;
using PromptRunner.AI.Models;
using PromptRunner.Utilities.Configuration;
using PromptRunner.Utilities.References;

namespace PromptRunner.Utilities
{    public static class ConfigurationLoader
    {
        public static (ApiSettings ApiConfig, PromptsConfig PromptsConfig) LoadConfigurations(
            string promptsConfigFilename, 
            string[] imageArguments = null,
            string[] textArguments = null,
            string promptFilesFolder = "")
        {
            try
            {
                var apiLoader = new ApiConfigLoader();
                var promptsLoader = new PromptsConfigLoader();
                
                // Process any image references
                ImageReferenceManager imageManager = null;
                if (imageArguments != null && imageArguments.Length > 0)
                {
                    imageManager = new ImageReferenceManager();
                    
                    // Process image arguments in format "key=path"
                    foreach (string arg in imageArguments)
                    {
                        var parts = arg.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            imageManager.AddImageReference(parts[0], parts[1]);
                        }
                    }
                }

                // Process any text references
                TextReferenceManager textManager = null;
                if (textArguments != null && textArguments.Length > 0)
                {
                    textManager = new TextReferenceManager();
                    
                    // Process text arguments in format "key=value"
                    foreach (string arg in textArguments)
                    {
                        var parts = arg.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            textManager.AddTextReference(parts[0], parts[1]);
                        }
                    }
                }
                
                var promptProcessor = new PromptFileProcessor(imageManager, textManager);

                var apiConfig = apiLoader.LoadApiConfig();
                var (promptsConfig, configDirectory) = promptsLoader.LoadPromptsConfig(promptsConfigFilename);

                // Pass the prompt files folder (if specified) or use the config directory
                string[] folderPaths = !string.IsNullOrEmpty(promptFilesFolder) 
                    ? new string[] { promptFilesFolder, configDirectory }
                    : new string[] { configDirectory };
                    
                promptProcessor.ProcessPromptFiles(promptsConfig, folderPaths);

                return (apiConfig, promptsConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configurations: {ex.Message}");
                return (new ApiSettings(), new PromptsConfig());
            }
        }
    }
}

