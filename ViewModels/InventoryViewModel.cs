using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SmartShopping.Data;
using SmartShopping.Models;

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
        inventoryItems = new ObservableCollection<InventoryItem>();

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

            InventoryItems.Clear();
            foreach (var item in items)
            {
                InventoryItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", $"Impossibile caricare l'inventario: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "OK");
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

            InventoryItems.Clear();
            foreach (var item in items)
            {
                InventoryItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile cercare nell'inventario", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        await Shell.Current.GoToAsync("//ScannerPage");
    }

    [RelayCommand]
    private async Task EditItemAsync(InventoryItem item)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Item", item }
        };
        await Shell.Current.GoToAsync("//EditItemPage", parameters);
    }

    [RelayCommand]
    private async Task DeleteItemAsync(InventoryItem item)
    {
        var confirm = await Shell.Current.DisplayAlert(
            "Conferma",
            $"Sei sicuro di voler eliminare {item.Product?.Name}?",
            "Sì",
            "No");

        if (confirm)
        {
            try
            {
                _context.InventoryItems.Remove(item);
                await _context.SaveChangesAsync();
                InventoryItems.Remove(item);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Errore", "Impossibile eliminare l'elemento", "OK");
            }
        }
    }
} 