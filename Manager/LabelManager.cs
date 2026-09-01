using System.Threading.Tasks;
using GeminiLabelApi.Data;
using GeminiLabelApi.Models;
using GeminiLabelApi.Services;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace GeminiLabelApi.Manager
{
    public class LabelManager
    {
        private readonly AppDbContext _context;
        private readonly GeminiOcrService _geminiService;

        public LabelManager(AppDbContext context, GeminiOcrService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }
        public async Task<ProductLabel> ProcessUploadedFileAsync(IFormFile file)
{
    // 1. Gelen fiziksel dosyayı byte dizisine çevir
    using var memoryStream = new MemoryStream();
    await file.CopyToAsync(memoryStream);
    var imageBytes = memoryStream.ToArray();

    // 2. Gemini servisine gönderip okut (MIME türünü dosyadan alıyoruz)
    var extractedText = await _geminiService.ExtractTextAsync(imageBytes, file.ContentType);

    // 3. Veritabanı için modeli hazırla
    var label = new ProductLabel
    {
        ImageUrl = "Yerel_Yukleme_" + file.FileName, // Link olmadığı için dosya adını kaydediyoruz
        ExtractedText = extractedText,
        CreatedAt = DateTime.UtcNow
    };

    // 4. Veritabanına kaydet
    _context.ProductLabels.Add(label);
    await _context.SaveChangesAsync();

    return label;
}

        public async Task<ProductLabel> ProcessAndSaveLabelAsync(string imageUrl)
        {
            using var client = new HttpClient();
            byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
            
            string mimeType = imageUrl.ToLower().Contains(".png") ? "image/png" : "image/jpeg";
            string text = await _geminiService.ExtractTextAsync(imageBytes, mimeType);

            var label = new ProductLabel { ImageUrl = imageUrl, ExtractedText = text };
            _context.ProductLabels.Add(label);
            await _context.SaveChangesAsync();

            return label;
        }
    }
}