using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PromptRunner.AI.Models;

namespace PromptRunner.AI.Clients
{
    public class OllamaClient : BaseApiClient
    {
        public OllamaClient(string apiKey, string endpoint, string model) 
            : base(apiKey, endpoint != null ? endpoint.TrimEnd('/') : "http://localhost:11434", model)
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
                    if (!string.IsNullOrEmpty(ApiKey))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                    }
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var requestBody = new
                    {
                        model = Model,
                        messages = messages
                        //.Where(msg => msg.Role != "system") //keep system role message, seems to work better than system 
                        .Select(msg => new
                        {
                            role = msg.Role,
                            content = msg.Content
                        }).ToArray(),
                        stream = false,
                        options = new
                        {
                            temperature = options.Temperature,
                            num_predict = options.MaxOutputTokens
                        },
                        system = messages.FirstOrDefault(m => m.Role == "system")?.Content //this does not have impact on the response even if present according to the documentation
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var requestUrl = $"{Endpoint}/api/chat";
                    var response = await client.PostAsync(requestUrl, content);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonDocument.Parse(jsonResponse);


                    string modelId = result.RootElement.GetProperty("model").GetString();
                    string responseMessage = result.RootElement
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
