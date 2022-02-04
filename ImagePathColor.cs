using System.Drawing;
using System.IO;

namespace ImageMosaicGenerator
{
    public class ImagePathColor
    {
        public string ImagePath;
        public double[] ImageColor;

        public ImagePathColor(string imagePath, double[] imageColor)
        {
            ImagePath = imagePath;
            ImageColor = imageColor;
        }
    }
}