// Temporary: stamp up/down arrow on common exhibit icons
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public class StampExhibitArrows
{
    static Color SampleFg(Bitmap bmp)
    {
        // sample bright opaque pixel near center
        for (int y = 30; y < 90; y++)
            for (int x = 30; x < 90; x++)
            {
                var c = bmp.GetPixel(x, y);
                if (c.A > 200 && (c.R + c.G + c.B) / 3 > 180)
                    return Color.FromArgb(255, c.R, c.G, c.B);
            }
        return Color.FromArgb(255, 245, 240, 230);
    }

    static void ClearCorner(Bitmap bmp)
    {
        // clear bottom-right 28x28 to remove wrong generated arrows
        for (int y = 92; y < 120; y++)
            for (int x = 92; x < 120; x++)
                bmp.SetPixel(x, y, Color.Transparent);
    }

    static void DrawArrow(Bitmap bmp, bool up, Color fg)
    {
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingMode = CompositingMode.SourceOver;
            int cx = 108, cy = 108, s = 9;
            Point[] pts = up
                ? new[] { new Point(cx, cy - s), new Point(cx - s, cy + s / 2 + 1), new Point(cx + s, cy + s / 2 + 1) }
                : new[] { new Point(cx, cy + s), new Point(cx - s, cy - s / 2 - 1), new Point(cx + s, cy - s / 2 - 1) };
            using (var brush = new SolidBrush(fg))
                g.FillPolygon(brush, pts);
        }
    }

    static void Process(string path, bool up)
    {
        using (var src = new Bitmap(path))
        using (var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb))
        using (var g0 = Graphics.FromImage(bmp))
        {
            g0.CompositingMode = CompositingMode.SourceCopy;
            g0.DrawImage(src, 0, 0);
            var fg = SampleFg(bmp);
            ClearCorner(bmp);
            DrawArrow(bmp, up, fg);
            bmp.Save(path + ".tmp", ImageFormat.Png);
        }
        File.Delete(path);
        File.Move(path + ".tmp", path);
        Console.WriteLine((up ? "UP  " : "DOWN") + " " + Path.GetFileName(path));
    }

    public static void Run()
    {
        string pos = @"D:\riderProject\hextech\Resources\Exhibits\Common\Positive";
        string neg = @"D:\riderProject\hextech\Resources\Exhibits\Common\Negative";
        foreach (var f in Directory.GetFiles(pos, "*.png")) Process(f, true);
        foreach (var f in Directory.GetFiles(neg, "*.png")) Process(f, false);
        Console.WriteLine("DONE");
    }
}
