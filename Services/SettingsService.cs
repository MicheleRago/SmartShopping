using Microsoft.EntityFrameworkCore;
using SmartShopping.Data;
using SmartShopping.Models;

namespace SmartShopping.Services;

public class SettingsService
{
    private readonly AppDbContext _context;

    public SettingsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new AppSettings
            {
                OpenFoodFactsApiUrl = GetDefaultOpenFoodFactsUrl(),
                Theme = Theme.Auto,
                Language = "it-IT",
                ExportFormat = ExportFormat.JSON
            };
            _context.AppSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return settings;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        _context.AppSettings.Update(settings);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateApiUrlAsync(string apiUrl)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(apiUrl);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task ResetToDefaultsAsync()
    {
        var settings = await GetSettingsAsync();
        settings.OpenFoodFactsApiUrl = GetDefaultOpenFoodFactsUrl();
        settings.Theme = Theme.Auto;
        settings.Language = "it-IT";
        settings.ExportFormat = ExportFormat.JSON;
        await SaveSettingsAsync(settings);
    }

    public string GetDefaultOpenFoodFactsUrl() => "https://world.openfoodfacts.org/api/v2";
} 