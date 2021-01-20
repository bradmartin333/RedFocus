Public Class Form1
    Dim files() As String
    Dim steepness As Double = 0.5
    Dim stepSize As Integer = 5
    Dim loaded = False

    Private Sub Images_DragDrop(sender As Panel, e As DragEventArgs) Handles Panel1.DragDrop
        files = CType(e.Data.GetData(DataFormats.FileDrop), String())
        loaded = True
        IterateFiles()
    End Sub

    Private Sub Image_DragEnter(sender As Object, e As DragEventArgs) Handles Panel1.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub IterateFiles()
        BackColor = Color.Yellow
        Refresh()
        Dim scores As New List(Of Integer)
        For Each f As String In files
            scores.Add(Score(New Bitmap(Image.FromFile(f))))
        Next
        ShowResult(files(scores.IndexOf(scores.Min())))
        BackColor = SystemColors.Control
    End Sub

    Private Sub ShowResult(file As String)
        PictureBox1.Image = Image.FromFile(file)
        Text = file.Split("\").Last.Replace(".bmp", "")
    End Sub

    Private Function Score(img As Bitmap)
        Dim vals(255) As Double
        For i As Integer = 0 To img.Width - 1 Step stepSize
            For j As Integer = 0 To img.Height - 1 Step stepSize
                vals(img.GetPixel(i, j).R) += 1
            Next
        Next

        Dim peak = vals.ToList.IndexOf(vals.Max())
        Dim left, right As Integer
        For i As Integer = 0 To 255
            If left = 0 AndAlso peak - i > 0 AndAlso vals(peak - i) / vals(peak) < steepness Then left = peak - i
            If right = 0 AndAlso peak + i < 255 AndAlso vals(peak + i) / vals(peak) < steepness Then right = peak + i
        Next
        Return right - left
    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        numSteepness.Value = steepness
        numStepSize.Value = stepSize
    End Sub

    Private Sub numSteepness_ValueChanged(sender As Object, e As EventArgs) Handles numSteepness.ValueChanged
        steepness = numSteepness.Value
        If loaded Then IterateFiles()
    End Sub

    Private Sub numStepSize_ValueChanged(sender As Object, e As EventArgs) Handles numStepSize.ValueChanged
        stepSize = numStepSize.Value
        If loaded Then IterateFiles()
    End Sub
End Class
