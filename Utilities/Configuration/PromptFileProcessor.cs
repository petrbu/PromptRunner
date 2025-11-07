using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PromptRunner.AI.Models;
using PromptRunner.Utilities.References;

namespace PromptRunner.Utilities.Configuration
{
    public class PromptFileProcessor
    {
        // Image reference patterns
        private static readonly Regex DirectImagePattern = new Regex(@"\{image:(.*?)\}", RegexOptions.Compiled);
        private static readonly Regex ReferenceImagePattern = new Regex(@"\{img:(.*?)\}", RegexOptions.Compiled);

        // Text reference patterns
        private static readonly Regex DirectTextPattern = new Regex(@"\{text:(.*?)\}", RegexOptions.Compiled);
        private static readonly Regex ReferenceTextPattern = new Regex(@"\{txt:(.*?)\}", RegexOptions.Compiled);

        private readonly ImageReferenceManager _imageReferenceManager;
        private readonly TextReferenceManager _textReferenceManager;

        public PromptFileProcessor(ImageReferenceManager imageReferenceManager = null, TextReferenceManager textReferenceManager = null)
        {
            _imageReferenceManager = imageReferenceManager;
            _textReferenceManager = textReferenceManager;
        }

        public void ProcessPromptFiles(PromptsConfig promptsConfig, string[] configDirectory)
        {
            foreach (var prompt in promptsConfig.Prompts.Where(p => !string.IsNullOrEmpty(p.FilePath)))
            {
                foreach (var directory in configDirectory)
                {
                    string fullPromptPath = GetFullPromptPath(prompt.FilePath, directory);
                    if (File.Exists(fullPromptPath))
                    {
                        var promptContent = File.ReadAllText(fullPromptPath);
                        ParsePromptContent(prompt, promptContent, Path.GetDirectoryName(fullPromptPath));
                        break;
                    }
                }
            }
        }

