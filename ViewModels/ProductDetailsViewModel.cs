using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using SmartShopping.Data;
using SmartShopping.Models;
using System.Diagnostics;

namespace SmartShopping.ViewModels;

[QueryProperty(nameof(Barcode), "barcode")]
public partial class ProductDetailsViewModel : BaseViewModel
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
    private decimal quantity = 1;

    [ObservableProperty]
    private decimal minThreshold = 1;

    [ObservableProperty]
    private DateTime? expiryDate;

    [ObservableProperty]
    private string location = string.Empty;

    [ObservableProperty]
    private bool isValid;

    public ProductDetailsViewModel(AppDbContext context)
    {
        _context = context;
        Title = "Dettagli Prodotto";
    }

    partial void OnBarcodeChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            MainThread.BeginInvokeOnMainThread(async () => await LoadProductDetailsAsync());
        }
    }

    private async Task LoadProductDetailsAsync()
    {
        if (string.IsNullOrEmpty(Barcode))
        {
            Debug.WriteLine("LoadProductDetailsAsync: Barcode is null or empty");
            return;
        }

        if (IsBusy)
        {
            Debug.WriteLine("LoadProductDetailsAsync: Already busy");
            return;
        }

        IsBusy = true;

        try
        {
            Debug.WriteLine($"LoadProductDetailsAsync: Loading product details for barcode {Barcode}");
            
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Barcode == Barcode);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (product != null)
                {
                    Name = product.Name;
                    Brand = product.Brand;
                    Category = product.Category;
                    Unit = product.Unit;
                    Debug.WriteLine($"LoadProductDetailsAsync: Product found - {Name}");
                }
                else
                {
                    Debug.WriteLine("LoadProductDetailsAsync: No product found");
                }
            });

            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Barcode == Barcode);

            if (inventoryItem != null)
            {
                Quantity = inventoryItem.CurrentQuantity;
                MinThreshold = inventoryItem.MinThreshold;
                ExpiryDate = inventoryItem.ExpiryDate;
                Location = inventoryItem.Location;
            }

            ValidateForm();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"LoadProductDetailsAsync Exception: {ex.Message}");
            Debug.WriteLine($"LoadProductDetailsAsync StackTrace: {ex.StackTrace}");
            
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlert("Errore", 
                    "Impossibile caricare i dettagli del prodotto. Riprova più tardi.", "OK"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ValidateForm()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsValid = !string.IsNullOrWhiteSpace(Name) && 
                     !string.IsNullOrWhiteSpace(Brand) &&
                     !string.IsNullOrWhiteSpace(Barcode) &&
                     Quantity > 0;
            
            Debug.WriteLine($"ValidateForm: Form is {(IsValid ? "valid" : "invalid")}");
        });
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            Debug.WriteLine("SaveAsync: Already busy");
            return;
        }

        if (!IsValid)
        {
            Debug.WriteLine("SaveAsync: Form is not valid");
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlert("Errore", 
                    "Verifica di aver compilato tutti i campi obbligatori.", "OK"));
            return;
        }

        IsBusy = true;

        try
        {
            Debug.WriteLine($"SaveAsync: Saving product with barcode {Barcode}");
            
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Barcode == Barcode);

            if (product == null)
            {
                product = new Product
                {
                    Barcode = Barcode
                };
                _context.Products.Add(product);
                Debug.WriteLine("SaveAsync: Created new product");
            }

            product.Name = Name;
            product.Brand = Brand;
            product.Category = Category;
            product.Unit = Unit;

            // Crea sempre un nuovo InventoryItem quando si aggiunge una nuova unità
            var inventoryItem = new InventoryItem
            {
                Barcode = Barcode,
                CurrentQuantity = Quantity,
                MinThreshold = MinThreshold,
                ExpiryDate = ExpiryDate,
                Location = Location,
                PurchaseDate = DateTime.Now,
                LastUpdated = DateTime.Now
            };
            _context.InventoryItems.Add(inventoryItem);
            Debug.WriteLine("SaveAsync: Created new inventory item");

            await _context.SaveChangesAsync();
            Debug.WriteLine("SaveAsync: Changes saved successfully");
            
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.GoToAsync(".."));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SaveAsync Exception: {ex.Message}");
            Debug.WriteLine($"SaveAsync StackTrace: {ex.StackTrace}");
            
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlert("Errore", 
                    "Impossibile salvare le modifiche. Verifica la connessione al database.", "OK"));
        }
        finally
        {
            IsBusy = false;
        }
    }
} 