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

            using var graphics = Graphics.FromImage(destImage);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(image, destRect, 0, 0, image.Width,image.Height, GraphicsUnit.Pixel, wrapMode);

            return destImage;
        }
        
        // Credit to Philippe Leybaert, https://stackoverflow.com/questions/1068373/how-to-calculate-the-average-rgb-color-values-of-a-bitmap
        public static Color AverageImageColor(Bitmap img)
        {
            var srcData = img.LockBits(
                new Rectangle(0, 0, img.Width, img.Height), 
                ImageLockMode.ReadOnly, 
                PixelFormat.Format32bppArgb);

            var stride = srcData.Stride;

            var Scan0 = srcData.Scan0;

            var totals = new long[] {0,0,0};

            var width = img.Width;
            var height = img.Height;

            unsafe
            {
                var p = (byte*) (void*) Scan0;

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        for (var color = 0; color < 3; color++)
                        {
                            var idx = (y*stride) + x*4 + color;

                            totals[color] += p[idx];
                        }
                    }
                }
            }

            var avgB = (int)(totals[0] / (width*height));
            var avgG = (int)(totals[1] / (width*height));
            var avgR = (int)(totals[2] / (width*height));
            
            img.UnlockBits(srcData);
            img.Dispose();
            
            return Color.FromArgb(avgR, avgG, avgB);
        }

        public static Bitmap SquareImage(Bitmap img)
        {
            int smallestSide = Math.Min(img.Width, img.Height);
            Rectangle cropArea = new Rectangle((img.Width - smallestSide) / 2, (img.Height - smallestSide) / 2, smallestSide, smallestSide);
            Bitmap croppedImage = new Bitmap(img);
            return croppedImage.Clone(cropArea, croppedImage.PixelFormat);
        }

        // This function is based on https://github.com/sumtype/CIEDE2000/blob/master/ciede2000.py
        public double Ciede2000(double[] colorA, double[] colorB)
        {
            var L1 = colorA[0];
            var A1 = colorA[1];
            var B1 = colorA[2];
            var L2 = colorB[0];
            var A2 = colorB[1];
            var B2 = colorB[2];

            var C1 = Math.Sqrt(Math.Pow(A1, 2.0) + Math.Pow(B1, 2.0));
            var C2 = Math.Sqrt(Math.Pow(A2, 2.0) + Math.Pow(B2, 2.0));

            var aC1C2 = (C1 + C2) / 2.0;

            var G = 0.5 * (1.0 - Math.Sqrt(Math.Pow(aC1C2, 7.0) / (Math.Pow(aC1C2, 7.0) + Math.Pow(25.0, 7.0))));
            
            var a1P = (1.0 + G) * A1;
            var a2P = (1.0 + G) * A2;

            var c1P = Math.Sqrt(Math.Pow(a1P, 2.0) + Math.Pow(B1, 2.0));
            var c2P = Math.Sqrt(Math.Pow(a2P, 2.0) + Math.Pow(B2, 2.0));
            
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

            var dLP = L2 - L1;
            var dCP = c2P - c1P;

            var dhC = (h2P - h1P) switch
            {
                > 180 => 1,
                < -180 => 2,
                _ => 0
            };

            var dhP = dhC switch
            {
                0 => h2P - h1P,
                1 => h2P - h1P - 360.0,
                _ => h2P + 360 - h1P
            };

            var dHP = 2.0 * Math.Sqrt(c1P * c2P) * Math.Sin(Misc.ConvertDegreesToRadians(dhP / 2.0));

            var aL = (L1 + L2) / 2.0;
            var aCP = (c1P + c2P) / 2.0;

            int haC;
            if (c1P * c2P == 0)
                haC = 3;
            else if (Math.Abs(h2P - h1P) <= 180)
                haC = 0;
            else if (h2P + h1P < 360)
                haC = 1;
            else
                haC = 2;

            var haP = (h1P + h2P) / 2.0;

            double aHP = haC switch
            {
                3 => h1P + h2P,
                0 => haP,
                1 => haP + 180,
                _ => haP - 180
            };

            var lPa50 = Math.Pow((aL - 50), 2.0);
            var sL = 1.0 + (0.015 * lPa50 / Math.Sqrt(20.0 + lPa50));
            var sC = 1.0 + 0.045 * aCP;

            var T = 1.0 - 0.17 * Math.Cos(Misc.ConvertDegreesToRadians(aHP - 30.0)) + 0.24 * Math.Cos(Misc.ConvertDegreesToRadians(2.0 * aHP)) +
                0.32 * Math.Cos(Misc.ConvertDegreesToRadians(3.0 * aHP + 6.0)) - 0.2 * Math.Cos(Misc.ConvertDegreesToRadians(4.0 * aHP - 63.0));
                
            var sH = 1.0 + 0.015 * aCP * T;
            var dTheta = 30.0 * Math.Exp(-1.0 * Math.Pow(((aHP - 275.0) / 25.0), 2.0));
            var rC = 2.0 * Math.Sqrt(Math.Pow(aCP, 7.0) / (Math.Pow(aCP, 7.0) + Math.Pow(25.0, 7.0)));
            var rT = -Math.Sin(Misc.ConvertDegreesToRadians(2.0 * dTheta)) * rC;

            var fL = dLP / sL / 1.0;
            var fC = dCP / sC / 1.0;
            var fH = dHP / sH / 1.0;
            var dE2000 = Math.Sqrt(Math.Pow(fL, 2.0) + Math.Pow(fC, 2.0) + Math.Pow(fH, 2.0) + rT * fC * fH);
            return dE2000;
        }
    }
}