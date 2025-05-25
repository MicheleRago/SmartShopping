using Microsoft.EntityFrameworkCore;
using SmartShopping.Models;

namespace SmartShopping.Data;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<ShoppingListItem> ShoppingListItems { get; set; }
    public DbSet<AppSettings> AppSettings { get; set; }
    public DbSet<ExportData> ExportData { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .HasKey(p => p.Barcode);

        modelBuilder.Entity<InventoryItem>()
            .HasOne(i => i.Product)
            .WithMany()
            .HasForeignKey(i => i.Barcode);

        modelBuilder.Entity<ShoppingListItem>()
            .HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.Barcode);

        modelBuilder.Entity<AppSettings>()
            .HasData(new AppSettings
            {
                Id = 1,
                OpenFoodFactsApiUrl = "https://world.openfoodfacts.org/api/v2",
                Theme = Models.Theme.Auto,
                Language = "it-IT",
                ExportFormat = ExportFormat.JSON
            });
    }
} 