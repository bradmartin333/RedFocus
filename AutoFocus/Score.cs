using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace AutoFocus
{
    static class Score
    {
        private static int _ScanSize, _Wid, _Hgt;

        /// <summary>
        /// Handy tool used as a method
        /// </summary>
        /// <param name="crop"></param>
        /// <param name="src"></param>
        /// <param name="target"></param>
        private static void BitmapCrop(Rectangle crop, Bitmap src, ref Bitmap target)
        {
            using var g = Graphics.FromImage(target);
            g.DrawImage(src, new Rectangle(0, 0, crop.Width, crop.Height), crop, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Gets 5x5 grid pixels and then scores the image
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
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

            return GetNeighborSharpness(PxlVals);
        }

        /// <summary>
        /// The core of AutoFocus
        /// </summary>
        /// <param name="PxlVals"></param>
        /// <returns></returns>
        private static double GetNeighborSharpness(double[,] PxlVals)
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

            // Return average sharpness
            double score = PDiff.Sum() / PDiff.Count;
            return score;
        }

        /// <summary>
        /// Scores an image based off the tiles from a trained grid only
        /// </summary>
        /// <param name="img"></param>
        /// <param name="tiles"></param>
        /// <returns></returns>
        public static double ScoreImageGrid(Bitmap img, int[] tiles)
        {
            int tileScanSize = (int)(Math.Min(img.Height, img.Width) * 0.2); // Break into a 5x5 grid
            int tileIDX = 0;
            List<double> scores = new List<double>();
            
            for (int i = 0; i < img.Width; i += tileScanSize)
            {
                for (int j = 0; j < img.Height; j += tileScanSize)
                {
                    if (tiles.Contains(tileIDX))
                    {
                        Bitmap tile = new Bitmap(tileScanSize, tileScanSize);
                        BitmapCrop(new Rectangle(i, j, tileScanSize, tileScanSize), img, ref tile);
                        scores.Add(ScoreImageFocus(tile));
                    }
                    tileIDX++;
                }
            }

            return scores.Sum() / scores.Count();
        }

        /// <summary>
        /// Returns the tiles within a 5x5 grid that have entropies in the 3rd out of 3 histogram bins
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static int[] GetTiles(Bitmap img)
        {
            int tileScanSize = (int)(Math.Min(img.Height, img.Width) * 0.2); // Break into a 5x5 grid
            List<int> tiles = new List<int>();
            List<double> entropies = new List<double>();

            for (int i = 0; i < img.Width; i += tileScanSize)
            {
                for (int j = 0; j < img.Height; j += tileScanSize)
                {
                    Bitmap tile = new Bitmap(tileScanSize, tileScanSize);
                    BitmapCrop(new Rectangle(i, j, tileScanSize, tileScanSize), img, ref tile);
                    List<double> entropyList = new List<double>();
                    for (int k = 0; k < tile.Width; k++)
                    {
                        for (int l = 0; l < tile.Height; l++)
                        {
                            entropyList.Add(tile.GetPixel(k, l).ToArgb());
                        }
                    }
                    entropies.Add(Statistics.Entropy(entropyList.ToArray()));
                }
            }

            Histogram histogram = new Histogram(entropies, 3);

            int tileIDX = 0;
            using (Graphics g = Graphics.FromImage(img))
            {
                for (int i = 0; i < img.Width; i += tileScanSize)
                {
                    for (int j = 0; j < img.Height; j += tileScanSize)
                    {
                        if (histogram.GetBucketIndexOf(entropies[tileIDX]) == 2)
                        {
                            tiles.Add(tileIDX);
                        }
                        tileIDX++;
                    }
                }
            }

            return tiles.ToArray();
        }

        /// <summary>
        /// Will colorize with the grid layout of the grid training image
        /// </summary>
        /// <param name="img"></param>
        /// <param name="tiles"></param>
        public static void HighlightTiles(ref Bitmap img, int[] tiles)
        {
            int tileScanSize = (int)(Math.Min(img.Height, img.Width) * 0.2); // Break into a 5x5 grid
            int tileIDX = 0;
            using (Graphics g = Graphics.FromImage(img))
            {
                for (int i = 0; i < img.Width; i += tileScanSize)
                {
                    for (int j = 0; j < img.Height; j += tileScanSize)
                    {
                        if (tiles.Contains(tileIDX))
                        {
                            g.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.Green)), new Rectangle(i, j, tileScanSize, tileScanSize));
                        }
                        tileIDX++;
                    }
                }
            }
        }

        /// <summary>
        /// Will colorize with the grid layout of the last scored image
        /// </summary>
        /// <param name="img"></param>
        public static void HighlightGrid(ref Bitmap img)
        {
            List<double> entropies = new List<double>();

            for (int i = 0; i < img.Width; i += _ScanSize)
            {
                for (int j = 0; j < img.Height; j += _ScanSize)
                {
                    Bitmap tile = new Bitmap(_ScanSize, _ScanSize);
                    BitmapCrop(new Rectangle(i, j, _ScanSize, _ScanSize), img, ref tile);
                    List<double> entropyList = new List<double>();
                    for (int k = 0; k < tile.Width; k++)
                    {
                        for (int l = 0; l < tile.Height; l++)
                        {
                            entropyList.Add(tile.GetPixel(k, l).ToArgb());
                        }
                    }
                    entropies.Add(Statistics.Entropy(entropyList.ToArray()));
                }
            }

            Histogram histogram = new Histogram(entropies, 3);

            int tileIDX = 0;
            using (Graphics g = Graphics.FromImage(img))
            {
                for (int i = 0; i < img.Width; i += _ScanSize)
                {
                    for (int j = 0; j < img.Height; j += _ScanSize)
                    {
                        if (histogram.GetBucketIndexOf(entropies[tileIDX]) == 2)
                        {
                            g.FillRectangle(new SolidBrush(Color.FromArgb(100 ,Color.Green)), new Rectangle(i, j, _ScanSize, _ScanSize));
                        }
                        tileIDX++;
                    }
                }
            }
        }
    }
}
