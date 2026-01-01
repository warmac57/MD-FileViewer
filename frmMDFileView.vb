Imports System.IO
Imports System.Diagnostics
Imports CDS.Markdown

Public Class frmMDFileView

    ' State variables
    Private currentFilePath As String = ""
    Private currentHtmlPath As String = ""
    Private Const DEBUG_MODE As Boolean = False  ' Set to True to see debug dialogs
    Private isWebViewInitialized As Boolean = False
    Private lastOpenedDirectory As String = ""  ' Remember last directory for Open dialog
    Private lastFilterIndex As Integer = 1  ' Remember last file type filter (1-based index)

    Private Async Sub frmMDFileView_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Initialize WebView2
        Try
            Await WebView21.EnsureCoreWebView2Async(Nothing)
            WebView21.ZoomFactor = 1.0
            isWebViewInitialized = True
        Catch ex As Exception
            MessageBox.Show($"Error initializing WebView2: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End Try

        ' Check if a file path was passed via command-line argument
        If Not String.IsNullOrEmpty(My.MyApplication.StartupFilePath) Then
            LoadMarkdownFile(My.MyApplication.StartupFilePath)
        Else
            ' Load README.md if it exists in the application directory
            Dim readmePath As String = Path.Combine(Application.StartupPath, "README.md")
            If File.Exists(readmePath) Then
                LoadMarkdownFile(readmePath)
            End If
        End If
    End Sub

    Private Sub frmMDFileView_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        ' Handle Escape key to clear content
        If e.KeyCode = Keys.Escape Then
            If isWebViewInitialized Then
                WebView21.NavigateToString("")
                currentFilePath = ""
                currentHtmlPath = ""
                lblStatus.Text = "Ready"
            End If
            e.Handled = True
        End If
    End Sub

    Private Sub frmMDFileView_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' Clean up: Delete temporary HTML file if it exists
        Try
            If Not String.IsNullOrEmpty(currentHtmlPath) AndAlso File.Exists(currentHtmlPath) Then
                File.Delete(currentHtmlPath)
            End If
        Catch
            ' Ignore errors during cleanup
        End Try
    End Sub

    Private Sub btnOpen_Click(sender As Object, e As EventArgs) Handles btnOpen.Click

        Using ofd As New OpenFileDialog()
            ofd.Filter = "Markdown Files (*.md;*.markdown)|*.md;*.markdown|Mermaid Diagrams (*.mmd;*.mermaid)|*.mmd;*.mermaid|Word Documents (*.docx)|*.docx|Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
            ofd.Title = "Open Document"
            ofd.FilterIndex = lastFilterIndex  ' Set to last used filter
            ofd.RestoreDirectory = False  ' Allow dialog to remember the directory we set

            ' Set initial directory to last opened directory if available
            If Not String.IsNullOrEmpty(lastOpenedDirectory) AndAlso Directory.Exists(lastOpenedDirectory) Then
                ofd.InitialDirectory = lastOpenedDirectory
            End If

            If ofd.ShowDialog() = DialogResult.OK Then

                ' Remember the filter index for next time
                lastFilterIndex = ofd.FilterIndex

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

            ' Set up the pandoc process with UTF-8 encoding
            Dim pandocProcess As New Process()
            pandocProcess.StartInfo.FileName = "pandoc"
            pandocProcess.StartInfo.Arguments = $"""{docxFilePath}"" -o ""{mdFilePath}"" --wrap=preserve"
            pandocProcess.StartInfo.UseShellExecute = False
            pandocProcess.StartInfo.RedirectStandardOutput = True
            pandocProcess.StartInfo.RedirectStandardError = True
            pandocProcess.StartInfo.CreateNoWindow = True
            pandocProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8
            pandocProcess.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8

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

    Private Function ConvertMarkdownToHtmlUsingPandoc(markdownContent As String) As String
        Try
            ' Create temporary markdown file with UTF-8 encoding
            Dim tempMdPath As String = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.md")
            File.WriteAllText(tempMdPath, markdownContent, System.Text.Encoding.UTF8)

            ' Set up the pandoc process to convert to HTML fragment with UTF-8 encoding
            Dim pandocProcess As New Process()
            pandocProcess.StartInfo.FileName = "pandoc"
            pandocProcess.StartInfo.Arguments = $"""{tempMdPath}"" -f markdown -t html"
            pandocProcess.StartInfo.UseShellExecute = False
            pandocProcess.StartInfo.RedirectStandardOutput = True
            pandocProcess.StartInfo.RedirectStandardError = True
            pandocProcess.StartInfo.CreateNoWindow = True
            pandocProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8
            pandocProcess.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8

            ' Start the process and capture output
            pandocProcess.Start()
            Dim htmlOutput As String = pandocProcess.StandardOutput.ReadToEnd()
            pandocProcess.WaitForExit()

            ' Clean up temp file
            Try
                File.Delete(tempMdPath)
            Catch
                ' Ignore cleanup errors
            End Try

            ' Check if conversion was successful
            If pandocProcess.ExitCode = 0 Then
                Return htmlOutput
            Else
                Dim errorOutput As String = pandocProcess.StandardError.ReadToEnd()
                Throw New Exception($"Pandoc conversion failed: {errorOutput}")
            End If

        Catch ex As Exception
            Throw New Exception($"Error converting Markdown to HTML: {ex.Message}", ex)
        End Try
    End Function

    ''' <summary>
    ''' Creates HTML for markdown content
    ''' </summary>
    Private Function CreateMarkdownHtml(markdownFilePath As String) As String
        Try
            ' Read the Markdown content with UTF-8 encoding
            Dim markdownContent As String = File.ReadAllText(markdownFilePath, System.Text.Encoding.UTF8)

            ' Convert markdown to HTML using pandoc
            Dim htmlBody As String = ConvertMarkdownToHtmlUsingPandoc(markdownContent)

            ' Get the file name for the title
            Dim fileName As String = Path.GetFileName(markdownFilePath)

            ' Create complete HTML document
            Dim htmlContent As String = $"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>{fileName}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            max-width: 900px;
            margin: 0 auto;
            padding: 20px;
            background-color: #ffffff;
            color: #333;
        }}
        h1, h2, h3, h4, h5, h6 {{
            margin-top: 24px;
            margin-bottom: 16px;
            font-weight: 600;
            line-height: 1.25;
        }}
        h1 {{ font-size: 2em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }}
        h2 {{ font-size: 1.5em; border-bottom: 1px solid #eaecef; padding-bottom: 0.3em; }}
        code {{
            background-color: #f6f8fa;
            padding: 2px 6px;
            border-radius: 3px;
            font-family: 'Consolas', 'Monaco', monospace;
            font-size: 85%;
        }}
        pre {{
            background-color: #f6f8fa;
            padding: 16px;
            border-radius: 6px;
            overflow: auto;
        }}
        pre code {{
            background-color: transparent;
            padding: 0;
        }}
        blockquote {{
            border-left: 4px solid #dfe2e5;
            padding-left: 16px;
            color: #6a737d;
            margin: 0;
        }}
        table {{
            border-collapse: collapse;
            width: 100%;
            margin: 16px 0;
        }}
        table th, table td {{
            border: 1px solid #dfe2e5;
            padding: 8px 13px;
        }}
        table th {{
            background-color: #f6f8fa;
            font-weight: 600;
        }}
        a {{
            color: #0366d6;
            text-decoration: none;
        }}
        a:hover {{
            text-decoration: underline;
        }}
        img {{
            max-width: 100%;
            height: auto;
        }}
    </style>
</head>
<body>
{htmlBody}
</body>
</html>"

            ' Create temporary HTML file with UTF-8 encoding
            Dim tempPath As String = Path.Combine(Path.GetTempPath(), $"markdown_{Guid.NewGuid()}.html")
            File.WriteAllText(tempPath, htmlContent, System.Text.Encoding.UTF8)

            Return tempPath

        Catch ex As Exception
            Throw New Exception($"Error creating Markdown HTML: {ex.Message}", ex)
        End Try
    End Function

    ''' <summary>
    ''' Creates simple browser HTML for mermaid diagrams (just renders the diagram, no pan/zoom)
    ''' </summary>
    Public Shared Function CreateMermaidHtmlForBrowser(mermaidFilePath As String) As String
        Try
            ' Read the Mermaid diagram content with UTF-8 encoding
            Dim mermaidContent As String = File.ReadAllText(mermaidFilePath, System.Text.Encoding.UTF8)

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

            ' Create simple HTML - just render the diagram naturally
            Dim htmlContent As String = $"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>{fileName}</title>
    <script src=""https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.min.js""></script>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 20px;
            padding: 0;
            background-color: #f5f5f5;
        }}
        h1 {{
            color: #333;
            text-align: center;
            margin-bottom: 20px;
        }}
        .mermaid {{
            background-color: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
    </style>
</head>
<body>
    <div class=""mermaid"">
{escapedContent}
    </div>
    <script>
        mermaid.initialize({{
            startOnLoad: true,
            theme: 'default',
            securityLevel: 'loose',
            flowchart: {{
                useMaxWidth: false
            }},
            class: {{
                useMaxWidth: false
            }},
            er: {{
                useMaxWidth: false
            }},
            sequence: {{
                useMaxWidth: false
            }},
            gantt: {{
                useMaxWidth: false
            }}
        }});
    </script>
</body>
</html>"

            ' Create temporary HTML file with UTF-8 encoding
            Dim tempPath As String = Path.Combine(Path.GetTempPath(), $"mermaid_browser_{Guid.NewGuid()}.html")
            File.WriteAllText(tempPath, htmlContent, System.Text.Encoding.UTF8)

            Return tempPath

        Catch ex As Exception
            Throw New Exception($"Error creating Mermaid HTML for browser: {ex.Message}", ex)
        End Try
    End Function

    Private Function CreateMermaidHtml(mermaidFilePath As String) As String
        Try
            ' Read the Mermaid diagram content with UTF-8 encoding
            Dim mermaidContent As String = File.ReadAllText(mermaidFilePath, System.Text.Encoding.UTF8)

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

            ' Create HTML with pan/zoom controls for the diagram
            Dim htmlContent As String = $"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{fileName}</title>
    <script src=""https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.min.js""></script>
    <script src=""https://cdn.jsdelivr.net/npm/svg-pan-zoom@3.6.1/dist/svg-pan-zoom.min.js""></script>
    <style>
        html {{
            width: 100vw;
            height: 100vh;
            margin: 0;
            padding: 0;
            overflow: hidden;
        }}
        * {{
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 0;
            width: 100vw !important;
            height: 100vh !important;
            background-color: #f5f5f5;
            overflow: hidden;
        }}
        .diagram-wrapper {{
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            width: 100vw !important;
            height: 100vh !important;
            min-width: 100vw !important;
            min-height: 100vh !important;
            background-color: white;
            border-top: 1px solid #ddd;
            overflow: hidden;
        }}
        .mermaid-container {{
            position: absolute !important;
            top: 0 !important;
            left: 0 !important;
            width: 100% !important;
            height: 100% !important;
            min-width: 100% !important;
            min-height: 100% !important;
            max-width: none !important;
            max-height: none !important;
            overflow: hidden !important;
        }}
        #diagram {{
            position: absolute !important;
            top: 0 !important;
            left: 0 !important;
            width: 100% !important;
            height: 100% !important;
            min-width: 100% !important;
            min-height: 100% !important;
        }}
        #diagram svg {{
            max-width: none !important;
            max-height: none !important;
            width: auto !important;
            height: auto !important;
            display: block;
        }}
        #error-message {{
            display: none;
            background-color: #fee;
            border: 2px solid #c33;
            color: #c33;
            padding: 20px;
            border-radius: 8px;
            margin: 20px;
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
    <div class=""diagram-wrapper"">
        <div id=""error-message""></div>
        <div id=""loading"">Loading diagram...</div>
        <div class=""mermaid-container"">
            <div class=""mermaid"" id=""diagram"">
{escapedContent}
            </div>
        </div>
    </div>
    <script>
        let panZoomInstance = null;

        // Reusable function to fit diagram to full container
        function fitToContainer() {{
            if (!panZoomInstance) return;

            // Get container dimensions (don't forcibly reset - use CSS-defined size)
            const container = document.querySelector('.mermaid-container');

            // Validate container has proper dimensions
            if (container && container.clientHeight > 100) {{
                // Update svg-pan-zoom with current container dimensions
                panZoomInstance.resize();
                panZoomInstance.updateBBox();

                // Now fit to the full container
                panZoomInstance.fit();
                panZoomInstance.center();
            }} else {{
                console.warn('Container dimensions too small, skipping fit');
            }}
        }}

        try {{
            mermaid.initialize({{
                startOnLoad: true,
                theme: 'default',
                securityLevel: 'loose',
                flowchart: {{
                    useMaxWidth: false,
                    htmlLabels: true,
                    curve: 'basis'
                }},
                class: {{
                    useMaxWidth: false
                }},
                er: {{
                    useMaxWidth: false
                }},
                sequence: {{
                    useMaxWidth: false
                }},
                gantt: {{
                    useMaxWidth: false
                }},
                logLevel: 'error'
            }});

            // Initialize pan-zoom after Mermaid renders
            mermaid.run().then(function() {{
                // Longer timeout for complex diagrams (class, ER, etc.) to fully render
                setTimeout(function() {{
                    try {{
                        const svg = document.querySelector('#diagram svg');
                        if (svg) {{
                            // Initialize svg-pan-zoom (let it manage SVG dimensions)
                            panZoomInstance = svgPanZoom(svg, {{
                                zoomEnabled: true,
                                controlIconsEnabled: false,
                                fit: false,
                                contain: false,
                                center: true,
                                minZoom: 0.1,
                                maxZoom: 10,
                                zoomScaleSensitivity: 0.3
                            }});

                            // Don't auto-fit - let diagram load at natural size
                            // Containers stay full size thanks to CSS
                            // User can click Fit button to fit diagram
                            panZoomInstance.resize();

                            // Debug: Log container dimensions
                            const container = document.querySelector('.mermaid-container');
                            const wrapper = document.querySelector('.diagram-wrapper');
                            console.log('Container dimensions:', {{
                                wrapper: {{ w: wrapper.clientWidth, h: wrapper.clientHeight }},
                                container: {{ w: container.clientWidth, h: container.clientHeight }},
                                svg: {{ w: svg.clientWidth, h: svg.clientHeight }}
                            }});

                            // Keyboard shortcuts
                            document.addEventListener('keydown', function(e) {{
                                if (e.ctrlKey && (e.key === '+' || e.key === '=')) {{
                                    e.preventDefault();
                                    panZoomInstance.zoomIn();
                                }} else if (e.ctrlKey && e.key === '-') {{
                                    e.preventDefault();
                                    panZoomInstance.zoomOut();
                                }} else if (e.ctrlKey && e.key === '0') {{
                                    e.preventDefault();
                                    fitToContainer();
                                }} else if (e.key === 'f' || e.key === 'F') {{
                                    e.preventDefault();
                                    fitToContainer();
                                }}
                            }});

                            document.getElementById('loading').style.display = 'none';
                        }} else {{
                            throw new Error('SVG element not found');
                        }}
                    }} catch(err) {{
                        console.error('Pan-zoom error:', err);
                        document.getElementById('loading').textContent = 'Diagram loaded (pan-zoom unavailable)';
                    }}
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

            ' Create temporary HTML file with UTF-8 encoding
            Dim tempPath As String = Path.Combine(Path.GetTempPath(), $"mermaid_{Guid.NewGuid()}.html")
            File.WriteAllText(tempPath, htmlContent, System.Text.Encoding.UTF8)

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

            ' Set up the pandoc process with UTF-8 encoding
            Dim pandocProcess As New Process()
            pandocProcess.StartInfo.FileName = "pandoc"
            pandocProcess.StartInfo.Arguments = $"""{mdFilePath}"" -o ""{docxFilePath}"""
            pandocProcess.StartInfo.UseShellExecute = False
            pandocProcess.StartInfo.RedirectStandardOutput = True
            pandocProcess.StartInfo.RedirectStandardError = True
            pandocProcess.StartInfo.CreateNoWindow = True
            pandocProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8
            pandocProcess.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8

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
            If Not isWebViewInitialized Then
                MessageBox.Show("WebView2 is not initialized yet. Please wait.", "Not Ready", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            If Not File.Exists(filePath) Then
                MessageBox.Show("File not found: " & filePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            ' Clean up previous HTML file
            If Not String.IsNullOrEmpty(currentHtmlPath) AndAlso File.Exists(currentHtmlPath) Then
                Try
                    File.Delete(currentHtmlPath)
                Catch
                    ' Ignore errors
                End Try
            End If

            Dim fileToLoad As String = filePath
            Dim fileExtension As String = Path.GetExtension(filePath).ToLower()
            Dim isDocx As Boolean = fileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase)
            Dim isMermaid As Boolean = fileExtension.Equals(".mmd", StringComparison.OrdinalIgnoreCase) OrElse fileExtension.Equals(".mermaid", StringComparison.OrdinalIgnoreCase)

            ' If it's a DOCX file, convert it to markdown first
            If isDocx Then
                lblStatus.Text = "Converting Word document to Markdown..."
                Application.DoEvents()

                Try
                    fileToLoad = ConvertDocxToMarkdown(filePath)
                    fileExtension = ".md"
                    isMermaid = False
                Catch ex As Exception
                    MessageBox.Show($"Error converting Word document: {ex.Message}" & vbCrLf & vbCrLf & "Please ensure Pandoc is installed and available in your system PATH.", "Conversion Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    lblStatus.Text = "Conversion failed"
                    Return
                End Try
            End If

            lblStatus.Text = "Loading file..."
            Application.DoEvents()

            ' Create HTML and load in WebView2
            Dim htmlPath As String = ""
            If isMermaid Then
                htmlPath = CreateMermaidHtml(fileToLoad)
            Else
                htmlPath = CreateMarkdownHtml(fileToLoad)
            End If

            ' Load into WebView2
            WebView21.Source = New Uri(htmlPath)

            ' Update state
            currentFilePath = filePath
            currentHtmlPath = htmlPath
            lastOpenedDirectory = Path.GetDirectoryName(filePath)  ' Remember directory for next Open

            ' Set filter index based on file extension
            Select Case fileExtension
                Case ".md", ".markdown"
                    lastFilterIndex = 1  ' Markdown Files
                Case ".mmd", ".mermaid"
                    lastFilterIndex = 2  ' Mermaid Diagrams
                Case ".docx"
                    lastFilterIndex = 3  ' Word Documents
                Case ".txt"
                    lastFilterIndex = 4  ' Text Files
                Case Else
                    lastFilterIndex = 5  ' All Files
            End Select

            Dim fileInfo As New FileInfo(filePath)
            Dim statusMessage As String = $"Loaded: {Path.GetFileName(filePath)}"
            If isDocx Then
                statusMessage &= " (converted from DOCX)"
            ElseIf isMermaid Then
                statusMessage &= " (Mermaid diagram)"
            End If
            statusMessage &= $" ({fileInfo.Length:N0} bytes)"
            lblStatus.Text = statusMessage

        Catch ex As Exception
            MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            lblStatus.Text = "Error loading file"
        End Try
    End Sub

    Private Sub btnOpenInBrowser_Click(sender As Object, e As EventArgs) Handles btnOpenInBrowser.Click
        ' Open the current file in the default browser
        Try
            If String.IsNullOrEmpty(currentFilePath) Then
                MessageBox.Show("No file is currently loaded.", "No File", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            If Not File.Exists(currentFilePath) Then
                MessageBox.Show("The current file no longer exists.", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            ' Check file type
            Dim fileExtension As String = Path.GetExtension(currentFilePath).ToLower()
            Dim isMermaid As Boolean = fileExtension.Equals(".mmd", StringComparison.OrdinalIgnoreCase) OrElse fileExtension.Equals(".mermaid", StringComparison.OrdinalIgnoreCase)

            ' Generate browser-friendly HTML
            Dim browserHtmlPath As String
            If isMermaid Then
                browserHtmlPath = CreateMermaidHtmlForBrowser(currentFilePath)
            Else
                ' For markdown files, use the existing HTML
                browserHtmlPath = currentHtmlPath
            End If

            ' Open in default browser
            Process.Start(New ProcessStartInfo(browserHtmlPath) With {.UseShellExecute = True})

        Catch ex As Exception
            MessageBox.Show($"Error opening browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Class
