
namespace AutoFocus
{
    partial class AutoFocus
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Plot = new ScottPlot.FormsPlot();
            this.Pbx = new System.Windows.Forms.PictureBox();
            this.FocusWorker = new System.ComponentModel.BackgroundWorker();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Pbx)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.Plot, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.Pbx, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(322, 442);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // Plot
            // 
            this.Plot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Plot.Location = new System.Drawing.Point(4, 224);
            this.Plot.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Plot.Name = "Plot";
            this.Plot.Size = new System.Drawing.Size(314, 215);
            this.Plot.TabIndex = 1;
            this.Plot.Visible = false;
            // 
            // Pbx
            // 
            this.Pbx.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Pbx.Image = global::AutoFocus.Properties.Resources.RunSPOT;
            this.Pbx.Location = new System.Drawing.Point(3, 3);
            this.Pbx.Name = "Pbx";
            this.Pbx.Size = new System.Drawing.Size(316, 215);
            this.Pbx.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Pbx.TabIndex = 2;
            this.Pbx.TabStop = false;
            // 
            // FocusWorker
            // 
            this.FocusWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.FocusWorker_DoWork);
            this.FocusWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.FocusWorker_RunWorkerCompleted);
            // 
            // AutoFocus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(322, 442);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "AutoFocus";
            this.Text = "AutoFocus";
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Pbx)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private ScottPlot.FormsPlot Plot;
        private System.Windows.Forms.PictureBox Pbx;
        private System.ComponentModel.BackgroundWorker FocusWorker;
    }
}