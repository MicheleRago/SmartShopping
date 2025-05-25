using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartShopping.Data;
using SmartShopping.Services;
using System.Collections.ObjectModel;

namespace SmartShopping.ViewModels;

public partial class BackupViewModel : BaseViewModel
{
    private readonly BackupService _backupService;

    [ObservableProperty]
    private ObservableCollection<BackupInfo> backups;

    public BackupViewModel(BackupService backupService)
    {
        _backupService = backupService;
        Title = "Gestione Backup";
        Backups = new ObservableCollection<BackupInfo>();
    }

    public async Task LoadBackupsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var backupList = await _backupService.GetBackupHistoryAsync();
            Backups.Clear();
            foreach (var backup in backupList)
            {
                Backups.Add(backup);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile caricare i backup", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var filePath = await _backupService.CreateBackupAsync();
            await Shell.Current.DisplayAlert("Successo", "Backup creato con successo", "OK");
            await LoadBackupsAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile creare il backup", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RestoreBackupAsync(BackupInfo backup)
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Conferma Ripristino",
                "Tutti i dati attuali verranno sostituiti con quelli del backup. Vuoi continuare?",
                "Sì",
                "No");

            if (confirm)
            {
                await _backupService.RestoreFromBackupAsync(backup.FilePath);
                await Shell.Current.DisplayAlert("Successo", "Backup ripristinato con successo", "OK");
                await LoadBackupsAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile ripristinare il backup", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ShareBackupAsync(BackupInfo backup)
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Condividi Backup",
                File = new ShareFile(backup.FilePath)
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile condividere il backup", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteBackupAsync(BackupInfo backup)
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var confirm = await Shell.Current.DisplayAlert(
                "Conferma Eliminazione",
                "Vuoi eliminare questo backup?",
                "Sì",
                "No");

            if (confirm)
            {
                await _backupService.DeleteBackupAsync(backup.FilePath);
                await LoadBackupsAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile eliminare il backup", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CleanupBackupsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var days = await Shell.Current.DisplayPromptAsync(
                "Pulisci Backup",
                "Inserisci il numero di giorni da mantenere:",
                initialValue: "30",
                keyboard: Keyboard.Numeric);

            if (int.TryParse(days, out int daysToKeep))
            {
                await _backupService.CleanupOldBackupsAsync(daysToKeep);
                await LoadBackupsAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Errore", "Impossibile pulire i backup", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
} 