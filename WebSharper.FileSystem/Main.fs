namespace WebSharper.FileSystem

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =

    module Enum = 
        let FileSystemChangeRecordType = 
            Pattern.EnumStrings "FileSystemChangeRecordType" [
                "appeared"
                "disappeared"
                "errored"
                "modified"
                "moved"
                "unknown"
            ]

        let QueryPermissionMode = 
            Pattern.EnumStrings "QueryPermissionMode" [
                "read"
                "readwrite"
            ]

    let FileSystemChangeRecord =
        Pattern.Config "FileSystemChangeRecord" {
            Required = []
            Optional = [
                "changedHandle", T<obj> // Can be FileSystemFileHandle, FileSystemDirectoryHandle, or FileSystemSyncAccessHandle
                "relativePathComponents", !|T<obj> // Array of path components leading to changedHandle
                "relativePathMovedFrom", !|T<obj> // Path components of the previous location (for "moved" type)
                "root", T<obj> // The root file system handle
                "type", T<string> // Type of change observed ("appeared", "disappeared", "errored", etc.)
            ]
        }

    let PermissionDescriptor = 
        Pattern.Config "PermissionDescriptor" {
            Required = []
            Optional = [
                "mode", Enum.QueryPermissionMode.Type
            ]
        }

    let FileSystemHandleRemoveOptions = 
        Pattern.Config "FileSystemHandleRemoveOptions" {
            Required = []
            Optional = [
                "recursive", T<bool>
            ]
        }

    let FileSystemHandle =
        Class "FileSystemHandle"
        |+> Instance [
            "kind" =? T<string> // Read-only: "file" if entry is a file, "directory" if it's a directory
            "name" =? T<string> // Read-only: Name of the associated entry
            
            "isSameEntry" => TSelf?fileSystemHandle ^-> T<Promise<_>>[T<bool>] 
            "queryPermission" => !?PermissionDescriptor?descriptor ^-> T<Promise<_>>[T<string>]             
            "remove" => !?FileSystemHandleRemoveOptions?options ^-> T<Promise<_>>[T<unit>]
            "requestPermission" => !?PermissionDescriptor?descriptor ^-> T<Promise<_>>[T<string>] 
        ]

    let CreateSyncAccessHandleOptions = 
        Pattern.Config "CreateSyncAccessHandleOptions" {
            Required = []
            Optional = [
                "mode", T<string>
            ]
        }

    let CreateWritableOptions = 
        Pattern.Config "CreateWritableOptions" {
            Required = []
            Optional = [
                "keepExistingData", T<bool>
                "mode", T<string>
            ]
        }

    let TypedArray = T<Int8Array> + T<Uint8Array> + T<Uint8ClampedArray> + T<Int16Array> + T<Uint16Array>
                        + T<Int32Array> + T<Uint32Array> + T<Float64Array> + T<Float32Array>

    let WriteData = T<ArrayBuffer> + TypedArray + T<DataView> + T<Blob> + T<string>

    let WritableFileStreamData = 
        Pattern.Config "WritableFileStreamData" {
            Required = []
            Optional = [
                "type", T<string>
                "data", WriteData
                "position", T<int>
                "size", T<int>
            ]
        }

    let FileSystemWritableFileStream =
        Class "FileSystemWritableFileStream"
        |=> Inherits T<WritableStream> // Inherits from WritableStream
        |+> Instance [
            "write" => (WriteData + WritableFileStreamData)?data ^-> T<Promise<_>>[T<unit>]             
            "seek" => T<int>?position ^-> T<Promise<_>>[T<unit>]             
            "truncate" => T<int>?size ^-> T<Promise<_>>[T<unit>]
        ]

    let FileSystemSyncAccessHandle =

        let Buffer = T<ArrayBuffer> + T<ArrayBufferView>

        Class "FileSystemSyncAccessHandle"
        |+> Instance [
            "close" => T<unit> ^-> T<unit>             
            "flush" => T<unit> ^-> T<unit>             
            "getSize" => T<unit> ^-> T<int>             
            "read" => Buffer?buffer * !?T<int>?offset ^-> T<int>             
            "truncate" => T<int>?newSize ^-> T<unit>             
            "write" => Buffer?buffer * !?T<int>?offset ^-> T<int> 
        ]

    let FileSystemFileHandle =
        Class "FileSystemFileHandle"
        |=> Inherits FileSystemHandle // Inherits from FileSystemHandle
        |+> Instance [
            "getFile" => T<unit> ^-> T<Promise<_>>[T<File>]  
            "createSyncAccessHandle" => !?CreateSyncAccessHandleOptions?options ^-> T<Promise<_>>[FileSystemSyncAccessHandle] 
            "createWritable" => !?CreateWritableOptions?options ^-> T<Promise<_>>[FileSystemWritableFileStream] 
        ]

    let GetHandleOptions = 
        Pattern.Config "GetHandleOptions" {
            Required = []
            Optional = [
                "create", T<bool>
            ]
        }

    let FileSystemDirectoryHandle =
        Class "FileSystemDirectoryHandle"
        |=> Inherits FileSystemHandle // Inherits from FileSystemHandle
        |+> Instance [
            "getDirectoryHandle" => T<string>?name * !?GetHandleOptions?options^-> T<Promise<_>>[TSelf] 
            "getFileHandle" => T<string>?name * !?GetHandleOptions?options ^-> T<Promise<_>>[FileSystemFileHandle] 
            "removeEntry" => T<string>?name * !?FileSystemHandleRemoveOptions?options ^-> T<Promise<_>>[T<unit>] 
            "resolve" => FileSystemHandle?possibleDescendant ^-> T<Promise<_>>[!|T<string>] 
            "entries" => T<unit> ^-> T<obj> // Async iterator of key-value pairs
            "keys" => T<unit> ^-> T<obj> // Async iterator of keys
            "values" => T<unit> ^-> T<obj> // Async iterator of values
        ]

    let FileSystemObserverOptions =
        Class "FileSystemObserverOptions"
        |=> Inherits FileSystemHandleRemoveOptions

    let FileSystemObserver =

        let callback = (!|FileSystemChangeRecord)?records * TSelf?observer ^-> T<unit>

        Class "FileSystemObserver"
        |+> Static [
            Constructor (callback?callback)
        ]
        |+> Instance [
            "disconnect" => T<unit> ^-> T<unit>
            "sconnect" => T<obj>?handle * !?FileSystemObserverOptions?options ^-> T<unit>
        ]

    let StorageManager = 
        Class "StorageManager"
        |+> Instance [
            "getDirectory" => T<unit> ^-> T<Promise<_>>[FileSystemDirectoryHandle]
        ]

    let ShowDirectoryPickerOptions = 
        Pattern.Config "ShowDirectoryPickerOptions" {
            Required = []
            Optional = [
                "id", T<string>
                "mode", T<string>
                "startIn", T<string> + FileSystemHandle
            ]
        }

    let FilePickerAcceptType =
        Pattern.Config "FilePickerAcceptType" {
            Required = [ "accept", T<obj> ] // MIME types with corresponding file extensions
            Optional = [ "description", T<string> ] // Description of file type category
        }

    let FilePickerOptions =
        Pattern.Config "FilePickerOptions" {
            Required = []
            Optional = [
                "excludeAcceptAllOption", T<bool>
                "id", T<string>
                "multiple", T<bool>
                "startIn", T<string> + FileSystemHandle // Can be FileSystemHandle or a well-known directory string
                "types", !|FilePickerAcceptType
            ]
        }

    let SaveFilePickerOptions =
        Pattern.Config "SaveFilePickerOptions" {
            Required = []
            Optional = [
                "excludeAcceptAllOption", T<bool>
                "id", T<string>
                "startIn", T<string> + FileSystemHandle // Can be FileSystemHandle or a well-known directory string
                "suggestedName", T<string>
                "types", !|FilePickerAcceptType
            ]
        }

    let Assembly =
        Assembly [
            Namespace "WebSharper.FileSystem" [
                SaveFilePickerOptions
                FilePickerOptions
                FilePickerAcceptType
                ShowDirectoryPickerOptions
                StorageManager
                FileSystemObserver
                FileSystemObserverOptions
                FileSystemDirectoryHandle
                GetHandleOptions
                FileSystemFileHandle
                FileSystemSyncAccessHandle
                FileSystemWritableFileStream
                WritableFileStreamData
                CreateWritableOptions
                CreateSyncAccessHandleOptions
                FileSystemHandle
                FileSystemHandleRemoveOptions
                PermissionDescriptor
                FileSystemChangeRecord
                Enum.QueryPermissionMode
                Enum.FileSystemChangeRecordType
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
