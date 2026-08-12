namespace NandVm.Core.Tests

open NandVm.Core
open Xunit

module AluTests =

    /// Runs a single C-instruction that stores the ALU result to M[aReg].
    /// `aReg` is both the A register value and the address of the destination word
    /// (and the source operand when `useMem` is false). `memVal` is the word stored
    /// at that address (the source operand when `useMem` is true).
    let private compute (useMem: bool) (comp: OpCode) (d: Value) (aReg: Value) (memVal: Value) : Value =
        let rom = [| TestUtil.c useMem comp 0b001 JumpCode.None |]
        let mem = Memory.Create(rom, None)
        mem.Write(aReg |> uint16, memVal)
        let cpu = { Pc = 0us; A = aReg; D = d; Debug = false }
        Cpu.step mem cpu |> ignore
        mem.Read(aReg |> uint16)

    let private check (useMem: bool) (comp: OpCode) (d: Value) (s: Value) (expected: Value) =
        // A register equals the source value `s`, so it doubles as the store address.
        let aReg = if useMem then 3s else s
        Assert.Equal(expected, compute useMem comp d aReg s)

    [<Fact>]
    let ``all comp ops with A source`` () =
        let d = 5s
        let s = 3s // the A register value

        let cases: (OpCode * Value) list =
            [ OpCode.Zero, 0s
              OpCode.One, 1s
              OpCode.MinusOne, -1s
              OpCode.DReg, d
              OpCode.Src, s
              OpCode.NotDReg, ~~~d
              OpCode.NotSrc, ~~~s
              OpCode.MinusDReg, 0s - d
              OpCode.MinusSrc, 0s - s
              OpCode.IncDReg, d + 1s
              OpCode.IncSrc, s + 1s
              OpCode.DecDReg, d - 1s
              OpCode.DecSrc, s - 1s
              OpCode.DPlusSrc, d + s
              OpCode.DMinusSrc, d - s
              OpCode.SrcMinusD, s - d
              OpCode.DAndSrc, d &&& s
              OpCode.DOrSrc, d ||| s ]

        for comp, expected in cases do
            check false comp d s expected

    [<Fact>]
    let ``all comp ops with M source`` () =
        let d = 5s
        let s = 7s // the value at M[3]

        let cases: (OpCode * Value) list =
            [ OpCode.Zero, 0s
              OpCode.One, 1s
              OpCode.MinusOne, -1s
              OpCode.DReg, d
              OpCode.Src, s
              OpCode.NotDReg, ~~~d
              OpCode.NotSrc, ~~~s
              OpCode.MinusDReg, 0s - d
              OpCode.MinusSrc, 0s - s
              OpCode.IncDReg, d + 1s
              OpCode.IncSrc, s + 1s
              OpCode.DecDReg, d - 1s
              OpCode.DecSrc, s - 1s
              OpCode.DPlusSrc, d + s
              OpCode.DMinusSrc, d - s
              OpCode.SrcMinusD, s - d
              OpCode.DAndSrc, d &&& s
              OpCode.DOrSrc, d ||| s ]

        for comp, expected in cases do
            check true comp d s expected

    [<Fact>]
    let ``addition wraps on overflow`` () =
        check false OpCode.DPlusSrc 32767s 1s -32768s

    [<Fact>]
    let ``subtraction wraps on underflow`` () =
        check false OpCode.DMinusSrc -32768s 1s 32767s

    [<Fact>]
    let ``unary minus wraps at int16 minimum`` () =
        check false OpCode.MinusDReg -32768s 0s -32768s

    [<Fact>]
    let ``minusDReg is zero minus D (regression for the old s-d bug)`` () =
        // The old implementation computed src - D for MinusDReg, i.e. 3 - 5 = -2.
        check false OpCode.MinusDReg 5s 3s -5s

    [<Fact>]
    let ``minusSrc is zero minus source (regression for the old d-s bug)`` () =
        // The old implementation computed D - src for MinusSrc, i.e. 5 - 3 = 2.
        check false OpCode.MinusSrc 5s 3s -3s

    [<Fact>]
    let ``unknown comp code raises`` () =
        // 0b000001 is not a named comp code.
        let rom = [| TestUtil.c false (enum<OpCode> 0b000001) 0b001 JumpCode.None |]
        let mem = Memory.Create(rom, None)
        let cpu = { Pc = 0us; A = 0s; D = 0s; Debug = false }
        Assert.Throws<System.Exception>(fun () -> Cpu.step mem cpu |> ignore) |> ignore
