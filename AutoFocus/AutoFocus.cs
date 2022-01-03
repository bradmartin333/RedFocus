using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static AutoFocus.Score;

namespace AutoFocus
{
    public partial class AutoFocus : Form
    {
        private double _ImageScale = 1; // CONFIG

        private string[] _Files;
        private (int, int)[] _FocusTiles;
        private List<double> _Scores = new List<double>();
        private double _MaxScore;
        private int _BestIDX;
        
        private Stopwatch _SW = new Stopwatch();

        public AutoFocus()
        {
            InitializeComponent();

            string startupPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            _Files = Directory.GetFiles(startupPath + "/Examples/Test4");

            _SW.Start();
            FocusWorker.RunWorkerAsync();
        }

        private void MakePlot()
        {
            var xs = Enumerable.Range(0, _Files.Length).Select(number => (double)number);
            Plot.plt.PlotScatter(xs.ToArray(), _Scores.ToArray());
            Plot.plt.PlotPoint(_BestIDX, _MaxScore); // Makes it a new color
            Plot.plt.XLabel("Image #");
            Plot.plt.YLabel("Focus Score");
            Plot.plt.Title(string.Format("Using {0} Tile{1}", _FocusTiles.Length, _FocusTiles.Length > 1 ? "s" : ""));
            Plot.Visible = true;
            Plot.ScrollWheelProcessor(); // Makes the plot show wihtout user interation
        }

        private void ShowImage()
        {
            Bitmap bitmap = new Bitmap(Image.FromFile(_Files[_BestIDX]));
            HighlightTiles(ref bitmap, _FocusTiles);
            Pbx.Image = bitmap;
        }

        private void FocusWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Bitmap bitmap = new Bitmap(Image.FromFile(_Files[0]));
            Bitmap resizedImage = new Bitmap(bitmap, new Size((int)Math.Round(bitmap.Width * _ImageScale), (int)Math.Round(bitmap.Height * _ImageScale)));
            _FocusTiles = GetTiles(resizedImage); // Get tiles from first image in directory

            foreach (string f in _Files) // Score each image
            {
                bitmap = new Bitmap(Image.FromFile(f));
                resizedImage = new Bitmap(bitmap, new Size((int)Math.Round(bitmap.Width * _ImageScale), (int)Math.Round(bitmap.Height * _ImageScale)));
                _Scores.Add(ScoreImage(resizedImage, _FocusTiles));
            }

            e.Result = true; // Complete the background job
        }

        private void FocusWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            _SW.Stop();

            _MaxScore = _Scores.Max();
            _BestIDX = _Scores.IndexOf(_MaxScore);

            MakePlot();
            ShowImage();

            FileInfo fileInfo = new FileInfo(_Files[_BestIDX]);
            Text = $"{fileInfo.Name}   {Math.Round(_SW.Elapsed.TotalSeconds, 3)}s";
        }
    }
}
