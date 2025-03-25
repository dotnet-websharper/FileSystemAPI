namespace WebSharper.FileSystem

open WebSharper
open WebSharper.JavaScript

[<JavaScript; AutoOpen>]
module Extensions =

    type Window with

        [<Inline "$this.showDirectoryPicker($options)">]
        member this.ShowDirectoryPicker
            (options: ShowDirectoryPickerOptions) : Promise<FileSystemDirectoryHandle> = X<Promise<FileSystemDirectoryHandle>>
        [<Inline "$this.showDirectoryPicker()">]
        member this.ShowDirectoryPicker() : Promise<FileSystemDirectoryHandle> = X<Promise<FileSystemDirectoryHandle>>

        [<Inline "$this.showOpenFilePicker($options)">]
        member this.ShowOpenFilePicker
            (options: FilePickerOptions) : Promise<FileSystemFileHandle[]> = X<Promise<FileSystemFileHandle[]>>
        [<Inline "$this.showOpenFilePicker()">]
        member this.ShowOpenFilePicker() : Promise<FileSystemFileHandle[]> = X<Promise<FileSystemFileHandle[]>>

        [<Inline "$this.showSaveFilePicker($options)">]
        member this.ShowSaveFilePicker
            (options: SaveFilePickerOptions) : Promise<FileSystemFileHandle> = X<Promise<FileSystemFileHandle>>
        [<Inline "$this.showSaveFilePicker()">]
        member this.ShowSaveFilePicker() : Promise<FileSystemFileHandle> = X<Promise<FileSystemFileHandle>>
