using SmartShopping.ViewModels;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.StartScanningCommand.Execute(null);
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