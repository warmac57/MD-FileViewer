<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMDFileView
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        WebView21 = New Microsoft.Web.WebView2.WinForms.WebView2()
        btnOpen = New Button()
        btnExportDocx = New Button()
        lblStatus = New Label()
        Label1 = New Label()
        btnOpenInBrowser = New Button()
        SplitContainer1 = New SplitContainer()
        CType(WebView21, ComponentModel.ISupportInitialize).BeginInit()
        CType(SplitContainer1, ComponentModel.ISupportInitialize).BeginInit()
        SplitContainer1.Panel1.SuspendLayout()
        SplitContainer1.Panel2.SuspendLayout()
        SplitContainer1.SuspendLayout()
        SuspendLayout()
        ' 
        ' WebView21
        ' 
        WebView21.AllowExternalDrop = True
        WebView21.CreationProperties = Nothing
        WebView21.DefaultBackgroundColor = Color.White
        WebView21.Dock = DockStyle.Fill
        WebView21.Location = New Point(0, 0)
        WebView21.Name = "WebView21"
        WebView21.Size = New Size(633, 433)
        WebView21.TabIndex = 0
        WebView21.ZoomFactor = 1R
        ' 
        ' btnOpen
        ' 
        btnOpen.Location = New Point(543, 9)
        btnOpen.Name = "btnOpen"
        btnOpen.Size = New Size(78, 40)
        btnOpen.TabIndex = 1
        btnOpen.Text = "Open"
        btnOpen.UseVisualStyleBackColor = True
        ' 
        ' btnExportDocx
        ' 
        btnExportDocx.Location = New Point(459, 9)
        btnExportDocx.Name = "btnExportDocx"
        btnExportDocx.Size = New Size(78, 40)
        btnExportDocx.TabIndex = 4
        btnExportDocx.TabStop = False
        btnExportDocx.Text = "Export to DOCX"
        btnExportDocx.UseVisualStyleBackColor = True
        ' 
        ' lblStatus
        ' 
        lblStatus.BorderStyle = BorderStyle.FixedSingle
        lblStatus.Dock = DockStyle.Bottom
        lblStatus.Location = New Point(0, 406)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(633, 27)
        lblStatus.TabIndex = 2
        lblStatus.Text = "Open File.md"
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Label1
        ' 
        Label1.Location = New Point(3, 9)
        Label1.Name = "Label1"
        Label1.Size = New Size(366, 40)
        Label1.TabIndex = 3
        Label1.Text = "Opens Markdown (MD) and renders Mermaid (MMD)  diagrams, or converts DOCX <> MD. Use Mouse to Pan and Zoom view."
        Label1.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' btnOpenInBrowser
        ' 
        btnOpenInBrowser.Location = New Point(375, 9)
        btnOpenInBrowser.Name = "btnOpenInBrowser"
        btnOpenInBrowser.Size = New Size(78, 40)
        btnOpenInBrowser.TabIndex = 5
        btnOpenInBrowser.TabStop = False
        btnOpenInBrowser.Text = "Open in Browser"
        btnOpenInBrowser.UseVisualStyleBackColor = True
        ' 
        ' SplitContainer1
        ' 
        SplitContainer1.Dock = DockStyle.Fill
        SplitContainer1.Location = New Point(0, 0)
        SplitContainer1.Name = "SplitContainer1"
        SplitContainer1.Orientation = Orientation.Horizontal
        ' 
        ' SplitContainer1.Panel1
        ' 
        SplitContainer1.Panel1.Controls.Add(btnOpen)
        SplitContainer1.Panel1.Controls.Add(btnExportDocx)
        SplitContainer1.Panel1.Controls.Add(btnOpenInBrowser)
        SplitContainer1.Panel1.Controls.Add(Label1)
        ' 
        ' SplitContainer1.Panel2
        ' 
        SplitContainer1.Panel2.Controls.Add(lblStatus)
        SplitContainer1.Panel2.Controls.Add(WebView21)
        SplitContainer1.Size = New Size(633, 493)
        SplitContainer1.SplitterDistance = 56
        SplitContainer1.TabIndex = 6
        ' 
        ' frmMDFileView
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(633, 493)
        Controls.Add(SplitContainer1)
        FormBorderStyle = FormBorderStyle.SizableToolWindow
        KeyPreview = True
        Name = "frmMDFileView"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Markdown File Viewer -- Sideline Software Systems"
        CType(WebView21, ComponentModel.ISupportInitialize).EndInit()
        SplitContainer1.Panel1.ResumeLayout(False)
        SplitContainer1.Panel2.ResumeLayout(False)
        CType(SplitContainer1, ComponentModel.ISupportInitialize).EndInit()
        SplitContainer1.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents WebView21 As Microsoft.Web.WebView2.WinForms.WebView2
    Friend WithEvents btnOpen As Button
    Friend WithEvents btnExportDocx As Button
    Friend WithEvents lblStatus As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents btnOpenInBrowser As Button
    Friend WithEvents SplitContainer1 As SplitContainer

End Class
