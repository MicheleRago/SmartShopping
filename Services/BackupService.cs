using System.Text.Json;
using SmartShopping.Data;
using SmartShopping.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartShopping.Services;

public class BackupService
{
    private readonly AppDbContext _context;
    private readonly string _backupDirectory;

    public BackupService(AppDbContext context)
    {
        _context = context;
        _backupDirectory = Path.Combine(FileSystem.AppDataDirectory, "Backups");
        Directory.CreateDirectory(_backupDirectory);
    }

    public async Task<string> CreateBackupAsync()
    {
        try
        {
            var backupData = new BackupData
            {
                Products = await _context.Products.ToListAsync(),
                InventoryItems = await _context.InventoryItems.ToListAsync(),
                ShoppingListItems = await _context.ShoppingListItems.ToListAsync(),
                AppSettings = await _context.AppSettings.ToListAsync(),
                ExportData = await _context.ExportData.ToListAsync(),
                BackupDate = DateTime.Now
            };

            var json = JsonSerializer.Serialize(backupData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var fileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(_backupDirectory, fileName);
            await File.WriteAllTextAsync(filePath, json);

            // Registra l'esportazione
            var exportData = new ExportData
            {
                ExportId = Guid.NewGuid().ToString(),
                ExportDate = DateTime.Now,
                DataType = ExportType.Full,
                Format = ExportFormat.JSON,
                FilePath = filePath,
                FileSize = new FileInfo(filePath).Length
            };

            _context.ExportData.Add(exportData);
            await _context.SaveChangesAsync();

            return filePath;
        }
        catch (Exception ex)
        {
            throw new Exception("Errore durante la creazione del backup", ex);
        }
    }

    public async Task RestoreFromBackupAsync(string backupFilePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(backupFilePath);
            var backupData = JsonSerializer.Deserialize<BackupData>(json);

            if (backupData == null)
                throw new Exception("File di backup non valido");

            // Pulisci il database esistente
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ExportData");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ShoppingListItems");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM InventoryItems");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Products");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM AppSettings");

            // Ripristina i dati
            _context.Products.AddRange(backupData.Products);
            _context.InventoryItems.AddRange(backupData.InventoryItems);
            _context.ShoppingListItems.AddRange(backupData.ShoppingListItems);
            _context.AppSettings.AddRange(backupData.AppSettings);
            _context.ExportData.AddRange(backupData.ExportData);

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Errore durante il ripristino del backup", ex);
        }
    }

    public async Task<List<BackupInfo>> GetBackupHistoryAsync()
    {
        var backups = new List<BackupInfo>();
        var files = Directory.GetFiles(_backupDirectory, "backup_*.json");

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            var exportData = await _context.ExportData
                .FirstOrDefaultAsync(e => e.FilePath == file);

            backups.Add(new BackupInfo
            {
                FilePath = file,
                FileName = Path.GetFileName(file),
                FileSize = fileInfo.Length,
                CreationDate = fileInfo.CreationTime,
                ExportId = exportData?.ExportId
            });
        }

        return backups.OrderByDescending(b => b.CreationDate).ToList();
    }

    public async Task DeleteBackupAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);

                var exportData = await _context.ExportData
                    .FirstOrDefaultAsync(e => e.FilePath == filePath);

                if (exportData != null)
                {
                    _context.ExportData.Remove(exportData);
                    await _context.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Errore durante l'eliminazione del backup", ex);
        }
    }

    public async Task CleanupOldBackupsAsync(int daysToKeep)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            var oldBackups = await _context.ExportData
                .Where(e => e.ExportDate < cutoffDate)
                .ToListAsync();

            foreach (var backup in oldBackups)
            {
                if (File.Exists(backup.FilePath))
                {
                    File.Delete(backup.FilePath);
                }
                _context.ExportData.Remove(backup);
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Errore durante la pulizia dei backup", ex);
        }
    }
}

public class BackupData
{
    public List<Product> Products { get; set; } = new();
    public List<InventoryItem> InventoryItems { get; set; } = new();
    public List<ShoppingListItem> ShoppingListItems { get; set; } = new();
    public List<AppSettings> AppSettings { get; set; } = new();
    public List<ExportData> ExportData { get; set; } = new();
    public DateTime BackupDate { get; set; }
}

public class BackupInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreationDate { get; set; }
    public string? ExportId { get; set; }
} 