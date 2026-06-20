using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace FFXIVVenues.Veni.AI.Davinci
{
    internal class DavinciService : IDavinciService
    {
        private readonly HttpClient _httpClient;
        
        public DavinciService( DavinciConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", configuration.ApiKey);
        }

        public async Task<String> AskTheAI(string prompt)
        {
           
            var requestPayload = new
                {
                    model = "gpt-4-turbo",
                    messages = new[]
                    {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = prompt }
                }
                };

            var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

                if (result is not null)
                    return result.choices[0].message.content;   
                
                Log.Error("Could not parse response from GPT.");
            }
            throw new Exception($"Request failed with status code {response.StatusCode}");
        }
    }
}
