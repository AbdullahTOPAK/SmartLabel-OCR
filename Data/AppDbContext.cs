using Microsoft.EntityFrameworkCore;
using GeminiLabelApi.Models;

namespace GeminiLabelApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<ProductLabel> ProductLabels { get; set; }
    }
}