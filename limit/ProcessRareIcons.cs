// Process rare gold exhibit icons
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

public class ProcessRareIcons
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

    static Color SampleFg(Bitmap bmp)
    {
        for (int y = 20; y < 100; y++)
            for (int x = 20; x < 100; x++)
            {
                var c = bmp.GetPixel(x, y);
                if (c.A > 200 && (c.R + c.G + c.B) / 3 > 120)
                    return Color.FromArgb(255, c.R, c.G, c.B);
            }
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

    public static void Process(string src, string dst, bool upArrow)
    {
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
            bool whiteBg = cornerLum >= 240;

            if (whiteBg)
            {
                // global near-white paper key (gold icons on white)
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
                    DrawArrow(final, upArrow, SampleFg(final));
                    Directory.CreateDirectory(Path.GetDirectoryName(dst));
                    if (File.Exists(dst)) File.Delete(dst);
                    final.Save(dst, ImageFormat.Png);
                }
            }
            Console.WriteLine("OK " + Path.GetFileName(dst) + (whiteBg ? " (whiteKey)" : ""));
        }
    }

    public static void Run()
    {
        string assets = @"C:\Users\taole\.cursor\projects\d-riderProject-hextech\assets";
        string pos = @"D:\riderProject\hextech\Resources\Exhibits\Rare\Positive";
        string neg = @"D:\riderProject\hextech\Resources\Exhibits\Rare\Negative";
        string[] positives = {
            "HuoZhuanRen","CaiGouQingDan","DiYuMenYao","KuanXiuHeFu","HuiGuangDaMoTai",
            "KangMieLin","XiShuiXinWenShe","KaiMuErChong","WanMeiKaiMu","PoFangJiejie"
        };
        string[] negatives = {
            "JingGongJia","BaiDongHeXin","YanXiLing","ChouPaiFengKou","QuanMianXiuShi",
            "ShiPaiZhongYan","FaLiFengYin","ChunHuaLvJing","SanShiTieLin","DiFangXuHuo"
        };
        foreach (var n in positives) Process(Path.Combine(assets, n + ".png"), Path.Combine(pos, n + ".png"), true);
        foreach (var n in negatives) Process(Path.Combine(assets, n + ".png"), Path.Combine(neg, n + ".png"), false);
        Console.WriteLine("DONE");
    }
}
