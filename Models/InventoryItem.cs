namespace SmartShopping.Models;

public class InventoryItem
{
    public int Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public decimal CurrentQuantity { get; set; }
    public decimal MinThreshold { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Location { get; set; } = string.Empty; // Frigo, Dispensa, etc.
    
    public virtual Product? Product { get; set; }
} 