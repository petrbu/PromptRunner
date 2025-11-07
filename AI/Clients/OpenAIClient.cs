using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PromptRunner.AI.Models;
using System.Linq;
using System.Net.Http;

namespace PromptRunner.AI.Clients
{
    public class OpenAIClient : BaseApiClient
    {
        public OpenAIClient(string apiKey, string endpoint, string model) 
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
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var _messages = messages.Select(msg => new
                    {
                        role = msg.Role,
                        content = msg.Content
                    }).ToArray();

                    var requestBody = new
                    {
                        model = Model,
                        messages = _messages,
                        max_tokens = options.MaxOutputTokens,
                        temperature = options.Temperature,
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var requestUrl = $"{Endpoint}/v1/chat/completions";
                    var response = await client.PostAsync(requestUrl, content);
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