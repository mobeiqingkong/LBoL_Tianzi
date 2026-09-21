using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
public class One {
  static void Flood(byte[] px, int w, int h, int stride, Func<byte, byte, byte, bool> pred, bool[] mark) {
    bool[] visited = new bool[w * h];
    var q = new Queue<int>();
    Action<int,int> enq = (x,y) => { if(x<0||y<0||x>=w||y>=h) return; int i=y*w+x; if(visited[i]) return; visited[i]=true; q.Enqueue(i); };
    for(int x=0;x<w;x++){enq(x,0);enq(x,h-1);} for(int y=0;y<h;y++){enq(0,y);enq(w-1,y);}
    while(q.Count>0){ int i=q.Dequeue(); int x=i%w,y=i/w; int off=y*stride+x*4; if(!pred(px[off+2],px[off+1],px[off])) continue; mark[i]=true; enq(x+1,y);enq(x-1,y);enq(x,y+1);enq(x,y-1); }
  }
  static Color SampleFg(Bitmap bmp) {
    Color best=Color.FromArgb(255,180,120,255); int bestSat=-1;
    for(int y=25;y<95;y++) for(int x=25;x<95;x++){ var c=bmp.GetPixel(x,y); if(c.A<200) continue; int sat=Math.Max(c.R,Math.Max(c.G,c.B))-Math.Min(c.R,Math.Min(c.G,c.B)); if(sat>bestSat){bestSat=sat;best=Color.FromArgb(255,c.R,c.G,c.B);} }
    return best;
  }
  static void DrawArrow(Bitmap bmp, Color fg) {
    using(var g=Graphics.FromImage(bmp)){
      g.CompositingMode=CompositingMode.SourceCopy; using(var clear=new SolidBrush(Color.Transparent)) g.FillRectangle(clear,104,104,16,16);
      g.CompositingMode=CompositingMode.SourceOver; g.SmoothingMode=SmoothingMode.AntiAlias;
      int cx=111,cy=111,s=7;
      var pts=new[]{new Point(cx,cy+s),new Point(cx-s,cy-s/2-1),new Point(cx+s,cy-s/2-1)};
      using(var brush=new SolidBrush(fg)) g.FillPolygon(brush,pts);
    }
  }
  public static void Main(){
    string src=@"C:\Users\taole\.cursor\projects\d-riderProject-hextech\assets\HuoBuDanXing.png";
    string dst=@"D:\riderProject\hextech\Resources\Exhibits\Mythic\Negative\HuoBuDanXing.png";
    using(var img=new Bitmap(src)){
      int w=img.Width,h=img.Height; var rect=new Rectangle(0,0,w,h);
      var data=img.LockBits(rect,ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb); int stride=data.Stride; byte[] px=new byte[Math.Abs(stride)*h];
      Marshal.Copy(data.Scan0,px,0,px.Length); img.UnlockBits(data);
      bool[] kill=new bool[w*h];
      Flood(px,w,h,stride,(r,g,b)=>{ int lum=(r+g+b)/3; int sat=Math.Max(r,Math.Max(g,b))-Math.Min(r,Math.Min(g,b)); return lum<=42&&sat<35; }, kill);
      for(int i=0;i<kill.Length;i++) if(kill[i]){ int x=i%w,y=i/w; px[y*stride+x*4+3]=0; }
      using(var tmp=new Bitmap(w,h,PixelFormat.Format32bppArgb)){
        var tdata=tmp.LockBits(rect,ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb); byte[] outPx=new byte[Math.Abs(tdata.Stride)*h];
        for(int y=0;y<h;y++) for(int x=0;x<w;x++){ int off=y*stride+x*4; int toff=y*tdata.Stride+x*4; outPx[toff]=px[off]; outPx[toff+1]=px[off+1]; outPx[toff+2]=px[off+2]; outPx[toff+3]=px[off+3]; }
        Marshal.Copy(outPx,0,tdata.Scan0,outPx.Length); tmp.UnlockBits(tdata);
        using(var final=new Bitmap(120,120,PixelFormat.Format32bppArgb))
        using(var g=Graphics.FromImage(final)){
          g.CompositingMode=CompositingMode.SourceCopy; g.InterpolationMode=InterpolationMode.HighQualityBicubic; g.Clear(Color.Transparent);
          g.DrawImage(tmp,new Rectangle(0,0,120,120),new Rectangle(0,0,w,h),GraphicsUnit.Pixel);
          DrawArrow(final, SampleFg(final));
          Directory.CreateDirectory(Path.GetDirectoryName(dst));
          if(File.Exists(dst)) File.Delete(dst);
          final.Save(dst, ImageFormat.Png);
        }
      }
    }
    Console.WriteLine("OK");
  }
}
