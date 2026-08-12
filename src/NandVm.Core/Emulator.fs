namespace NandVm.Core

/// Runs a HACK ROM to completion with optional per-instruction pacing.
[<RequireQualifiedAccess>]
module Emulator =

    type Config =
        { /// Milliseconds to wait per instruction; 0 runs at full speed.
          TickMs: int
          /// Enables per-instruction trace output on stdout.
          Debug: bool }

    let defaultConfig : Config = { TickMs = 0; Debug = false }

    /// Steps until the machine halts. Returns the exit code (0 on normal halt).
    let run (config: Config) (mem: Memory) (cpu: CpuState) : int =
        let sw = System.Diagnostics.Stopwatch.StartNew()

        let rec loop state =
            match Cpu.step mem state with
            | None -> 0
            | Some next ->
                if config.TickMs > 0 then
                    let remaining = int64 config.TickMs - sw.ElapsedMilliseconds

                    if remaining > 0L then
                        System.Threading.Thread.Sleep(int remaining)

                sw.Restart()
                loop next

        loop cpu
