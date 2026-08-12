namespace NandVm.Core.Tests

open NandVm.Core
open Xunit

module KeysTests =

    [<Fact>]
    let ``function keys map to 140 + index`` () =
        Assert.Equal(141s, KeyMapper.keyToCode (NandKeys.Func 1))
        Assert.Equal(152s, KeyMapper.keyToCode (NandKeys.Func 12))

    [<Fact>]
    let ``special keys map to HACK scan codes`` () =
        Assert.Equal(140s, KeyMapper.keyToCode NandKeys.Esc)
        Assert.Equal(128s, KeyMapper.keyToCode NandKeys.Enter)
        Assert.Equal(129s, KeyMapper.keyToCode NandKeys.BSpace)
        Assert.Equal(130s, KeyMapper.keyToCode NandKeys.Left)
        Assert.Equal(131s, KeyMapper.keyToCode NandKeys.Up)
        Assert.Equal(132s, KeyMapper.keyToCode NandKeys.Right)
        Assert.Equal(133s, KeyMapper.keyToCode NandKeys.Down)
        Assert.Equal(134s, KeyMapper.keyToCode NandKeys.Home)
        Assert.Equal(135s, KeyMapper.keyToCode NandKeys.End)
        Assert.Equal(136s, KeyMapper.keyToCode NandKeys.PgUp)
        Assert.Equal(137s, KeyMapper.keyToCode NandKeys.PgDown)
        Assert.Equal(138s, KeyMapper.keyToCode NandKeys.Ins)
        Assert.Equal(139s, KeyMapper.keyToCode NandKeys.Del)

    [<Fact>]
    let ``printable characters map to their code points`` () =
        Assert.Equal(32s, KeyMapper.keyToCode NandKeys.Space)
        Assert.Equal(65s, KeyMapper.keyToCode (NandKeys.Char 65))

    [<Fact>]
    let ``ignored keys write nothing`` () =
        Assert.Equal(0s, KeyMapper.keyToCode NandKeys.Ignored)
