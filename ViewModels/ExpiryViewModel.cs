using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SmartShopping.Data;
using SmartShopping.Models;
using SmartShopping.Services;
using System.Collections.ObjectModel;

namespace SmartShopping.ViewModels;

public partial class ExpiryViewModel : BaseViewModel
{
    private readonly AppDbContext _context;
    private readonly NotificationService _notificationService;

    [ObservableProperty]
    private ObservableCollection<ExpiryItemViewModel> expiringItems;

    [ObservableProperty]
    private bool isFiltering;

    [ObservableProperty]
    private bool isNotFilteringExpired;

    [ObservableProperty]
    private bool isNotFilteringExpiring;

    public ExpiryViewModel(AppDbContext context, NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
        Title = "Gestione Scadenze";
        ExpiringItems = new ObservableCollection<ExpiryItemViewModel>();
        IsNotFilteringExpired = true;
        IsNotFilteringExpiring = true;
    }

    public async Task LoadExpiringItemsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var items = await _context.InventoryItems
                .Include(i => i.Product)
                .Where(i => i.ExpiryDate.HasValue)
                .OrderBy(i => i.ExpiryDate)
                .ToListAsync();

            ExpiringItems.Clear();
            foreach (var item in items)
            {
                ExpiringItems.Add(new ExpiryItemViewModel(item));
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile caricare i prodotti", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task FilterExpiredAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var today = DateTime.Today;
            var items = await _context.InventoryItems
                .Include(i => i.Product)
                .Where(i => i.ExpiryDate.HasValue && i.ExpiryDate.Value < today)
                .OrderBy(i => i.ExpiryDate)
                .ToListAsync();

            ExpiringItems.Clear();
            foreach (var item in items)
            {
                ExpiringItems.Add(new ExpiryItemViewModel(item));
            }

            IsFiltering = true;
            IsNotFilteringExpired = false;
            IsNotFilteringExpiring = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile filtrare i prodotti", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task FilterExpiringAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var today = DateTime.Today;
            var threshold = today.AddDays(7); // Prodotti in scadenza nei prossimi 7 giorni
            var items = await _context.InventoryItems
                .Include(i => i.Product)
                .Where(i => i.ExpiryDate.HasValue && 
                           i.ExpiryDate.Value >= today && 
                           i.ExpiryDate.Value <= threshold)
                .OrderBy(i => i.ExpiryDate)
                .ToListAsync();

            ExpiringItems.Clear();
            foreach (var item in items)
            {
                ExpiringItems.Add(new ExpiryItemViewModel(item));
            }

            IsFiltering = true;
            IsNotFilteringExpired = true;
            IsNotFilteringExpiring = false;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile filtrare i prodotti", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ClearFilterAsync()
    {
        await LoadExpiringItemsAsync();
        IsFiltering = false;
        IsNotFilteringExpired = true;
        IsNotFilteringExpiring = true;
    }

    [RelayCommand]
    private async Task EditExpiryAsync(ExpiryItemViewModel item)
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var result = await Shell.Current.DisplayPromptAsync(
                "Modifica Scadenza",
                "Inserisci la nuova data di scadenza (gg/mm/aaaa):",
                initialValue: item.ExpiryDate?.ToString("dd/MM/yyyy") ?? string.Empty,
                keyboard: Keyboard.Numeric);

            if (DateTime.TryParse(result, out DateTime newDate))
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.Id == item.Id);

                if (inventoryItem != null)
                {
                    inventoryItem.ExpiryDate = newDate;
                    await _context.SaveChangesAsync();
                    await LoadExpiringItemsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile modificare la scadenza", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteItemAsync(ExpiryItemViewModel item)
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Conferma",
                "Vuoi eliminare questo prodotto dall'inventario?",
                "Sì",
                "No");

            if (confirm)
            {
                var inventoryItem = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.Id == item.Id);

                if (inventoryItem != null)
                {
                    _context.InventoryItems.Remove(inventoryItem);
                    await _context.SaveChangesAsync();
                    await LoadExpiringItemsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile eliminare il prodotto", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public class ExpiryItemViewModel
{
    public int Id { get; }
    public Product Product { get; }
    public DateTime? ExpiryDate { get; }
    public int DaysUntilExpiry { get; }
    public Color ExpiryColor { get; }

    public ExpiryItemViewModel(InventoryItem item)
    {
        Id = item.Id;
        Product = item.Product;
        ExpiryDate = item.ExpiryDate;

        if (ExpiryDate.HasValue)
        {
            var today = DateTime.Today;
            DaysUntilExpiry = (ExpiryDate.Value - today).Days;

            ExpiryColor = DaysUntilExpiry switch
            {
                < 0 => Colors.Red,
                <= 3 => Colors.Orange,
                _ => Colors.Green
            };
        }
        else
        {
            DaysUntilExpiry = 0;
            ExpiryColor = Colors.Gray;
        }
    }
} 