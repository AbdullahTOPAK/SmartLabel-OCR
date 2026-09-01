using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeminiLabelApi.Data;
using GeminiLabelApi.Models;
using GeminiLabelApi.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GeminiLabelApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabelController : ControllerBase
    {
        private readonly GeminiOcrService _geminiOcrService;
        private readonly AppDbContext _context;

        public LabelController(GeminiOcrService geminiOcrService, AppDbContext context)
        {
            _geminiOcrService = geminiOcrService;
            _context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllLabels()
        {
            var labels = await _context.ProductLabels.OrderByDescending(x => x.Id).ToListAsync();
            return Ok(labels);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadLabel([FromForm] IFormFile file, [FromForm] string? customPrompt)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Geçersiz dosya." });

            try
            {
                // 1. Klasör Yoksa Oluştur
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Orijinal dosya adını alma
                string originalFileName = System.IO.Path.GetFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, originalFileName);

                // 2. Dosyayı Doğrudan Kaydet (Aynı isimde varsa üzerine yazar)
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 3. Gemini Yapay Zekaya Gönderme
                byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                string extractedText = await _geminiOcrService.ExtractTextAsync(
                    imageBytes, 
                    file.ContentType, 
                    customPrompt
                );

                // 4. Veritabanına Kaydetme / Güncelleme
                if (!extractedText.StartsWith("API_HATASI") && extractedText != "JSON_PARSE_HATASI")
                {
                    var existingEntity = await _context.ProductLabels.FirstOrDefaultAsync(x => x.ImageUrl == originalFileName);

                    if (existingEntity != null)
                    {
                        // Aynı isimde kayıt varsa içeriğini ve tarihini güncelle
                        existingEntity.ExtractedText = extractedText;
                        existingEntity.CreatedAt = DateTime.Now;
                        _context.ProductLabels.Update(existingEntity);
                    }
                    else
                    {
                        // Yoksa yeni kayıt ekle
                        var labelEntity = new ProductLabel
                        {
                            ImageUrl = originalFileName,
                            ExtractedText = extractedText,
                            CreatedAt = DateTime.Now
                        };
                        _context.ProductLabels.Add(labelEntity);
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(new { extractedText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "İşlem hatası: " + ex.Message });
            }
        }

        // --- YERİNDE YENİDEN İŞLEME (IN-PLACE REPROCESS) ---
        [HttpPut("reprocess/{id:int}")]
        public async Task<IActionResult> ReprocessLabel([FromRoute] int id, [FromForm] string? customPrompt)
        {
            var existingLabel = await _context.ProductLabels.FindAsync(id);
            if (existingLabel == null)
                return NotFound(new { message = "Güncellenecek kayıt bulunamadı." });

            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", existingLabel.ImageUrl);
                
                if (!System.IO.File.Exists(filePath))
                {
                    return BadRequest(new { message = "Orijinal görsel dosyası sunucuda bulunamadı." });
                }

                byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                
                string newExtractedText = await _geminiOcrService.ExtractTextAsync(
                    imageBytes, 
                    "image/jpeg", 
                    customPrompt
                );

                existingLabel.ExtractedText = newExtractedText;
                existingLabel.CreatedAt = DateTime.Now;

                _context.ProductLabels.Update(existingLabel);
                await _context.SaveChangesAsync();

                return Ok(new { extractedText = newExtractedText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Yeniden işleme hatası: " + ex.Message });
            }
        }

        // DELETE: api/Label/deleteAll
        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteAllLabels()
        {
            try
            {
                var allLabels = await _context.ProductLabels.ToListAsync();
                if (!allLabels.Any())
                {
                    return BadRequest(new { message = "Silinecek kayıt bulunamadı." });
                }

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (Directory.Exists(uploadsFolder))
                {
                    foreach (var label in allLabels)
                    {
                        string filePath = Path.Combine(uploadsFolder, label.ImageUrl);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }

                _context.ProductLabels.RemoveRange(allLabels);
                await _context.SaveChangesAsync();

                try 
                {
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM sqlite_sequence WHERE name='ProductLabels'");
                }
                catch 
                {
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM sqlite_sequence WHERE name='ProductLabel'");
                }

                return Ok(new { message = "Tüm arşiv başarıyla temizlendi ve ID sayacı 1'e sıfırlandı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Toplu silme hatası: " + ex.Message });
            }
        }

        // DELETE: api/Label/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteLabel([FromRoute] int id)
        {
            var label = await _context.ProductLabels.FindAsync(id);
            if (label == null)
                return NotFound(new { message = "Kayıt bulunamadı." });

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", label.ImageUrl);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.ProductLabels.Remove(label);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kayıt ve görsel başarıyla silindi." });
        }
    }
}