using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TranslationManager.Models;
using TranslationManager.Services;

namespace TranslationManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly TranslationService _translationService = new();

    [ObservableProperty]
    private ObservableCollection<TranslationCategory> categories = new();

    [ObservableProperty]
    private TranslationCategory? selectedCategory;

    [ObservableProperty]
    private ObservableCollection<TranslationEntry> displayedEntries = new();

    [ObservableProperty]
    private TranslationEntry? selectedEntry;

    [ObservableProperty]
    private string statusText = "Připraveno. Načtěte soubor přes Soubor → Načíst.";

    [ObservableProperty]
    private string filterText = "";

    [ObservableProperty]
    private bool showOnlyPreExisting;

    [ObservableProperty]
    private bool showOnlyLongDialogs;

    [ObservableProperty]
    private bool showOnlyUntranslated;

    [ObservableProperty]
    private int batchSize = 15;

    [ObservableProperty]
    private string batchOutput = "";

    [ObservableProperty]
    private string batchInput = "";

    [ObservableProperty]
    private string customInstructions = "";

    [ObservableProperty]
    private string loadedFilePath = "";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private int totalEntries;

    [ObservableProperty]
    private int translatedEntries;

    [ObservableProperty]
    private int improvedEntries;

    // Selection tracking for batch
    private readonly HashSet<TranslationEntry> _selectedForBatch = new();

    [ObservableProperty]
    private int batchCount;

    public ObservableCollection<TranslationEntry> SelectedForBatch { get; } = new();

    partial void OnSelectedCategoryChanged(TranslationCategory? value)
    {
        RefreshDisplayedEntries();
    }

    partial void OnFilterTextChanged(string value) => RefreshDisplayedEntries();
    partial void OnShowOnlyPreExistingChanged(bool value) => RefreshDisplayedEntries();
    partial void OnShowOnlyLongDialogsChanged(bool value) => RefreshDisplayedEntries();
    partial void OnShowOnlyUntranslatedChanged(bool value) => RefreshDisplayedEntries();

    [RelayCommand]
    private async Task LoadJsonDirectoryAsync()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Vyberte složku translations/"
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        StatusText = "Načítám překlady z JSON souborů...";

        try
        {
            var cats = await _translationService.LoadFromJsonDirectoryAsync(dialog.FolderName);
            Categories = new ObservableCollection<TranslationCategory>(cats);
            LoadedFilePath = dialog.FolderName;
            UpdateStats();
            StatusText = $"Načteno {Categories.Count} kategorií, {TotalEntries} textů z JSON.";
        }
        catch (Exception ex)
        {
            StatusText = $"Chyba: {ex.Message}";
            MessageBox.Show(ex.Message, "Chyba při načítání", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadZipFileAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Vyberte herní ZIP soubor (LocDB_en-US.zip)",
            Filter = "ZIP soubory (*.zip)|*.zip",
            DefaultExt = ".zip"
        };

        if (dialog.ShowDialog() != true) return;

        var backupDialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Vyberte zálohu (anglický originál) LocDB_en-US.zip.bak",
            Filter = "ZIP/BAK soubory (*.zip;*.bak)|*.zip;*.bak",
            DefaultExt = ".zip"
        };

        if (backupDialog.ShowDialog() != true) return;

        IsLoading = true;
        StatusText = "Načítám překlady z herních souborů...";

        try
        {
            var cats = await _translationService.LoadFromZipAsync(dialog.FileName, backupDialog.FileName);
            Categories = new ObservableCollection<TranslationCategory>(cats);
            LoadedFilePath = dialog.FileName;
            UpdateStats();
            StatusText = $"Načteno {Categories.Count} kategorií, {TotalEntries} textů ze ZIP.";
        }
        catch (Exception ex)
        {
            StatusText = $"Chyba: {ex.Message}";
            MessageBox.Show(ex.Message, "Chyba při načítání", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddToBatch()
    {
        if (SelectedEntry == null) return;

        if (_selectedForBatch.Add(SelectedEntry))
        {
            SelectedForBatch.Add(SelectedEntry);
            BatchCount = _selectedForBatch.Count;
            StatusText = $"Do dávky přidán: {SelectedEntry.Key} ({SelectedForBatch.Count} položek v dávce)";
        }
    }

    [RelayCommand]
    private void AddPageToBatch()
    {
        int added = 0;
        foreach (var entry in DisplayedEntries.Take(BatchSize))
        {
            if (entry.Source == TranslationSource.DoNotTranslate) continue;
            if (_selectedForBatch.Add(entry))
            {
                SelectedForBatch.Add(entry);
                added++;
            }
        }
        BatchCount = _selectedForBatch.Count;
        StatusText = $"Přidáno {added} položek do dávky (celkem {SelectedForBatch.Count})";
    }

    [RelayCommand]
    private void ClearBatch()
    {
        _selectedForBatch.Clear();
        BatchCount = _selectedForBatch.Count;
        SelectedForBatch.Clear();
        BatchOutput = "";
        BatchInput = "";
        StatusText = "Dávka vyčištěna.";
    }

    [RelayCommand]
    private void GenerateBatch()
    {
        try
        {
            if (SelectedForBatch.Count == 0)
            {
                StatusText = "Dávka je prázdná – nejdřív přidejte texty.";
                return;
            }

            BatchOutput = BatchService.GenerateBatchForAI(
                SelectedForBatch.ToList(),
                CustomInstructions);

            try
            {
                System.Windows.Clipboard.SetDataObject(BatchOutput, true);
                StatusText = $"Dávka ({SelectedForBatch.Count} textů) vygenerována a zkopírována do schránky!";
            }
            catch
            {
                StatusText = $"Dávka ({SelectedForBatch.Count} textů) vygenerována. Zkopírujte ručně z pole vlevo.";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Chyba při generování dávky: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ImportBatch()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(BatchInput))
            {
                StatusText = "Vložte odpověď z AI chatu do pole pro import.";
                return;
            }

            var applied = BatchService.ParseAndApplyBatch(BatchInput, SelectedForBatch.ToList());
            StatusText = $"Importováno {applied} vylepšených překladů z AI odpovědi.";
            RefreshDisplayedEntries();
            UpdateStats();
        }
        catch (Exception ex)
        {
            StatusText = $"Chyba při importu dávky: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ApproveAll()
    {
        int count = 0;
        foreach (var entry in SelectedForBatch.Where(e => e.IsModified))
        {
            entry.IsApproved = true;
            count++;
        }
        StatusText = $"Schváleno {count} překladů.";
        RefreshDisplayedEntries();
    }

    [RelayCommand]
    private async Task SaveToJsonAsync()
    {
        if (string.IsNullOrEmpty(LoadedFilePath))
        {
            StatusText = "Není načten žádný soubor.";
            return;
        }

        var dir = Directory.Exists(LoadedFilePath)
            ? LoadedFilePath
            : Path.GetDirectoryName(LoadedFilePath);

        if (string.IsNullOrEmpty(dir))
        {
            StatusText = "Nelze určit výstupní adresář.";
            return;
        }

        IsLoading = true;
        StatusText = "Ukládám schválené překlady...";

        try
        {
            await _translationService.SaveToJsonDirectoryAsync(dir, Categories.ToList());
            int saved = Categories.Sum(c => c.Entries.Count(e => e.IsApproved && e.IsModified));
            StatusText = $"Uloženo {saved} schválených překladů do JSON.";
        }
        catch (Exception ex)
        {
            StatusText = $"Chyba: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void PasteFromClipboard()
    {
        if (Clipboard.ContainsText())
        {
            BatchInput = Clipboard.GetText();
            StatusText = "Text vložen ze schránky.";
        }
    }

    private void RefreshDisplayedEntries()
    {
        if (SelectedCategory == null)
        {
            DisplayedEntries = new ObservableCollection<TranslationEntry>();
            return;
        }

        IEnumerable<TranslationEntry> filtered = SelectedCategory.Entries;

        if (ShowOnlyPreExisting)
            filtered = filtered.Where(e => e.Source == TranslationSource.PreExisting);
        if (ShowOnlyUntranslated)
            filtered = filtered.Where(e => e.Source == TranslationSource.Untranslated);
        if (ShowOnlyLongDialogs)
            filtered = filtered.Where(e => e.EnglishText.Length > 80);

        if (!string.IsNullOrWhiteSpace(FilterText))
        {
            var search = FilterText.ToLowerInvariant();
            filtered = filtered.Where(e =>
                e.EnglishText.Contains(search, StringComparison.InvariantCultureIgnoreCase) ||
                e.CzechText.Contains(search, StringComparison.InvariantCultureIgnoreCase) ||
                e.Key.Contains(search, StringComparison.InvariantCultureIgnoreCase));
        }

        DisplayedEntries = new ObservableCollection<TranslationEntry>(filtered);
    }

    private void UpdateStats()
    {
        TotalEntries = Categories.Sum(c => c.TotalCount);
        TranslatedEntries = Categories.Sum(c => c.TranslatedCount);
        ImprovedEntries = Categories.Sum(c => c.ImprovedCount);
    }
}





