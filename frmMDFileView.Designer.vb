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
        MarkdownViewer1 = New CDS.Markdown.MarkdownViewer()
        btnOpen = New Button()
        btnExportDocx = New Button()
        lblStatus = New Label()
        Label1 = New Label()
        SuspendLayout()
        ' 
        ' MarkdownViewer1
        ' 
        MarkdownViewer1.Dock = DockStyle.Fill
        MarkdownViewer1.Location = New Point(0, 0)
        MarkdownViewer1.Name = "MarkdownViewer1"
        MarkdownViewer1.Size = New Size(633, 493)
        MarkdownViewer1.TabIndex = 0
        ' 
        ' btnOpen
        ' 
        btnOpen.Location = New Point(543, 0)
        btnOpen.Name = "btnOpen"
        btnOpen.Size = New Size(78, 43)
        btnOpen.TabIndex = 1
        btnOpen.Text = "Open"
        btnOpen.UseVisualStyleBackColor = True
        ' 
        ' btnExportDocx
        ' 
        btnExportDocx.Location = New Point(453, 0)
        btnExportDocx.Name = "btnExportDocx"
        btnExportDocx.Size = New Size(84, 43)
        btnExportDocx.TabIndex = 4
        btnExportDocx.Text = "Export to DOCX"
        btnExportDocx.UseVisualStyleBackColor = True
        ' 
        ' lblStatus
        ' 
        lblStatus.BorderStyle = BorderStyle.FixedSingle
        lblStatus.Dock = DockStyle.Bottom
        lblStatus.Location = New Point(0, 466)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(633, 27)
        lblStatus.TabIndex = 2
        lblStatus.Text = "Open File.md"
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Label1
        ' 
        Label1.Location = New Point(204, 9)
        Label1.Name = "Label1"
        Label1.Size = New Size(243, 34)
        Label1.TabIndex = 3
        Label1.Text = "Opens Markdown and renders Mermaid diagrams, or converts DOCX to MD."
        ' 
        ' frmMDFileView
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(633, 493)
        Controls.Add(Label1)
        Controls.Add(lblStatus)
        Controls.Add(btnExportDocx)
        Controls.Add(btnOpen)
        Controls.Add(MarkdownViewer1)
        FormBorderStyle = FormBorderStyle.SizableToolWindow
        Name = "frmMDFileView"
        StartPosition = FormStartPosition.CenterScreen
        Text = "MD and MMD File Viewer by Sideline Software - Cedar Rapids IA"
        ResumeLayout(False)
    End Sub

    Friend WithEvents MarkdownViewer1 As CDS.Markdown.MarkdownViewer
    Friend WithEvents btnOpen As Button
    Friend WithEvents btnExportDocx As Button
    Friend WithEvents lblStatus As Label
    Friend WithEvents Label1 As Label

End Class
