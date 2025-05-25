using SmartShopping.ViewModels;
using Microsoft.Maui.Controls;

namespace SmartShopping.Views;

[QueryProperty(nameof(Barcode), "barcode")]
public partial class AddProductPage : ContentPage
{
    private readonly AddProductViewModel _viewModel;

    public string Barcode
    {
        get => _viewModel.Barcode;
        set => _viewModel.Barcode = value;
    }

    public AddProductPage(AddProductViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
} 