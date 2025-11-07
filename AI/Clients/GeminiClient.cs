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
    public class GeminiClient : BaseApiClient
    {
        public GeminiClient(string apiKey, string endpoint, string model)
            : base(apiKey, endpoint, model)
        {
            // Additional initialization if needed
        }

        public override async Task<ChatResponse> CompleteAsync(List<ChatMessage> messages, ChatOptions options)
        {
            try
            {
                HttpClient client = new HttpClient();
                try
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Extract system message
                    var systemMessage = messages.FirstOrDefault(m => m.Role == "system");
                    var userMessages = messages.Where(m => m.Role != "system").ToList();

                    // Build content parts list with support for images
                    var contents = new List<object>();
                    
                    foreach (var msg in userMessages)
                    {
                        var roleStr = msg.Role == "user" ? "user" : "model";
                        
                        // For messages with images, we need to build a parts array containing
                        // both text and image data
                        if (msg.HasImages)
                        {
                            var parts = new List<object>();
                            
                            // Add text part if not empty
                            if (!string.IsNullOrEmpty(msg.Content))
                            {
                                parts.Add(new { text = msg.Content });
                            }
                            
                            // Add each image as an inline_data part
                            foreach (var image in msg.Images)
                            {
                                // Load image as base64 if not already loaded
                                if (string.IsNullOrEmpty(image.Base64Data))
                                {
                                    image.Base64Data = ImageHelper.ImageToBase64(image.Path);
                                }
                                
                                if (!string.IsNullOrEmpty(image.Base64Data))
                                {
                                    parts.Add(new 
                                    { 
                                        inline_data = new 
                                        {
                                            mime_type = image.MimeType,
                                            data = image.Base64Data
                                        }
                                    });
                                }
                            }
                            
                            contents.Add(new
                            {
                                role = roleStr,
                                parts = parts.ToArray()
                            });
                        }
                        else
                        {
                            // Text-only message (original logic)
                            contents.Add(new
                            {
                                role = roleStr,
                                parts = new[] { new { text = msg.Content } }
                            });
                        }
                    }

                    var requestBody = new
                    {
                        contents = contents.ToArray(),
                        generationConfig = new
                        {
                            temperature = options.Temperature,
                            maxOutputTokens = options.MaxOutputTokens,
                        },
                        system_instruction = systemMessage != null ? new
                        {
                            parts = new[] { new { text = systemMessage.Content } }
                        } : null
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var requestUrl = $"{Endpoint}/v1beta/models/{Model}:generateContent?key={ApiKey}";
                    var response = await client.PostAsync(requestUrl, content);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonDocument.Parse(jsonResponse);

                    string responseMessage = result.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return new ChatResponse
                    {
                        ModelID = Model,
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
