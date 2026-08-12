module NandVm.Ui.Program

open Avalonia
open NandVm.Core

let private usage (exitCode: int) =
    printfn "usage:"
    printfn "$ nandvm [/h] [/d] [/t <value>] [file_name]"
    printfn "/h - show this message and exit"
    printfn "/d - enable DEBUG mode, optional, default false"
    printfn "/t <value> - set CPU tick duration to <value> ms, where value is integer, optional, default 10"
    printfn "file_name - path to a program in HACK binary format. If empty, the built-in"
    printfn "            sample (2 + 3) is executed. Only one file is executed."
    exit exitCode

[<EntryPoint>]
let main argv =
    match Cli.parseArgs (fun () -> usage 0) (argv |> List.ofArray) with
    | Error message ->
        eprintfn "error: %s" message
        eprintfn ""
        usage 1
    | Ok opts ->
        match opts.FileName with
        | Some file -> RomLoader.loadFile file
        | None -> Ok RomLoader.sampleRom
        |> function
            | Error message ->
                eprintfn "error: %s" message
                1
            | Ok rom ->
                Launch.set rom opts.Debug opts.Tick

                AppBuilder
                    .Configure<App>()
                    .UsePlatformDetect()
                    .StartWithClassicDesktopLifetime(argv)
