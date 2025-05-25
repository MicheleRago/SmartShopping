using SmartShopping.ViewModels;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace SmartShopping.Views;

public partial class ScannerPage : ContentPage
{
    private readonly ScannerViewModel _viewModel;

    public ScannerPage(ScannerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permesso negato", 
                        "L'app necessita del permesso della fotocamera per funzionare.", "OK");
                    return;
                }
            }

            _viewModel.StartScanningCommand.Execute(null);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Errore", 
                "Impossibile accedere alla fotocamera. Verifica i permessi dell'app.", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.StopScanningCommand.Execute(null);
    }

    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (e.Results.Any())
        {
            _viewModel.OnBarcodeDetected(e.Results.First());
        }
    }
} 