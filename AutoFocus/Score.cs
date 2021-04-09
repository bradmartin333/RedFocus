using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static AutoFocus.Colorizer;

namespace AutoFocus
{
    static class Score
    {
        private static int _ScanSize, _Wid, _Hgt;

        private static void BitmapCrop(Rectangle crop, Bitmap src, ref Bitmap target)
        {
            using var g = Graphics.FromImage(target);
            g.DrawImage(src, new Rectangle(0, 0, crop.Width, crop.Height), crop, GraphicsUnit.Pixel);
        }

        public static double ScoreImageFocus(Bitmap img)
        {
            _ScanSize = (int)(Math.Min(img.Height, img.Width) * 0.2); // Break into a 5x5 grid
            _Wid = img.Width / _ScanSize;
            _Hgt = img.Height / _ScanSize;

            // Get red values for every pixel in a 2D array
            double[,] PxlVals = new double[_Wid, _Hgt];
            for (int i = 0; i < _Wid; i++)
            {
                for (int j = 0; j < _Hgt; j++)
                {
                    PxlVals[i, j] = img.GetPixel(i * _ScanSize, j * _ScanSize).R;
                }
            }

            return NeighborSharpness(PxlVals);
        }

        public static void HighlightGrid(ref Bitmap img)
        {
            using (Graphics g = Graphics.FromImage(img))
            {
                for (int i = 0; i < img.Width; i += _ScanSize)
                {
                    for (int j = 0; j < img.Height; j += _ScanSize)
                    {
                        Color color = GetNextRainbowColor();
                        g.FillRectangle(new SolidBrush(color), new Rectangle(i, j, _ScanSize, _ScanSize));
                    }
                }
            }
        }

        private static double NeighborSharpness(double[,] PxlVals)
        {
            List<double> PDiff = new List<double>();
            for (int i = 0; i < _Wid; i++)
            {
                for (int j = 0; j < _Hgt; j++)
                {
                    double LocalPDiff = 0.0;
                    for (int k = i - _ScanSize; k <= i + _ScanSize; k++)
                    {
                        for (int l = j - _ScanSize; l <= j + _ScanSize; l++)
                        {
                            if (k > 0 && k < _Wid - 1 && l > 0 && l < _Hgt - 1)
                            {
                                double p1 = PxlVals[i, j];
                                double p2 = PxlVals[k, l];
                                if (p1 == 0 && p2 == 0)
                                {
                                    continue;
                                }
                                LocalPDiff += Math.Abs(p1 - p2) / ((p1 + p2) / 2);
                            }
                        }
                    }
                    PDiff.Add(LocalPDiff / (_ScanSize * _ScanSize));
                }
            }

            // Return inverse of average sharpness
            double score = 1.0 / (PDiff.Count / PDiff.Sum());
            return score;
        }
    }
}
