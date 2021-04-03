using Lms.Stock.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Lms.Stock.EntityFrameworkCore
{
    public class StockDbContext: DbContext
    {
        public StockDbContext(DbContextOptions<StockDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Product> Products { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>();
        }
    }
}