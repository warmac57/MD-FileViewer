# Markdown Document Viewer

A Windows Forms desktop application for viewing Markdown documents, built with VB.NET using the CDS.Markdown package.

## Features

- **Clean Markdown Rendering**: Uses CDS.Markdown to convert Markdown to beautifully formatted HTML
- **Dark Mode**: Toggle between light and dark themes
- **Font Size Control**: Easily adjust font size with keyboard shortcuts
- **File Operations**: Open and reload Markdown files
- **Source View**: Split view to see both rendered and source Markdown (optional)
- **Keyboard Shortcuts**: 
  - Ctrl+O: Open file
  - F5: Reload current file
  - Ctrl++: Increase font size
  - Ctrl+-: Decrease font size
  - Ctrl+0: Reset font size

## Setup Instructions

### Option 1: Using Visual Studio

1. Open Visual Studio 2019 or later
2. File → New → Project
3. Select "Windows Forms App (.NET)" for Visual Basic
4. Copy the provided `MarkdownViewer.vb` code into your Form1.vb (or create new class)
5. Install the NuGet package:
   - Tools → NuGet Package Manager → Manage NuGet Packages for Solution
   - Search for "CDS.Markdown"
   - Install the package

### Option 2: Using .NET CLI

1. Create a new directory for your project
2. Copy both `MarkdownViewer.vb` and `MarkdownViewer.vbproj` to the directory
3. Open command prompt in that directory
4. Run: `dotnet restore`
5. Run: `dotnet build`
6. Run: `dotnet run`

### Option 3: Using Existing VB.NET Project

1. Add the `MarkdownViewer.vb` file to your existing project
2. Install CDS.Markdown via NuGet Package Manager Console:
   ```
   Install-Package CDS.Markdown
   ```
3. Build and run

## Usage

1. Launch the application
2. Click "Open" or press Ctrl+O to select a Markdown file (.md, .markdown, docx, or .txt)
3. The document will be rendered in the main viewing area
4. Use the View menu to:
   - Toggle Dark Mode
   - Adjust font size
5. Press F5 to reload the current document (useful when editing)
6. Converts and saves *.docx file to md format using pandoc
7. pandoc must be installed for use. Visit https://pandoc.org/ to download and install
   
## Supported Markdown Features

The CDS.Markdown package supports standard Markdown syntax including:
- Headings (H1-H6)
- Bold and italic text
- Links and images
- Code blocks and inline code
- Lists (ordered and unordered)
- Blockquotes
- Tables
- Horizontal rules

## System Requirements

- Windows 7 or later
- .NET Framework 4.7.2+ or .NET 6.0+
- Visual Studio 2019 or later (for development)

## Customization

You can easily customize the appearance by modifying the `GetCssStyle()` function to adjust:
- Colors and themes
- Font families
- Spacing and margins
- Code block styling
- Table appearance

## Notes

- The application uses the WebBrowser control for rendering HTML
- Files are read using UTF-8 encoding by default
- The status bar shows file size and current status
- Large files are supported (within memory constraints)

## License

Free to use and modify for personal or commercial projects.
