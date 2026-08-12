namespace NandVm.Core

/// CPU (ALU + registers + program counter) of the HACK computer.
///
/// State is immutable: each `step` produces a new `CpuState`, which makes the
/// core trivially testable and safe to drive from a UI timer or a headless loop.
type CpuState =
    { Pc: Address
      A: Value
      D: Value
      Debug: bool }

[<RequireQualifiedAccess>]
module Cpu =

    let private zeroOp (_d: Value) (_s: Value) : Value = 0s
    let private oneOp (_d: Value) (_s: Value) : Value = 1s
    let private minusOneOp (_d: Value) (_s: Value) : Value = -1s
    let private dOp (d: Value) (_s: Value) : Value = d
    let private srcOp (_d: Value) (s: Value) : Value = s
    let private notDOp (d: Value) (_s: Value) : Value = ~~~d
    let private notSrcOp (_d: Value) (s: Value) : Value = ~~~s
    let private minusDOp (d: Value) (_s: Value) : Value = 0s - d
    let private minusSrcOp (_d: Value) (s: Value) : Value = 0s - s
    let private incDOp (d: Value) (_s: Value) : Value = d + 1s
    let private incSrcOp (_d: Value) (s: Value) : Value = s + 1s
    let private decDOp (d: Value) (_s: Value) : Value = d - 1s
    let private decSrcOp (_d: Value) (s: Value) : Value = s - 1s
    let private dPlusSrcOp (d: Value) (s: Value) : Value = d + s
    let private dMinusSrcOp (d: Value) (s: Value) : Value = d - s
    let private srcMinusDOp (d: Value) (s: Value) : Value = s - d
    let private dAndSrcOp (d: Value) (s: Value) : Value = d &&& s
    let private dOrSrcOp (d: Value) (s: Value) : Value = d ||| s
    let private haltOp (_d: Value) (_s: Value) : Value = 0s

    /// Maps a comp code to its ALU function. Unknown codes are a decode error and
    /// raise rather than silently producing a wrong result.
    let private compFunction (op: OpCode) : Value -> Value -> Value =
        match op with
        | OpCode.Zero -> zeroOp
        | OpCode.One -> oneOp
        | OpCode.MinusOne -> minusOneOp
        | OpCode.DReg -> dOp
        | OpCode.Src -> srcOp
        | OpCode.NotDReg -> notDOp
        | OpCode.NotSrc -> notSrcOp
        | OpCode.MinusDReg -> minusDOp
        | OpCode.MinusSrc -> minusSrcOp
        | OpCode.IncDReg -> incDOp
        | OpCode.IncSrc -> incSrcOp
        | OpCode.DecDReg -> decDOp
        | OpCode.DecSrc -> decSrcOp
        | OpCode.DPlusSrc -> dPlusSrcOp
        | OpCode.DMinusSrc -> dMinusSrcOp
        | OpCode.SrcMinusD -> srcMinusDOp
        | OpCode.DAndSrc -> dAndSrcOp
        | OpCode.DOrSrc -> dOrSrcOp
        | OpCode.Halt -> haltOp
        | unknown -> failwithf "unknown comp code: %A" unknown

    /// Next program counter: jumps to A when the condition holds, otherwise advances.
    let private calcJump (jump: JumpCode) (result: Value) (pc: Address) (target: Address) : Address =
        match jump with
        | JumpCode.Gt when result > 0s -> target
        | JumpCode.Eq when result = 0s -> target
        | JumpCode.Ge when result >= 0s -> target
        | JumpCode.Lt when result < 0s -> target
        | JumpCode.Ne when result <> 0s -> target
        | JumpCode.Le when result <= 0s -> target
        | JumpCode.Now -> target
        | _ -> pc + 1us

    let private dump (cpu: CpuState) (mem: Memory) : unit =
        printfn $"pc: {cpu.Pc}; d: {cpu.D}; a: {cpu.A}; mem[a]: {mem.Read(cpu.A |> uint16)}"

    /// Executes a single instruction against the given memory and returns the new
    /// CPU state. Returns `None` when the emulation should stop: the ROM is
    /// exhausted or a `Halt` instruction is executed.
    let step (mem: Memory) (cpu: CpuState) : CpuState option =
        if int cpu.Pc >= mem.RomLength then
            None
        else
            let word = mem.GetInstruction cpu.Pc

            if cpu.Debug then
                printfn $"{cpu.Pc}: {word} (d={cpu.D}, a={cpu.A})"

            match Decode.decode word with
            | Decoded.Address value -> Some { cpu with Pc = cpu.Pc + 1us; A = value }
            | Decoded.Computation op ->
                if op.Comp = OpCode.Halt then
                    None
                else
                    let source =
                        match op.Source with
                        | OperandSource.Memory -> mem.Read(cpu.A |> uint16)
                        | OperandSource.ARegister -> cpu.A

                    if cpu.Debug then
                        dump cpu mem
                        printfn $"{op}"

                    let result = compFunction op.Comp cpu.D source

                    if op.Dest.ToMemory then
                        mem.Write(cpu.A |> uint16, result)

                    let pc = calcJump op.Jump result cpu.Pc (cpu.A |> uint16)

                    Some
                        { cpu with
                            Pc = pc
                            A = if op.Dest.ToARegister then result else cpu.A
                            D = if op.Dest.ToDRegister then result else cpu.D }

    /// Fresh CPU state, PC reset to the start of the ROM.
    let create (debug: bool) : CpuState =
        { Pc = 0us
          A = 0s
          D = 0s
          Debug = debug }
