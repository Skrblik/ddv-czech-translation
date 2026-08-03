using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using TranslationManager.Models;

namespace TranslationManager.Services;

/// <summary>
/// Service for loading translations from game ZIP files and repo JSON files.
/// </summary>
public class TranslationService
{
    private static readonly Regex DoNotTranslatePattern = new(
        @"^\{(DoNotTranslate|ItemName|CharacterName|LocationName|BuildingName|FurnitureName|SetName|ActivityName|UpgradeName|SkinName|PointOfInterestName)[^}]*\}\.?$",
        RegexOptions.Compiled);

    /// <summary>
    /// Load translations directly from the game's ZIP files.
    /// </summary>
    /// <param name="gameZipPath">Path to current game LocDB_en-US.zip (contains Czech translations)</param>
    /// <param name="backupZipPath">Path to English backup LocDB_en-US.zip</param>
    /// <param name="originalCzPath">Optional path to original Czech .bak file</param>
    public async Task<List<TranslationCategory>> LoadFromZipAsync(
        string gameZipPath,
        string backupZipPath,
        string? originalCzPath = null)
    {
        return await Task.Run(() =>
        {
            var categories = new Dictionary<string, TranslationCategory>();

            using var gameZip = ZipFile.OpenRead(gameZipPath);
            using var backupZip = ZipFile.OpenRead(backupZipPath);

            var gameEntries = gameZip.Entries.Where(e => e.Name.EndsWith(".locbin"))
                .ToDictionary(e => e.FullName);
            var backupEntries = backupZip.Entries.Where(e => e.Name.EndsWith(".locbin"))
                .ToDictionary(e => e.FullName);

            Dictionary<string, ZipArchiveEntry>? origCzEntries = null;
            ZipArchive? origCzZip = null;

            if (!string.IsNullOrEmpty(originalCzPath) && File.Exists(originalCzPath))
            {
                origCzZip = ZipFile.OpenRead(originalCzPath);
                origCzEntries = origCzZip.Entries.Where(e => e.Name.EndsWith(".locbin"))
                    .ToDictionary(e => e.FullName);
            }

            try
            {
                var allFiles = backupEntries.Keys.Union(gameEntries.Keys).OrderBy(f => f).ToList();

                foreach (var filePath in allFiles)
                {
                    if (!filePath.EndsWith(".locbin")) continue;

                    var category = GetCategory(filePath);
                    if (!categories.ContainsKey(category))
                        categories[category] = new TranslationCategory { Name = category };

                    // Parse backup (English original)
                    var englishTexts = new Dictionary<string, string>();
                    if (backupEntries.TryGetValue(filePath, out var backupEntry))
                    {
                        using var stream = backupEntry.Open();
                        using var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        foreach (var (id, text) in LocbinParser.Parse(ms.ToArray()))
                            englishTexts[id] = text;
                    }

                    // Parse game (current Czech)
                    var czechTexts = new Dictionary<string, string>();
                    if (gameEntries.TryGetValue(filePath, out var gameEntry))
                    {
                        using var stream = gameEntry.Open();
                        using var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        foreach (var (id, text) in LocbinParser.Parse(ms.ToArray()))
                            czechTexts[id] = text;
                    }

                    // Parse original CZ if available
                    HashSet<string>? origCzKeys = null;
                    var origCzTexts = new Dictionary<string, string>();
                    if (origCzEntries != null && origCzEntries.TryGetValue(filePath, out var origEntry))
                    {
                        using var stream = origEntry.Open();
                        using var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        var parsed = LocbinParser.Parse(ms.ToArray());
                        origCzKeys = new HashSet<string>(parsed.Select(p => p.AudioId));
                        foreach (var (id, text) in parsed)
                            origCzTexts[id] = text;
                    }

                    foreach (var (audioId, enText) in englishTexts)
                    {
                        if (string.IsNullOrEmpty(enText)) continue;

                        var czText = czechTexts.GetValueOrDefault(audioId, "");
                        var origCzText = origCzTexts.GetValueOrDefault(audioId, "");

                        // Determine source
                        TranslationSource source;
                        if (IsDoNotTranslate(enText))
                            source = TranslationSource.DoNotTranslate;
                        else if (origCzKeys != null && origCzKeys.Contains(audioId))
                            source = TranslationSource.PreExisting;
                        else if (!string.IsNullOrEmpty(czText) && czText != enText)
                            source = TranslationSource.OurTranslation;
                        else
                            source = TranslationSource.Untranslated;

                        categories[category].Entries.Add(new TranslationEntry
                        {
                            FilePath = filePath,
                            Key = audioId,
                            EnglishText = enText,
                            CzechText = source == TranslationSource.PreExisting && !string.IsNullOrEmpty(czText) ? czText : czText,
                            OriginalCzechText = origCzText,
                            Source = source
                        });
                    }
                }
            }
            finally
            {
                origCzZip?.Dispose();
            }

            return categories.Values.OrderBy(c => c.Name).ToList();
        });
    }

