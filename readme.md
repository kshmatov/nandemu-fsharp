# Virtual HACK computer for NAND to Tetris course

## Disclaimer:
    I just begin to learn F# and dotnet.

## Done
- ram
- rom
- cpu
- screen output
- keyboard input (CapsLock sends Esc code)

## Usage
```
$ nandvm.exe [programm_file_path]
```

Programm file is optional, VM expects any programm in "hack" format, see NAND to Tetris course or samples/rect.hack. If no filename is given VM will run sample programm wich computes 2 + 3 and store result in D-register.

I plan to add flags for debug mode and tick duration later.

## Info

One more C-instruction added: **halt**. Opcode 011110, full instruction may looks like 0b1111011110111111. Cpu ignores Jump and Store parts of this instruction, it is used just to stop calculation cycle. It will not work in real Hack-processor but is usefull in emulation.

This is "just for fun" project, do not expect any cool stuff here. And I never write anything with C# or WinForms, so visual part may be more ugly then any other.

Fill free to get this code, blame it or print and burn it. 

**Good luck and be happy!**
