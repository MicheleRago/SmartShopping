namespace SmartShopping.Models;

public enum Theme
{
    Light,
    Dark,
    Auto
}

public enum ExportFormat
{
    JSON,
    CSV,
    XML
}

public class AppSettings
{
    public int Id { get; set; }
    public string OpenFoodFactsApiUrl { get; set; } = string.Empty;
    public string BackupApiUrl { get; set; } = string.Empty;
    public string CustomApiEndpoint { get; set; } = string.Empty;
    public bool AutoBackupEnabled { get; set; }
    public bool NotificationsEnabled { get; set; }
    public Theme Theme { get; set; }
    public string Language { get; set; } = "it-IT";
    public DateTime? LastExportDate { get; set; }
    public ExportFormat ExportFormat { get; set; }
} 