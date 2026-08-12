# Virtual HACK computer for NAND to Tetris course

An emulator for the HACK computer from the [NAND to Tetris](https://www.nand2tetris.org/) course, written in F#.

## Architecture

The code is split into three projects:

```
nandemu.sln              core + UI + CLI + tests (builds and runs on any OS)

src/NandVm.Core/         platform-independent emulator core
    Types.fs             Value/Address/Instruction, memory map, OpCode/JumpCode enums
    Memory.fs            HACK data memory (RAM + Screen + Keyboard) and separate ROM
    Decode.fs            A-/C-instruction decoder
    Cpu.fs               immutable CPU state, pure `Cpu.step` per instruction
    Emulator.fs          headless run loop with optional per-instruction pacing
    RomLoader.fs         HACK binary program file loader
    Cli.fs               shared /h /d /t argument parsing
    Keys.fs              HACK keyboard scan codes
    ScreenBuffer.fs      UI-agnostic screen back buffer (word -> 16 pixels)

src/NandVm.Ui/           Avalonia front-end (Linux, macOS, Windows)
    Keys.fs              Avalonia keys -> semantic key codes
    ScreenControl.fs     WriteableBitmap mirror of the screen back buffer
    MainWindow.fs        wires the core to a window
    App.fs, Program.fs   Avalonia application bootstrap

src/NandVm.Cli/          headless console runner (works without a display)

tests/NandVm.Core.Tests/ xUnit: decoder, ALU, jumps, memory, screen, keys, integration
```

The core is decoupled from any UI: `Cpu.step` is a pure function that takes the
current memory and CPU state and returns the next state (or `None` when the
machine halts). Screens and the keyboard are memory-mapped devices at the HACK
addresses `0x4000..0x5FFF` and `0x6000`.

## Usage

### Headless (CLI)

```
$ dotnet run --project src/NandVm.Cli -- [/h] [/d] [/t <value>] [file_name]
```

- `/h` - show help and exit
- `/d` - enable DEBUG mode (per-instruction trace), default false
- `/t <value>` - ms per instruction, default 10; `0` runs at full speed
- `file_name` - a program in HACK binary format (one 16-bit binary string per line).
  If omitted, the built-in sample (computes 2 + 3 and stores it in RAM[0]) runs.
  Only one file is executed. Absolute paths like `/tmp/x.hack` are supported;
  use `--` to pass a file that starts with a slash unambiguously.

```
$ dotnet run --project src/NandVm.Cli -- /t 0 samples/rect.hack
```

### Desktop (Avalonia, cross-platform)

On any of Linux, macOS or Windows, build and run the Avalonia app from
`nandemu.sln` with the same arguments:

```
$ dotnet run --project src/NandVm.Ui -- /t 10 samples/rect.hack
```

## Notes

- The emulator expects programs in "hack" format, see `samples/rect.hack`.
- The `halt` instruction is a non-standard C-instruction extension
  (`comp = 011110`), used to stop the emulation loop. It will not work on real
  HACK hardware. Full instruction: `0b1111011110111111`.
- All int16 ALU arithmetic wraps on overflow, as in the real 16-bit HACK ALU.
