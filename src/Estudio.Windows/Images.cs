using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Estudio.Windows;

internal static class Images
{
    static Image Open(string path)
    {
        var image = Image.FromFile(path);
        try
        {
            if ((long)image.Width * image.Height > 40_000_000)
                throw new ArgumentException("La imagen supera 40 megapíxeles.");
            if (image.PropertyIdList.Contains(0x112))
            {
                var item = image.GetPropertyItem(0x112);
                int orientation = item?.Value is { Length: >= 2 } value ? BitConverter.ToUInt16(value, 0) : 1;
                var flip = orientation switch
                {
                    2 => RotateFlipType.RotateNoneFlipX, 3 => RotateFlipType.Rotate180FlipNone,
                    4 => RotateFlipType.Rotate180FlipX, 5 => RotateFlipType.Rotate90FlipX,
                    6 => RotateFlipType.Rotate90FlipNone, 7 => RotateFlipType.Rotate270FlipX,
                    8 => RotateFlipType.Rotate270FlipNone, _ => RotateFlipType.RotateNoneFlipNone
                };
                image.RotateFlip(flip);
            }
            return image;
        }
        catch { image.Dispose(); throw; }
    }

    static void Save(Bitmap image, string path)
    {
        if (!Path.GetExtension(path).Equals(".png", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("El destino debe terminar en .png.");
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        image.Save(stream, ImageFormat.Png);
    }

    public static void Resize(string source, string target, int maximumWidth)
    {
        using var image = Open(source);
        int width = Math.Min(maximumWidth, image.Width);
        int height = Math.Max(1, (int)Math.Round(image.Height * (double)width / image.Width));
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(image, 0, 0, width, height);
        Save(bitmap, target);
    }

    public static void ContactSheet(string folder, string target, int columns)
    {
        var extensions = new HashSet<string>([".png", ".jpg", ".jpeg", ".bmp", ".gif"], StringComparer.OrdinalIgnoreCase);
        var paths = Directory.EnumerateFiles(folder).Where(path => extensions.Contains(Path.GetExtension(path)))
            .Where(path => !Path.GetFullPath(path).Equals(Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).Take(60).ToArray();
        if (paths.Length == 0) throw new ArgumentException("No hay imágenes compatibles en esta carpeta.");
        const int cellWidth = 256, cellHeight = 210;
        int rows = (paths.Length + columns - 1) / columns;
        using var canvas = new Bitmap(columns * cellWidth, rows * cellHeight);
        using var graphics = Graphics.FromImage(canvas);
        graphics.Clear(Color.FromArgb(18, 31, 40));
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        using var font = new Font("Segoe UI", 9);
        for (int i = 0; i < paths.Length; i++)
        {
            using var image = Open(paths[i]);
            double ratio = Math.Min((cellWidth - 24d) / image.Width, (cellHeight - 48d) / image.Height);
            int width = Math.Max(1, (int)(image.Width * ratio)), height = Math.Max(1, (int)(image.Height * ratio));
            int x = i % columns * cellWidth, y = i / columns * cellHeight;
            graphics.DrawImage(image, x + (cellWidth - width) / 2, y + 12, width, height);
            var label = Path.GetFileName(paths[i]);
            if (label.Length > 30) label = label[..27] + "…";
            graphics.DrawString(label, font, Brushes.WhiteSmoke, new PointF(x + 12, y + cellHeight - 27));
        }
        Save(canvas, target);
    }

    public static void Gradient(string target, Color top, Color bottom, int width, int height)
    {
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        using var brush = new LinearGradientBrush(new Rectangle(0, 0, width, height), top, bottom, LinearGradientMode.Vertical);
        graphics.FillRectangle(brush, 0, 0, width, height);
        Save(bitmap, target);
    }
}
