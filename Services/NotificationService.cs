using SmartShopping.Models;
using SmartShopping.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartShopping.Services;

public class NotificationService
{
    private readonly AppDbContext _context;
    private const int DAYS_BEFORE_EXPIRY = 3;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task CheckExpiringItemsAsync()
    {
        var today = DateTime.Today;
        var expiryThreshold = today.AddDays(DAYS_BEFORE_EXPIRY);

        var expiringItems = await _context.InventoryItems
            .Include(i => i.Product)
            .Where(i => i.ExpiryDate.HasValue && 
                       i.ExpiryDate.Value <= expiryThreshold && 
                       i.ExpiryDate.Value >= today)
            .ToListAsync();

        foreach (var item in expiringItems)
        {
            var daysUntilExpiry = (item.ExpiryDate!.Value - today).Days;
            var message = daysUntilExpiry == 0
                ? $"Il prodotto {item.Product.Name} scade oggi!"
                : $"Il prodotto {item.Product.Name} scade tra {daysUntilExpiry} giorni";

            await ShowNotificationAsync("Scadenza Prossima", message);
        }
    }

    private async Task ShowNotificationAsync(string title, string message)
    {
        // Implementazione base delle notifiche
        await Application.Current.MainPage.DisplayAlert(title, message, "OK");
    }
} 