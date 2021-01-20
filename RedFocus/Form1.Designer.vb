<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.numSteepness = New System.Windows.Forms.NumericUpDown()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.numStepSize = New System.Windows.Forms.NumericUpDown()
        Me.Panel1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numSteepness, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numStepSize, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Panel1
        '
        Me.Panel1.AllowDrop = True
        Me.Panel1.Controls.Add(Me.PictureBox1)
        Me.Panel1.Location = New System.Drawing.Point(0, 0)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(494, 473)
        Me.Panel1.TabIndex = 0
        '
        'PictureBox1
        '
        Me.PictureBox1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.PictureBox1.Location = New System.Drawing.Point(0, 0)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(494, 473)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        '
        'numSteepness
        '
        Me.numSteepness.DecimalPlaces = 2
        Me.numSteepness.Increment = New Decimal(New Integer() {5, 0, 0, 131072})
        Me.numSteepness.Location = New System.Drawing.Point(77, 479)
        Me.numSteepness.Maximum = New Decimal(New Integer() {1, 0, 0, 0})
        Me.numSteepness.Name = "numSteepness"
        Me.numSteepness.Size = New System.Drawing.Size(75, 23)
        Me.numSteepness.TabIndex = 1
        Me.numSteepness.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 481)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(59, 15)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Steepness"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(167, 481)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(240, 15)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Step Size (Higher = Faster = Less Resolution)"
        '
        'numStepSize
        '
        Me.numStepSize.Location = New System.Drawing.Point(413, 479)
        Me.numStepSize.Maximum = New Decimal(New Integer() {10, 0, 0, 0})
        Me.numStepSize.Name = "numStepSize"
        Me.numStepSize.Size = New System.Drawing.Size(75, 23)
        Me.numStepSize.TabIndex = 3
        Me.numStepSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(494, 511)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.numStepSize)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.numSteepness)
        Me.Controls.Add(Me.Panel1)
        Me.Name = "Form1"
        Me.Text = "Autofocus"
        Me.Panel1.ResumeLayout(False)
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numSteepness, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numStepSize, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Panel1 As Panel
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents numSteepness As NumericUpDown
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents numStepSize As NumericUpDown
End Class
