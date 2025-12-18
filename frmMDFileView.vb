Imports System.IO
Imports CDS.Markdown

Public Class frmMDFileView

    ' State variables
    Private currentFilePath As String = ""

    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Markdown Files (*.md;*.markdown)|*.md;*.markdown|Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
            ofd.Title = "Open Markdown Document"

            If ofd.ShowDialog() = DialogResult.OK Then
                LoadMarkdownFile(ofd.FileName)
            End If
        End Using
    End Sub
    Private Async Sub LoadMarkdownFile(filePath As String)
        Try
            If Not File.Exists(filePath) Then
                MessageBox.Show("File not found: " & filePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            lblStatus.Text = "Loading file..."
            Application.DoEvents()

            ' Use the CDS.Markdown control's LoadMarkdownAsync method
            Await MarkdownViewer1.LoadMarkdownAsync(filePath)

            ' Update state
            currentFilePath = filePath
            'filePathLabel.Text = filePath
            'reloadMenuItem.Enabled = True
            'reloadButton.Enabled = True

            Dim fileInfo As New FileInfo(filePath)
            lblStatus.Text = $"Loaded: {Path.GetFileName(filePath)} ({fileInfo.Length:N0} bytes)"

        Catch ex As Exception
            MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            lblStatus.Text = "Error loading file"
        End Try
    End Sub


End Class
