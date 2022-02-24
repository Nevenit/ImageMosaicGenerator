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

        public static PixelColorAndPosition[] BitmapToColorList(Bitmap img)
        {
            var srcData = img.LockBits(
                new Rectangle(0, 0, img.Width, img.Height), 
                ImageLockMode.ReadOnly, 
                PixelFormat.Format32bppArgb);

            var stride = srcData.Stride;

            var Scan0 = srcData.Scan0;

            var fullColor = new int[] {0,0,0};
            PixelColorAndPosition[] output = new PixelColorAndPosition[img.Width * img.Height];
            //var outColor = new Color[img.Width * img.Height];

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

                        output[img.Width * y + x] = new PixelColorAndPosition(ColorConversion.RGBtoCIELAB(Color.FromArgb(fullColor[2], fullColor[1], fullColor[0])), new int[2] { x, y });
                        //outColor[img.Width * y + x] = Color.FromArgb(fullColor[2], fullColor[1], fullColor[0]);
                    }
                }
            }

            return output;
        }

        public static ImagePathColor FindClosesColor(double[] originalColor, ImagePathColor[] possibleColors)
        {
            // Define the variables with a starting value just so we can compare against them
            ImagePathColor output = possibleColors[0];
            double lastDifference = ImageProcessing.ColorDifference(originalColor, possibleColors[0].ImageColor);

            foreach (ImagePathColor possibleColor in possibleColors)
            {
                double colorDifference = ImageProcessing.ColorDifference(originalColor, possibleColor.ImageColor);

                if (colorDifference < lastDifference)
                {
                    output = possibleColor;
                    lastDifference = colorDifference;
                }
            }

            return output;
        }
    }
}