namespace NandVm.Core

open System

/// Command-line options shared by all front-ends (CLI and desktop).
[<RequireQualifiedAccess>]
module Cli =

    type Options =
        { Debug: bool
          Tick: int
          FileName: string option }

    let defaultOptions : Options = { Debug = false; Tick = 10; FileName = None }

    /// Parses /h, /d, /t and a single program file. `/h` never returns normally:
    /// it invokes `onHelp` (which typically exits). Unknown bare "/word" tokens
    /// (no path separator) are rejected, while path-like tokens (e.g. `/tmp/x.hack`,
    /// `./x.hack`) are treated as files. Use `--` to pass a file that starts with a
    /// slash unambiguously.
    let parseArgs (onHelp: unit -> Result<Options, string>) (args: string list) : Result<Options, string> =
        let rec go (args: string list) (opts: Options) : Result<Options, string> =
            match args with
            | [] -> Ok opts
            | "/h" :: _ -> onHelp ()
            | "/d" :: rest -> go rest { opts with Debug = true }
            | "/t" :: value :: rest ->
                match Int32.TryParse value with
                | true, v when v >= 0 -> go rest { opts with Tick = v }
                | _ -> Error $"invalid tick value: '{value}'"
            | "/t" :: [] -> Error "missing value for /t"
            | "--" :: files ->
                match files, opts.FileName with
                | [ file ], None -> Ok { opts with FileName = Some file }
                | _ -> Error "exactly one program file is expected after '--'"
            | flag :: _ when flag.StartsWith "/" && not (flag.Substring(1).Contains('/')) ->
                Error $"unknown option: '{flag}'"
            | file :: rest ->
                match opts.FileName with
                | Some previous -> Error $"only one program file is supported, got both '{previous}' and '{file}'"
                | None -> go rest { opts with FileName = Some file }

        go args defaultOptions
