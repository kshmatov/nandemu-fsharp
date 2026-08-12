namespace NandVm.Core

/// Semantic key codes used by the HACK keyboard device.
type NandKeys =
    | Char of int
    | Esc
    | Space
    | BSpace
    | Del
    | Ins
    | Home
    | End
    | PgUp
    | PgDown
    | Up
    | Down
    | Left
    | Right
    | Enter
    | Func of int
    | Ignored

[<RequireQualifiedAccess>]
module KeyMapper =

    /// Maps a semantic key to the scan code written to the keyboard device (0x6000).
    let keyToCode (key: NandKeys) : Value =
        match key with
        | NandKeys.Esc -> 140s
        | NandKeys.Func x -> 140 + x |> int16
        | NandKeys.BSpace -> 129s
        | NandKeys.Del -> 139s
        | NandKeys.Ins -> 138s
        | NandKeys.Home -> 134s
        | NandKeys.End -> 135s
        | NandKeys.PgUp -> 136s
        | NandKeys.PgDown -> 137s
        | NandKeys.Up -> 131s
        | NandKeys.Down -> 133s
        | NandKeys.Left -> 130s
        | NandKeys.Right -> 132s
        | NandKeys.Enter -> 128s
        | NandKeys.Space -> ' ' |> int |> int16
        | NandKeys.Char x -> x |> int16
        | NandKeys.Ignored -> 0s
