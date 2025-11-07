using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using PromptRunner.AI.Models;
using PromptRunner.Utilities;

namespace PromptRunner.AI.Clients
{
    public class AzureOpenAIClient : BaseApiClient
    {
        private readonly string _apiVersion;

        public AzureOpenAIClient(string apiKey, string endpoint, string model, string apiVersion) 
            : base(apiKey, endpoint, model)
        {
            _apiVersion = apiVersion;
        }

        public override async Task<ChatResponse> CompleteAsync(List<ChatMessage> messages, ChatOptions options)
        {
            try
            {
                HttpClient client = new HttpClient();
                try
                {
                    client.DefaultRequestHeaders.Add("api-key", ApiKey);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Build messages with content that can include text and images
                    var formattedMessages = new List<object>();

                    foreach (var msg in messages)
                    {
                        // Check if the message has images
                        if (msg.HasImages)
                        {
                            // For messages with images, we need to build a content array
                            var contentParts = new List<object>();

                            // Add text content if not empty
                            if (!string.IsNullOrEmpty(msg.Content))
                            {
                                contentParts.Add(new 
                                { 
                                    type = "text", 
                                    text = msg.Content 
                                });
                            }

                            // Add each image as a content part
                            foreach (var image in msg.Images)
                            {
                                // Load image as base64 if not already loaded
                                if (string.IsNullOrEmpty(image.Base64Data))
                                {
                                    image.Base64Data = ImageHelper.ImageToBase64(image.Path);
                                }

                                if (!string.IsNullOrEmpty(image.Base64Data))
                                {
                                    // Format according to Azure OpenAI requirements
                                    contentParts.Add(new
                                    {
                                        type = "image_url",
                                        image_url = new
                                        {
                                            url = $"data:{image.MimeType};base64,{image.Base64Data}"
                                        }
                                    });
                                }
                            }

                            formattedMessages.Add(new
                            {
                                role = msg.Role.ToLowerInvariant(), // ensure lowercase
                                content = contentParts
                            });
                        }
                        else
                        {
                            // Text-only message with explicit format
                            formattedMessages.Add(new
                            {
                                role = msg.Role.ToLowerInvariant(), // ensure lowercase
                                content = msg.Content
                            });
                        }
                    }

                    // Create more complete request body with all required parameters
                    var requestBody = new
                    {
                        messages = formattedMessages,
                        max_tokens = options.MaxOutputTokens,
                        temperature = options.Temperature,
                        // model = Model, // Include model explicitly
                        // stream = false // Disable streaming for standard response
                    };

                    var jsonSerializerOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    };

                    var json = JsonSerializer.Serialize(requestBody, jsonSerializerOptions);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var requestUrl = $"{Endpoint}/openai/deployments/{Model}/chat/completions?api-version={_apiVersion}";

                    var response = await client.PostAsync(requestUrl, content);
                    
                    // For debugging - get error details
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error Status: {response.StatusCode}");
                        Console.WriteLine($"Error Response: {errorContent}");
                        return HandleError(new HttpRequestException($"API returned {response.StatusCode}: {errorContent}"), Model);
                    }
                    
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonDocument.Parse(jsonResponse);

                    string modelId = result.RootElement.GetProperty("model").GetString();
                    string responseMessage = result.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    return new ChatResponse
                    {
                        ModelID = modelId,
                        Message = new ChatMessage("assistant", responseMessage)
                    };
                }
                finally
                {
                    client.Dispose();
                }
            }
            catch (HttpRequestException ex)
            {
                return HandleError(ex, Model);
            }
            catch (Exception ex)
            {
                return HandleError(ex, Model);
            }
        }
    }
}