namespace NandVm.Core.Tests

open NandVm.Core
open Xunit

module IntegrationTests =

    let private loadRom (fileName: string) : Instruction array =
        let path = System.IO.Path.Combine("TestData", fileName)

        System.IO.File.ReadAllLines path
        |> Array.choose (fun line ->
            let l = line.Trim()
            if l.Length = 0 then None else Some (uint16 ("0b" + l)))

    [<Fact>]
    let ``sample program computes 2 + 3 and stores it`` () =
        let mem = Memory.Create(TestUtil.sampleRom, None)

        let rec loop cpu =
            match Cpu.step mem cpu with
            | Some next -> loop next
            | None -> cpu

        let cpu = loop (Cpu.create false)
        Assert.Equal(5s, mem.Read 0us)
        Assert.Equal(5s, cpu.D)
        Assert.Equal(0s, cpu.A) // left over from @0 before M=D

    [<Fact>]
    let ``emulator.run returns zero exit code`` () =
        let mem = Memory.Create(TestUtil.sampleRom, None)
        Assert.Equal(0, Emulator.run Emulator.defaultConfig mem (Cpu.create false))

    [<Fact>]
    let ``rect program writes to the screen`` () =
        let rom = loadRom "rect.hack"
        let screenWrites = ResizeArray()
        let updater (addr: uint16) _ = screenWrites.Add addr
        let mem = Memory.Create(rom, Some updater)

        let rec loop n cpu =
            if n = 0 then
                cpu
            else
                match Cpu.step mem cpu with
                | Some next -> loop (n - 1) next
                | None -> cpu

        loop 1000 (Cpu.create false) |> ignore
        Assert.NotEmpty screenWrites

    [<Fact>]
    let ``rect program has no halt and keeps executing`` () =
        // rect ends with `0;JMP`, an infinite loop: the PC keeps cycling the program.
        let cpu, _ = TestUtil.runSteps 5000 (loadRom "rect.hack")
        Assert.True(int cpu.Pc < 25)
