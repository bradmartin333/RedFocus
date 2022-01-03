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
        private static double _GridSize = 15.0; // EDITABLE n x n grid
        private static double _AmountDataDesired = 0.1; // Highest 10% of available data from training grid

        /// <summary>
        /// Scores an image based off the tiles from a trained grid only
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="tiles"></param>
        /// <returns></returns>
        public static async Task<double> ScoreImageGridAsync(Bitmap bmp, int[] tiles)
        {
            int tileScanSize = (int)(Math.Min(bmp.Height, bmp.Width) * (1.0 / _GridSize));
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int size = bmpData.Stride * bmpData.Height;
            byte[] data = new byte[size];
            Marshal.Copy(bmpData.Scan0, data, 0, size);

            Task[] tasks = new Task[tiles.Length];
            List<double> scores = new List<double>();
            for (int i = 0; i < tiles.Length; i++)
            {
                Rectangle rect = new Rectangle(
                    (int)(i / _GridSize * tileScanSize),
                    (int)(i % _GridSize * tileScanSize),
                    tileScanSize, tileScanSize);
                tasks[i] = Task.Run(() => scores.Add(ScoreTileFocus(data, rect, bmpData.Stride)));
            }

            await Task.WhenAll(tasks);
            scores.RemoveAll(x => double.IsNaN(x));
            return scores.Average();
        }

        private static double ScoreTileFocus(byte[] data, Rectangle tile, int stride)
        {
            List<double> PDiff = new List<double>();
            for (int i = tile.Left; i < tile.Right; i += (int)(1 / _AmountDataDesired))
                for (int j = tile.Top; j < tile.Bottom; j += (int)(1 / _AmountDataDesired))
                {
                    int idx = i * 3 + j * stride;
                    if (idx + 2 < data.Length)
                    {
                        double p1 = data[idx + 2];
                        double localPDiff = 0;
                        int counts = 0;
                        for (int k = i - (int)(0.5 / _AmountDataDesired); k < i + (int)(0.5 / _AmountDataDesired); k++)
                            for (int l = j - (int)(0.5 / _AmountDataDesired); l < j + (int)(0.5 / _AmountDataDesired); l++)
                            {
                                int neighborIdx = k * 3 + l * stride;
                                if (neighborIdx + 2 > 0 && neighborIdx + 2 < data.Length)
                                {
                                    double p2 = data[neighborIdx + 2];
                                    localPDiff += Math.Abs(p1 - p2) / ((p1 + p2) / 2);
                                    counts++;
                                }
                            }
                        PDiff.Add(localPDiff / counts);
                    }
                }
            return PDiff.Average();
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
