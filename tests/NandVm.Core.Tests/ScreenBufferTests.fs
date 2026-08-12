namespace NandVm.Core.Tests

open NandVm.Core
open Xunit

module ScreenBufferTests =

    let private black = -16777216
    let private white = -1

    [<Fact>]
    let ``starts black and dirty`` () =
        let s = ScreenBuffer(512, 256)
        Assert.True s.IsDirty
        Assert.All(s.Pixels, fun p -> Assert.Equal(black, p))

    [<Fact>]
    let ``dirty flag tracks ClearDirty and SetWord`` () =
        let s = ScreenBuffer(512, 256)
        s.ClearDirty()
        Assert.False s.IsDirty
        s.SetWord(0us, 0s)
        Assert.True s.IsDirty

    [<Fact>]
    let ``all ones word paints 16 white pixels`` () =
        let s = ScreenBuffer(512, 256)
        s.ClearDirty()
        s.SetWord(0us, 0xFFFFs)

        for i = 0 to 15 do
            Assert.Equal(white, s.Pixels[i])

        Assert.Equal(black, s.Pixels[16])

    [<Fact>]
    let ``bit 15 is the leftmost pixel`` () =
        let s = ScreenBuffer(512, 256)
        s.SetWord(0us, 0x8000s)
        Assert.Equal(white, s.Pixels[0])
        Assert.Equal(black, s.Pixels[1])

    [<Fact>]
    let ``bit 0 is the rightmost pixel`` () =
        let s = ScreenBuffer(512, 256)
        s.SetWord(0us, 0x0001s)
        Assert.Equal(white, s.Pixels[15])
        Assert.Equal(black, s.Pixels[14])

    [<Fact>]
    let ``words lay out row major with 32 columns`` () =
        let s = ScreenBuffer(512, 256)
        s.SetWord(32us, 0x8000s)
        Assert.Equal(white, s.Pixels[512])
        Assert.Equal(black, s.Pixels[511])

    [<Fact>]
    let ``bottom row word is accepted`` () =
        let s = ScreenBuffer(512, 256)
        s.SetWord(8191us, 0xFFFFs)
        Assert.True s.IsDirty
        Assert.Equal(black, s.Pixels[131055])
        for i = 131056 to 131071 do
            Assert.Equal(white, s.Pixels[i])

    [<Fact>]
    let ``out of range words are ignored`` () =
        let s = ScreenBuffer(512, 256)
        s.ClearDirty()
        s.SetWord(8192us, 0xFFFFs)
        Assert.False s.IsDirty

    [<Fact>]
    let ``clear resets to black and marks dirty`` () =
        let s = ScreenBuffer(512, 256)
        s.SetWord(0us, 0xFFFFs)
        s.Clear()
        Assert.True s.IsDirty
        Assert.All(s.Pixels, fun p -> Assert.Equal(black, p))
