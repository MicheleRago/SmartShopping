using System.Text.Json;
using SmartShopping.Models;

namespace SmartShopping.Services;

public class OpenFoodFactsService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public OpenFoodFactsService(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SmartShopping - MAUI App");
    }

    public async Task<Product?> GetProductByBarcodeAsync(string barcode)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/product/{barcode}.json");
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            var product = jsonDoc.RootElement.GetProperty("product");

            return new Product
            {
                Barcode = barcode,
                Name = GetStringProperty(product, "product_name_it") ?? GetStringProperty(product, "product_name"),
                Brand = GetStringProperty(product, "brands"),
                Category = GetStringProperty(product, "categories_tags")?.Split(',')[0] ?? string.Empty,
                ImageUrl = GetStringProperty(product, "image_url"),
                Unit = GetStringProperty(product, "quantity"),
                NutritionalInfo = product.TryGetProperty("nutriments", out var nutriments) ? nutriments.ToString() : null
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        try
        {
            return element.GetProperty(propertyName).GetString();
        }
        catch
        {
            return null;
        }
    }
} 