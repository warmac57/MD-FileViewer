Imports System.IO
Imports System.Diagnostics
Imports CDS.Markdown

Public Class frmMDFileView

    ' State variables
    Private currentFilePath As String = ""
    Private lastMermaidHtmlPath As String = ""
    Private Const DEBUG_MODE As Boolean = False  ' Set to True to see debug dialogs

    Private Sub frmMDFileView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Check if a file path was passed via command-line argument
        If Not String.IsNullOrEmpty(My.MyApplication.StartupFilePath) Then
            LoadMarkdownFile(My.MyApplication.StartupFilePath)
        End If
    End Sub

    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click
        Using ofd As New OpenFileDialog()
            ofd.Filter = "Markdown Files (*.md;*.markdown)|*.md;*.markdown|Mermaid Diagrams (*.mmd;*.mermaid)|*.mmd;*.mermaid|Word Documents (*.docx)|*.docx|Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
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

    Private Sub btnExportDocx_Click(sender As Object, e As EventArgs) Handles btnExportDocx.Click
        ' Check if a file is currently loaded
        If String.IsNullOrEmpty(currentFilePath) Then
            MessageBox.Show("No file is currently loaded. Please open a file first.", "No File Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Check if the current file is a markdown file
        Dim currentExtension As String = Path.GetExtension(currentFilePath).ToLower()
        If currentExtension <> ".md" AndAlso currentExtension <> ".markdown" Then
            MessageBox.Show("The currently loaded file is not a Markdown file. Only Markdown files can be exported to DOCX.", "Invalid File Type", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            ' Create the output DOCX file path
            Dim sourceDirectory As String = Path.GetDirectoryName(currentFilePath)
            Dim baseFileName As String = Path.GetFileNameWithoutExtension(currentFilePath)
            Dim docxFilePath As String = Path.Combine(sourceDirectory, baseFileName & ".docx")

            ' Check if the file already exists and prompt for overwrite
            If File.Exists(docxFilePath) Then
                Dim result As DialogResult = MessageBox.Show($"The file '{Path.GetFileName(docxFilePath)}' already exists." & vbCrLf & vbCrLf & "Do you want to overwrite it?", "File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.No Then
                    Return
                End If
            End If

            ' Export the file
            ExportMarkdownToDocx(currentFilePath, docxFilePath)

            ' Show success message
            MessageBox.Show($"File successfully exported to:{vbCrLf}{docxFilePath}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information)
            lblStatus.Text = $"Exported to: {Path.GetFileName(docxFilePath)}"

        Catch ex As Exception
            MessageBox.Show($"Error exporting file: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            lblStatus.Text = "Export failed"
        End Try
    End Sub

    Private Function CreateMermaidHtml(mermaidFilePath As String) As String
        Try
            ' Read the Mermaid diagram content
            Dim mermaidContent As String = File.ReadAllText(mermaidFilePath)

            ' Auto-fix deprecated 'graph' syntax to 'flowchart'
            mermaidContent = System.Text.RegularExpressions.Regex.Replace(
                mermaidContent,
                "^graph\s+(TD|TB|BT|RL|LR)",
                "flowchart $1",
                System.Text.RegularExpressions.RegexOptions.Multiline)

            ' Escape for HTML/JavaScript
            Dim escapedContent As String = mermaidContent.Replace("\", "\\").Replace("`", "\`").Replace("$", "\$")

            ' Get the file name for the title
            Dim fileName As String = Path.GetFileName(mermaidFilePath)

            ' Create HTML with UMD version of Mermaid (more compatible with WebView2)
            Dim htmlContent As String = $"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{fileName}</title>
    <script src=""https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.min.js""></script>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        .mermaid-container {{
            background-color: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            margin: 20px auto;
            max-width: 95%;
            overflow: auto;
        }}
        h1 {{
            color: #333;
            text-align: center;
            margin-bottom: 20px;
        }}
        #error-message {{
            display: none;
            background-color: #fee;
            border: 2px solid #c33;
            color: #c33;
            padding: 20px;
            border-radius: 8px;
            margin: 20px auto;
            max-width: 800px;
            font-family: monospace;
            white-space: pre-wrap;
        }}
        #loading {{
            text-align: center;
            padding: 40px;
            color: #666;
        }}
    </style>
</head>
<body>
    <h1>{fileName}</h1>
    <div id=""error-message""></div>
    <div id=""loading"">Loading diagram...</div>
    <div class=""mermaid-container"">
        <div class=""mermaid"">
{escapedContent}
        </div>
    </div>
    <script>
        try {{
            mermaid.initialize({{
                startOnLoad: true,
                theme: 'default',
                securityLevel: 'loose',
                flowchart: {{
                    useMaxWidth: true,
                    htmlLabels: true,
                    curve: 'basis'
                }},
                logLevel: 'debug'
            }});

            // Hide loading message after render
            document.addEventListener('DOMContentLoaded', function() {{
                setTimeout(function() {{
                    document.getElementById('loading').style.display = 'none';
                }}, 1000);
            }});
        }} catch(err) {{
            console.error('Mermaid error:', err);
            document.getElementById('error-message').style.display = 'block';
            document.getElementById('error-message').textContent = 'Error: ' + err.message;
            document.getElementById('loading').style.display = 'none';
        }}
    </script>
</body>
</html>"

            ' Create temporary HTML file
            Dim tempPath As String = Path.Combine(Path.GetTempPath(), $"mermaid_{Guid.NewGuid()}.html")
            File.WriteAllText(tempPath, htmlContent)

            ' Store for debug purposes
            lastMermaidHtmlPath = tempPath

            ' Debug mode: Show diagnostic info
            If DEBUG_MODE Then
                Dim debugMsg As String = $"Debug Info:{vbCrLf}" &
                    $"Temp HTML: {tempPath}{vbCrLf}" &
                    $"Auto-converted 'graph' to 'flowchart': {mermaidContent.Contains("flowchart")}{vbCrLf}{vbCrLf}" &
                    $"Opening in default browser..."

                MessageBox.Show(debugMsg, "Mermaid Debug Mode", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            Return tempPath

        Catch ex As Exception
            Throw New Exception($"Error creating Mermaid HTML: {ex.Message}", ex)
        End Try
    End Function

    Private Sub ExportMarkdownToDocx(mdFilePath As String, docxFilePath As String)
        Try
            lblStatus.Text = "Exporting to DOCX..."
            Application.DoEvents()

            ' Set up the pandoc process
            Dim pandocProcess As New Process()
            pandocProcess.StartInfo.FileName = "pandoc"
            pandocProcess.StartInfo.Arguments = $"""{mdFilePath}"" -o ""{docxFilePath}"""
            pandocProcess.StartInfo.UseShellExecute = False
            pandocProcess.StartInfo.RedirectStandardOutput = True
            pandocProcess.StartInfo.RedirectStandardError = True
            pandocProcess.StartInfo.CreateNoWindow = True

            ' Start the process and wait for it to complete
            pandocProcess.Start()
            pandocProcess.WaitForExit()

            ' Check if conversion was successful
            If pandocProcess.ExitCode <> 0 Then
                Dim errorOutput As String = pandocProcess.StandardError.ReadToEnd()
                Throw New Exception($"Pandoc conversion failed: {errorOutput}")
            End If

            If Not File.Exists(docxFilePath) Then
                Throw New Exception("The DOCX file was not created successfully.")
            End If

        Catch ex As Exception
            Throw New Exception($"Error exporting Markdown to DOCX: {ex.Message}", ex)
        End Try
    End Sub

    Private Async Sub LoadMarkdownFile(filePath As String)
        Try
            If Not File.Exists(filePath) Then
                MessageBox.Show("File not found: " & filePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            Dim fileToLoad As String = filePath
            Dim fileExtension As String = Path.GetExtension(filePath).ToLower()
            Dim isDocx As Boolean = fileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase)
            Dim isMermaid As Boolean = fileExtension.Equals(".mmd", StringComparison.OrdinalIgnoreCase) OrElse fileExtension.Equals(".mermaid", StringComparison.OrdinalIgnoreCase)

            ' If it's a Mermaid file, open in browser instead of WebView2
            If isMermaid Then
                lblStatus.Text = "Opening Mermaid diagram in browser..."
                Application.DoEvents()

                Try
                    ' Create HTML and open in default browser
                    Dim htmlPath As String = CreateMermaidHtml(filePath)

                    ' Open in browser (debug mode already handles this, but we'll do it explicitly here too)
                    If Not DEBUG_MODE Then
                        Process.Start(New ProcessStartInfo(htmlPath) With {.UseShellExecute = True})
                    End If

                    ' Update state and status
                    currentFilePath = filePath
                    Dim mermaidFileInfo As New FileInfo(filePath)
                    lblStatus.Text = $"Opened in browser: {Path.GetFileName(filePath)} ({mermaidFileInfo.Length:N0} bytes)"

                    ' Exit early - no need to load into MarkdownViewer
                    Return

                Catch ex As Exception
                    MessageBox.Show($"Error rendering Mermaid diagram: {ex.Message}", "Rendering Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    lblStatus.Text = "Rendering failed"
                    Return
                End Try
            End If

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
            ElseIf isMermaid Then
                statusMessage &= " (Mermaid diagram)"
                If DEBUG_MODE AndAlso Not String.IsNullOrEmpty(lastMermaidHtmlPath) Then
                    statusMessage &= $" | Debug: {lastMermaidHtmlPath}"
                End If
            End If
            statusMessage &= $" ({fileInfo.Length:N0} bytes)"
            lblStatus.Text = statusMessage

        Catch ex As Exception
            MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            lblStatus.Text = "Error loading file"
        End Try
    End Sub

    Private Sub MarkdownViewer1_Load(sender As Object, e As EventArgs) Handles MarkdownViewer1.Load

    End Sub
End Class
