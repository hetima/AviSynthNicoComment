using System;
//using System.Collections;
//using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows;
using NCEmoji.Wpf;
using System.Runtime.InteropServices;
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using NCNicoNico.Comment;
using NCNicoNico.Comment.Primitive;

namespace NicoCommentCS
{

    public class CommentRenderer
    {
        public Comment Cmt { get; set; }
        public Font Font { get; set; }
        public float EmSize { get; set; }
        public System.Drawing.Color Color;


        public CommentRenderer(Comment cmt, Font font, float emSize)
        {
            this.Cmt = cmt;
            this.Font = font;
            this.EmSize = emSize;
            this.Color = System.Drawing.Color.FromArgb(cmt.Color.R, cmt.Color.G, cmt.Color.B);
        }

        public Bitmap RenderImage(Filter filter)
        {
            if (Cmt.IsAA) return RenderAA(filter.CanvasHeight, 0);
            if (Cmt.IsPerm) return RenderAA(filter.RowHeight, filter.CanvasWidth);
            List<Bitmap> ary = BitmapArray();
            Bitmap result = ConcatBitmap(ary);
            if (result != null)
            {
                NCSize realSize = new NCSize(result.Width, result.Height);
                Cmt.Size = realSize;
            }
            return result;
        }

        private List<Bitmap> BitmapArray()
        {
            string text = Cmt.Text;
            int pos = 0;
            double size = Font.Size;
            List<Bitmap> ary = new List<Bitmap>();

            List<EmojiLoc> locs = Cmt.GetEmojiLocs();
            foreach (EmojiLoc m in locs)
            {
                if (m.Index != pos)
                {
                    string normal = text.Substring(pos, m.Index - pos);
                    Bitmap bm = RenderString(normal);
                    ary.Add(bm);
                }
                if (m.Length > 0)
                {
                    string emoji = text.Substring(m.Index, m.Length);
                    Bitmap bm = RenderEmoji(emoji, size);
                    ary.Add(bm);
                }
                pos = m.Index + m.Length;
            }
            string remain = text.Substring(pos);
            if (remain.Length > 0)
            {
                Bitmap bm = RenderString(remain);
                ary.Add(bm);
            }

            return ary;
        }
        private Bitmap RenderEmoji(string text, double size)
        {
            BitmapSource bitmapSource = EmojiRenderer.RenderBitmap(text, size);
            return SourceToBitmap(bitmapSource);
        }

        private Bitmap RenderString(string text)
        {
            using (GraphicsPath gp = new GraphicsPath())
            { 
                gp.AddString(text, Font.FontFamily, (int)System.Drawing.FontStyle.Bold, EmSize, new System.Drawing.Point(0, 0), StringFormat.GenericDefault);
                RectangleF rect = gp.GetBounds();
                Bitmap bm = new Bitmap((int)Math.Ceiling(rect.Width+rect.X)+2+4, (int)Math.Ceiling(rect.Height+ rect.Y)+2+4, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(bm))
                {
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.CompositingQuality = CompositingQuality.HighQuality;

                    using (Brush fillBrush = new SolidBrush(Color))
                    {
                        gr.FillPath(fillBrush, gp);
                    }

                    //gr.DrawRectangle(Pens.Red, rect.X, rect.Y, rect.Width, rect.Height);

                    return bm;
                }
            }
        }


        public Bitmap RenderAA(double maxHeight, double maxWidth)
        {
            //GraphicsPathは空行が考慮されない
            NCSize size = Cmt.Size;
            double scale = 1.0;
            if (maxHeight > 0 && size.Height > maxHeight)
            {
                scale = maxHeight / (double)size.Height;
                size.Width = scale * (double)size.Width;
                size.Height = maxHeight;
            }
            if (maxWidth > 0 && size.Width > maxWidth)
            {
                double scaleX = maxWidth / (double)size.Width;
                size.Height = scaleX * (double)size.Height;
                size.Width = maxWidth;
                scale *= scaleX;
            }
            RectangleF rect = new RectangleF(0, 0, (float)size.Width, (float)size.Height);

            using (var bitmap = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics gr = Graphics.FromImage(bitmap))
                {
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.CompositingQuality = CompositingQuality.HighQuality;
                    gr.ScaleTransform((float)scale, (float)scale);
                    System.Drawing.Brush fillBrush = new System.Drawing.SolidBrush(Color);
                    gr.DrawString(Cmt.Text, Font, fillBrush, 0,0);

                }
                return new Bitmap(bitmap);
            }
        }

        private Bitmap ConcatBitmap(List<Bitmap> ary) {
            int width=0;
            int height=0;
            if (ary.Count == 1)
            {
                return ary[0];
            }

            foreach (Bitmap item in ary)
            {
                width += item.Width;
                if (height < item.Height) height = item.Height;

            }
            if (width == 0) return null;


            int x = 0;
            using (var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics gr = Graphics.FromImage(bitmap))
                {
                    foreach (Bitmap item in ary)
                    {
                        gr.DrawImage(item, x, 0);
                        x += item.Width;
                    }
                }
                return new Bitmap(bitmap);
            }
        }

        private Bitmap SourceToBitmap(BitmapSource bitmapSource)
        {
            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;
            int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            IntPtr intPtr = IntPtr.Zero;
            try
            {
                intPtr = Marshal.AllocCoTaskMem(height * stride);
                bitmapSource.CopyPixels(new Int32Rect(0, 0, width, height), intPtr, height * stride, stride);
                using (var bitmap = new Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, intPtr))
                {
                    return new Bitmap(bitmap);
                }
            }
            finally
            {
                if (intPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(intPtr);
            }
        }

    }
}
