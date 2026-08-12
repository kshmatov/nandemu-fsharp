namespace NandVm.Core

open System
open System.Globalization

/// Loading and parsing of HACK binary programs.
[<RequireQualifiedAccess>]
module RomLoader =

    /// The built-in demo program: @2; D=A; @3; D=D+A; @0; M=D; halt
    let sampleRom : Instruction array =
        [| 0b0000000000000010us
           0b1110110000010000us
           0b0000000000000011us
           0b1110000010010000us
           0b0000000000000000us
           0b1110001100001000us
           0b1111011110111111us |]

    /// Strips an optional 0b/0x prefix and returns the matching NumberStyles.
    let private normalize (line: string) : (string * NumberStyles) option =
        if line.StartsWith "0x" || line.StartsWith "0X" then
            Some(line.Substring 2, NumberStyles.HexNumber)
        elif line.StartsWith "0b" || line.StartsWith "0B" then
            Some(line.Substring 2, NumberStyles.BinaryNumber)
        else
            Some(line, NumberStyles.BinaryNumber)

    /// Reads a HACK binary program file. Returns an error description on any problem.
    let loadFile (path: string) : Result<Instruction array, string> =
        try
            let rec parse (lines: string list) (acc: Instruction list) : Result<Instruction list, string> =
                match lines with
                | [] -> Ok(List.rev acc)
                | raw :: rest ->
                    let line = raw.Trim()

                    if line.Length = 0 then
                        parse rest acc
                    else
                        match normalize line with
                        | Some(digits, style) ->
                            match UInt16.TryParse(digits, style, CultureInfo.InvariantCulture) with
                            | true, value -> parse rest (value :: acc)
                            | false, _ -> Error $"cannot parse line as a 16-bit instruction: '{raw}'"
                        | None -> parse rest acc

            IO.File.ReadAllLines path |> Array.toList |> fun lines -> parse lines []
            |> Result.map Array.ofList
        with
        | :? IO.FileNotFoundException -> Error $"file not found: {path}"
        | :? IO.DirectoryNotFoundException -> Error $"directory not found: {path}"
        | :? IO.IOException as ex -> Error $"cannot read '{path}': {ex.Message}"
        | :? UnauthorizedAccessException -> Error $"access denied while reading '{path}'"
