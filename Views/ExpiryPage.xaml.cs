using SmartShopping.ViewModels;

namespace SmartShopping.Views;

public partial class ExpiryPage : ContentPage
{
    private readonly ExpiryViewModel _viewModel;

    public ExpiryPage(ExpiryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadExpiringItemsAsync();
    }
} 