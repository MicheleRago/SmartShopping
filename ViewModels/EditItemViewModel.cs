using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SmartShopping.Data;
using SmartShopping.Models;

namespace SmartShopping.ViewModels;

[QueryProperty(nameof(Item), "Item")]
public partial class EditItemViewModel : BaseViewModel
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private InventoryItem item;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string brand;

    [ObservableProperty]
    private string category;

    [ObservableProperty]
    private string unit;

    [ObservableProperty]
    private decimal quantity;

    [ObservableProperty]
    private decimal minThreshold;

    [ObservableProperty]
    private DateTime? expiryDate;

    [ObservableProperty]
    private string location;

    [ObservableProperty]
    private bool isValid;

    public EditItemViewModel(AppDbContext context)
    {
        _context = context;
        Title = "Modifica Prodotto";
    }

    partial void OnItemChanged(InventoryItem value)
    {
        if (value?.Product == null) return;
        
        Name = value.Product.Name;
        Brand = value.Product.Brand;
        Category = value.Product.Category;
        Unit = value.Product.Unit;
        Quantity = value.CurrentQuantity;
        MinThreshold = value.MinThreshold;
        ExpiryDate = value.ExpiryDate;
        Location = value.Location;

        ValidateForm();
    }

    private void ValidateForm()
    {
        IsValid = !string.IsNullOrWhiteSpace(Name) && 
                 !string.IsNullOrWhiteSpace(Brand) &&
                 Quantity > 0;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Barcode == Item.Barcode);

            if (product != null)
            {
                product.Name = Name;
                product.Brand = Brand;
                product.Category = Category;
                product.Unit = Unit;
            }

            Item.CurrentQuantity = Quantity;
            Item.MinThreshold = MinThreshold;
            Item.ExpiryDate = ExpiryDate;
            Item.Location = Location;
            Item.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile salvare le modifiche", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
} 