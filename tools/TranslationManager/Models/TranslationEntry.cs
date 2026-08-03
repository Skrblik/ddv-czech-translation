namespace TranslationManager.Models;

/// <summary>
/// Represents a single translation entry from a .locbin file.
/// </summary>
public class TranslationEntry
{
    public string FilePath { get; set; } = "";
    public string Key { get; set; } = "";
    public string EnglishText { get; set; } = "";
    public string CzechText { get; set; } = "";
    public string OriginalCzechText { get; set; } = "";
    public string ImprovedCzechText { get; set; } = "";
    public TranslationSource Source { get; set; } = TranslationSource.PreExisting;
    public bool IsModified => !string.IsNullOrEmpty(ImprovedCzechText) && ImprovedCzechText != CzechText;
    public bool IsApproved { get; set; }

    /// <summary>
    /// The effective Czech text (improved if available, otherwise current).
    /// </summary>
    public string EffectiveCzechText => IsApproved && !string.IsNullOrEmpty(ImprovedCzechText)
        ? ImprovedCzechText
        : CzechText;

    /// <summary>
    /// Full key for translator_tool.py format: "filepath::audio_id"
    /// </summary>
    public string FullKey => $"{FilePath}::{Key}";
}

public enum TranslationSource
{
    PreExisting,
    OurTranslation,
    Untranslated,
    DoNotTranslate,
    Improved
}

/// <summary>
/// Represents a category (character/feature folder) containing translation entries.
/// </summary>
public class TranslationCategory
{
    public string Name { get; set; } = "";
    public List<TranslationEntry> Entries { get; set; } = new();

    public int TotalCount => Entries.Count;
    public int TranslatedCount => Entries.Count(e => !string.IsNullOrEmpty(e.CzechText));
    public int PreExistingCount => Entries.Count(e => e.Source == TranslationSource.PreExisting);
    public int UntranslatedCount => Entries.Count(e => e.Source == TranslationSource.Untranslated);
    public int ImprovedCount => Entries.Count(e => e.Source == TranslationSource.Improved || e.IsModified);
    public int LongDialogCount => Entries.Count(e => e.EnglishText.Length > 80 && e.Source == TranslationSource.PreExisting);

    public string DisplayName
    {
        get
        {
            var name = Name;
            if (name.Contains('_'))
            {
                // e.g. "WinnieDLC_Winnie" -> "Winnie (WinnieDLC)"
                var parts = name.Split('_', 2);
                return $"{parts[1]} ({parts[0]})";
            }
            return name;
        }
    }
}
