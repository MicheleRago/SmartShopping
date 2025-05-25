namespace SmartShopping.Models;

public class ExportData
{
    public string ExportId { get; set; } = string.Empty;
    public DateTime ExportDate { get; set; }
    public ExportType DataType { get; set; }
    public ExportFormat Format { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
} 