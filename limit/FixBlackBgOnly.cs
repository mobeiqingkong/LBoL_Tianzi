// Fix: only remove near-black background; never treat cream/white icon rings as bg
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

public class FixBlackBgOnly
{
    static bool IsBlackBg(byte r, byte g, byte b)
    {
        int lum = (r + g + b) / 3;
        int sat = Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b));
        return lum <= 48 && sat < 35;
    }

    static Color SampleFg(Bitmap bmp)
    {
        for (int y = 25; y < 95; y++)
            for (int x = 25; x < 95; x++)
            {
                var c = bmp.GetPixel(x, y);
                if (c.A > 200 && (c.R + c.G + c.B) / 3 > 160)
                    return Color.FromArgb(255, c.R, c.G, c.B);
            }
        return Color.FromArgb(255, 245, 240, 230);
    }

    static void DrawArrow(Bitmap bmp, bool up, Color fg)
    {
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            // clear corner first
            using (var clear = new SolidBrush(Color.Transparent))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.FillRectangle(clear, 92, 92, 28, 28);
            }
            g.CompositingMode = CompositingMode.SourceOver;
            int cx = 108, cy = 108, s = 9;
            Point[] pts = up
                ? new[] { new Point(cx, cy - s), new Point(cx - s, cy + s / 2 + 1), new Point(cx + s, cy + s / 2 + 1) }
                : new[] { new Point(cx, cy + s), new Point(cx - s, cy - s / 2 - 1), new Point(cx + s, cy - s / 2 - 1) };
            using (var brush = new SolidBrush(fg))
                g.FillPolygon(brush, pts);
        }
    }

    public static void Process(string src, string dst, bool upArrow)
    {
        using (var img = new Bitmap(src))
        {
            int w = img.Width, h = img.Height;
            var rect = new Rectangle(0, 0, w, h);
            var data = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            byte[] pixels = new byte[Math.Abs(stride) * h];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
            img.UnlockBits(data);

            bool[] visited = new bool[w * h];
            bool[] kill = new bool[w * h];
            var q = new Queue<int>();
            Action<int, int> enq = (x, y) =>
            {
                if (x < 0 || y < 0 || x >= w || y >= h) return;
                int i = y * w + x;
                if (visited[i]) return;
                visited[i] = true;
                q.Enqueue(i);
            };
            for (int x = 0; x < w; x++) { enq(x, 0); enq(x, h - 1); }
            for (int y = 0; y < h; y++) { enq(0, y); enq(w - 1, y); }

            while (q.Count > 0)
            {
                int i = q.Dequeue();
                int x = i % w, y = i / w;
                int off = y * stride + x * 4;
                byte b = pixels[off], g = pixels[off + 1], r = pixels[off + 2];
                if (!IsBlackBg(r, g, b)) continue;
                kill[i] = true;
                enq(x + 1, y); enq(x - 1, y); enq(x, y + 1); enq(x, y - 1);
            }

            using (var tmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                var tdata = tmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                byte[] outPx = new byte[Math.Abs(tdata.Stride) * h];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int i = y * w + x;
                        int off = y * stride + x * 4;
                        int toff = y * tdata.Stride + x * 4;
                        byte bb = pixels[off], gg = pixels[off + 1], rr = pixels[off + 2], aa = pixels[off + 3];
                        if (kill[i]) aa = 0;
                        outPx[toff] = bb; outPx[toff + 1] = gg; outPx[toff + 2] = rr; outPx[toff + 3] = aa;
                    }
                }
                Marshal.Copy(outPx, 0, tdata.Scan0, outPx.Length);
                tmp.UnlockBits(tdata);

                using (var final = new Bitmap(120, 120, PixelFormat.Format32bppArgb))
                using (var g = Graphics.FromImage(final))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.Clear(Color.Transparent);
                    g.DrawImage(tmp, new Rectangle(0, 0, 120, 120), new Rectangle(0, 0, w, h), GraphicsUnit.Pixel);
                    var fg = SampleFg(final);
                    DrawArrow(final, upArrow, fg);
                    Directory.CreateDirectory(Path.GetDirectoryName(dst));
                    if (File.Exists(dst)) File.Delete(dst);
                    final.Save(dst, ImageFormat.Png);
                }
            }
        }
        Console.WriteLine("OK " + Path.GetFileName(dst));
    }

    public static void Run()
    {
        string assets = @"C:\Users\taole\.cursor\projects\d-riderProject-hextech\assets";
        string pos = @"D:\riderProject\hextech\Resources\Exhibits\Common\Positive";
        string neg = @"D:\riderProject\hextech\Resources\Exhibits\Common\Negative";
        string[] positives = { "HeTongFeizao", "JinBoTuanzi", "TidengHuozhong", "TieShenJinNang", "YongYuanTingShangYao" };
        string[] negatives = { "DiJunYuShou", "CiYanShanGuangDeng", "GuoQiHaoWai", "WuShangJiangZhang" };
        foreach (var n in positives) Process(Path.Combine(assets, n + ".png"), Path.Combine(pos, n + ".png"), true);
        foreach (var n in negatives) Process(Path.Combine(assets, n + ".png"), Path.Combine(neg, n + ".png"), false);
        Console.WriteLine("DONE");
    }
}
