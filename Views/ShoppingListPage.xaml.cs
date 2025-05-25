using SmartShopping.Models;
using SmartShopping.ViewModels;

namespace SmartShopping.Views;

public partial class ShoppingListPage : ContentPage
{
    private readonly ShoppingListViewModel _viewModel;

    public ShoppingListPage(ShoppingListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadShoppingListCommand.ExecuteAsync(null);
    }

    private async void OnItemCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is ShoppingListItem item)
        {
            await _viewModel.ToggleItemCompletionCommand.ExecuteAsync(item);
        }
    }
} 