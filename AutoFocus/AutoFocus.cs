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
        private string[] _Files;
        private int[] _FocusTiles;
        private List<double> _Scores = new List<double>();
        private double _MaxScore;
        private int _BestIDX;
        private string _BestFileName;
        private double _ImageScale = 0.2; // EDITABLE
        private string _Path = ""; // EDITBALE

        public AutoFocus()
        {
            InitializeComponent();

            string startupPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            _Files = Directory.GetFiles(startupPath + "/Examples/Test4");
            
            //_Files = Directory.GetFiles(_Path);
            
            FocusWorker.RunWorkerAsync();
        }

        private void MakePlot()
        {
            var xs = Enumerable.Range(0, _Files.Length).Select(number => (double)number);
            Plot.plt.PlotScatter(xs.ToArray(), _Scores.ToArray());
            Plot.plt.PlotPoint(_BestIDX, _MaxScore);
            Plot.plt.XLabel("Image #");
            Plot.plt.YLabel("Focus Score");
            Plot.plt.Title(string.Format("Using {0} Tile{1}", _NumTiles, _NumTiles > 1 ? "s" : ""));
            Plot.Visible = true;
            Plot.ScrollWheelProcessor();
        }

        private void ShowImage()
        {
            Bitmap bitmap = new Bitmap(Image.FromFile(_Files[_BestIDX]));
            HighlightTiles(ref bitmap, _FocusTiles); // Show what grid was used
            Pbx.Image = bitmap;
        }

        private void FocusWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Bitmap image = new Bitmap(Image.FromFile(_Files[0]));
            Bitmap resizedImage = new Bitmap(image, new Size((int)Math.Round(image.Width * _ImageScale), (int)Math.Round(image.Height * _ImageScale)));
            _FocusTiles = GetTiles(resizedImage); // Get tiles from first image in directory

            foreach (string f in _Files) // Score each image
            {
                image = new Bitmap(Image.FromFile(f));
                resizedImage = new Bitmap(image, new Size((int)Math.Round(image.Width * _ImageScale), (int)Math.Round(image.Height * _ImageScale)));
                _Scores.Add(ScoreImageGrid(resizedImage, _FocusTiles));
            }

            e.Result = true; // Complete the background job
        }

        private void FocusWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            _MaxScore = _Scores.Max(); // Max is usually the best
            _BestIDX = _Scores.IndexOf(_MaxScore);
            _BestFileName = _Files[_BestIDX];

            MakePlot();
            ShowImage();

            FileInfo fileInfo = new FileInfo(_BestFileName);
            Text = fileInfo.Name;
        }
    }
}
