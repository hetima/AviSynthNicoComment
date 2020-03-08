using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

using NCNicoNico.Comment;
using NCNicoNico.Comment.Primitive;

using NCEmoji.Wpf;
using System.Runtime.InteropServices;
using System.Windows.Media;
//using System.Windows.Media.Imaging;


namespace NicoCommentCS
{

    public class Filter : NCNicoNico.Comment.CommentPresenter.ICanvas
    {
        string filePath;
        CommentPresenter cmp;

        public double CanvasWidth { get; set; }
        public double CanvasHeight { get; set; }
        public int RowHeight { get; set; }
        public double FontSize { get; set; }
        public CommentPresenter.Baseline Baseline { get; set; }
        public CommentPresenter.FontKind FontKind { get; set; }
        public double CurrentSecond { get; set; }
        public double Duration { get; set; }
        public Font Font { get; set; }
        public Font FontM { get; set; }
        public float EmSize { get; set; }
        public Font DebugFont { get; set; }

        public string ErrorStr { get; set; }

        private Graphics Scratch;


        private Bitmap Canvas;
        
        //private ColorMatrix AlphaMatrix;
        private ImageAttributes NormalAttr;
        private ImageAttributes AlphaAttr;
        private List<string> DebugInfo;

        public System.Windows.Media.Brush FallbackBrush;

        private List<KeyValuePair<int, Bitmap>> Cache = new List<KeyValuePair<int, Bitmap>>(128);
        private int PermCacheNo;
        private Bitmap PermCache;

