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

    let openDirectory () =
        promise {
            try
                // Open directory picker
                let window = As<Window>(JS.Window)
                let! handle = window.ShowDirectoryPicker()
                let fileList = JS.Document.GetElementById("fileList")
                fileList.InnerHTML <- ""

                let iterator = handle.Values()

                // Loop through directory entries asynchronously
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
