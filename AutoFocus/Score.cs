using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace AutoFocus
{
    static class Score
    {
        public static int NumTiles = 1;
        private static int ScanSize, Wid, Hgt;

        /// Handy tool used as a method
        private static void BitmapCrop(Rectangle crop, Bitmap src, ref Bitmap target)
        {
            using var g = Graphics.FromImage(target);
            g.DrawImage(src, new Rectangle(0, 0, crop.Width, crop.Height), crop, GraphicsUnit.Pixel);
        }

        // Gets grid pixels and then scores the image
        public static double ScoreImageFocus(Bitmap img)
        {
            ScanSize = (int)Math.Ceiling(Math.Min(img.Height, img.Width) * (1.0 / AutoFocus.GridSize));
            Wid = img.Width / ScanSize;
            Hgt = img.Height / ScanSize;

            // Get red values for every pixel in a 2D array
            double[,] PxlVals = new double[Wid, Hgt];
            for (int i = 0; i < Wid; i++)
            {
                for (int j = 0; j < Hgt; j++)
                {
                    PxlVals[i, j] = img.GetPixel(i * ScanSize, j * ScanSize).R;
                }
            }

            return GetNeighborSharpness(PxlVals);
        }

        // The core of AutoFocus
        private static double GetNeighborSharpness(double[,] PxlVals)
        {
            List<double> PDiff = new List<double>();
            for (int i = 0; i < Wid; i++)
            {
                for (int j = 0; j < Hgt; j++)
                {
                    double LocalPDiff = 0.0;
                    for (int k = i - ScanSize; k <= i + ScanSize; k++)
                    {
                        for (int l = j - ScanSize; l <= j + ScanSize; l++)
                        {
                            if (k > 0 && k < Wid - 1 && l > 0 && l < Hgt - 1)
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
                    PDiff.Add(LocalPDiff / (ScanSize * ScanSize));
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
            int tileScanSize = (int)(Math.Min(img.Height, img.Width) * (1.0 / AutoFocus.GridSize));
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

                    if (scores.Count == tiles.Length)
                        break;
                }
            }

            GC.Collect();
            return scores.Sum() / scores.Count();
        }

        // Returns the tiles within a grid that have entropies in the highest 2 histogram bins
        public static int[] GetTiles(Bitmap img)
        {
            int tileScanSize = (int)(Math.Min(img.Height, img.Width) * (1.0 / AutoFocus.GridSize));
            Dictionary<int, double> entropyDict = new Dictionary<int, double>();
            int entropyIDX = 0;

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
                    entropyDict.Add(entropyIDX, (Statistics.Entropy(entropyList.ToArray())));
                    entropyIDX++;
                }
            }

            IEnumerable<int> sortedTiles = entropyDict.OrderBy(x => x.Value).Select(x => x.Key).Reverse();
            int[] tiles = sortedTiles.Take((int)(sortedTiles.Count() * AutoFocus.AmountDataDesired)).ToArray();
            NumTiles = tiles.Length;
            return tiles;
        }

        /// <summary>
        /// Will colorize with the grid layout of the grid training image
        /// </summary>
        /// <param name="img"></param>
        /// <param name="tiles"></param>
        public static void HighlightTiles(ref Bitmap img, int[] tiles)
        {
            int tileScanSize = (int)(Math.Min(img.Height, img.Width) * (1.0 / AutoFocus.GridSize));
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
    }
}
