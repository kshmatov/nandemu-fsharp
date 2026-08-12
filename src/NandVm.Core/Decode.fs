namespace NandVm.Core

/// Source operand of a C-instruction comp operation.
type OperandSource =
    | ARegister
    | Memory

/// Destination bits of a C-instruction (which registers receive the ALU result).
type Destination =
    { ToMemory: bool
      ToDRegister: bool
      ToARegister: bool }

/// A fully decoded C-instruction.
type OpDesc =
    { Source: OperandSource
      Comp: OpCode
      Dest: Destination
      Jump: JumpCode }

/// Result of decoding a single 16-bit ROM word.
type Decoded =
    | Address of Value
    | Computation of OpDesc

/// Decodes 16-bit ROM words into A- and C-instructions.
[<RequireQualifiedAccess>]
module Decode =

    [<Literal>]
    let CompMask = 0x0FC0us

    [<Literal>]
    let DestMask = 0x0038us

    [<Literal>]
    let JumpMask = 0x0007us

    /// Decodes a 16-bit ROM word into an A-instruction (bit 15 clear) or a C-instruction.
    let decode (word: Instruction) : Decoded =
        if word &&& Masks.InstructionBit = 0us then
            Address(word |> int16)
        else
            let jump = int (JumpMask &&& word) |> enum<JumpCode>
            let comp = int ((CompMask &&& word) >>> 6) |> enum<OpCode>
            let dest = int ((DestMask &&& word) >>> 3)

            let source =
                if word &&& Masks.SourceBit = 0us then OperandSource.ARegister else OperandSource.Memory

            Computation
                { Source = source
                  Comp = comp
                  Dest =
                    { ToMemory = dest &&& 0b001 <> 0
                      ToDRegister = dest &&& 0b010 <> 0
                      ToARegister = dest &&& 0b100 <> 0 }
                  Jump = jump }