        public Filter(double width, double height, double duration, int rowHeight)
        {
            CanvasWidth = width;
            CanvasHeight = height;
            Duration = duration;
            RowHeight = rowHeight;
            FontSize = rowHeight*0.7;
            //Font= new Font("MS Pゴシック", (float)FontSize);
            //Font = new Font("MS PGothic", (float)FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
            Font = new Font("BIZ UDPGothic", (float)FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
            FontM = new Font("BIZ UDMincho Medium", (float)FontSize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
            EmSize = (float)Font.Height * Font.FontFamily.GetEmHeight(Font.Style) / Font.FontFamily.GetLineSpacing(Font.Style);

            ErrorStr = "Comment file not specified";
            cmp = new CommentPresenter();

            AlphaAttr = new ImageAttributes();
            AlphaAttr.SetColorMatrix(new ColorMatrix()
            {
                Matrix00 = 1,
                Matrix11 = 1,
                Matrix22 = 1,
                Matrix33 = 0.6F,
                Matrix44 = 1
            });
            NormalAttr = new ImageAttributes();
            NormalAttr.SetColorMatrix(new ColorMatrix()
            {
                Matrix00 = 1,
                Matrix11 = 1,
                Matrix22 = 1,
                Matrix33 = 0.9F,
                Matrix44 = 1
            });
            Bitmap testBitmap = new Bitmap(40, 30);
            Scratch = Graphics.FromImage(testBitmap);


            DebugFont = new Font("MS PGothic", 24);


            FallbackBrush = System.Windows.Media.Brushes.White;
            //FallbackBrush.Opacity = 0;


        }

        public unsafe void LoadFilePathWithCString(byte* s, int vposShift, int startPos, int endPos)
        {
            //何かセットしとかないと読み込めなかったとき落ちる
            cmp.Comments = new Comment[0];

            filePath = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((System.IntPtr)s);
            if (File.Exists(filePath))
            {
                try
                {
                    cmp.Load(filePath, vposShift, startPos, endPos);
                    if (cmp.Comments.Length == 0)
                    {
                        ErrorStr = "No comments found in file";
                    }
                    else
                    {
                        ErrorStr = "";
                    }
                }
                catch (Exception e)
                {
                    ErrorStr = "Couldn't load comment file:" + Environment.NewLine + e.Message;
                }
            }
            else
            {
                ErrorStr = "Comment file not found";
            }
        }

        public NCSize MeasureText(Comment comment)
        {
            SizeF siz = Scratch.MeasureString(comment.Text, Font);
            return new NCSize(siz.Width, siz.Height);
        }

        private Bitmap GenerateCommentBitmap(Comment cmt, Font font, float emSize)
        {
            CommentRenderer cell = new CommentRenderer(cmt, font, emSize);
            Bitmap b = cell.RenderImage(this);
            if (b != null)
            {
                b = DropShadow.AppendDropShadow(b, 3, 2, 1, 0.7f);
            }
            return b;
        }
        public void DoDraw()
        {
            DebugInfo = new List<string>();
            Graphics gr = Graphics.FromImage(Canvas);

            FrameData result = cmp.GetFrameData(this);

            if(result.Perm != null)
            {
                Bitmap b=null;
                if (PermCacheNo == result.Perm.Comment.Count && PermCache != null)
                {
                    b = PermCache;
                }
                else
                {
                    b = GenerateCommentBitmap(result.Perm.Comment, Font, EmSize);
                    if (b != null)
                    {
                        PermCacheNo=result.Perm.Comment.Count;
                        PermCache = b;
                    }
                }
                if (b != null)
                {
                    Rectangle rect = new Rectangle((int)0, (int)0, b.Width, b.Height);
                    gr.DrawImage(b, rect, 0, 0, b.Width, b.Height, GraphicsUnit.Pixel, NormalAttr);
                }
            }

            int cacheHit = 0;
            int create = 0;
            foreach (CommentLayout item in result.Layouts)
            {
                Comment cmt = item.Comment;

                Bitmap b = CachedBitmap(cmt.Count);
                if (b == null)
                {
                    Font font = Font;
                    if (cmt.ContainsCommand("mincho")) font = FontM;
                    b = GenerateCommentBitmap(cmt, font, EmSize);
                    if (b != null) {
                        AddToCache(b, cmt.Count);
                    }
                    create++;
                }
                else
                {
                    cacheHit++;
                }
                if (b == null) continue;

                Rectangle rect = new Rectangle((int)item.Coordinate.X, (int)item.Coordinate.Y, b.Width, b.Height);

                if (cmt.IsAA)
                {
                    rect.Y = 0;
                    if (b.Height < CanvasHeight)
                    {
                        rect.Y = (int)(CanvasHeight - b.Height) / 2;
                        //rect.Width = (int)(CanvasHeight / (double)b.Height * (double)b.Width);
                        //rect.Height = (int)CanvasHeight;
                    }
                    gr.DrawImage(b, rect, 0, 0, b.Width, b.Height, GraphicsUnit.Pixel, NormalAttr);
                }
                else
                {
                    if (item.OverFlow)
                    {
                        gr.DrawImage(b, rect, 0, 0, b.Width, b.Height, GraphicsUnit.Pixel, AlphaAttr);
                    }
                    else
                    {
                        gr.DrawImage(b, rect, 0, 0, b.Width, b.Height, GraphicsUnit.Pixel, NormalAttr);
                        //Gr.DrawImage(b, (float)item.Coordinate.X, (float)item.Coordinate.Y);
                    }
                }
            }

            //DebugInfo.Add("Cache:" + cacheHit.ToString());
            //DebugInfo.Add("create:" + create.ToString());
            //DebugInfo.Add( CurrentSecond.ToString());

            if (ErrorStr.Length > 0)
            {
                DebugInfo.Add(ErrorStr);
            }
            if (DebugInfo.Count > 0)
            {
                string debug = string.Join(Environment.NewLine, DebugInfo.ToArray());
                gr.DrawString(debug, DebugFont, System.Drawing.Brushes.White, 0, 0);
            }

            gr.Dispose();
        }

        private Bitmap CachedBitmap(int no)
        {
            foreach (var item in Cache)
            {
                if (item.Key == no)
                {
                    return item.Value;
                }
            }
            return null;
        }
        private void AddToCache(Bitmap bm, int no)
        {
            if (Cache.Count >= 120)
            {
                Cache.RemoveRange(0, 30);
            }
            Cache.Add(new KeyValuePair<int, Bitmap>(no, bm));
        }


        public unsafe void DrawFrame(byte* dstp, double position) {
            Canvas = new Bitmap((int)CanvasWidth, (int)CanvasHeight);
            CurrentSecond = position;

            try
            {
                DoDraw();
            }
            catch (Exception e)
            {
                throw;
            }

            WriteToAVSPtr(Canvas, dstp);

            Canvas.Dispose();
            Canvas = null;
        }

        public unsafe void WriteToAVSPtr(Bitmap bm, byte* dstp)
        {
            BitmapData raw = bm.LockBits(new Rectangle(0, 0, (int)bm.Width, (int)bm.Height), ImageLockMode.ReadOnly,                 System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int size = raw.Height * raw.Stride;
            byte[] arr = new byte[size];
            System.Runtime.InteropServices.Marshal.Copy(raw.Scan0, arr, 0, size);

            for (int y = raw.Height-1; y >= 0; y--)
            {
                System.Runtime.InteropServices.Marshal.Copy(arr, raw.Stride * y, (IntPtr)dstp, raw.Stride);
                dstp += raw.Stride;
            }

            bm.UnlockBits(raw);
        }

    }
}
