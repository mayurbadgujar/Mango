using Mango.Services.ProductApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ProductApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().HasData(new Product
            {
                ProductId = 1,
                Name="Samosa",
                Price=15,
                Description="Spicy",
                ImageUrl= "https://www.licious.in/blog/wp-content/uploads/2022/08/Shutterstock_1508653862.jpg",
                CategoryName="Appetizer"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                ProductId = 2,
                Name = "Paneer Tikka",
                Price = 199,
                Description = "Spicy",
                ImageUrl = "https://myfoodstory.com/wp-content/uploads/2017/02/Oven-Baked-Tandoori-Paneer-Tikka-2.jpg",
                CategoryName = "Appetizer"
            });
        }
    }
}
