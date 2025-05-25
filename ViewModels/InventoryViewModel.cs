using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using SmartShopping.Data;
using SmartShopping.Models;
using SmartShopping.Views;

namespace SmartShopping.ViewModels;

public partial class InventoryViewModel : BaseViewModel
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private ObservableCollection<InventoryItem> inventoryItems;

    [ObservableProperty]
    private string searchText = string.Empty;

    public InventoryViewModel(AppDbContext context)
    {
        _context = context;
        Title = "Inventario";
        InventoryItems = new ObservableCollection<InventoryItem>();

        try
        {
            // Verifica se il database esiste
            var dbExists = File.Exists(_context.Database.GetDbConnection().DataSource);
            System.Diagnostics.Debug.WriteLine($"Database exists: {dbExists}");
            System.Diagnostics.Debug.WriteLine($"Database path: {_context.Database.GetDbConnection().DataSource}");

            // Verifica se le tabelle esistono
            var tables = _context.Database.SqlQueryRaw<string>("SELECT name FROM sqlite_master WHERE type='table'").ToList();
            System.Diagnostics.Debug.WriteLine("Tables in database:");
            foreach (var table in tables)
            {
                System.Diagnostics.Debug.WriteLine($"- {table}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking database: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    [RelayCommand]
    private async Task LoadInventoryAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var items = await _context.InventoryItems
                .Include(i => i.Product)
                .OrderBy(i => i.Product.Name)
                .ToListAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                InventoryItems.Clear();
                foreach (var item in items)
                {
                    if (item.Product != null)
                    {
                        InventoryItems.Add(item);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlert("Errore", $"Impossibile caricare l'inventario: {ex.Message}", "OK"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SearchInventoryAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var items = await _context.InventoryItems
                .Include(i => i.Product)
                .Where(i => string.IsNullOrEmpty(SearchText) || 
                           i.Product.Name.Contains(SearchText) || 
                           i.Product.Brand.Contains(SearchText))
                .OrderBy(i => i.Product.Name)
                .ToListAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                InventoryItems.Clear();
                foreach (var item in items)
                {
                    if (item.Product != null)
                    {
                        InventoryItems.Add(item);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlert("Errore", "Impossibile cercare nell'inventario", "OK"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.GoToAsync("//ScannerPage"));
    }

    [RelayCommand]
    private async Task EditItemAsync(InventoryItem item)
    {
        if (item?.Product == null) return;

        var parameters = new Dictionary<string, object>
        {
            { "Item", item }
        };
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.GoToAsync($"{nameof(EditItemPage)}", parameters));
    }

    [RelayCommand]
    private async Task DeleteItemAsync(InventoryItem item)
    {
        try 
        {
            if (item == null)
            {
                System.Diagnostics.Debug.WriteLine("DeleteItemAsync: item is null");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"DeleteItemAsync: item.Id = {item.Id}");
            System.Diagnostics.Debug.WriteLine($"DeleteItemAsync: item.Product = {(item.Product == null ? "null" : "not null")}");

            // Ricarica l'item con il prodotto associato per essere sicuri di avere tutti i dati
            var itemWithProduct = await _context.InventoryItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == item.Id);

            if (itemWithProduct == null)
            {
                System.Diagnostics.Debug.WriteLine("DeleteItemAsync: itemWithProduct not found in database");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayAlert("Errore", "Elemento non trovato", "OK"));
                return;
            }

            if (itemWithProduct.Product == null)
            {
                System.Diagnostics.Debug.WriteLine("DeleteItemAsync: itemWithProduct.Product is null");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayAlert("Errore", "Prodotto non trovato", "OK"));
                return;
            }

            var productName = itemWithProduct.Product.Name;
            System.Diagnostics.Debug.WriteLine($"DeleteItemAsync: productName = {productName}");
            
            var confirm = await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlert(
                    "Conferma",
                    $"Sei sicuro di voler eliminare {productName}?",
                    "Sì",
                    "No"));

            if (confirm)
            {
                _context.InventoryItems.Remove(itemWithProduct);
                await _context.SaveChangesAsync();
                
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // Rimuovi l'item dalla collezione osservabile
                    var itemToRemove = InventoryItems.FirstOrDefault(i => i.Id == item.Id);
                    if (itemToRemove != null)
                    {
                        InventoryItems.Remove(itemToRemove);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DeleteItemAsync error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlert("Errore", "Impossibile eliminare l'elemento", "OK"));
        }
    }
} 