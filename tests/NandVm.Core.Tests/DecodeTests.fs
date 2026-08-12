namespace NandVm.Core.Tests

open NandVm.Core
open Xunit

module DecodeTests =

    [<Fact>]
    let ``A-instruction carries its value`` () =
        match Decode.decode (TestUtil.a 2) with
        | Decoded.Address v -> Assert.Equal(2s, v)
        | _ -> failwith "expected an Address instruction"

    [<Fact>]
    let ``A-instruction supports the full 15-bit range`` () =
        match Decode.decode (TestUtil.a 0x7FFF) with
        | Decoded.Address v -> Assert.Equal(32767s, v)
        | _ -> failwith "expected an Address instruction"

    [<Fact>]
    let ``D=A decodes source, comp and dest`` () =
        match Decode.decode (TestUtil.c false OpCode.Src 0b010 JumpCode.None) with
        | Decoded.Computation op ->
            Assert.Equal(OperandSource.ARegister, op.Source)
            Assert.Equal(OpCode.Src, op.Comp)
            Assert.False op.Dest.ToMemory
            Assert.True op.Dest.ToDRegister
            Assert.False op.Dest.ToARegister
            Assert.Equal(JumpCode.None, op.Jump)
        | _ -> failwith "expected a Computation instruction"

    [<Fact>]
    let ``M=D selects memory source and memory destination`` () =
        match Decode.decode (TestUtil.c true OpCode.DReg 0b001 JumpCode.None) with
        | Decoded.Computation op ->
            Assert.Equal(OperandSource.Memory, op.Source)
            Assert.True op.Dest.ToMemory
            Assert.False op.Dest.ToDRegister
        | _ -> failwith "expected a Computation instruction"

    [<Fact>]
    let ``destination may target multiple registers`` () =
        match Decode.decode (TestUtil.c false OpCode.DPlusSrc 0b111 JumpCode.None) with
        | Decoded.Computation op ->
            Assert.True op.Dest.ToMemory
            Assert.True op.Dest.ToDRegister
            Assert.True op.Dest.ToARegister
        | _ -> failwith "expected a Computation instruction"

    [<Fact>]
    let ``jump bits are preserved`` () =
        match Decode.decode (TestUtil.c false OpCode.Zero 0b000 JumpCode.Gt) with
        | Decoded.Computation op -> Assert.Equal(JumpCode.Gt, op.Jump)
        | _ -> failwith "expected a Computation instruction"

    [<Fact>]
    let ``halt extension decodes`` () =
        match Decode.decode (TestUtil.c false OpCode.Halt 0b111 JumpCode.Now) with
        | Decoded.Computation op -> Assert.Equal(OpCode.Halt, op.Comp)
        | _ -> failwith "expected a Computation instruction"

    [<Fact>]
    let ``sample program instructions decode as expected`` () =
        let decoded =
            TestUtil.sampleRom
            |> Array.map Decode.decode

        match decoded[0] with
        | Decoded.Address 2s -> ()
        | _ -> failwith "first instruction should be @2"

        match decoded[1] with
        | Decoded.Computation op ->
            Assert.Equal(OpCode.Src, op.Comp)
            Assert.True op.Dest.ToDRegister
        | _ -> failwith "second instruction should be D=A"

        match decoded[3] with
        | Decoded.Computation op ->
            Assert.Equal(OpCode.DPlusSrc, op.Comp)
            Assert.True op.Dest.ToDRegister
        | _ -> failwith "fourth instruction should be D=D+A"

        match decoded[5] with
        | Decoded.Computation op ->
            Assert.Equal(OpCode.DReg, op.Comp)
            Assert.Equal(OperandSource.Memory, op.Source)
            Assert.True op.Dest.ToMemory
        | _ -> failwith "sixth instruction should be M=D"
