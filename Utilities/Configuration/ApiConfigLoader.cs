using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using PromptRunner.AI.Models;

namespace PromptRunner.Utilities.Configuration
{
    public class ApiConfigLoader
    {
        public ApiSettings LoadApiConfig(string customPath = null)
        {
            string[] apiConfigPaths = GetConfigPaths(customPath ?? "api_config.development.json")
                .Concat(GetConfigPaths("api_config.json"))
                .ToArray();

            string apiConfigPath = apiConfigPaths.FirstOrDefault(File.Exists);
            if (apiConfigPath == null)
            {
                throw new FileNotFoundException("No API configuration file found");
            }

            Console.WriteLine($"Using API configuration from: {apiConfigPath}");

            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(apiConfigPath));
            if (config == null)
            {
                throw new JsonException("Failed to deserialize API configuration");
            }

            return config.ApiSettings;
        }

        private string[] GetConfigPaths(string filename)
        {
            return new[]
            {
                Path.Combine(AppContext.BaseDirectory, filename),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", filename),
                Path.Combine(Directory.GetCurrentDirectory(), filename)
            };
        }
    }
}