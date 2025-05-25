using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SmartShopping.Data;
using SmartShopping.Models;

namespace SmartShopping.ViewModels;

public partial class ShoppingListViewModel : BaseViewModel
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private ObservableCollection<ShoppingListItem> shoppingItems;

    [ObservableProperty]
    private decimal totalEstimatedCost;

    public ShoppingListViewModel(AppDbContext context)
    {
        _context = context;
        Title = "Lista della Spesa";
        shoppingItems = new ObservableCollection<ShoppingListItem>();
    }

    [RelayCommand]
    private async Task LoadShoppingListAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var items = await _context.ShoppingListItems
                .Include(s => s.Product)
                .Where(s => !s.IsCompleted)
                .OrderBy(s => s.Priority)
                .ThenBy(s => s.Product.Name)
                .ToListAsync();

            ShoppingItems.Clear();
            foreach (var item in items)
            {
                ShoppingItems.Add(item);
            }

            UpdateTotalCost();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile caricare la lista della spesa", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GenerateShoppingListAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            // Trova gli elementi dell'inventario sotto la soglia minima
            var lowStockItems = await _context.InventoryItems
                .Include(i => i.Product)
                .Where(i => i.CurrentQuantity <= i.MinThreshold)
                .ToListAsync();

            foreach (var item in lowStockItems)
            {
                var existingItem = await _context.ShoppingListItems
                    .FirstOrDefaultAsync(s => s.Barcode == item.Barcode && !s.IsCompleted);

                if (existingItem == null)
                {
                    var shoppingItem = new ShoppingListItem
                    {
                        Barcode = item.Barcode,
                        QuantityNeeded = item.MinThreshold - item.CurrentQuantity,
                        Priority = Priority.Normal,
                        CreatedDate = DateTime.Now
                    };
                    _context.ShoppingListItems.Add(shoppingItem);
                }
            }

            await _context.SaveChangesAsync();
            await LoadShoppingListAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile generare la lista della spesa", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleItemCompletionAsync(ShoppingListItem item)
    {
        try
        {
            item.IsCompleted = !item.IsCompleted;
            await _context.SaveChangesAsync();
            
            if (item.IsCompleted)
            {
                ShoppingItems.Remove(item);
            }
            
            UpdateTotalCost();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile aggiornare lo stato dell'elemento", "OK");
        }
    }

    [RelayCommand]
    private async Task ShareShoppingListAsync()
    {
        if (ShoppingItems.Count == 0)
        {
            await Shell.Current.DisplayAlert("Attenzione", "La lista della spesa è vuota", "OK");
            return;
        }

        var listText = "Lista della Spesa:\n\n";
        foreach (var item in ShoppingItems)
        {
            listText += $"- {item.Product?.Name}: {item.QuantityNeeded} {item.Product?.Unit}\n";
        }
        listText += $"\nTotale stimato: €{TotalEstimatedCost:F2}";

        await Share.RequestAsync(new ShareTextRequest
        {
            Text = listText,
            Title = "Condividi Lista della Spesa"
        });
    }

    private void UpdateTotalCost()
    {
        TotalEstimatedCost = ShoppingItems.Sum(item => item.EstimatedPrice);
    }
} 