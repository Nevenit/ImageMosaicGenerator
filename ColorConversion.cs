using System;
using System.Drawing;

namespace ImageMosaicGenerator
{
    public class ColorConversion
    {
        // All of these conversions are from https://www.easyrgb.com/en/math.php
        public double[] RGBtoXYZ(Color color)
        {
            double varR = color.R / 255.0;
            double varG = color.G / 255.0;
            double varB = color.B / 255.0;

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

            double X = varR * 0.4124 + varG * 0.3576 + varB * 0.1805;
            double Y = varR * 0.2126 + varG * 0.7152 + varB * 0.0722;
            double Z = varR * 0.0193 + varG * 0.1192 + varB * 0.9505;

            return new double[] { X, Y, Z};
        }

        public double[] XYZtoCIELab(double[] XYZ)
        {
            double varX = XYZ[0] / 100.0;
            double varY = XYZ[1] / 100.0;
            double varZ = XYZ[2] / 100.0;

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

            double CIEL = (116 * varY) - 16;
            double CIEA = 500 * (varX - varY);
            double CIEB = 200 * (varY - varZ);
            
            return new double[] {CIEL, CIEA, CIEB};
        }

        public double[] CIELABtoXYZ(double[] CIELAB)
        {
            double varY = (CIELAB[0] + 16) / 116;
            double varX = CIELAB[1] / 500 + varY;
            double varZ = varY - CIELAB[2] / 200;

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

            double X = varX * 100.0;
            double Y = varY * 100.0;
            double Z = varZ * 100.0;

            return new double[] {X, Y, Z};
        }

        public Color XYZtoRGB(double[] XYZ)
        {
            double varX = XYZ[0] / 100;
            double varY = XYZ[1] / 100;
            double varZ = XYZ[2] / 100;

            double varR = varX * 3.2406 + varY * -1.5372 + varZ * -0.4986;
            double varG = varX * -0.9689 + varY * 1.8758 + varZ * 0.0415;
            double varB = varX * 0.0557 + varY * -0.2040 + varZ * 1.0570;

            
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

            int sR = (int)Math.Round(varR * 255);
            int sG = (int)Math.Round(varG * 255);
            int sB = (int)Math.Round(varB * 255);

            return Color.FromArgb(sR, sG, sB);
        }
        
        public double[] RGBtoCIELAB(Color color)
        {
            double[] XYZ = RGBtoXYZ(color);
            return XYZtoCIELab(XYZ);
        }

        public Color CIELABtoRGB(double[] CIELAB)
        {
            double[] XYZ = CIELABtoXYZ(CIELAB);
            return XYZtoRGB(XYZ);
        }
        
    }
}