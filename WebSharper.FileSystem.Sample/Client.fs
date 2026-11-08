namespace WebSharper.FileSystem.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.FileSystem

[<JavaScript>]
module Client =
    // The template is loaded from the DOM, so editing index.html and refreshing
    // the browser is enough; a rebuild is only needed when holes change.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let status = Var.Create "Ready. Pick a directory to inspect."
    let directoryName = Var.Create "No directory selected."
    let entries = Var.Create ([] : string list)

    let describeEntry (handle: FileSystemHandle) =
        let icon = if handle.Kind = "directory" then "📁" else "📄"
        $"{icon} {handle.Name}"

    let openDirectory () =
        promise {
            try
                status := "Opening the directory picker..."
                let! handle = JS.Window.ShowDirectoryPicker()
                directoryName := handle.Name
                entries := []
                status := "Reading the directory contents..."

                let iterator = handle.Values()
                let items = ResizeArray<string>()

                let rec loop () =
                    promise {
                        let! result = iterator?next() |> Promise.AsAsync
                        if not (isNull result) then
                            let isDone = result?``done`` |> As<bool>
                            if not isDone then
                                let entry = result?value |> As<FileSystemHandle>
                                items.Add (describeEntry entry)
                                return! loop ()
                    }

                do! loop ()

                entries := List.ofSeq items
                status := $"Found {items.Count} entries in {handle.Name}."
            with ex ->
                status := $"Error: {ex.Message}"
        }

    [<SPAEntryPoint>]
    let Main () =
        IndexTemplate()
            .openDirectory(fun _ -> openDirectory () |> ignore)
            .directoryName(directoryName.V)
            .status(status.V)
            .entries(
                Doc.BindSeqCached (fun name -> Doc.Element "li" [] [ Doc.TextNode name ]) entries.View
            )
            .Bind()
