using SmartShopping.ViewModels;

namespace SmartShopping.Views;

public partial class EditItemPage : ContentPage
{
    private readonly EditItemViewModel _viewModel;

    public EditItemPage(EditItemViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
} 