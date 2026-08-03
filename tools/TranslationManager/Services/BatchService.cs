using System.Text;
using System.Text.Json;
using TranslationManager.Models;

namespace TranslationManager.Services;

/// <summary>
/// Service for creating and parsing AI translation batches.
/// Generates text format suitable for copy-pasting into AI chat.
/// </summary>
public static class BatchService
{
    /// <summary>
    /// Generate a batch of entries formatted for AI translation.
    /// </summary>
    public static string GenerateBatchForAI(List<TranslationEntry> entries, string instructions = "")
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== DÁVKA K PŘEKLADU / VYLEPŠENÍ ===");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(instructions))
        {
            sb.AppendLine(instructions);
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("Vylepši české překlady z videohry Disney Dreamlight Valley.");
            sb.AppendLine("Pravidla:");
            sb.AppendLine("- Postava VŽDY tyká hráči (2. os. sg.)");
            sb.AppendLine("- Zachovej přirozený, plynulý český jazyk");
            sb.AppendLine("- Zachovej herní termíny (Noční trny, Údolí snů, Dreamlight atd.)");
            sb.AppendLine("- Zachovej {PlayerName} a další proměnné v {}");
            sb.AppendLine("- Oprav chybné překlady, krkolomné formulace, anglicismy");
            sb.AppendLine("- Pokud je překlad v pořádku, ponech ho beze změny");
            sb.AppendLine();
        }

        sb.AppendLine("Formát odpovědi: vrať POUZE JSON pole ve stejném formátu.");
        sb.AppendLine("U každé položky vrať pole \"new_cz\" s vylepšeným překladem.");
        sb.AppendLine();
        sb.AppendLine("```json");
        sb.AppendLine("[");

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            sb.AppendLine("  {");
            sb.AppendLine($"    \"id\": {i},");
            sb.AppendLine($"    \"key\": {JsonEncode(e.Key)},");
            sb.AppendLine($"    \"file\": {JsonEncode(e.FilePath)},");
            sb.AppendLine($"    \"en\": {JsonEncode(e.EnglishText)},");
            sb.AppendLine($"    \"cz\": {JsonEncode(e.CzechText)},");
            sb.AppendLine($"    \"new_cz\": \"\"");
            sb.Append("  }");
            if (i < entries.Count - 1) sb.Append(',');
            sb.AppendLine();
        }

        sb.AppendLine("]");
        sb.AppendLine("```");

        return sb.ToString();
    }

    /// <summary>
    /// Parse AI response and apply translations back to entries.
    /// Returns number of applied translations.
    /// </summary>
    public static int ParseAndApplyBatch(string aiResponse, List<TranslationEntry> entries)
    {
        // Try to extract JSON from the response
        var jsonText = ExtractJson(aiResponse);
        if (string.IsNullOrEmpty(jsonText)) return 0;

        try
        {
            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array) return 0;

            int applied = 0;

            foreach (var item in root.EnumerateArray())
            {
                string? newCz = null;
                if (item.TryGetProperty("new_cz", out var newCzProp))
                    newCz = newCzProp.GetString();

                if (string.IsNullOrEmpty(newCz)) continue;

                // Match by id (index), key, or file+key
                TranslationEntry? entry = null;

                if (item.TryGetProperty("id", out var idProp) && idProp.TryGetInt32(out int id) && id >= 0 && id < entries.Count)
                {
                    entry = entries[id];
                }
                else if (item.TryGetProperty("key", out var keyProp))
                {
                    var key = keyProp.GetString();
                    var file = item.TryGetProperty("file", out var fileProp) ? fileProp.GetString() : null;

                    entry = file != null
                        ? entries.FirstOrDefault(e => e.Key == key && e.FilePath == file)
                        : entries.FirstOrDefault(e => e.Key == key);
                }

                if (entry != null && newCz != entry.CzechText)
                {
                    entry.ImprovedCzechText = newCz;
                    applied++;
                }
            }

            return applied;
        }
        catch
        {
            return 0;
        }
    }

    private static string ExtractJson(string text)
    {
        // Try to find JSON between ```json and ```
        var startIdx = text.IndexOf("```json");
        if (startIdx >= 0)
        {
            startIdx = text.IndexOf('\n', startIdx) + 1;
            var endIdx = text.IndexOf("```", startIdx);
            if (endIdx > startIdx)
                return text[startIdx..endIdx].Trim();
        }

        // Try to find JSON between ``` and ```
        startIdx = text.IndexOf("```");
        if (startIdx >= 0)
        {
            startIdx = text.IndexOf('\n', startIdx) + 1;
            var endIdx = text.IndexOf("```", startIdx);
            if (endIdx > startIdx)
                return text[startIdx..endIdx].Trim();
        }

        // Try to find raw JSON array
        startIdx = text.IndexOf('[');
        if (startIdx >= 0)
        {
            var endIdx = text.LastIndexOf(']');
            if (endIdx > startIdx)
                return text[startIdx..(endIdx + 1)];
        }

        return text.Trim();
    }

    private static string JsonEncode(string value)
    {
        return JsonSerializer.Serialize(value);
    }
}
