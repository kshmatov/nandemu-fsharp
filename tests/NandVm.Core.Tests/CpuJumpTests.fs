namespace NandVm.Core.Tests

open NandVm.Core
open Xunit

module CpuJumpTests =

    let private target = 8us

    /// Builds a ROM that loads `d` into D (works for small negative values too),
    /// sets A to the jump target and then executes a D;jump instruction.
    let private buildRom (d: Value) (jump: JumpCode) : uint16 array =
        let setD =
            if d >= 0s then
                [| TestUtil.a (int d)
                   TestUtil.c false OpCode.Src 0b010 JumpCode.None |]
            else
                [| TestUtil.a (int -d)
                   TestUtil.c false OpCode.Src 0b010 JumpCode.None
                   TestUtil.c false OpCode.MinusDReg 0b010 JumpCode.None |]

        Array.append setD [| TestUtil.a (int target)
                             TestUtil.c false OpCode.DReg 0b000 jump |]

    let private jumpTaken (d: Value) (jump: JumpCode) : bool =
        let cpu = TestUtil.runRom (buildRom d jump)
        cpu.Pc = target

    [<Fact>]
    let ``JGT jumps when D is positive`` () =
        Assert.True(jumpTaken 1s JumpCode.Gt)
        Assert.True(jumpTaken 5s JumpCode.Gt)

    [<Fact>]
    let ``JGT does not jump on zero or negative`` () =
        Assert.False(jumpTaken 0s JumpCode.Gt)
        Assert.False(jumpTaken -1s JumpCode.Gt)

    [<Fact>]
    let ``JEQ jumps only on zero`` () =
        Assert.True(jumpTaken 0s JumpCode.Eq)
        Assert.False(jumpTaken 1s JumpCode.Eq)
        Assert.False(jumpTaken -1s JumpCode.Eq)

    [<Fact>]
    let ``JGE jumps on zero and positive`` () =
        Assert.True(jumpTaken 0s JumpCode.Ge)
        Assert.True(jumpTaken 1s JumpCode.Ge)
        Assert.False(jumpTaken -1s JumpCode.Ge)

    [<Fact>]
    let ``JLT jumps only on negative`` () =
        Assert.True(jumpTaken -1s JumpCode.Lt)
        Assert.False(jumpTaken 0s JumpCode.Lt)
        Assert.False(jumpTaken 1s JumpCode.Lt)

    [<Fact>]
    let ``JNE jumps on non-zero`` () =
        Assert.True(jumpTaken 1s JumpCode.Ne)
        Assert.True(jumpTaken -1s JumpCode.Ne)
        Assert.False(jumpTaken 0s JumpCode.Ne)

    [<Fact>]
    let ``JLE jumps on zero and negative`` () =
        Assert.True(jumpTaken 0s JumpCode.Le)
        Assert.True(jumpTaken -1s JumpCode.Le)
        Assert.False(jumpTaken 1s JumpCode.Le)

    [<Fact>]
    let ``unconditional jump always taken`` () =
        Assert.True(jumpTaken 0s JumpCode.Now)
        Assert.True(jumpTaken -1s JumpCode.Now)
        Assert.True(jumpTaken 1s JumpCode.Now)

    [<Fact>]
    let ``no jump advances the program counter`` () =
        // @5; D=A; @8; D=A with no jump bits -> PC walks past the ROM end.
        let rom =
            [| TestUtil.a 5
               TestUtil.c false OpCode.Src 0b010 JumpCode.None
               TestUtil.a 8
               TestUtil.c false OpCode.Src 0b000 JumpCode.None |]

        let cpu = TestUtil.runRom rom
        Assert.Equal(4us, cpu.Pc)
        Assert.Equal(8s, cpu.A)

    [<Fact>]
    let ``halt stops execution before the jump`` () =
        // JGT with D=1 would jump to address 8, but the halt comes first.
        let rom =
            [| TestUtil.a 1
               TestUtil.c false OpCode.Src 0b010 JumpCode.None
               TestUtil.c false OpCode.Halt 0b000 JumpCode.None
               TestUtil.a 8
               TestUtil.c false OpCode.DReg 0b000 JumpCode.Gt |]

        let cpu = TestUtil.runRom rom
        Assert.Equal(2us, cpu.Pc)

    [<Fact>]
    let ``out of ROM program counter halts`` () =
        // Unconditional jump to address 100 which is beyond the ROM.
        let rom = [| TestUtil.a 100; TestUtil.c false OpCode.Zero 0b000 JumpCode.Now |]
        let cpu = TestUtil.runRom rom
        Assert.Equal(100us, cpu.Pc)
