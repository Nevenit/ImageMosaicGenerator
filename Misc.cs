using System;

namespace ImageMosaicGenerator
{
    public class Misc
    {
        public static double ConvertRadiansToDegrees(double radians)
        {
            double degrees = (180 / Math.PI) * radians;
            return (degrees);
        }
        
        public static double ConvertDegreesToRadians (double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }
    }
}