using Microsoft.EntityFrameworkCore;
using SmartShopping.Data;
using SmartShopping.Services;
using SmartShopping.ViewModels;
using SmartShopping.Views;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

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
		{
			options.UseSqlite($"Data Source={dbPath}");
			// Abilita il logging dettagliato in debug
			#if DEBUG
			options.EnableSensitiveDataLogging();
			options.EnableDetailedErrors();
			#endif
		});

		// Register services
		builder.Services.AddSingleton<SettingsService>();
		builder.Services.AddSingleton<NotificationService>();
		builder.Services.AddSingleton<BackupService>();
		builder.Services.AddSingleton(sp => 
		{
			var settings = sp.GetRequiredService<SettingsService>();
			return new OpenFoodFactsService(settings.GetDefaultOpenFoodFactsUrl());
		});

		// Register Views
		builder.Services.AddTransient<AppShell>();
		builder.Services.AddTransient<InventoryPage>();
		builder.Services.AddTransient<ShoppingListPage>();
		builder.Services.AddTransient<ScannerPage>();
		builder.Services.AddTransient<AddProductPage>();
		builder.Services.AddTransient<ProductDetailsPage>();
		builder.Services.AddTransient<EditItemPage>();
		builder.Services.AddTransient<ExpiryPage>();
		builder.Services.AddTransient<BackupPage>();

		// Register ViewModels
		builder.Services.AddTransient<InventoryViewModel>();
		builder.Services.AddTransient<ShoppingListViewModel>();
		builder.Services.AddTransient<ScannerViewModel>();
		builder.Services.AddTransient<AddProductViewModel>();
		builder.Services.AddTransient<ExpiryViewModel>();
		builder.Services.AddTransient<BackupViewModel>();
		builder.Services.AddTransient<ProductDetailsViewModel>();
		builder.Services.AddTransient<EditItemViewModel>();

		var app = builder.Build();

		// Initialize database
		using (var scope = app.Services.CreateScope())
		{
			try
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
				
				try
				{
					// Verifica se il database è accessibile
					if (dbContext.Database.CanConnect())
					{
						// Prova a fare una query di test
						dbContext.Products.FirstOrDefault();
						System.Diagnostics.Debug.WriteLine("Database exists and is valid");
					}
				}
				catch (Exception)
				{
					// Se c'è un errore, il database potrebbe essere corrotto o avere una struttura non valida
					System.Diagnostics.Debug.WriteLine("Database exists but might be corrupted, recreating...");
					dbContext.Database.EnsureDeleted();
					dbContext.Database.EnsureCreated();
				}

				// Se il database non esiste, crealo
				if (!dbContext.Database.CanConnect())
				{
					System.Diagnostics.Debug.WriteLine("Creating new database...");
					dbContext.Database.EnsureCreated();
				}

				// Verifica finale
				if (!dbContext.Database.CanConnect())
				{
					throw new Exception("Cannot connect to database after initialization");
				}

				System.Diagnostics.Debug.WriteLine($"Database initialization completed successfully");
				System.Diagnostics.Debug.WriteLine($"Database path: {dbPath}");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Critical error initializing database: {ex.Message}");
				System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
				throw;
			}
		}

		return app;
	}
}
