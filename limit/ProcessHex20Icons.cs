// Process Hex 2.0 exhibit icons: de-bg + 120x120 + arrow stamp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

public class ProcessHex20Icons
{
    static void Flood(byte[] px, int w, int h, int stride, Func<byte, byte, byte, bool> pred, bool[] mark)
    {
        bool[] visited = new bool[w * h];
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
            if (!pred(px[off + 2], px[off + 1], px[off])) continue;
            mark[i] = true;
            enq(x + 1, y); enq(x - 1, y); enq(x, y + 1); enq(x, y - 1);
        }
    }

    static Color SampleFg(Bitmap bmp, string mode)
    {
        if (mode == "mythic")
        {
            Color best = Color.FromArgb(255, 180, 120, 255);
            int bestSat = -1;
            for (int y = 25; y < 95; y++)
                for (int x = 25; x < 95; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    if (c.A < 200) continue;
                    int sat = Math.Max(c.R, Math.Max(c.G, c.B)) - Math.Min(c.R, Math.Min(c.G, c.B));
                    if (sat > bestSat) { bestSat = sat; best = Color.FromArgb(255, c.R, c.G, c.B); }
                }
            return best;
        }
        int lumMin = mode == "common" ? 160 : 120;
        for (int y = 20; y < 100; y++)
            for (int x = 20; x < 100; x++)
            {
                var c = bmp.GetPixel(x, y);
                if (c.A > 200 && (c.R + c.G + c.B) / 3 > lumMin)
                    return Color.FromArgb(255, c.R, c.G, c.B);
            }
        if (mode == "common") return Color.FromArgb(255, 245, 240, 230);
        if (mode == "uncommon") return Color.FromArgb(255, 200, 205, 215);
        return Color.FromArgb(255, 232, 172, 40);
    }

    static void DrawArrow(Bitmap bmp, bool up, Color fg)
    {
        using (var g = Graphics.FromImage(bmp))
        {
            g.CompositingMode = CompositingMode.SourceCopy;
            using (var clear = new SolidBrush(Color.Transparent))
                g.FillRectangle(clear, 104, 104, 16, 16);
            g.CompositingMode = CompositingMode.SourceOver;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int cx = 111, cy = 111, s = 7;
            Point[] pts = up
                ? new[] { new Point(cx, cy - s), new Point(cx - s, cy + s / 2 + 1), new Point(cx + s, cy + s / 2 + 1) }
                : new[] { new Point(cx, cy + s), new Point(cx - s, cy - s / 2 - 1), new Point(cx + s, cy - s / 2 - 1) };
            using (var brush = new SolidBrush(fg))
                g.FillPolygon(brush, pts);
        }
    }

    public static void Process(string src, string dst, bool upArrow, string mode)
    {
        if (!File.Exists(src))
        {
            Console.WriteLine("MISSING " + src);
            return;
        }
        using (var img = new Bitmap(src))
        {
            int w = img.Width, h = img.Height;
            var rect = new Rectangle(0, 0, w, h);
            var data = img.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int stride = data.Stride;
            byte[] px = new byte[Math.Abs(stride) * h];
            Marshal.Copy(data.Scan0, px, 0, px.Length);
            img.UnlockBits(data);

            int cOff = 2 * stride + 2 * 4;
            int cornerLum = (px[cOff + 2] + px[cOff + 1] + px[cOff]) / 3;
            bool whiteBg = cornerLum >= 230;

            if (whiteBg)
            {
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        int off = y * stride + x * 4;
                        byte b = px[off], g = px[off + 1], r = px[off + 2];
                        int lum = (r + g + b) / 3;
                        int sat = Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b));
                        if (lum >= 247 && sat < 18) { px[off + 3] = 0; }
                        else if (lum >= 240 && sat < 10) { px[off + 3] = (byte)Math.Max(0, (247 - lum) * 36); }
                    }
            }
            else
            {
                bool[] kill = new bool[w * h];
                Flood(px, w, h, stride, (r, g, b) =>
                {
                    int lum = (r + g + b) / 3;
                    int sat = Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b));
                    return lum <= 42 && sat < 35;
                }, kill);
                for (int i = 0; i < kill.Length; i++)
                {
                    if (!kill[i]) continue;
                    int x = i % w, y = i / w;
                    px[y * stride + x * 4 + 3] = 0;
                }
            }

            using (var tmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                var tdata = tmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                byte[] outPx = new byte[Math.Abs(tdata.Stride) * h];
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        int off = y * stride + x * 4;
                        int toff = y * tdata.Stride + x * 4;
                        outPx[toff] = px[off];
                        outPx[toff + 1] = px[off + 1];
                        outPx[toff + 2] = px[off + 2];
                        outPx[toff + 3] = px[off + 3];
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
                    DrawArrow(final, upArrow, SampleFg(final, mode));
                    Directory.CreateDirectory(Path.GetDirectoryName(dst));
                    if (File.Exists(dst)) File.Delete(dst);
                    final.Save(dst, ImageFormat.Png);
                }
            }
            Console.WriteLine("OK " + Path.GetFileName(dst) + (whiteBg ? " (whiteKey)" : "") + " [" + mode + "]");
        }
    }

    static void Go(string assets, string root, string rarity, string polarity, string name, bool up, string mode)
    {
        Process(
            Path.Combine(assets, name + ".png"),
            Path.Combine(root, rarity, polarity, name + ".png"),
            up, mode);
    }

    public static void Main()
    {
        string assets = @"C:\Users\taole\.cursor\projects\d-riderProject-hextech\assets";
        string root = @"D:\riderProject\hextech\Resources\Exhibits";

        Go(assets, root, "Common", "Positive", "MeiWeiYueBing", true, "common");
        Go(assets, root, "Common", "Positive", "ShenEnHuiXiang", true, "common");
        Go(assets, root, "Common", "Negative", "ChunHuaZuZhou", false, "common");

        Go(assets, root, "Uncommon", "Positive", "ChenXiBiHu", true, "uncommon");
        Go(assets, root, "Uncommon", "Positive", "LiuLianGuanTou", true, "uncommon");
        Go(assets, root, "Uncommon", "Positive", "LingFeiChouQian", true, "uncommon");
        Go(assets, root, "Uncommon", "Positive", "WanSeZhangBen", true, "uncommon");
        Go(assets, root, "Uncommon", "Negative", "DiFangFeiZao", false, "uncommon");
        Go(assets, root, "Uncommon", "Negative", "HuiHeDianJi", false, "uncommon");
        Go(assets, root, "Uncommon", "Negative", "HongTaiJiaQian", false, "uncommon");

        Go(assets, root, "Rare", "Positive", "DaiDuoJieJie", true, "rare");
        Go(assets, root, "Rare", "Positive", "ShangJieLiJin", true, "rare");
        Go(assets, root, "Rare", "Negative", "JiBenFenLie", false, "rare");
        Go(assets, root, "Rare", "Negative", "LaoYinPaiBei", false, "rare");
        Go(assets, root, "Rare", "Negative", "ShouJiKuaiMen", false, "rare");
        Go(assets, root, "Rare", "Negative", "ChuangShangKeDao", false, "rare");
        Go(assets, root, "Rare", "Negative", "ErChongJiaKe", false, "rare");
        Go(assets, root, "Rare", "Negative", "DanSeQiYue", false, "rare");
        Go(assets, root, "Rare", "Negative", "WuRanJiaJu", false, "rare");

        Go(assets, root, "Mythic", "Positive", "SanJieZengLi", true, "mythic");
        Go(assets, root, "Mythic", "Negative", "XueYanYin", false, "mythic");
        Go(assets, root, "Mythic", "Negative", "GanHeShengQuan", false, "mythic");
        Go(assets, root, "Mythic", "Negative", "YouShangXieYi", false, "mythic");

        Console.WriteLine("DONE");
    }
}
