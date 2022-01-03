using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace AutoFocus
{
    static class Score
    {
        private static int _GridSize = 15; // CONFIG
        private static double _AmountDataDesired = 0.1; // CONFIG

        #region Public Methods

        /// <summary>
        /// Get tiles with highest
        /// entropies within image
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns>
        /// (int, int) tuple
        /// where item1 = Rect.X
        /// and item2 = Rect.Y
        /// </returns>
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
                    if (bmpRect.Contains(rect)) tiles.Add((i, j, GetCropEntropyARGB(data, rect, bmpData.Stride)));
                }

            tiles.Sort((x, y) => y.Item3.CompareTo(x.Item3)); // Sort list by greatest entropy to smallest entropy
            (int, int)[] selectedTiles = tiles.Take((int)(tiles.Count() * _AmountDataDesired)).Select(x => (x.Item1, x.Item2)).ToArray();
            return selectedTiles;
        }

        /// <summary>
        /// Return average of scores
        /// for desired tiles in image
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="tiles"></param>
        /// <returns></returns>
        public static double ScoreImage(Bitmap bmp, (int, int)[] tiles)
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

            return scores.Average();
        }

        /// <summary>
        /// Colorize tiles within image
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

        #endregion

        #region Private Methods

        /// <summary>
        /// Crop BMP Pixel Data
        /// and calculate entropy
        /// for that tile
        /// </summary>
        /// <param name="data">
        /// BitmapData for current image
        /// </param>
        /// <param name="tile">
        /// Rectangle to crop BitmapData
        /// </param>
        /// <param name="stride"></param>
        /// <returns></returns>
        private static double GetCropEntropyARGB(byte[] data, Rectangle tile, int stride)
        {
            List<int> counts = new List<int>(); // Each int is a ARGB value of a pixel
            for (int i = tile.Left; i < tile.Right; i += (int)(1 / _AmountDataDesired))
                for (int j = tile.Top; j < tile.Bottom; j += (int)(1 / _AmountDataDesired))
                {
                    int idx = i * 3 + j * stride; // Find starting index of pixel in data
                    if (idx + 2 < data.Length) // Determine if it is wihtin the padding
                        counts.Add(data[idx + 2] << 16 | data[idx + 1] << 8 | data[idx]);
                    // Create ARGB int32 from RGB values which are ordered in the data array as BGR
                }
            double entropy = 0; // Calculate Shannon entropy
            foreach (var g in counts.GroupBy(i => i)) // Turn list into an array of counts
            {
                double val = g.Count() / (double)(tile.Width * tile.Height);
                entropy -= val * Math.Log2(val);
            }
            return entropy;
        }

        /// <summary>
        /// Core of AutoFocus
        /// </summary>
        /// <param name="data">
        /// BitmapData for current image
        /// </param>
        /// <param name="tile">
        /// Rectangle to crop BitmapData
        /// </param>
        /// <param name="stride"></param>
        /// <returns>
        /// Calculated score for tile
        /// </returns>
        private static double ScoreTile(byte[] data, Rectangle tile, int stride)
        {
            List<double> PDiff = new List<double>(); // Turn the data array into a convolutional data array

            for (int i = tile.Left; i < tile.Right; i += (int)(1 / _AmountDataDesired))
                for (int j = tile.Top; j < tile.Bottom; j += (int)(1 / _AmountDataDesired))
                {
                    int idx = i * 3 + j * stride; // Find starting index of pixel in data
                    if (idx + 2 < data.Length) // Determine if it is wihtin the padding
                    {
                        byte p1 = data[idx + 2]; // Get current pixel's Red channel value
                        double localPDiff = 0; // Percent difference of p1 and neighboring p2's
                        int counts = 0; // Number of p2's we found
                        for (int k = i - (int)(0.5 / _AmountDataDesired); k < i + (int)(0.5 / _AmountDataDesired); k++)
                            for (int l = j - (int)(0.5 / _AmountDataDesired); l < j + (int)(0.5 / _AmountDataDesired); l++)
                            {
                                int neighborIdx = k * 3 + l * stride;  // Find starting index of pixel in data
                                if (neighborIdx + 2 > 0 && neighborIdx + 2 < data.Length)  // Determine if it is wihtin the array
                                {
                                    byte p2 = data[neighborIdx + 2]; // Get current pixel's Red channel value
                                    localPDiff += Math.Abs(p1 - p2) / (double)((p1 + p2) / 2.0); // Add the calculated percent difference
                                    counts++;
                                }
                            }
                        PDiff.Add(localPDiff / counts); // Add the average to the list
                    }
                }

            return PDiff.Average();
        }

        #endregion
    }
}
