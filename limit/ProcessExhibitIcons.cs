// Temporary: flood-fill de-bg + resize exhibit icons to 120x120
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

class ProcessExhibitIcons
{
    static bool IsBg(byte r, byte g, byte b)
    {
        int lum = (r + g + b) / 3;
        int sat = Math.Max(r, Math.Max(g, b)) - Math.Min(r, Math.Min(g, b));
        return (lum <= 40 && sat < 30) || (lum >= 235 && sat < 25) || (lum >= 220 && sat < 12);
    }

    static void ProcessOne(string src, string dst)
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
                if (!IsBg(r, g, b)) continue;
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
                        else
                        {
                            int lum = (rr + gg + bb) / 3;
                            int sat = Math.Max(rr, Math.Max(gg, bb)) - Math.Min(rr, Math.Min(gg, bb));
                            if (lum <= 55 && sat < 30) aa = (byte)Math.Max(0, Math.Min(255, lum * 4));
                        }
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
                    Directory.CreateDirectory(Path.GetDirectoryName(dst));
                    if (File.Exists(dst)) File.Delete(dst);
                    final.Save(dst, ImageFormat.Png);
                }
            }
        }
        Console.WriteLine("OK " + Path.GetFileName(dst));
    }

    static void Main()
    {
        string srcRoot = @"C:\Users\taole\.cursor\projects\d-riderProject-hextech\assets";
        string pos = @"D:\riderProject\hextech\Resources\Exhibits\Common\Positive";
        string neg = @"D:\riderProject\hextech\Resources\Exhibits\Common\Negative";
        string[] positives = {
            "SheShenYuShou","HeTongFeizao","JinBoTuanzi","TieShenJinNang","XiMoDiShi",
            "TidengHuozhong","YongYuanTingShangYao","ShuangShengRongQiu","BaoJiejie","JianFeiFuZhou"
        };
        string[] negatives = {
            "DiJunYuShou","CiYanShanGuangDeng","WuShangJiangZhang","GuoQiHaoWai","LieFengDunMian",
            "DunKouChaiDao","LouDiQianDai","GeYeCha","CaiZhiRen","ZhaiXiuKou"
        };
        foreach (var n in positives) ProcessOne(Path.Combine(srcRoot, n + ".png"), Path.Combine(pos, n + ".png"));
        foreach (var n in negatives) ProcessOne(Path.Combine(srcRoot, n + ".png"), Path.Combine(neg, n + ".png"));
        Console.WriteLine("DONE");
    }
}
