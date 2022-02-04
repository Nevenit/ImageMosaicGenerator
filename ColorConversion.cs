using System;
using System.Drawing;

namespace ImageMosaicGenerator
{
    public static class ColorConversion
    {
        // All of these conversions are from https://www.easyrgb.com/en/math.php
        private static double[] RGBtoXYZ(Color color)
        {
            var varR = color.R / 255.0;
            var varG = color.G / 255.0;
            var varB = color.B / 255.0;

            if (varR > 0.04045)
                varR = Math.Pow((varR + 0.055) / 1.055, 2.4);
            else
                varR = varR / 12.92;
            
            if (varG > 0.04045)
                varG = Math.Pow((varG + 0.055) / 1.055, 2.4);
            else
                varG = varG / 12.92;
            
            if (varB > 0.04045)
                varB = Math.Pow((varB + 0.055) / 1.055, 2.4);
            else
                varB = varB / 12.92;

            varR *= 100.0;
            varG *= 100.0;
            varB *= 100.0;

            var X = varR * 0.4124 + varG * 0.3576 + varB * 0.1805;
            var Y = varR * 0.2126 + varG * 0.7152 + varB * 0.0722;
            var Z = varR * 0.0193 + varG * 0.1192 + varB * 0.9505;

            return new double[] { X, Y, Z};
        }

        private static double[] XYZtoCIELab(double[] XYZ)
        {
            var varX = XYZ[0] / 95.047;
            var varY = XYZ[1] / 100.0;
            var varZ = XYZ[2] / 108.883;

            if (varX > 0.008856)
                varX = Math.Pow(varX, 1.0 / 3.0);
            else
                varX = (7.787 * varX) + (16.0 / 116.0);

            if (varY > 0.008856)
                varY = Math.Pow(varY, 1.0 / 3.0);
            else
                varY = (7.787 * varY) + (16.0 / 116.0);

            if (varZ > 0.008856)
                varZ = Math.Pow(varZ, 1.0 / 3.0);
            else
                varZ = (7.787 * varZ) + (16.0 / 116.0);

            var CIEL = (116 * varY) - 16;
            var CIEA = 500 * (varX - varY);
            var CIEB = 200 * (varY - varZ);
            
            return new double[] {CIEL, CIEA, CIEB};
        }

        public static double[] CIELABtoXYZ(double[] CIELAB)
        {
            var varY = (CIELAB[0] + 16) / 116;
            var varX = CIELAB[1] / 500 + varY;
            var varZ = varY - CIELAB[2] / 200;

            if (Math.Pow(varY, 3) > 0.008856)
                varY = Math.Pow(varY, 3);
            else
                varY = (varY - 16.0 / 116.0) / 7.787;

            if (Math.Pow(varX, 3) > 0.008856)
                varX = Math.Pow(varX, 3);
            else
                varX = (varX - 16.0 / 116.0) / 7.787;

            if (Math.Pow(varZ, 3) > 0.008856)
                varZ = Math.Pow(varZ, 3);
            else
                varZ = (varZ - 16.0 / 116.0) / 7.787;

            var X = varX * 95.047;
            var Y = varY * 100.0;
            var Z = varZ * 108.883;

            return new double[] {X, Y, Z};
        }

        private static Color XYZtoRGB(double[] XYZ)
        {
            var varX = XYZ[0] / 100;
            var varY = XYZ[1] / 100;
            var varZ = XYZ[2] / 100;

            var varR = varX * 3.2406 + varY * -1.5372 + varZ * -0.4986;
            var varG = varX * -0.9689 + varY * 1.8758 + varZ * 0.0415;
            var varB = varX * 0.0557 + varY * -0.2040 + varZ * 1.0570;

            
            if (varR > 0.0031308)
                varR = 1.055 * Math.Pow(varR, 1.0 / 2.4) - 0.055;
            else
                varR = 12.92 * varR;

            if (varG > 0.0031308)
                varG = 1.055 * Math.Pow(varG, 1.0 / 2.4) - 0.055;
            else
                varG = 12.92 * varG;
                    
            if (varB > 0.0031308)
                varB = 1.055 * Math.Pow(varB, 1.0 / 2.4) - 0.055;
            else
                varB = 12.92 * varB;

            var sR = (int)Math.Round(varR * 255);
            var sG = (int)Math.Round(varG * 255);
            var sB = (int)Math.Round(varB * 255);

            return Color.FromArgb(sR, sG, sB);
        }
        
        public static double[] RGBtoCIELAB(Color color)
        {
            var XYZ = RGBtoXYZ(color);
            return XYZtoCIELab(XYZ);
        }

        public static Color CIELABtoRGB(double[] CIELAB)
        {
            var XYZ = CIELABtoXYZ(CIELAB);
            return XYZtoRGB(XYZ);
        }
        
    }
}