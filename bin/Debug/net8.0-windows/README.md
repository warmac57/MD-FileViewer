# Markdown File Viewer

**By Sideline Software Systems - Cedar Rapids, IA**

A versatile document viewer for Markdown files and Mermaid diagrams with built-in conversion capabilities.

## Features

- **Markdown Rendering**: View `.md` and `.markdown` files with styled HTML output
- **Mermaid Diagrams**: Render `.mmd` and `.mermaid` diagram files with interactive pan/zoom
- **DOCX Conversion**: Convert Word documents (`.docx`) to Markdown and view them
- **Export to DOCX**: Convert Markdown files back to Word format using Pandoc
- **Browser Integration**: Open any document in your default web browser

## Supported File Types

| Extension | Description |
|-----------|-------------|
| `.md`, `.markdown` | Markdown documents |
| `.mmd`, `.mermaid` | Mermaid diagram files |
| `.docx` | Microsoft Word documents (converted to MD) |
| `.txt` | Plain text files |

## Keyboard Shortcuts

### Mermaid Diagram Navigation

| Key | Action |
|-----|--------|
| **Ctrl +** or **Ctrl =** | Zoom in |
| **Ctrl -** | Zoom out |
| **Ctrl 0** or **F** | Fit diagram to screen |
| **Mouse Scroll** | Zoom in/out |
| **Click + Drag** | Pan around diagram |
| **Esc** | Clear current view |

## Buttons

- **Open**: Select a file to view (MD, MMD, DOCX, TXT)
- **Open in Browser**: Open the current document in your default web browser
- **Export to DOCX**: Convert the current Markdown file to Word format

## Requirements

- **Pandoc**: Required for DOCX conversion features
  - Install from: https://pandoc.org/installing.html
  - Must be available in your system PATH

## Technical Details

- Built with VB.NET and .NET 8.0
- Uses WebView2 for rendering
- Mermaid diagrams rendered with Mermaid.js v11
- Markdown conversion via Pandoc

## Usage Tips

1. **Command-line**: You can pass a file path as an argument to open it directly:
   ```
   MD-DocViewer.exe "path\to\your\file.md"
   ```

2. **Mermaid Diagrams**: Automatically converts deprecated `graph` syntax to `flowchart`

3. **Pan and Zoom**: For Mermaid diagrams, use the zoom controls in the top-right corner or keyboard shortcuts

4. **Export**: Only Markdown files can be exported to DOCX format

## Mermaid Diagram Types Supported

- Flowcharts
- Sequence diagrams
- Class diagrams
- State diagrams
- Entity-relationship diagrams
- Gantt charts
- And more...

## File Location

Temporary HTML files are created in your system temp folder and automatically cleaned up when the viewer is closed.

---

**Need Help?**

For issues or questions, visit: https://github.com/warmac57/MD-FileViewer
