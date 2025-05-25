using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SmartShopping.Data;
using SmartShopping.Models;
using SmartShopping.Services;
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

        ScanResult = result.Value;
        IsScanning = false;
        
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
                    var addManually = await Shell.Current.DisplayAlert(
                        "Prodotto non trovato",
                        "Il prodotto non è presente nel database. Vuoi aggiungerlo manualmente?",
                        "Sì",
                        "No");

                    if (addManually)
                    {
                        await Shell.Current.GoToAsync($"//AddProductPage?barcode={barcode}");
                    }
                    return;
                }

                // Salva il nuovo prodotto nel database
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }

            // Verifica se il prodotto è già nell'inventario
            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Barcode == barcode);

            if (inventoryItem == null)
            {
                // Aggiungi all'inventario
                inventoryItem = new InventoryItem
                {
                    Barcode = barcode,
                    CurrentQuantity = 1,
                    MinThreshold = 1,
                    PurchaseDate = DateTime.Now,
                    LastUpdated = DateTime.Now
                };
                _context.InventoryItems.Add(inventoryItem);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Incrementa la quantità
                inventoryItem.CurrentQuantity++;
                inventoryItem.LastUpdated = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile processare il barcode", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void StartScanning()
    {
        IsScanning = true;
        ScanResult = string.Empty;
    }

    [RelayCommand]
    private void StopScanning()
    {
        IsScanning = false;
    }
} 