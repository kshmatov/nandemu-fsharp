module alu

open memory

let firstBit = 0b1000000000000000us
let private jumpMask = 0b0000000000000111us
let private destMask = 0b0000000000111000us
let private compMask = 0b0000111111000000us
let private srcMask = 0b0001000000000000us

let private dstMemMask = 0b1uy
let private dstDRegisterMask = 0b10uy
let private dstARegisterMask = 0b100uy

type OpCode = 
    | zero = 0b101010
    | one = 0b111111
    | minusOne = 0b111010
    | dReg = 0b001100
    | src = 0b110000
    | notDReg = 0b001101
    | notSrc = 0b110001
    | minusDReg = 0b001111
    | minusSrc = 0b110011
    | incDReg = 0b011111
    | incSrcReg = 0b110111
    | decDReg = 0b001110
    | decSrcReg = 0b110010
    | dPlusSrc = 0b000010
    | dMinusSrc = 0b010011
    | srcMinusD = 0b000111
    | dAndSrc = 0b000000
    | dOrSrc = 0b010101
    | halt = 0b011110

type JumpCode = 
    | None = 0b000
    | Gt = 0b001
    | Eq = 0b010
    | Ge = 0b011
    | Lt = 0b100
    | Ne = 0b101
    | Le = 0b110
    | Now = 0b111

let zeroOp (d: Value) (s:Value): Value = 0s
let oneOp (d: Value) (s:Value): Value = 1s
let minusOneOp (d: Value) (s:Value): Value  = -1s
let dRegisterOp (d: Value) (s:Value): Value  = d
let srcOp (d: Value) (s:Value): Value  = s
let notDOp (d: Value) (s:Value): Value  = ~~~ d
let notSrcOp (d: Value) (s:Value): Value  = ~~~ s
let IncDOp (d: Value) (s:Value): Value  = d + 1s
let IncSrcOp (d: Value) (s:Value): Value  = s + 1s
let decDOp (d: Value) (s:Value): Value  = d - 1s
let decSrcOp (d: Value) (s:Value): Value  = s - 1s
let dPlusSrcOp (d: Value) (s:Value): Value  = d + s
let dMinusSrcOp (d: Value) (s:Value): Value  = d - s
let srcMinusDOp (d: Value) (s:Value): Value  = s - d
let dAndSrcOp (d: Value) (s:Value): Value   = d &&& s
let dOrSrcOp (d: Value) (s:Value): Value = d ||| s
let haltOp (d: Value) (s:Value): Value = 0s

type alu = {
    mutable ProgramCounter: Address
    mutable ARegister: Value
    mutable DRegister: Value
    Debug: bool
}

type OperandSource = 
    | Memory
    | ARegister

type Destination = {
    toMem:bool
    toDRegister:bool
    toARegister:bool
}

type OpDesc = {
    src: OperandSource
    comp: OpCode
    dest: Destination
    jump: JumpCode
}

type VMOp = 
    | AddressOp of Value
    | ComputationOp of OpDesc


let isMask what mask =
    what &&& mask = mask

let parseDest dst : Destination = 
    {
        toMem  = isMask dst dstMemMask
        toDRegister = isMask dst dstDRegisterMask
        toARegister = isMask dst dstARegisterMask
    }

let getOp (op:OpCode) =
    match op with
    | OpCode.zero -> zeroOp
    | OpCode.one -> oneOp
    | OpCode.minusOne -> minusOneOp
    | OpCode.dReg -> dRegisterOp
    | OpCode.src -> srcOp
    | OpCode.notDReg -> notDOp
    | OpCode.notSrc -> notSrcOp
    | OpCode.minusDReg -> srcMinusDOp
    | OpCode.minusSrc -> dMinusSrcOp
    | OpCode.incDReg -> IncDOp
    | OpCode.incSrcReg -> IncSrcOp
    | OpCode.decDReg -> decDOp
    | OpCode.decSrcReg -> decSrcOp
    | OpCode.dPlusSrc -> dPlusSrcOp
    | OpCode.dMinusSrc -> dMinusSrcOp
    | OpCode.srcMinusD -> srcMinusDOp
    | OpCode.dAndSrc -> dAndSrcOp
    | OpCode.dOrSrc -> dOrSrcOp
    | _ -> haltOp

let calcJump (jmp:JumpCode) (v: Value) (cp: Address) (alt:Address) :Address= 
    match jmp with
    | JumpCode.Gt when v > 0s -> alt
    | JumpCode.Eq when v = 0s -> alt
    | JumpCode.Ge when v >= 0s-> alt
    | JumpCode.Lt when v < 0s -> alt
    | JumpCode.Ne when v <> 0s -> alt
    | JumpCode.Le when v <= 0s -> alt
    | JumpCode.Now -> alt
    | _ -> cp + 1us

let parseInstruction (op: Instruction): VMOp = 
    if op &&& firstBit = 0us then
        AddressOp (op |> int16)
    else
        ComputationOp {
            jump = jumpMask &&& op |> int32 |> enum<JumpCode>;
            dest = destMask &&& op >>> 3 |> byte |> parseDest;
            comp = compMask &&& op >>> 6 |> int32 |> enum<OpCode>;
            src = if op &&& srcMask = 0us then OperandSource.ARegister else OperandSource.Memory
        }

let makeCOp (op: OpDesc) (regs: alu) (mem: State): alu =
    if regs.Debug then
        printfn $"pc: {regs.ProgramCounter}; d: {regs.DRegister}; a: {regs.ARegister}; mem[a]:{mem.GetMem(regs.ARegister |> uint16)}"
        printfn $"{op}"
    let v = 
        if op.src = OperandSource.Memory then 
            regs.ARegister |> uint16 |> mem.GetMem
        else 
            regs.ARegister
    let res = getOp op.comp regs.DRegister v
    if op.dest.toMem then mem.SetMem (regs.ARegister |> uint16) res
    regs.ProgramCounter <- calcJump op.jump res regs.ProgramCounter (regs.ARegister |> uint16)
    regs.ARegister <- if op.dest.toARegister then res else regs.ARegister
    regs.DRegister <- if op.dest.toDRegister then res else regs.DRegister
    regs

let makeAOp(regs: alu) (a: Value): alu =
    if regs.Debug then
        printfn $"pc: {regs.ProgramCounter}; d: {regs.DRegister}; a: {regs.ARegister}"
        printfn $"AddressOp old: {regs.ARegister} new: {a}"
    regs.ProgramCounter <- regs.ProgramCounter + 1us
    regs.ARegister <- a
    regs

let oneStep (mem: State) (regs: alu):alu option= 
    let op = regs.ProgramCounter |> mem.GetInstaruction |> parseInstruction
    match op with
    | _ when regs.ProgramCounter > 0x6000us -> None
    | VMOp.ComputationOp x when x.comp = OpCode.halt -> None
    | VMOp.AddressOp a -> makeAOp regs a |> Some
    | VMOp.ComputationOp op -> makeCOp op regs mem |> Some


let InitAlu(debug:bool): alu = 
    {ProgramCounter = 0us; ARegister = 0s; DRegister = 0s; Debug = debug}

let RunAluSettings (rom: memory.Instruction array) (tick: int) (debug: bool): int = 
    let regs = InitAlu debug
    let mem = memory.Init
    rom |> mem.Load

    let rec step (mem: State) (regs: alu):int = 
        System.Threading.Thread.Sleep tick
        match oneStep mem regs with
        | None -> 0
        | Some regs -> step mem regs

    step mem regs

let RunAlu (rom: memory.Instruction array) =
    RunAluSettings rom 1 false
