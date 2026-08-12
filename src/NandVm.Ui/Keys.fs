namespace NandVm.Ui

open Avalonia.Input
open NandVm.Core

[<RequireQualifiedAccess>]
module KeyInput =

    /// Translates an Avalonia key to a semantic key. Regular characters are reported
    /// via `TextInput` instead and never reach here.
    let keyDown (ev: Key) : NandKeys =
        match ev with
        | x when x = Key.Back -> NandKeys.BSpace
        | x when x = Key.Escape || x = Key.CapsLock -> NandKeys.Esc
        | x when x >= Key.F1 && x <= Key.F12 -> int ev + 1 - int Key.F1 |> NandKeys.Func
        | x when x = Key.Delete -> NandKeys.Del
        | x when x = Key.Insert -> NandKeys.Ins
        | x when x = Key.Home -> NandKeys.Home
        | x when x = Key.End -> NandKeys.End
        | x when x = Key.PageUp -> NandKeys.PgUp
        | x when x = Key.PageDown -> NandKeys.PgDown
        | x when x = Key.Up -> NandKeys.Up
        | x when x = Key.Down -> NandKeys.Down
        | x when x = Key.Left -> NandKeys.Left
        | x when x = Key.Right -> NandKeys.Right
        | x when x = Key.Enter -> NandKeys.Enter
        | _ -> NandKeys.Ignored
