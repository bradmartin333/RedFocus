using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static AutoFocus.Score;

namespace AutoFocus
{
    public partial class AutoFocus : Form
    {
        public static double GridSize = 10.0; // N x N grid
        public static double AmountDataDesired = 0.2; // Highest % of available data from training grid
        private readonly double ImageScale = 0.5;
        private readonly string[] Files;
        private readonly List<double> Scores = new List<double>();
        private int[] FocusTiles;
        private double MaxScore;
        private int BestIDX;
        private string BestFileName;
        
        public AutoFocus()
        {
            InitializeComponent();
            string startupPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            Files = Directory.GetFiles(startupPath + "/Examples/Test");
            FocusWorker.RunWorkerAsync();
        }

        private void MakePlot()
        {
            var xs = Enumerable.Range(0, Files.Length).Select(number => (double)number);
            Plot.plt.PlotScatter(xs.ToArray(), Scores.ToArray());
            Plot.plt.PlotPoint(BestIDX, MaxScore);
            Plot.plt.XLabel("Image #");
            Plot.plt.YLabel("Focus Score");
            Plot.plt.Title(string.Format("Using {0} Tile{1}", NumTiles, NumTiles > 1 ? "s" : ""));
            Plot.Visible = true;
            Plot.ScrollWheelProcessor();
        }

        private void ShowImage()
        {
            Bitmap bitmap = new Bitmap(Image.FromFile(Files[BestIDX]));
            HighlightTiles(ref bitmap, FocusTiles); // Show what grid was used
            Pbx.Image = bitmap;
        }

        private void FocusWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Bitmap image = new Bitmap(Image.FromFile(Files[0]));
            Bitmap resizedImage = new Bitmap(image, new Size((int)Math.Round(image.Width * ImageScale), (int)Math.Round(image.Height * ImageScale)));
            FocusTiles = GetTiles(resizedImage); // Get tiles from first image in directory

            foreach (string f in Files) // Score each image
            {
                image = new Bitmap(Image.FromFile(f));
                resizedImage = new Bitmap(image, new Size((int)Math.Round(image.Width * ImageScale), (int)Math.Round(image.Height * ImageScale)));
                Scores.Add(ScoreImageGrid(resizedImage, FocusTiles));
            }

            e.Result = true; // Complete the background job
        }

        private void FocusWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            MaxScore = Scores.Max(); // Max is usually the best
            BestIDX = Scores.IndexOf(MaxScore);
            BestFileName = Files[BestIDX];

            MakePlot();
            ShowImage();

            FileInfo fileInfo = new FileInfo(BestFileName);
            Text = fileInfo.Name;
        }
    }
}
