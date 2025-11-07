using System;
using System.Collections.Generic;
using PromptRunner.AI;
using PromptRunner.AI.Clients;
using PromptRunner.Utilities;

internal class Program
{
    private static void Main(string[] args)
    {
        // Default config file if none specified via --config argument
        var promptsConfigFilename = "prompts_config - providers-azure.json";

        // Parse command line arguments
        var imageReferences = new List<string>();
        var textReferences = new List<string>();
        string promptFilesFolder = "";
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--config" && i + 1 < args.Length)
            {
                promptsConfigFilename = args[i + 1];
                i++;
            }
            else if (args[i] == "--image" && i + 1 < args.Length)
            {
                imageReferences.Add(args[i + 1]);
                i++;
            }
            else if (args[i] == "--text" && i + 1 < args.Length)
            {
                textReferences.Add(args[i + 1]);
                i++;
            }
            else if ((args[i] == "--prompt-folder" || args[i] == "--prompts-folder") && i + 1 < args.Length)
            {
                promptFilesFolder = args[i + 1];
                i++;
            }
        }
          var (apiConfig, promptsConfig) = ConfigurationLoader.LoadConfigurations(
            promptsConfigFilename,
            imageReferences.Count > 0 ? imageReferences.ToArray() : null,
            textReferences.Count > 0 ? textReferences.ToArray() : null,
            promptFilesFolder);

        // Create instances of ApiClient and PromptManager
        var apiClient = new ApiClient(apiConfig);
        var promptManager = new PromptManager(promptsConfig);

        // Create the ModelProcessor with the new instances
        var modelProcessor = new ModelProcessor(apiClient, promptManager);

        // Process all prompts if no specific prompt is specified
        var results = modelProcessor.ProcessPrompts();
        foreach (var result in results)
        {
            Console.WriteLine(result);
            Console.WriteLine("=============");
        }
    }
}

