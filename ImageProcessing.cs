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
            
            img.UnlockBits(srcData);
            img.Dispose();
            
            return Color.FromArgb(avgR, avgG, avgB);
        }

        // This function is based on https://github.com/sumtype/CIEDE2000/blob/master/ciede2000.py
        public double ciede2000(double[] colorA, double[] colorB)
        {
            double L1 = colorA[0];
            double A1 = colorA[1];
            double B1 = colorA[2];
            double L2 = colorB[0];
            double A2 = colorB[1];
            double B2 = colorB[2];

            double C1 = Math.Sqrt(Math.Pow(A1, 2.0) + Math.Pow(B1, 2.0));
            double C2 = Math.Sqrt(Math.Pow(A2, 2.0) + Math.Pow(B2, 2.0));

            double aC1C2 = (C1 + C2) / 2.0;

            double G = 0.5 * (1.0 - Math.Sqrt(Math.Pow(aC1C2, 7.0) / (Math.Pow(aC1C2, 7.0) + Math.Pow(25.0, 7.0))));
            
            double a1P = (1.0 + G) * A1;
            double a2P = (1.0 + G) * A2;

            double c1P = Math.Sqrt(Math.Pow(a1P, 2.0) + Math.Pow(B1, 2.0));
            double c2P = Math.Sqrt(Math.Pow(a2P, 2.0) + Math.Pow(B2, 2.0));
            
            double h1P = 0;
            if (a1P == 0 && B1 == 0)
                h1P = 0;
            else
                if (B1 >= 0)
                    h1P = Misc.ConvertRadiansToDegrees(Math.Atan2(B1, a1P));
                else
                    h1P = Misc.ConvertRadiansToDegrees(Math.Atan2(B1, a1P)) + 360.0;

            
            double h2P = 0;
            if (a2P == 0 && B2 == 0)
                h2P = 0;
            else
                if (B2 >= 0)
                    h2P = Misc.ConvertRadiansToDegrees(Math.Atan2(B2, a2P));
                else
                    h2P = Misc.ConvertRadiansToDegrees(Math.Atan2(B2, a2P)) + 360.0;

            double dLP = L2 - L1;
            double dCP = c2P - c1P;

            int dhC;
            if (h2P - h1P > 180)
                dhC = 1;
            else if (h2P - h1P < -180)
                dhC = 2;
            else
                dhC = 0;

            double dhP = 0;
            if (dhC == 0)
                dhP = h2P - h1P;
            else if (dhC == 1)
                dhP = h2P - h1P - 360.0;
            else
                dhP = h2P + 360 - h1P;
            
            double dHP = 2.0 * Math.Sqrt(c1P * c2P) * Math.Sin(Misc.ConvertDegreesToRadians(dhP / 2.0));

            double aL = (L1 + L2) / 2.0;
            double aCP = (c1P + c2P) / 2.0;

            int haC;
            if (c1P * c2P == 0)
                haC = 3;
            else if (Math.Abs(h2P - h1P) <= 180)
                haC = 0;
            else if (h2P + h1P < 360)
                haC = 1;
            else
                haC = 2;

            double haP = (h1P + h2P) / 2.0;

            double aHP;
            if (haC == 3)
                aHP = h1P + h2P;
            else if (haC == 0)
                aHP = haP;
            else if (haC == 1)
                aHP = haP + 180;
            else
                aHP = haP - 180;
            
            double lPa50 = Math.Pow((aL - 50), 2.0);
            double sL = 1.0 + (0.015 * lPa50 / Math.Sqrt(20.0 + lPa50));
            double sC = 1.0 + 0.045 * aCP;

            double T = 1.0 - 0.17 * Math.Cos(Misc.ConvertDegreesToRadians(aHP - 30.0)) + 0.24 * Math.Cos(Misc.ConvertDegreesToRadians(2.0 * aHP)) +
                0.32 * Math.Cos(Misc.ConvertDegreesToRadians(3.0 * aHP + 6.0)) - 0.2 * Math.Cos(Misc.ConvertDegreesToRadians(4.0 * aHP - 63.0));
                
            double sH = 1.0 + 0.015 * aCP * T;
            double dTheta = 30.0 * Math.Exp(-1.0 * Math.Pow(((aHP - 275.0) / 25.0), 2.0));
            double rC = 2.0 * Math.Sqrt(Math.Pow(aCP, 7.0) / (Math.Pow(aCP, 7.0) + Math.Pow(25.0, 7.0)));
            double rT = -Math.Sin(Misc.ConvertDegreesToRadians(2.0 * dTheta)) * rC;

            double fL = dLP / sL / 1.0;
            double fC = dCP / sC / 1.0;
            double fH = dHP / sH / 1.0;
            double dE2000 = Math.Sqrt(Math.Pow(fL, 2.0) + Math.Pow(fC, 2.0) + Math.Pow(fH, 2.0) + rT * fC * fH);
            return dE2000;
        }
    }
}