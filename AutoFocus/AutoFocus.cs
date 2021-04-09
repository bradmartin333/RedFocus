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
        private string[] files;
        private List<double> scores = new List<double>();
        private int[] focusTiles;

        public AutoFocus()
        {
            InitializeComponent();
            files = Directory.GetFiles(@"S:\RedFocus\Examples\Test");

            // Get tiles from first image in directory
            focusTiles = GetTiles(new Bitmap(Image.FromFile(files[0])));

            // Score each image
            foreach (string f in files)
                scores.Add(ScoreImageGrid(new Bitmap(Image.FromFile(f)), focusTiles));

            // Scatterplot of scores
            var xs = Enumerable.Range(0, files.Length).Select(number => (double) number);
            Plot.plt.PlotScatter(xs.ToArray(), scores.ToArray());
            // Note that you have to click the plot after form is shown for it to load

            double MaxScore = scores.Max(); // Max is usually the best
            int BestIdx = scores.IndexOf(MaxScore);

            Bitmap bitmap = new Bitmap(Image.FromFile(files[BestIdx]));
            HighlightTiles(ref bitmap, focusTiles); // Show what grid was used

            Panel.BackgroundImage = bitmap;
            Plot.plt.PlotPoint(BestIdx, MaxScore);
        }
    }
}
