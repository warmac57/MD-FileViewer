Imports System.IO
Imports System.Diagnostics
Imports CDS.Markdown

Public Class frmMDFileView

    ' State variables
    Private currentFilePath As String = ""

    Private Sub frmMDFileView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Check if a file path was passed via command-line argument
        If Not String.IsNullOrEmpty(My.MyApplication.StartupFilePath) Then
            LoadMarkdownFile(My.MyApplication.StartupFilePath)
        End If
    End Sub

    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Markdown Files (*.md;*.markdown)|*.md;*.markdown|Word Documents (*.docx)|*.docx|Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
            ofd.Title = "Open Document"

            If ofd.ShowDialog() = DialogResult.OK Then
                LoadMarkdownFile(ofd.FileName)
            End If
        End Using
    End Sub

    Private Function ConvertDocxToMarkdown(docxFilePath As String) As String
        Try
            ' Create markdown file path in the same directory as the source file
            Dim sourceDirectory As String = Path.GetDirectoryName(docxFilePath)
            Dim baseFileName As String = Path.GetFileNameWithoutExtension(docxFilePath)
            Dim mdFilePath As String = Path.Combine(sourceDirectory, baseFileName & ".md")

            ' Set up the pandoc process
            Dim pandocProcess As New Process()
            pandocProcess.StartInfo.FileName = "pandoc"
            pandocProcess.StartInfo.Arguments = $"""{docxFilePath}"" -o ""{mdFilePath}"""
            pandocProcess.StartInfo.UseShellExecute = False
            pandocProcess.StartInfo.RedirectStandardOutput = True
            pandocProcess.StartInfo.RedirectStandardError = True
            pandocProcess.StartInfo.CreateNoWindow = True

            ' Start the process and wait for it to complete
            pandocProcess.Start()
            pandocProcess.WaitForExit()

            ' Check if conversion was successful
            If pandocProcess.ExitCode = 0 AndAlso File.Exists(mdFilePath) Then
                Return mdFilePath
            Else
                Dim errorOutput As String = pandocProcess.StandardError.ReadToEnd()
                Throw New Exception($"Pandoc conversion failed: {errorOutput}")
            End If

        Catch ex As Exception
            Throw New Exception($"Error converting DOCX to Markdown: {ex.Message}", ex)
        End Try
    End Function

    Private Async Sub LoadMarkdownFile(filePath As String)
        Try
            If Not File.Exists(filePath) Then
                MessageBox.Show("File not found: " & filePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            Dim fileToLoad As String = filePath
            Dim isDocx As Boolean = Path.GetExtension(filePath).Equals(".docx", StringComparison.OrdinalIgnoreCase)

            ' If it's a DOCX file, convert it to markdown first
            If isDocx Then
                lblStatus.Text = "Converting Word document to Markdown..."
                Application.DoEvents()

                Try
                    fileToLoad = ConvertDocxToMarkdown(filePath)
                Catch ex As Exception
                    MessageBox.Show($"Error converting Word document: {ex.Message}" & vbCrLf & vbCrLf & "Please ensure Pandoc is installed and available in your system PATH.", "Conversion Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    lblStatus.Text = "Conversion failed"
                    Return
                End Try
            End If

            lblStatus.Text = "Loading file..."
            Application.DoEvents()

            ' Use the CDS.Markdown control's LoadMarkdownAsync method
            Await MarkdownViewer1.LoadMarkdownAsync(fileToLoad)

            ' Update state
            currentFilePath = filePath
            'filePathLabel.Text = filePath
            'reloadMenuItem.Enabled = True
            'reloadButton.Enabled = True

            Dim fileInfo As New FileInfo(filePath)
            Dim statusMessage As String = $"Loaded: {Path.GetFileName(filePath)}"
            If isDocx Then
                statusMessage &= " (converted from DOCX)"
            End If
            statusMessage &= $" ({fileInfo.Length:N0} bytes)"
            lblStatus.Text = statusMessage

        Catch ex As Exception
            MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            lblStatus.Text = "Error loading file"
        End Try
    End Sub


End Class