    /// <summary>
    /// Load translations from repo JSON files (translations/ directory).
    /// </summary>
    public async Task<List<TranslationCategory>> LoadFromJsonDirectoryAsync(string translationsDir)
    {
        return await Task.Run(() =>
        {
            var categories = new List<TranslationCategory>();
            var jsonFiles = Directory.GetFiles(translationsDir, "*.json");

            foreach (var jsonFile in jsonFiles.OrderBy(f => f))
            {
                var categoryName = Path.GetFileNameWithoutExtension(jsonFile);
                var category = new TranslationCategory { Name = categoryName };

                using var stream = File.OpenRead(jsonFile);
                using var doc = JsonDocument.Parse(stream);

                foreach (var fileProp in doc.RootElement.EnumerateObject())
                {
                    var filePath = fileProp.Name;
                    foreach (var entry in fileProp.Value.EnumerateArray())
                    {
                        var key = entry.GetProperty("key").GetString() ?? "";
                        var en = entry.GetProperty("en").GetString() ?? "";
                        var cz = entry.TryGetProperty("cz", out var czProp) ? czProp.GetString() ?? "" : "";
                        var sourceStr = entry.TryGetProperty("source", out var srcProp) ? srcProp.GetString() ?? "" : "";
                        var origCz = entry.TryGetProperty("original_cz", out var origProp) ? origProp.GetString() ?? "" : "";

                        var source = sourceStr switch
                        {
                            "pre_existing" => TranslationSource.PreExisting,
                            "our_translation" => TranslationSource.OurTranslation,
                            "untranslated" => TranslationSource.Untranslated,
                            "do_not_translate" => TranslationSource.DoNotTranslate,
                            "improved" => TranslationSource.Improved,
                            _ => TranslationSource.PreExisting
                        };

                        category.Entries.Add(new TranslationEntry
                        {
                            FilePath = filePath,
                            Key = key,
                            EnglishText = en,
                            CzechText = cz,
                            OriginalCzechText = origCz,
                            Source = source
                        });
                    }
                }

                if (category.Entries.Count > 0)
                    categories.Add(category);
            }

            return categories;
        });
    }

    /// <summary>
    /// Save approved improvements back to repo JSON files.
    /// </summary>
    public async Task SaveToJsonDirectoryAsync(string translationsDir, List<TranslationCategory> categories)
    {
        await Task.Run(() =>
        {
            foreach (var category in categories)
            {
                var jsonFile = Path.Combine(translationsDir, $"{category.Name}.json");
                if (!File.Exists(jsonFile)) continue;

                // Read existing file to preserve structure
                var jsonText = File.ReadAllText(jsonFile);
                using var doc = JsonDocument.Parse(jsonText);

                var modified = category.Entries.Where(e => e.IsApproved && e.IsModified)
                    .ToDictionary(e => $"{e.FilePath}::{e.Key}", e => e.ImprovedCzechText);

                if (modified.Count == 0) continue;

                // Rebuild JSON with modifications
                var outputDict = new Dictionary<string, List<Dictionary<string, object>>>();

                foreach (var fileProp in doc.RootElement.EnumerateObject())
                {
                    var entries = new List<Dictionary<string, object>>();
                    foreach (var entry in fileProp.Value.EnumerateArray())
                    {
                        var key = entry.GetProperty("key").GetString() ?? "";
                        var fullKey = $"{fileProp.Name}::{key}";
                        var entryDict = new Dictionary<string, object>();

                        foreach (var prop in entry.EnumerateObject())
                            entryDict[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                                ? prop.Value.GetString()!
                                : prop.Value.ToString();

                        if (modified.TryGetValue(fullKey, out var newCz))
                        {
                            entryDict["cz"] = newCz;
                            entryDict["source"] = "improved";
                        }

                        entries.Add(entryDict);
                    }
                    outputDict[fileProp.Name] = entries;
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                File.WriteAllText(jsonFile, JsonSerializer.Serialize(outputDict, options));
            }
        });
    }

    /// <summary>
    /// Apply translations back to the game's ZIP file.
    /// </summary>
    public async Task ApplyToGameZipAsync(string gameZipPath, List<TranslationEntry> entries)
    {
        await Task.Run(() =>
        {
            var byFile = entries
                .Where(e => e.IsApproved && e.IsModified)
                .GroupBy(e => e.FilePath)
                .ToDictionary(g => g.Key, g => g.ToDictionary(e => e.Key, e => e.EffectiveCzechText));

            if (byFile.Count == 0) return;

            var tempPath = gameZipPath + ".tmp";

            using (var inputZip = ZipFile.OpenRead(gameZipPath))
            using (var outputZip = ZipFile.Open(tempPath, ZipArchiveMode.Create))
            {
                foreach (var entry in inputZip.Entries)
                {
                    if (byFile.TryGetValue(entry.FullName, out var translations))
                    {
                        using var stream = entry.Open();
                        using var ms = new MemoryStream();
                        stream.CopyTo(ms);

                        var parsed = LocbinParser.Parse(ms.ToArray());
                        var modified = parsed.Select(p =>
                            translations.TryGetValue(p.AudioId, out var newText)
                                ? (p.AudioId, newText)
                                : p).ToList();

                        var newEntry = outputZip.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                        using var outStream = newEntry.Open();
                        var data = LocbinParser.Serialize(modified);
                        outStream.Write(data);
                    }
                    else
                    {
                        var newEntry = outputZip.CreateEntry(entry.FullName, CompressionLevel.Optimal);
                        using var inStream = entry.Open();
                        using var outStream = newEntry.Open();
                        inStream.CopyTo(outStream);
                    }
                }
            }

            // Replace original with modified
            File.Move(gameZipPath, gameZipPath + ".bak", overwrite: true);
            File.Move(tempPath, gameZipPath);
        });
    }

    private static string GetCategory(string filePath)
    {
        var parts = filePath.Split('/');
        return parts.Length > 1 ? parts[0] : "Root";
    }

    private static bool IsDoNotTranslate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;
        return DoNotTranslatePattern.IsMatch(text.Trim());
    }
}

