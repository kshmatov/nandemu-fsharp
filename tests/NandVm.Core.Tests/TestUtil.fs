namespace NandVm.Core.Tests

open NandVm.Core
open Xunit

[<AutoOpen>]
module TestUtil =

    /// Builds an A-instruction from a 15-bit value.
    let a (v: int) : uint16 = uint16 v

    /// Builds a C-instruction from its fields.
    let c (useMem: bool) (comp: OpCode) (dest: int) (jump: JumpCode) : uint16 =
        0xE000us
        ||| (if useMem then 0x1000us else 0us)
        ||| (uint16 (int comp) <<< 6)
        ||| (uint16 (dest <<< 3))
        ||| uint16 (int jump)

    /// The built-in demo: @2; D=A; @3; D=D+A; @0; M=D; halt
    let sampleRom : uint16 array =
        [| a 2
           c false OpCode.Src 0b010 JumpCode.None
           a 3
           c false OpCode.DPlusSrc 0b010 JumpCode.None
           a 0
           c true OpCode.DReg 0b001 JumpCode.None
           c false OpCode.Halt 0b000 JumpCode.None |]

    /// Runs a ROM to completion (halt or end of ROM) and returns the final CPU state.
    let runRom (rom: uint16 array) : CpuState =
        let mem = Memory.Create(rom, None)

        let rec loop cpu =
            match Cpu.step mem cpu with
            | Some next -> loop next
            | None -> cpu

        loop (Cpu.create false)

    /// Steps the machine up to `maxSteps` times, stopping early on halt.
    /// Returns the final CPU state (useful for programs that never halt, like rect).
    let runSteps (maxSteps: int) (rom: uint16 array) : CpuState * Memory =
        let mem = Memory.Create(rom, None)

        let rec loop n cpu =
            if n <= 0 then
                cpu
            else
                match Cpu.step mem cpu with
                | Some next -> loop (n - 1) next
                | None -> cpu

        loop maxSteps (Cpu.create false), mem
