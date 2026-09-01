using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GeminiLabelApi.Services
{
    public class GeminiOcrService
    {
        private readonly string _apiKey;

        // Constructor üzerinden IConfiguration ile appsettings.json'dan API Key'i çekiyoruz
        public GeminiOcrService(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"] 
                ?? throw new ArgumentNullException("Gemini:ApiKey appsettings.json dosyasında bulunamadı!");
        }

        public async Task<string> ExtractTextAsync(byte[] imageBytes, string mimeType, string? customPrompt = null)
        {
            using var client = new HttpClient();
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
            string base64Image = Convert.ToBase64String(imageBytes);

            // Kullanıcı özel prompt girmişse onu kullan, boşsa varsayılan sert talimatı çalıştır
            string promptToUse = string.IsNullOrWhiteSpace(customPrompt)
                ? "Extract all readable text from this label image accurately. Maintain line breaks and text order. Output ONLY the extracted raw text without any formatting, markdown, commentary, or explanation."
                : customPrompt;

            var requestBody = new
            {
                contents = new[] { 
                    new { 
                        parts = new object[] {
                            new { text = promptToUse },
                            new { inline_data = new { mime_type = mimeType, data = base64Image } }
                        }
                    } 
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) return $"API_HATASI: {responseString}";

            try 
            {
                using var doc = JsonDocument.Parse(responseString);
                var textResult = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                return textResult?.Trim() ?? "BOS_VERI";
            } 
            catch 
            {
                return "JSON_PARSE_HATASI";
            }
        }
    }
}