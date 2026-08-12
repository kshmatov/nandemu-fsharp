namespace NandVm.Core

/// 16-bit signed data word (two's complement), HACK ALU operands.
type Value = int16

/// 16-bit address. A-instructions can address 15-bit values (0..0x7FFF).
type Address = uint16

/// 16-bit instruction word stored in the ROM.
type Instruction = uint16

/// HACK data-memory layout. All addresses are within one contiguous word array,
/// mirroring the real HACK memory map.
[<RequireQualifiedAccess>]
module MemoryMap =

    /// RAM segment size in words: 0x0000..0x3FFF.
    [<Literal>]
    let RamSize = 0x4000

    /// Words mapped to the screen: 0x4000..0x5FFF (512x256, 16 pixels per word).
    [<Literal>]
    let ScreenWords = 0x2000

    /// First address of the screen segment.
    [<Literal>]
    let ScreenBegin = 0x4000

    /// One past the last screen address (== 0x6000).
    [<Literal>]
    let ScreenEnd = ScreenBegin + ScreenWords

    /// Address of the single keyboard word (read-only).
    [<Literal>]
    let KeyboardAddress = ScreenEnd

    /// Total size of the data memory (RAM + screen + keyboard).
    [<Literal>]
    let DataMemorySize = ScreenEnd + 1

/// Instruction decoding masks.
[<RequireQualifiedAccess>]
module Masks =

    /// The 0x8000 bit selects instruction type: 0 -> A-instruction, 1 -> C-instruction.
    [<Literal>]
    let InstructionBit = 0x8000us

    /// The 0x1000 bit of a C-instruction selects the source operand (0 -> A, 1 -> M).
    [<Literal>]
    let SourceBit = 0x1000us

/// HACK C-instruction comp codes. The enum value is the raw comp bits
/// (bits 11..6, the 'a' bit selects the source operand). `Halt` is a non-standard
/// extension used to stop the emulation loop.
type OpCode =
    | Zero = 0b101010
    | One = 0b111111
    | MinusOne = 0b111010
    | DReg = 0b001100
    | Src = 0b110000
    | NotDReg = 0b001101
    | NotSrc = 0b110001
    | MinusDReg = 0b001111
    | MinusSrc = 0b110011
    | IncDReg = 0b011111
    | IncSrc = 0b110111
    | DecDReg = 0b001110
    | DecSrc = 0b110010
    | DPlusSrc = 0b000010
    | DMinusSrc = 0b010011
    | SrcMinusD = 0b000111
    | DAndSrc = 0b000000
    | DOrSrc = 0b010101
    | Halt = 0b011110

/// HACK C-instruction jump codes (bits 2..0).
type JumpCode =
    | None = 0b000
    | Gt = 0b001
    | Eq = 0b010
    | Ge = 0b011
    | Lt = 0b100
    | Ne = 0b101
    | Le = 0b110
    | Now = 0b111
