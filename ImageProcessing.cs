using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageMosaicGenerator
{
    public class ImageProcessing
    {
        // Credit to mpen, https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            

            destImage.SetResolution(image.Height, image.Width);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width,image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        
        // Credit to Philippe Leybaert, https://stackoverflow.com/questions/1068373/how-to-calculate-the-average-rgb-color-values-of-a-bitmap
        public static Color AverageImageColor(Bitmap img)
        {
            BitmapData srcData = img.LockBits(
                new Rectangle(0, 0, img.Width, img.Height), 
                ImageLockMode.ReadOnly, 
                PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            long[] totals = new long[] {0,0,0};

            int width = img.Width;
            int height = img.Height;

            unsafe
            {
                byte* p = (byte*) (void*) Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int color = 0; color < 3; color++)
                        {
                            int idx = (y*stride) + x*4 + color;

                            totals[color] += p[idx];
                        }
                    }
                }
            }

            int avgB = (int)(totals[0] / (width*height));
            int avgG = (int)(totals[1] / (width*height));
            int avgR = (int)(totals[2] / (width*height));
            
            return Color.FromArgb(avgR, avgG, avgB);
        }
    }
}