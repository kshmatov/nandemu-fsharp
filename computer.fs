module computer

open System
open System.Windows.Forms

type NandKeys = 
    | Char of int
    | Esc
    | Space
    | Func of int
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
    | Ignored

let keyToCode = function
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
    | NandKeys.Char x ->  x |> int16
    | _ -> 0s

let keyDown (ev: Keys): NandKeys =
    match ev with
        | x when x = Keys.Back -> NandKeys.BSpace
        | x when x = Keys.Escape -> NandKeys.Esc
        | x when x >= Keys.F1 && x <= Keys.F12 -> int(ev) + 1 - int(Keys.F1) |> Func
        | x when x = Keys.Delete -> NandKeys.Del
        | x when x = Keys.Insert -> NandKeys.Ins
        | x when x = Keys.Home -> NandKeys.Home
        | x when x = Keys.End -> NandKeys.End
        | x when x = Keys.PageUp -> NandKeys.PgUp
        | x when x = Keys.PageDown -> NandKeys.PgDown
        | x when x = Keys.Up -> NandKeys.Up
        | x when x = Keys.Down -> NandKeys.Down
        | x when x = Keys.Left -> NandKeys.Left
        | x when x = Keys. Right -> NandKeys.Right
        | x when x = Keys.Enter -> NandKeys.Enter
        | _ -> NandKeys.Ignored

let show  (p: PictureBox) (mem: memory.Value array):unit =
    let bmp =  p.Image :?> Drawing.Bitmap
    for row = 0 to 255 do
        let pos = 32 * row
        for col = 0 to 31 do
            if mem.Length <= pos+col then
                printfn $"pos {pos+col}"
            let b = mem[pos+col]
            for col1 = 0 to 15 do
                if (b <<< col1) &&& 0b1000000000000000s = 0s then
                    bmp.SetPixel(col*16+col1, row, Drawing.Color.Black)
                else
                    bmp.SetPixel(col*16+col1, row, Drawing.Color.White)

    let g = p.CreateGraphics()
    g.DrawImage(bmp, 0, 0 ,512, 256)

let RunComputer (debug: bool) (tick: int) (rom: uint16 array) = 
    let mutable regs = alu.InitAlu debug
    let mem = memory.Init
    mem.Load rom

    let display = new PictureBox()
    let form = new Form(Text="Nand to Tetris VM",
                        Visible = true,
                        TopMost = true)
    form.FormBorderStyle <- FormBorderStyle.FixedSingle
    form.ClientSize <- Drawing.Size(Drawing.Point(512,256))
    form.MaximizeBox <- false
    form.Controls.Add display

    display.BackColor <- Drawing.Color.Gray
    display.Size  <- Drawing.Size(Drawing.Point(512,256))
    display.Top <- 2
    display.Left <- 2
    display.Enabled <- false
    form.AutoSize <- true

    let bmp = new Drawing.Bitmap(512, 256)
    display.Image <- bmp

    form.Click.Add(fun evArgs -> System.Console.Beep())
    form.KeyDown.Add( fun ev -> 
        let x = keyDown ev.KeyCode 
        if x <> NandKeys.Ignored then
            x |> keyToCode |> mem.SetKey
            ev.Handled <- true
    )
    form.KeyPress.Add( fun ev -> 
        ev.KeyChar |> int16 |> mem.SetKey
    )
    form.KeyUp.Add(fun ev -> mem.SetKey 0s)

    let timer = new Timer()
    timer.Interval <- tick
    timer.Tick.Add(fun x  ->
        match alu.oneStep mem regs with
        | Some r -> 
            regs <-r
            mem.GetVideo |> show display
        | None -> 
            timer.Enabled <- false
            printfn "done"
            // form.Close()
    )
    form.Shown.Add(fun x -> timer.Enabled <- true)
    Application.Run(form)