        private string GetFullPromptPath(string filename, string configDirectory)
        {
            // If filename is already a full path, return it directly
            if (Path.IsPathRooted(filename) && File.Exists(filename))
            {
                return filename;
            }

            // If configDirectory is rooted, first check the combined path
            if (Path.IsPathRooted(configDirectory))
            {
                string fullPath = Path.Combine(configDirectory, filename);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            // List of potential base paths to check
            var pathsToCheck = new List<string>
            {
                // Check in AppContext.BaseDirectory
                Path.Combine(AppContext.BaseDirectory, filename),

                // Check in current working directory
                Path.Combine(Directory.GetCurrentDirectory(), filename),

                // Check relative to project directory (for debugging scenarios)
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", filename)
            };

            // Add workspace directory if in debug configuration
            string workspacePath = GetWorkspaceDirectory();
            if (!string.IsNullOrEmpty(workspacePath))
            {
                pathsToCheck.Add(Path.Combine(workspacePath, filename));
            }

            // Try all potential paths
            foreach (var path in pathsToCheck)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // Fallback to the original configDirectory if the file is not found in the specified paths
            return Path.Combine(configDirectory, filename);
        }

        private string GetWorkspaceDirectory()
        {
            try
            {
                // When running in debug, the BaseDirectory will typically be in a bin/Debug/netX.X path
                // Walk up from there to find the workspace root
                string baseDir = AppContext.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(baseDir);

                // Walk up to find the workspace root (maximum 5 levels up to prevent infinite loop)
                for (int i = 0; i < 5 && dir != null; i++)
                {
                    // Check for common project files that indicate workspace root
                    if (File.Exists(Path.Combine(dir.FullName, "StartAI.csproj")) ||
                        File.Exists(Path.Combine(dir.FullName, "StartAI.sln")))
                    {
                        return dir.FullName;
                    }

                    dir = dir.Parent;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting workspace directory: {ex.Message}");
            }

            return null;
        }

        private void ParsePromptContent(PromptConfig prompt, string content, string basePath)
        {
            var lines = content.Split('\n');
            var currentRole = "";
            var currentMessages = new List<string>();
            prompt.Messages.Clear();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("//") || string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                if (trimmedLine.StartsWith("### "))
                {
                    if (!string.IsNullOrEmpty(currentRole) && currentMessages.Any())
                    {
                        string messageText = string.Join("\n", currentMessages);
                        AddMessageWithImages(prompt, currentRole, messageText, basePath);
                        currentMessages.Clear();
                    }
                    currentRole = trimmedLine.Substring(4).Trim().ToLower();
                }
                else if (!string.IsNullOrEmpty(currentRole))
                {
                    currentMessages.Add(line);
                }
            }

            if (!string.IsNullOrEmpty(currentRole) && currentMessages.Count != 0)
            {
                string messageText = string.Join("\n", currentMessages);
                AddMessageWithImages(prompt, currentRole, messageText, basePath);
            }
        }        
        private void AddMessageWithImages(PromptConfig prompt, string role, string messageText, string basePath)
        {
            // First process text placeholders
            var resultText = ProcessTextPlaceholders(messageText);
            
            // Process direct image references: {image:path/to/image.jpg}
            var directMatches = DirectImagePattern.Matches(resultText);
            // Process named image references: {img:reference_key}
            var referenceMatches = _imageReferenceManager != null ? ReferenceImagePattern.Matches(resultText) : null;
            
            bool hasImages = directMatches.Count > 0 || (referenceMatches != null && referenceMatches.Count > 0);            if (hasImages)
            {
                // Extract image references and create a modified text content
                var images = new List<ImageAttachment>();
                var processedImageText = resultText;
                
                // Process direct image paths
                foreach (Match match in directMatches)
                {
                    // Get the image path
                    string imagePath = match.Groups[1].Value.Trim();

                    // If path isn't absolute, make it relative to the prompt file
                    if (!Path.IsPathRooted(imagePath))
                    {
                        imagePath = Path.Combine(basePath, imagePath);
                    }

                    // Check if file exists
                    if (File.Exists(imagePath))
                    {
                        // Add to images collection
                        images.Add(new ImageAttachment(imagePath));

                        // Remove the image reference from the text
                        resultText = resultText.Replace(match.Value, "");
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Image file not found: {imagePath}");
                    }
                }

                // Process image references if we have a reference manager
                if (_imageReferenceManager != null && referenceMatches != null)
                {
                    foreach (Match match in referenceMatches)
                    {
                        // Get the reference key
                        string referenceKey = match.Groups[1].Value.Trim();

                        // Look up the actual image path
                        string imagePath = _imageReferenceManager.GetImagePath(referenceKey);

                        if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                        {
                            // Add to images collection
                            images.Add(new ImageAttachment(imagePath));

                            // Remove the image reference from the text
                            resultText = resultText.Replace(match.Value, "");
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Referenced image '{referenceKey}' not found or file does not exist");
                        }
                    }
                }

                // Create message with both text and images
                prompt.Messages.Add(new ChatMessage(
                    role,
                    resultText.Trim(),
                    images.ToArray()
                ));
            }            else
            {
                // Regular text-only message
                prompt.Messages.Add(new ChatMessage(role, resultText));
            }
        }

        private string ProcessTextPlaceholders(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var processedText = text;

            // Process direct text insertions
            var directMatches = DirectTextPattern.Matches(processedText);
            foreach (Match match in directMatches)
            {
                string directText = match.Groups[1].Value.Trim();
                processedText = processedText.Replace(match.Value, directText);
            }

            // Process text references if we have a reference manager
            if (_textReferenceManager != null)
            {
                var referenceMatches = ReferenceTextPattern.Matches(processedText);
                foreach (Match match in referenceMatches)
                {
                    string referenceKey = match.Groups[1].Value.Trim();
                    string replacementText = _textReferenceManager.GetText(referenceKey);

                    if (!string.IsNullOrEmpty(replacementText))
                    {
                        processedText = processedText.Replace(match.Value, replacementText);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Text reference '{referenceKey}' not found");
                    }
                }
            }

            return processedText;
        }
    }
}