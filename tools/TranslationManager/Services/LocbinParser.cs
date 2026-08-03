using System.IO;
using System.Text;

namespace TranslationManager.Services;

/// <summary>
/// Parser for Disney Dreamlight Valley .locbin protobuf files.
/// Ported from the Python implementation in tools/export_for_github.py
/// </summary>
public static class LocbinParser
{
    /// <summary>
    /// Parse a .locbin file and return list of (audioId, text) pairs.
    /// </summary>
    public static List<(string AudioId, string Text)> Parse(byte[] data)
    {
        var entries = new List<(string, string)>();
        var outerFields = ParseProtobufList(data);

        foreach (var (fieldNum, val) in outerFields)
        {
            if (fieldNum != 1) continue;

            var innerFields = ParseProtobufList(val);
            string audioId = "";
            string text = "";

            foreach (var (innerFieldNum, innerVal) in innerFields)
            {
                switch (innerFieldNum)
                {
                    case 1:
                        audioId = Encoding.UTF8.GetString(innerVal);
                        break;
                    case 2:
                        text = Encoding.UTF8.GetString(innerVal);
                        break;
                }
            }

            entries.Add((audioId, text));
        }

        return entries;
    }

    /// <summary>
    /// Serialize a list of (audioId, text) pairs back to .locbin protobuf format.
    /// </summary>
    public static byte[] Serialize(List<(string AudioId, string Text)> entries)
    {
        using var ms = new MemoryStream();

        foreach (var (audioId, text) in entries)
        {
            using var innerMs = new MemoryStream();

            // Field 1: audio_id (string)
            var audioBytes = Encoding.UTF8.GetBytes(audioId);
            innerMs.WriteByte(0x0A); // field 1, wire type 2
            WriteVarint(innerMs, (ulong)audioBytes.Length);
            innerMs.Write(audioBytes);

            // Field 2: text (string)
            if (!string.IsNullOrEmpty(text))
            {
                var textBytes = Encoding.UTF8.GetBytes(text);
                innerMs.WriteByte(0x12); // field 2, wire type 2
                WriteVarint(innerMs, (ulong)textBytes.Length);
                innerMs.Write(textBytes);
            }

            var innerData = innerMs.ToArray();
            ms.WriteByte(0x0A); // field 1, wire type 2
            WriteVarint(ms, (ulong)innerData.Length);
            ms.Write(innerData);
        }

        return ms.ToArray();
    }

    private static List<(int FieldNum, byte[] Value)> ParseProtobufList(byte[] data)
    {
        var entries = new List<(int, byte[])>();
        int pos = 0;

        while (pos < data.Length)
        {
            byte key = data[pos];
            pos++;

            int wireType = key & 0x7;
            int fieldNum = key >> 3;

            if (wireType == 2) // Length-delimited
            {
                var (length, newPos) = ParseVarint(data, pos);
                pos = newPos;

                var val = new byte[length];
                Array.Copy(data, pos, val, 0, (int)length);
                pos += (int)length;

                entries.Add((fieldNum, val));
            }
            else
            {
                break;
            }
        }

        return entries;
    }

    private static (ulong Value, int NewPos) ParseVarint(byte[] data, int pos)
    {
        ulong result = 0;
        int shift = 0;

        while (pos < data.Length)
        {
            byte b = data[pos];
            result |= (ulong)(b & 0x7F) << shift;
            pos++;
            if ((b & 0x80) == 0) break;
            shift += 7;
        }

        return (result, pos);
    }

    private static void WriteVarint(Stream stream, ulong value)
    {
        do
        {
            byte b = (byte)(value & 0x7F);
            value >>= 7;
            if (value > 0) b |= 0x80;
            stream.WriteByte(b);
        } while (value > 0);
    }
}

