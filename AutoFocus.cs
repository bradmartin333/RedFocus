using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace __NAMESPACE__
{
    static class AutoFocus
    {
        // SCORING
        private static int _ScanSize, _Wid, _Hgt;

        private static double GetBestPosition(double[] scores, double[] positions)
        {
            bool isEmpty = scores.All(x => x.Equals(null));
            if (isEmpty)
            {
                return 0.0;
            }

            // Unused outliers code
            //double[] scoreDiffs = new double[scores.Length - 1];
            //for (int i = 0; i < scores.Length - 1; i++)
            //{
            //    scoreDiffs[i] = Math.Abs(scores[i] - scores[i + 1]) / ((scores[i] + scores[i + 1]) / 2);
            //}

            //List<double> scoresEdit = scores.ToList();
            //double bestScore = scoresEdit.Max();
            //while (scoreDiffs[scores.ToList().IndexOf(bestScore)] > scoreDiffs.Average() * 1.5)
            //{
            //    bestScore = scoresEdit.Max();
            //    scoresEdit.Remove(bestScore);
            //}

            return positions[scores.ToList().IndexOf(scores.Max())];
        }

        private static void BitmapCrop(Rectangle crop, Bitmap src, ref Bitmap target)
        {
            using var g = Graphics.FromImage(target);
            g.DrawImage(src, new Rectangle(0, 0, crop.Width, crop.Height), crop, GraphicsUnit.Pixel);
        }

        private static double ScoreImageFocus(Bitmap img)
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

        private static double NeighborSharpness(double[,] PxlVals)
        {
            List<double> PDiff = new();
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


        // RUNNING
        public async static void RunFocusLoop(int NumSteps)
        {
            await RunFocus(0.015, NumSteps);
            await RunFocus(0.010, NumSteps);
            await RunFocus(0.005, NumSteps);
        }

        public async static
        System.Threading.Tasks.Task
        RunFocus(double FocusStep, int NumSteps)
        {
            double StartingPoint = __Z_POSITION__
            List<double> scores = new();
            List<double> positions = new();
            bool movesuccess;
            RectangleF cropRectF = GetFocusRegionParameters(); // Initialized to center 200x200px of frmVision
            Rectangle cropRect = new((int)Math.Round(cropRectF.X), (int)Math.Round(cropRectF.Y), (int)Math.Round(cropRectF.Width), (int)Math.Round(cropRectF.Height));
            try
            {
                // Move to lowest step and work way up
                for (int i = -NumSteps; i <= NumSteps; i++)
                {
                    double DeltaMovement = i * FocusStep;
                    __MOVE_Z__
                    Bitmap image = __CAPTURE_IMAGE__
                    Bitmap croppedImage = new(cropRect.Width, cropRect.Height);
                    BitmapCrop(cropRect, image, ref croppedImage);
                    Bitmap resizedImage = new(croppedImage, new Size((int)Math.Round(croppedImage.Width * 0.1), (int)Math.Round(croppedImage.Height * 0.1))); // Scale to 10%
                    double score = await System.Threading.Tasks.Task.Run(() => ScoreImageFocus(resizedImage));
                    scores.Add(score);
                    positions.Add(__THIS_POSITION__);
                }

                double BestPosition = GetBestPosition(scores.ToArray(), positions.ToArray());
                if (BestPosition == 0.0)
                {
                    // Return the ZOptic to the original position
                    return;
                }

                // Move the ZOptic to the best position
            }
            catch (Exception ex)
            {
                __EXCEPTION__
            }
        }
    }
}
