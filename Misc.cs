using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageMosaicGenerator
{
    public static class Misc
    {
        public static double ConvertRadiansToDegrees(double radians)
        {
            var degrees = (180 / Math.PI) * radians;
            return (degrees);
        }
        
        public static double ConvertDegreesToRadians (double degrees)
        {
            var radians = (Math.PI / 180) * degrees;
            return (radians);
        }

        public static Color[] BitmapToColorList(Bitmap img)
        {
            var srcData = img.LockBits(
                new Rectangle(0, 0, img.Width, img.Height), 
                ImageLockMode.ReadOnly, 
                PixelFormat.Format32bppArgb);

            var stride = srcData.Stride;

            var Scan0 = srcData.Scan0;

            var fullColor = new int[] {0,0,0};
            var outColor = new Color[img.Width * img.Height];

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
                            fullColor[color] = p[idx];
                        }

                        outColor[img.Width * y + x] = Color.FromArgb(fullColor[0], fullColor[1], fullColor[2]);
                    }
                }
            }

            return outColor;
        }
    }
}