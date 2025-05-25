namespace SmartShopping.Models;

public enum Priority
{
    Urgent,
    Normal,
    WhenPossible
}

public class ShoppingListItem
{
    public int Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public decimal QuantityNeeded { get; set; }
    public Priority Priority { get; set; }
    public decimal EstimatedPrice { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public virtual Product? Product { get; set; }
} 