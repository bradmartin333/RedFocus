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

        public AutoFocus()
        {
            InitializeComponent();
            files = Directory.GetFiles(@"S:\RedFocus\Examples\Test");

            foreach (string f in files)
                scores.Add(ScoreImageFocus(new Bitmap(Image.FromFile(f))));

            var xs = Enumerable.Range(0, files.Length).Select(number => (double) number);
            Plot.plt.PlotScatter(xs.ToArray(), scores.ToArray());

            double MaxScore = scores.Max();
            int BestIdx = scores.IndexOf(MaxScore);
            Panel.BackgroundImage = new Bitmap(Image.FromFile(files[BestIdx]));
            Plot.plt.PlotPoint(BestIdx, MaxScore);
        }
    }
}
