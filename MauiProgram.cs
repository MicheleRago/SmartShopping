using Microsoft.EntityFrameworkCore;
using SmartShopping.Data;
using SmartShopping.Services;
using SmartShopping.ViewModels;
using ZXing.Net.Maui;

namespace SmartShopping;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register database
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "smartshopping.db");
		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite($"Data Source={dbPath}"));

		// Register services
		builder.Services.AddSingleton<SettingsService>();
		builder.Services.AddSingleton<NotificationService>();
		builder.Services.AddSingleton<BackupService>();
		builder.Services.AddSingleton(sp => 
		{
			var settings = sp.GetRequiredService<SettingsService>();
			return new OpenFoodFactsService(settings.GetDefaultOpenFoodFactsUrl());
		});

		// Register ViewModels
		builder.Services.AddTransient<InventoryViewModel>();
		builder.Services.AddTransient<ShoppingListViewModel>();
		builder.Services.AddTransient<ScannerViewModel>();
		builder.Services.AddTransient<AddProductViewModel>();
		builder.Services.AddTransient<ExpiryViewModel>();
		builder.Services.AddTransient<BackupViewModel>();

		var app = builder.Build();

		// Initialize database
		using (var scope = app.Services.CreateScope())
		{
			try
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
				var created = dbContext.Database.EnsureCreated();
				System.Diagnostics.Debug.WriteLine($"Database created: {created}");
				System.Diagnostics.Debug.WriteLine($"Database path: {dbPath}");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error creating database: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
			}
		}

		return app;
	}
}
