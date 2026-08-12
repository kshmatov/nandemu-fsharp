namespace NandVm.Ui

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open NandVm.Core

/// Launch parameters handed from `Program.main` to `App`. Kept as module-level
/// state so `App` can keep a parameterless constructor: creating the `Application`
/// instance before `AppBuilder.Configure` runs breaks the dispatcher on some
/// platforms (the UI thread dispatcher is created before the platform registers
/// its threading implementation).
[<RequireQualifiedAccess>]
module Launch =
    let mutable private rom : Instruction array = Array.empty
    let mutable private debug = false
    let mutable private tick = 10

    let set (loadedRom: Instruction array) (debugMode: bool) (tickMs: int) =
        rom <- loadedRom
        debug <- debugMode
        tick <- tickMs

    let get () : Instruction array * bool * int = rom, debug, tick

type App() =
    inherit Application()

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as lifetime ->
            let rom, debug, tick = Launch.get ()
            lifetime.MainWindow <- MainWindow(rom, debug, tick)
        | _ -> ()

        base.OnFrameworkInitializationCompleted()
