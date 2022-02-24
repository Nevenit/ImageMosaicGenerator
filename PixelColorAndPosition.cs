using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMosaicGenerator
{
    public class PixelColorAndPosition
    {
        public double[] color;
        public int[] position;

        public PixelColorAndPosition(double[] _pixelColor, int[] _pos)
        {
            color = _pixelColor;
            position = _pos;
        }
    }
}
