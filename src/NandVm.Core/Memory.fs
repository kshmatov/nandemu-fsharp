namespace NandVm.Core

/// Data memory of the HACK computer.
///
/// The three segments live in one contiguous word array, mirroring the real
/// HACK memory map:
///   - RAM     0x0000..0x3FFF  (read/write)
///   - Screen  0x4000..0x5FFF  (read/write, every write notifies the updater)
///   - Keyboard 0x6000         (read-only; written via `SetKey` by the input device)
///
/// Instructions live in a separate ROM, as in the real HACK architecture.
type Memory(rom: Instruction array, screenUpdater: (uint16 -> Value -> unit) option) =
    let data = Array.create MemoryMap.DataMemorySize 0s

    /// Size of the loaded program; the CPU stops when the PC reaches it.
    member _.RomLength = rom.Length

    member _.Read(address: Address) : Value =
        let a = int address
        if a >= 0 && a < data.Length then data[a] else 0s

    /// Writes a data-memory word. Screen writes forward the offset (0..ScreenWords-1)
    /// and the value to the registered updater. Keyboard and out-of-range writes are ignored.
    member _.Write(address: Address, value: Value) : unit =
        let a = int address

        if a >= 0 && a < data.Length then
            if a < MemoryMap.ScreenBegin then
                data[a] <- value
            elif a < MemoryMap.ScreenEnd then
                data[a] <- value

                match screenUpdater with
                | Some updater -> updater (address - uint16 MemoryMap.ScreenBegin) value
                | None -> ()

    /// Reads an instruction word from the ROM. The CPU must guarantee the address
    /// is in bounds (it stops once the PC reaches the end of the ROM).
    member _.GetInstruction(address: Address) : Instruction =
        rom[int address]

    /// Current keyboard word (last key pressed, cleared on key up).
    member _.Keyboard : Value = data[MemoryMap.KeyboardAddress]

    /// Written by the input device; simulated key press at address 0x6000.
    member _.SetKey(value: Value) : unit =
        data[MemoryMap.KeyboardAddress] <- value

    /// Creates a HACK memory instance for the given ROM.
    static member Create(rom: Instruction array, screenUpdater: (uint16 -> Value -> unit) option) =
        Memory(rom, screenUpdater)
