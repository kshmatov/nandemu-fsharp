namespace NandVm.Ui

open Avalonia.Controls
open Avalonia.Threading
open System
open NandVm.Core

/// Wires the headless core to an Avalonia window.
type MainWindow(rom: Instruction array, debug: bool, tick: int) as self =
    inherit Window()

    let screen = new ScreenBuffer(512, 256)
    let display = new ScreenControl(512, 256)
    let memory = Memory.Create(rom, Some(fun index value -> screen.SetWord(index, value)))
    let timer = DispatcherTimer()
    let mutable cpu = Cpu.create debug

    /// Instructions stepped per timer tick at full speed (/t 0). `DispatcherTimer`
    /// fires roughly every 15ms, so a batch keeps the machine moving fast while the
    /// UI thread stays responsive.
    let stepsPerTick = if tick > 0 then 1 else 100000

    do
        self.Title <- "Nand to Tetris VM"
        self.SizeToContent <- SizeToContent.WidthAndHeight
        self.Content <- display

        self.KeyDown.Add(fun ev ->
            match KeyInput.keyDown ev.Key with
            | NandKeys.Ignored -> ()
            | key ->
                KeyMapper.keyToCode key |> memory.SetKey
                ev.Handled <- true)

        self.TextInput.Add(fun ev ->
            if not (String.IsNullOrEmpty ev.Text) then
                ev.Text[0] |> int |> int16 |> memory.SetKey)

        self.KeyUp.Add(fun _ -> memory.SetKey 0s)

        timer.Interval <- TimeSpan.FromMilliseconds(max 1 tick)

        timer.Tick.Add(fun _ ->
            let mutable stepped = 0
            let mutable halted = false

            while not halted && stepped < stepsPerTick do
                match Cpu.step memory cpu with
                | Some next -> cpu <- next
                | None -> halted <- true

                stepped <- stepped + 1

            display.Refresh screen

            if halted then
                timer.Stop()
                printfn "done")

        self.Opened.Add(fun _ ->
            screen.Clear()
            display.Refresh screen
            display.Focus() |> ignore
            timer.Start())

        self.Closing.Add(fun _ ->
            timer.Stop()
            (display :> IDisposable).Dispose())
