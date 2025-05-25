using SmartShopping.ViewModels;

namespace SmartShopping.Views;

public partial class ProductDetailsPage : ContentPage
{
    private readonly ProductDetailsViewModel _viewModel;

    public ProductDetailsPage(ProductDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
} 