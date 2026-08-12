namespace NandVm.Core.Tests

open NandVm.Core
open Xunit

module MemoryTests =

    [<Fact>]
    let ``RAM read and write`` () =
        let mem = Memory.Create([||], None)
        mem.Write(0us, 42s)
        Assert.Equal(42s, mem.Read 0us)
        mem.Write(0x3FFFus, -1s)
        Assert.Equal(-1s, mem.Read 0x3FFFus)

    [<Fact>]
    let ``screen write notifies the updater with a screen-relative offset`` () =
        let mutable received = None
        let updater (addr: uint16) (value: Value) = received <- Some(addr, value)
        let mem = Memory.Create([||], Some updater)
        mem.Write(0x4000us, 123s)
        Assert.Equal(Some(0us, 123s), received)

    [<Fact>]
    let ``only screen addresses reach the updater`` () =
        let mutable count = 0
        let updater _ _ = count <- count + 1
        let mem = Memory.Create([||], Some updater)
        mem.Write(0x5FFFus, 1s)
        mem.Write(0x6000us, 1s) // keyboard
        mem.Write(0x0000us, 1s) // plain RAM
        Assert.Equal(1, count)

    [<Fact>]
    let ``keyboard word is written by SetKey and readable at 0x6000`` () =
        let mem = Memory.Create([||], None)
        Assert.Equal(0s, mem.Read 0x6000us)
        mem.SetKey 65s
        Assert.Equal(65s, mem.Read 0x6000us)
        Assert.Equal(65s, mem.Keyboard)

    [<Fact>]
    let ``keyboard is read-only for generic writes`` () =
        let mem = Memory.Create([||], None)
        mem.Write(0x6000us, 99s)
        Assert.Equal(0s, mem.Read 0x6000us)

    [<Fact>]
    let ``out of range reads return zero`` () =
        let mem = Memory.Create([||], None)
        Assert.Equal(0s, mem.Read 0xFFFFus)

    [<Fact>]
    let ``out of range writes are ignored`` () =
        let mem = Memory.Create([||], None)
        mem.Write(0xFFFFus, 1s)
        Assert.Equal(0s, mem.Read 0xFFFFus)

    [<Fact>]
    let ``ROM is fetched by the CPU`` () =
        let rom = [| TestUtil.a 7; TestUtil.c false OpCode.Halt 0 JumpCode.None |]
        let mem = Memory.Create(rom, None)
        Assert.Equal(TestUtil.a 7, mem.GetInstruction 0us)
