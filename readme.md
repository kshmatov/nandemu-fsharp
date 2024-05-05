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
$ nandvm.exe [/h] [/t <value>] [/d] [programm_file_path]
```
- /h - show help message and exit
- /d - enable DEBUG mod, optional, default false
- /t <value> - set cpu tick duration to <value> ms, where value is integer, optional, default 10
- file_name - path and file name of programm to run, optional. If parameter is empty, sample programm will be executed. Only one file will be executed

VM expects any programm in "hack" format, see NAND to Tetris course or samples/rect.hack. Built-in sample programm will computes 2 + 3 and store result in D-register.

There is lack of error handling in CPU cycle, so some urgent error messages possible. Will fix this later

## Info

One more C-instruction added: **halt**. Opcode 011110, full instruction may looks like 0b1111011110111111. Cpu ignores Jump and Store parts of this instruction, it is used just to stop calculation cycle. It will not work in real Hack-processor but is usefull in emulation.

This is "just for fun" project, do not expect any cool stuff here. And I never write anything with C# or WinForms, so visual part may be more ugly then any other.

Fill free to get this code, blame it or print and burn it. 

**Good luck and be happy!**
