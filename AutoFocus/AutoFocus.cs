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
        public AutoFocus()
        {
            InitializeComponent();
            Show();
            string startupPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Bitmap bitmap = new Bitmap(startupPath + "/Examples/car.jpg");
            int[] focusTiles = GetTiles(bitmap);
            double score = ScoreImageGrid(bitmap, focusTiles);
            HighlightTiles(ref bitmap, focusTiles);
            Pbx.Image = bitmap;
            stopwatch.Stop();
            rtb.Text = $"Score: {score}\nTime: {stopwatch.Elapsed})";
        }
    }
}
