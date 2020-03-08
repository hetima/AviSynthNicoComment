using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace NicoCommentCS
{
    public class DropShadow
    {

        // http://takabosoft.com/20101213073511.html
        /// <summary>
        /// ドロップシャドウを加えた新しいビットマップを作成します。
        /// </summary>
        /// <param name="srcBmp">対象のビットマップ</param>
        /// <param name="blur">ぼかし</param>
        /// <param name="distance">影の距離</param>
        /// <returns>ドロップシャドウが加えられた新しいビットマップ</returns>
        public static Bitmap AppendDropShadow(Bitmap srcBmp, int blur, int distanceX, int distanceY, float alpha /*, out Point baseOffset*/)
        {
            //baseOffset = new Point(0, 0);
            if (srcBmp == null || blur < 0)
            {
                return srcBmp;
            }

            // ドロップシャドウを含めた新しいサイズを算出
            Rectangle srcRect = new Rectangle(0, 0, srcBmp.Width, srcBmp.Height);
            Rectangle shadowRect = srcRect;
            shadowRect.Offset(distanceX, distanceY);
            Rectangle shadowBlurRect = shadowRect;
            shadowBlurRect.Inflate(blur, blur);
            Rectangle destRect = Rectangle.Union(srcRect, shadowBlurRect);
            //baseOffset.X = destRect.X - srcRect.X;
            //baseOffset.Y = destRect.Y - srcRect.Y;

            Bitmap destBmp = new Bitmap(destRect.Width, destRect.Height, PixelFormat.Format32bppArgb);

            // 影部分をレンダリングする
            BitmapData destBmpData = destBmp.LockBits(new Rectangle(0, 0, destRect.Width, destRect.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData srcBmpData = srcBmp.LockBits(srcRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* destLine = (byte*)destBmpData.Scan0 + (shadowBlurRect.Y - destRect.Y) * destBmpData.Stride + (shadowBlurRect.X - destRect.X) * 4;
                byte* srcBeginLine = (byte*)srcBmpData.Scan0 + (-blur - blur) * srcBmpData.Stride + (-blur - blur) * 4;

                int destWidth = shadowBlurRect.Width;
                int destHeight = shadowBlurRect.Height;

                int srcWidth = srcBmp.Width;
                int srcHeight = srcBmp.Height;

                int div = (1 + blur + blur) * (1 + blur + blur);

                byte r = 0;
                byte g = 0;
                byte b = 0;

                int destStride = destBmpData.Stride;
                int srcStride = srcBmpData.Stride;

                for (int destY = 0; destY < destHeight; destY++, destLine += destStride, srcBeginLine += srcStride)
                {
                    byte* dest = destLine;
                    byte* srcBegin = srcBeginLine;
                    for (int destX = 0; destX < destWidth; destX++, dest += 4, srcBegin += 4)
                    {
                        // α値をぼかす
                        int total = 0;
                        byte* srcLine = srcBegin;
                        for (int srcY = destY - blur - blur; srcY <= destY; srcY++, srcLine += srcStride)
                        {
                            if (srcY >= 0 && srcY < srcHeight)
                            {
                                byte* src = srcLine;
                                for (int srcX = destX - blur - blur; srcX <= destX; srcX++, src += 4)
                                {
                                    if (srcX >= 0 && srcX < srcWidth)
                                    {
                                        total += src[3];
                                    }
                                }
                            }
                        }

                        dest[0] = b;
                        dest[1] = g;
                        dest[2] = r;
                        dest[3] = (byte)((total / div) * alpha);
                    }
                }
            }

            srcBmp.UnlockBits(srcBmpData);
            destBmp.UnlockBits(destBmpData);

            // 元の画像を重ねる
            using (Graphics g = Graphics.FromImage(destBmp))
            {
                g.DrawImage(srcBmp, srcRect.X - destRect.X, srcRect.Y - destRect.Y, srcBmp.Width, srcBmp.Height);
            }

            return destBmp;
        }

    }
}
