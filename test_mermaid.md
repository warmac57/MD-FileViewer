# Mermaid Diagram Test File

This file tests various Mermaid diagram types to see if they render correctly.

## Flowchart

```mermaid
flowchart TD
    A[Start] --> B{Is it working?}
    B -->|Yes| C[Great!]
    B -->|No| D[Add Viewer]
    C --> E[End]
    D --> E
```

## Sequence Diagram

```mermaid
sequenceDiagram
    participant User
    participant App
    participant Pandoc
    User->>App: Open MD File
    App->>Pandoc: Convert if needed
    Pandoc-->>App: Return MD
    App-->>User: Display Content
```

## Class Diagram

```mermaid
classDiagram
    class MarkdownViewer {
        +LoadMarkdownAsync()
        +currentFilePath
    }
    class FileConverter {
        +ConvertDocxToMarkdown()
        +ExportMarkdownToDocx()
    }
    MarkdownViewer --> FileConverter
```

## Gantt Chart

```mermaid
gantt
    title Project Timeline
    dateFormat  YYYY-MM-DD
    section Planning
    Research           :a1, 2025-01-01, 7d
    Design            :a2, after a1, 5d
    section Development
    Implementation    :a3, after a2, 14d
    Testing           :a4, after a3, 7d
```

## Pie Chart

```mermaid
pie title File Types Supported
    "Markdown" : 45
    "DOCX" : 30
    "Mermaid" : 25
```

## State Diagram

```mermaid
stateDiagram-v2
    [*] --> Idle
    Idle --> Loading: Open File
    Loading --> Viewing: Success
    Loading --> Error: Failed
    Viewing --> Exporting: Export to DOCX
    Exporting --> Viewing: Complete
    Error --> Idle: Retry
    Viewing --> [*]
```
