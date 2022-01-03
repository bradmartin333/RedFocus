using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AutoFocus
{
    static class Score
    {
        public static int _NumTiles = 1;

        private static int _ScanSize, _Wid, _Hgt;
        private static double _GridSize = 15.0; // EDITABLE n x n grid
        private static double _AmountDataDesired = 0.1; // Highest 10% of available data from training grid

        ///// Handy tool used as a method
        //private static void BitmapCrop(Rectangle crop, Bitmap src, ref Bitmap target)
        //{
        //    using var g = Graphics.FromImage(target);
        //    g.DrawImage(src, new Rectangle(0, 0, crop.Width, crop.Height), crop, GraphicsUnit.Pixel);
        //}

        // Gets grid pixels and then scores the image
        public static double ScoreImageFocus(Bitmap img)
        {
            _ScanSize = (int)(Math.Min(img.Height, img.Width) * (1.0 / _GridSize));
            _Wid = img.Width / _ScanSize;
            _Hgt = img.Height / _ScanSize;

            // Get red values for every pixel in a 2D array
            byte[] data = GetRedChArr(img);
            int k = 0;
            double[,] PxlVals = new double[_Wid, _Hgt];
            for (int i = 0; i < _Wid; i++)
            {
                for (int j = 0; j < _Hgt; j++)
                {
                    PxlVals[i, j] = data[k];
                    k++;
                }
            }

            return GetNeighborSharpness(PxlVals);
        }

        // The core of AutoFocus
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
        /// <param name="bmp"></param>
        /// <param name="tiles"></param>
        /// <returns></returns>
        public static double ScoreImageGrid(Bitmap bmp, int[] tiles)
        {
            int tileScanSize = (int)(Math.Min(bmp.Height, bmp.Width) * (1.0 / _GridSize));
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int size = bmpData.Stride * bmpData.Height;
            byte[] data = new byte[size];
            Marshal.Copy(bmpData.Scan0, data, 0, size);

            List<double> scores = new List<double>();
            int tileIDX = 0;

            for (int i = 0; i < bmp.Width; i += tileScanSize)
            {
                for (int j = 0; j < bmp.Height; j += tileScanSize)
                {
                    if (tiles.Contains(tileIDX))
                    {
                        Rectangle tile = new Rectangle(i, j, tileScanSize, tileScanSize);
                        scores.Add(ScoreTileFocus(tile));
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
        public static int[] GetTiles(Bitmap bmp)
        {
            int tileScanSize = (int)(Math.Min(bmp.Height, bmp.Width) * (1.0 / _GridSize));
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int size = bmpData.Stride * bmpData.Height;
            byte[] data = new byte[size];
            Marshal.Copy(bmpData.Scan0, data, 0, size);

            Dictionary<int, double> entropyDict = new Dictionary<int, double>();
            int entropyIDX = 0;

            for (int i = 0; i < bmp.Width; i += tileScanSize)
                for (int j = 0; j < bmp.Height; j += tileScanSize)
                {
                    Rectangle tile = new Rectangle(i, j, tileScanSize, tileScanSize);
                    entropyDict.Add(entropyIDX, GetRedCropEntropy(data, tile, bmpData.Stride));
                    entropyIDX++;
                }

            IEnumerable<int> sortedTiles = entropyDict.OrderBy(x => x.Value).Select(x => x.Key).Reverse();
            int[] tiles = sortedTiles.Take((int)(sortedTiles.Count() * _AmountDataDesired)).ToArray();
            _NumTiles = tiles.Length;
            return tiles;
        }

        /// <summary>
        /// Crop BMP Pixel Data,
        /// take Red channel values,
        /// and calculate entropy
        /// for that tile
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tile"></param>
        /// <param name="stride"></param>
        /// <returns></returns>
        private static double GetRedCropEntropy(byte[] data, Rectangle tile, int stride)
        {
            byte[] counts = new byte[256];
            for (int i = tile.Left; i < tile.Right; i += (int)(1 / _AmountDataDesired))
                for (int j = tile.Top; j < tile.Bottom; j += (int)(1 / _AmountDataDesired))
                {
                    int idx = i * 3 + j * stride;
                    if (idx + 2 < data.Length) counts[data[idx + 2]]++;
                }
            double entropy = 0;
            foreach (byte count in counts)
                if (count != 0)
                {
                    double val = count / (double)(tile.Width * tile.Height);
                    entropy -= val * Math.Log2(val);
                }
            return entropy;
        }

        public static byte[] GetRedChArr(Bitmap bmp)
        {
            BitmapData bitmapData = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.ReadWrite, bmp.PixelFormat);
            int size = bmp.Width * 3 * bmp.Width; // Size of image in bytes - Image will always be square here
            byte[] data = new byte[size];
            Marshal.Copy(bitmapData.Scan0, data, 0, size); // Copies data of /size/ into /data/ from location specified by Scan0
            byte[] arr = new byte[size / 3];
            for (int i = 0; i < size; i += 3)
                arr[i / 3] = data[i + 2];
            return arr;
        }

        /// <summary>
        /// Will colorize with the grid layout of the grid training image
        /// </summary>
        /// <param name="img"></param>
        /// <param name="tiles"></param>
        public static void HighlightTiles(ref Bitmap img, int[] tiles)
        {
            int tileScanSize = (int)(Math.Min(img.Height, img.Width) * (1.0 / _GridSize));
            int tileIDX = 0;
            using (Graphics g = Graphics.FromImage(img))
                for (int i = 0; i < img.Width; i += tileScanSize)
                    for (int j = 0; j < img.Height; j += tileScanSize)
                    {
                        if (tiles.Contains(tileIDX))
                            g.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.Green)), new Rectangle(i, j, tileScanSize, tileScanSize));
                        tileIDX++;
                    }
        }
    }
}
