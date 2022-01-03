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
        private static int _GridSize = 15;
        private static double _AmountDataDesired = 0.1;

        public static double ScoreImageTiles(Bitmap bmp, (int, int)[] tiles)
        {
            Rectangle bmpRect = new Rectangle(Point.Empty, bmp.Size);
            BitmapData bmpData = bmp.LockBits(bmpRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int size = bmpData.Stride * bmpData.Height;
            byte[] data = new byte[size];
            Marshal.Copy(bmpData.Scan0, data, 0, size);

            Size scanSize = new Size(bmp.Width / _GridSize, bmp.Height / _GridSize);
            List<double> scores = new List<double>();

            foreach ((int, int) tile in tiles)
            {
                Rectangle rect = new Rectangle(tile.Item1, tile.Item2, scanSize.Width, scanSize.Height);
                if (bmpRect.Contains(rect)) scores.Add(ScoreTile(data, rect, bmpData.Stride));
            }

            scores.RemoveAll(x => double.IsNaN(x));
            return scores.Average();
        }

        private static double ScoreTile(byte[] data, Rectangle tile, int stride)
        {
            List<double> PDiff = new List<double>();

            for (int i = tile.Left; i < tile.Right; i++)
                for (int j = tile.Top; j < tile.Bottom; j++)
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
        public static (int, int)[] GetTiles(Bitmap bmp)
        {
            Rectangle bmpRect = new Rectangle(Point.Empty, bmp.Size);
            BitmapData bmpData = bmp.LockBits(bmpRect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int size = bmpData.Stride * bmpData.Height;
            byte[] data = new byte[size];
            Marshal.Copy(bmpData.Scan0, data, 0, size);

            Size scanSize = new Size(bmp.Width / _GridSize, bmp.Height / _GridSize);
            List<(int, int, double)> tiles = new List<(int, int, double)>();

            for (int i = 0; i < bmp.Width; i += scanSize.Width)
                for (int j = 0; j < bmp.Height; j += scanSize.Height)
                {
                    Rectangle rect = new Rectangle(i, j, scanSize.Width, scanSize.Height);
                    if (bmpRect.Contains(rect)) tiles.Add((i, j, GetRedCropEntropy(data, rect, bmpData.Stride)));
                }

            tiles.Sort((x, y) => y.Item3.CompareTo(x.Item3));
            (int, int)[] selectedTiles = tiles.Take((int)(tiles.Count() * _AmountDataDesired)).Select(x => (x.Item1, x.Item2)).ToArray();
            return selectedTiles;
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
            List<int> counts = new List<int>();
            for (int i = tile.Left; i < tile.Right; i++)
                for (int j = tile.Top; j < tile.Bottom; j++)
                {
                    int idx = i * 3 + j * stride;
                    if (idx + 2 < data.Length)
                        counts.Add(data[idx + 2] << 16 | data[idx + 1] << 8 | data[idx]);
                }
            double entropy = 0;
            foreach (var g in counts.GroupBy(i => i))
            {
                double val = g.Count() / (double)(tile.Width * tile.Height);
                entropy -= val * Math.Log2(val);
            }
            return entropy;
        }

        /// <summary>
        /// Will colorize with the grid layout of the grid training image
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="tiles"></param>
        public static void HighlightTiles(ref Bitmap bmp, (int, int)[] tiles)
        {
            Size scanSize = new Size(bmp.Width / _GridSize, bmp.Height / _GridSize);
            using (Graphics g = Graphics.FromImage(bmp))
                foreach ((int, int) tile in tiles)
                {
                    Rectangle rect = new Rectangle(tile.Item1, tile.Item2, scanSize.Width, scanSize.Height);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.Green)), rect);
                }
        }
    }
}
