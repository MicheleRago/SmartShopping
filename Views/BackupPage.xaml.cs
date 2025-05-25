using SmartShopping.ViewModels;

namespace SmartShopping.Views;

public partial class BackupPage : ContentPage
{
    private readonly BackupViewModel _viewModel;

    public BackupPage(BackupViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadBackupsAsync();
    }
} 