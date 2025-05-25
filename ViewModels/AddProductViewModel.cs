using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SmartShopping.Data;
using SmartShopping.Models;

namespace SmartShopping.ViewModels;

public partial class AddProductViewModel : BaseViewModel
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private string barcode = string.Empty;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string brand = string.Empty;

    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private string unit = string.Empty;

    [ObservableProperty]
    private string imageUrl = string.Empty;

    [ObservableProperty]
    private bool isValid;

    public AddProductViewModel(AppDbContext context)
    {
        _context = context;
        Title = "Aggiungi Prodotto";
    }

    partial void OnNameChanged(string value)
    {
        ValidateForm();
    }

    partial void OnBrandChanged(string value)
    {
        ValidateForm();
    }

    private void ValidateForm()
    {
        IsValid = !string.IsNullOrWhiteSpace(Name) && 
                 !string.IsNullOrWhiteSpace(Brand) &&
                 !string.IsNullOrWhiteSpace(Barcode);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var product = new Product
            {
                Barcode = Barcode,
                Name = Name,
                Brand = Brand,
                Category = Category,
                Unit = Unit,
                ImageUrl = ImageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Aggiungi automaticamente all'inventario
            var inventoryItem = new InventoryItem
            {
                Barcode = Barcode,
                CurrentQuantity = 1,
                MinThreshold = 1,
                PurchaseDate = DateTime.Now,
                LastUpdated = DateTime.Now
            };
            _context.InventoryItems.Add(inventoryItem);
            await _context.SaveChangesAsync();

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile salvare il prodotto", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
} 