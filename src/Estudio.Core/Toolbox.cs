using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Estudio.Core;

public record FileRow(string Ruta, long Bytes, DateTime ModificadoUtc);
public record DuplicateGroup(string Sha256, string[] Archivos);
public record TextReport(int Lineas, int Palabras, int Caracteres);
public record ContrastReport(double Ratio, bool AANormal, bool AAGrande, bool AAANormal);

public static class Toolbox
{
    public static string Hash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    static IEnumerable<string> Files(string root)
    {
        var info = new DirectoryInfo(root);
        if (!info.Exists || info.Attributes.HasFlag(FileAttributes.ReparsePoint))
            throw new ArgumentException("Selecciona una carpeta existente sin enlace o junction.");
        return Directory.EnumerateFiles(info.FullName, "*", new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = false,
            AttributesToSkip = FileAttributes.ReparsePoint
        });
    }

    public static FileRow[] Inventory(string root) => Files(root)
        .Select(path => new FileInfo(path))
        .Select(info => new FileRow(Path.GetRelativePath(root, info.FullName), info.Length, info.LastWriteTimeUtc))
        .OrderBy(row => row.Ruta, StringComparer.OrdinalIgnoreCase).ToArray();

    public static DuplicateGroup[] Duplicates(string root) => Files(root)
        .GroupBy(path => new FileInfo(path).Length)
        .Where(group => group.Count() > 1)
        .SelectMany(group => group.GroupBy(Hash))
        .Where(group => group.Count() > 1)
        .Select(group => new DuplicateGroup(group.Key, group.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray()))
        .OrderBy(group => group.Sha256, StringComparer.Ordinal).ToArray();

    public static TextReport TextStats(string text)
    {
        using var reader = new StringReader(text);
        int lines = 0;
        while (reader.ReadLine() is not null) lines++;
        return new(lines, Regex.Matches(text, @"\b[\p{L}\p{N}_]+\b").Count, text.Length);
    }

    public static string UniqueLines(string text)
    {
        using var reader = new StringReader(text);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<string>();
        string? line;
        while ((line = reader.ReadLine()) is not null)
            if (seen.Add(line)) result.Add(line);
        return string.Join(Environment.NewLine, result);
    }

    public static string Slug(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var plain = string.Concat(normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
        return Regex.Replace(plain.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
    }

    public static int[] ColorHex(string value)
    {
        if (!Regex.IsMatch(value, "^#[0-9a-fA-F]{6}$"))
            throw new ArgumentException("Usa colores #RRGGBB.");
        return [Convert.ToInt32(value.Substring(1, 2), 16), Convert.ToInt32(value.Substring(3, 2), 16), Convert.ToInt32(value.Substring(5, 2), 16)];
    }

    public static ContrastReport Contrast(string foreground, string background)
    {
        static double Luminance(string value)
        {
            var colors = ColorHex(value).Select(n => n / 255d)
                .Select(n => n <= 0.04045 ? n / 12.92 : Math.Pow((n + 0.055) / 1.055, 2.4)).ToArray();
            return colors[0] * 0.2126 + colors[1] * 0.7152 + colors[2] * 0.0722;
        }
        var a = Luminance(foreground);
        var b = Luminance(background);
        var ratio = (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
        return new(ratio, ratio >= 4.5, ratio >= 3, ratio >= 7);
    }

    public static void WriteNew(string path, string text)
    {
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(text);
    }

    public static string InventoryCsv(IEnumerable<FileRow> rows)
    {
        static string Quote(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
        var result = new StringBuilder("ruta,bytes,modificado_utc\r\n");
        foreach (var row in rows)
            result.Append(Quote(row.Ruta)).Append(',').Append(row.Bytes.ToString(CultureInfo.InvariantCulture))
                .Append(',').Append(Quote(row.ModificadoUtc.ToString("O", CultureInfo.InvariantCulture))).Append("\r\n");
        return result.ToString();
    }
}
