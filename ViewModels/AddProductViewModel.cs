using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SmartShopping.Data;
using SmartShopping.Models;
using System.Diagnostics;

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

    partial void OnBarcodeChanged(string value)
    {
        ValidateForm();
    }

    private void ValidateForm()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsValid = !string.IsNullOrWhiteSpace(Name) && 
                     !string.IsNullOrWhiteSpace(Brand) &&
                     !string.IsNullOrWhiteSpace(Barcode);
            
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
            Debug.WriteLine($"SaveAsync: Starting transaction for barcode {Barcode}");
            
            // Verifica se il prodotto esiste già
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Barcode == Barcode);

            if (existingProduct != null)
            {
                Debug.WriteLine("SaveAsync: Product already exists");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayAlert("Errore", 
                        "Questo prodotto esiste già nel database.", "OK"));
                return;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
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
                Debug.WriteLine("SaveAsync: Added product to context");

                var inventoryItem = new InventoryItem
                {
                    Barcode = Barcode,
                    CurrentQuantity = 1,
                    MinThreshold = 1,
                    PurchaseDate = DateTime.Now,
                    LastUpdated = DateTime.Now
                };
                _context.InventoryItems.Add(inventoryItem);
                Debug.WriteLine("SaveAsync: Added inventory item to context");

                await _context.SaveChangesAsync();
                Debug.WriteLine("SaveAsync: Changes saved successfully");

                await transaction.CommitAsync();
                Debug.WriteLine("SaveAsync: Transaction committed");

                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.GoToAsync(".."));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SaveAsync Transaction Exception: {ex.Message}");
                Debug.WriteLine($"SaveAsync Transaction StackTrace: {ex.StackTrace}");
                await transaction.RollbackAsync();
                throw; // Rilanciamo l'eccezione per gestirla nel catch esterno
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SaveAsync Exception: {ex.Message}");
            Debug.WriteLine($"SaveAsync StackTrace: {ex.StackTrace}");
            
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlert("Errore", 
                    "Impossibile salvare il prodotto. Verifica la connessione al database.", "OK"));
        }
        finally
        {
            IsBusy = false;
        }
    }
} 