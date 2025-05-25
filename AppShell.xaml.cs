using SmartShopping.Views;

namespace SmartShopping;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Registra le rotte per la navigazione
		Routing.RegisterRoute(nameof(ProductDetailsPage), typeof(ProductDetailsPage));
		Routing.RegisterRoute(nameof(AddProductPage), typeof(AddProductPage));
		Routing.RegisterRoute(nameof(EditItemPage), typeof(EditItemPage));
	}
}
