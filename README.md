# WebSharper FileSystem API Binding

This repository provides an F# [WebSharper](https://websharper.com/) binding for the [File System API](https://developer.mozilla.org/en-US/docs/Web/API/File_System_API), enabling WebSharper applications to interact with the file system, open directories, and read file metadata.

## Repository Structure

The repository consists of two main projects:

1. **Binding Project**:

   - Contains the F# WebSharper binding for the File System API.

2. **Sample Project**:
   - Demonstrates how to use the File System API with WebSharper syntax.
   - Includes a GitHub Pages demo: [View Demo](https://dotnet-websharper.github.io/FileSystemAPI/)

## Installation

To use this package in your WebSharper project, add the NuGet package:

```bash
   dotnet add package WebSharper.FileSystem
```

## Building

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/dotnet-websharper/FileSystem.git
   cd FileSystem
   ```

2. Build the Binding Project:

   ```bash
   dotnet build WebSharper.FileSystem/WebSharper.FileSystem.fsproj
   ```

3. Build and Run the Sample Project:

   ```bash
   cd WebSharper.FileSystem.Sample
   dotnet build
   dotnet run
   ```

## Example Usage

Below is an example of how to use the File System API in a WebSharper project:

```fsharp
namespace WebSharper.FileSystem.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.FileSystem

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    // Function to open a directory picker and read file names
    let openDirectory () =
        promise {
            try
                // Open directory picker
                let window = As<Window>(JS.Window)
                let! handle = window.ShowDirectoryPicker()
                let fileList = JS.Document.GetElementById("fileList")
                fileList.InnerHTML <- ""

                let iterator = handle.Values()

                // Function to iterate over directory contents
                let rec loop () =
                    promise {
                        let! result = iterator?next() |> Promise.AsAsync
                        if not (isNull result) then
                            let isDone = result?``done`` |> As<bool>
                            if not isDone then
                                let entry = result?value |> As<FileSystemFileHandle>
                                let name = entry.Name
                                let kind = entry.Kind
                                let icon = if kind = "directory" then " 📁" else " 📄"

                                // Create a list item for each file or directory
                                let listItem = JS.Document.CreateElement("li")
                                listItem.TextContent <- $"{name}{icon}"
                                fileList.AppendChild(listItem) |> ignore

                                return! loop ()
                    }
                do! loop ()
            with ex ->
                Console.Error($"Error accessing directory: {ex.Message}")
        }

    [<SPAEntryPoint>]
    let Main () =

        IndexTemplate.Main()
            .OpenDirectory(fun _ ->
                async {
                    do! openDirectory () |> Promise.AsAsync
                }
                |> Async.StartImmediate
            )
            .Doc()
        |> Doc.RunById "main"
```

## Important Considerations

- **Browser Compatibility**: The File System API is currently supported only in modern browsers with appropriate permissions.
- **Security Restrictions**: Access to files and directories is subject to user permissions and browser security policies.
- **Read-Only Mode**: Some browsers may only allow read operations without modification access.
