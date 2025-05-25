using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.ApplicationModel;
using SmartShopping.Data;
using SmartShopping.Models;
using SmartShopping.Services;
using SmartShopping.Views;
using ZXing.Net.Maui;

namespace SmartShopping.ViewModels;

public partial class ScannerViewModel : BaseViewModel
{
    private readonly AppDbContext _context;
    private readonly OpenFoodFactsService _openFoodFactsService;

    [ObservableProperty]
    private bool isScanning;

    [ObservableProperty]
    private string scanResult;

    public ScannerViewModel(AppDbContext context, OpenFoodFactsService openFoodFactsService)
    {
        _context = context;
        _openFoodFactsService = openFoodFactsService;
        Title = "Scanner";
    }

    [RelayCommand]
    public void OnBarcodeDetected(BarcodeResult result)
    {
        if (result == null || string.IsNullOrEmpty(result.Value)) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ScanResult = result.Value;
            IsScanning = false;
        });
        
        // Processa il barcode
        ProcessBarcode(result.Value);
    }

    private async void ProcessBarcode(string barcode)
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            // Verifica se il prodotto esiste già
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Barcode == barcode);

            if (product == null)
            {
                // Prova a recuperare il prodotto da OpenFoodFacts
                product = await _openFoodFactsService.GetProductByBarcodeAsync(barcode);

                if (product == null)
                {
                    var addManually = await MainThread.InvokeOnMainThreadAsync(async () =>
                        await Shell.Current.DisplayAlert(
                            "Prodotto non trovato",
                            "Il prodotto non è presente nel database. Vuoi aggiungerlo manualmente?",
                            "Sì",
                            "No"));

                    if (addManually)
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                            await Shell.Current.GoToAsync($"{nameof(AddProductPage)}?barcode={barcode}"));
                    }
                    return;
                }

                // Salva il nuovo prodotto nel database
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }

            // Chiedi all'utente se vuole aggiungere una nuova unità o modificare una esistente
            var existingItems = await _context.InventoryItems
                .Include(i => i.Product)
                .Where(i => i.Barcode == barcode)
                .OrderByDescending(i => i.ExpiryDate)
                .ToListAsync();

            if (existingItems.Any())
            {
                var action = await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.DisplayActionSheet(
                        "Prodotto già presente",
                        "Annulla",
                        null,
                        "Aggiungi nuova unità",
                        "Modifica unità esistente"));

                if (action == "Modifica unità esistente")
                {
                    // Se c'è più di un'unità, mostra un elenco per scegliere quale modificare
                    if (existingItems.Count > 1)
                    {
                        var items = existingItems.Select(i => 
                            $"{i.Product.Name} - Scadenza: {(i.ExpiryDate?.ToString("d") ?? "Non specificata")} - Quantità: {i.CurrentQuantity}").ToArray();
                        
                        var selectedItem = await MainThread.InvokeOnMainThreadAsync(async () =>
                            await Shell.Current.DisplayActionSheet(
                                "Seleziona l'unità da modificare",
                                "Annulla",
                                null,
                                items));

                        if (selectedItem != "Annulla" && selectedItem != null)
                        {
                            var index = Array.IndexOf(items, selectedItem);
                            if (index >= 0)
                            {
                                var parameters = new Dictionary<string, object>
                                {
                                    { "Item", existingItems[index] }
                                };
                                await MainThread.InvokeOnMainThreadAsync(async () =>
                                    await Shell.Current.GoToAsync($"{nameof(EditItemPage)}", parameters));
                            }
                        }
                    }
                    else
                    {
                        var parameters = new Dictionary<string, object>
                        {
                            { "Item", existingItems[0] }
                        };
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                            await Shell.Current.GoToAsync($"{nameof(EditItemPage)}", parameters));
                    }
                }
                else if (action == "Aggiungi nuova unità")
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "barcode", barcode }
                    };
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                        await Shell.Current.GoToAsync($"{nameof(ProductDetailsPage)}", parameters));
                }
            }
            else
            {
                // Se non ci sono unità esistenti, vai direttamente alla pagina dei dettagli
                var parameters = new Dictionary<string, object>
                {
                    { "barcode", barcode }
                };
                await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Shell.Current.GoToAsync($"{nameof(ProductDetailsPage)}", parameters));
            }
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlert("Errore", "Impossibile processare il barcode", "OK"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void StartScanning()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsScanning = true;
            ScanResult = string.Empty;
        });
    }

    [RelayCommand]
    private void StopScanning()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsScanning = false;
        });
    }
} 