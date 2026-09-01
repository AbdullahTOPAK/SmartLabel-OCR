namespace GeminiLabelApi.Models
{
    public class ProductLabel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ExtractedText { get; set; } = string.Empty;
        public System.DateTime CreatedAt { get; set; } = System.DateTime.Now;
    }
}