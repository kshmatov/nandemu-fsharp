namespace NandVm.Core

/// Back buffer of the HACK screen: 512x256 pixels, one 16-bit word per row of
/// 16 pixels (16 pixels in each of the 32 word columns, MSB first).
///
/// UI-agnostic: front-ends copy `Pixels` into their own framebuffer. The same
/// word-to-pixel mapping is used by all front-ends.
type ScreenBuffer(width: int, height: int) =
    let wordCols = width / 16
    let words = Array.create (wordCols * height) 0s
    // ARGB (Format32bppArgb): 0xFF000000 is opaque black, 0xFFFFFFFF opaque white.
    // As signed int32 these are -16777216 and -1 respectively.
    let black = -16777216
    let white = -1
    let pixels = Array.create (width * height) black
    let mutable dirty = true

    member _.Width = width
    member _.Height = height

    /// True when a word was written since the last render.
    member _.IsDirty = dirty

    /// Applies a screen word write. `index` is screen-relative (0..words.Length-1).
    member _.SetWord(index: uint16, value: Value) : unit =
        let i = int index

        if i >= 0 && i < words.Length then
            words[i] <- value
            let baseOffset = (i / wordCols) * width + (i % wordCols) * 16

            for bit = 0 to 15 do
                let on = (value &&& (1s <<< (15 - bit))) <> 0s
                pixels[baseOffset + bit] <- if on then white else black

            dirty <- true

    /// The ARGB pixel array (length = width * height).
    member _.Pixels = pixels

    /// Marks the buffer as rendered, clearing the dirty flag.
    member _.ClearDirty() = dirty <- false

    /// Fills the screen with black.
    member _.Clear() =
        System.Array.Fill(pixels, black)
        System.Array.Fill(words, 0s)
        dirty <- true
