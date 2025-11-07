using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using PromptRunner.AI.Models;

namespace PromptRunner.AI.Clients
{
    public class AnthropicClient : BaseApiClient
    {
        private readonly string _apiVersion;

        public AnthropicClient(string apiKey, string endpoint, string model, string apiVersion)
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
                    client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
                    client.DefaultRequestHeaders.Add("anthropic-version", _apiVersion);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var _messages = messages
                        .Where(msg => msg.Role != "system")
                        .Select(msg => new
                        {
                            role = msg.Role,
                            content = msg.Content
                        }).ToArray();

                    var systemMessage = messages
                        .FirstOrDefault(msg => msg.Role == "system")?.Content;

                    var requestBody = new
                    {
                        model = Model,
                        max_tokens = options.MaxOutputTokens,
                        messages = _messages,
                        system = systemMessage,
                        temperature = options.Temperature
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{Endpoint}/v1/messages", content);
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(responseBody);

                    string modelId = result.GetProperty("model").GetString();
                    string responseMessage = result.GetProperty("content")
                        .EnumerateArray()
                        .First(x => x.GetProperty("type").GetString() == "text")
                        .GetProperty("text").GetString();

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

    public class AnthropicResponse
    {
        public string ModelID { get; set; }
        public ChatMessage Message { get; set; }
        // Add properties as needed to match the Anthropic API response format
    }
}