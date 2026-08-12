namespace NandVm.Ui

open Avalonia
open Avalonia.Controls
open Avalonia.Media
open Avalonia.Media.Imaging
open Avalonia.Platform
open System
open System.Runtime.InteropServices
open NandVm.Core

/// Displays the HACK screen: a `WriteableBitmap` that mirrors a `ScreenBuffer`.
///
/// The screen is 1-bit black and white, so the ARGB int values (0xFF000000 and
/// 0xFFFFFFFF) have an identical byte layout in the BGRA8888 framebuffer and can
/// be copied verbatim with a single `Marshal.Copy`.
type ScreenControl(width: int, height: int) as self =
    inherit Image()

    let bitmap =
        new WriteableBitmap(
            PixelSize(width, height),
            Vector(96.0, 96.0),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque
        )

    do
        self.Width <- float width
        self.Height <- float height
        self.Stretch <- Stretch.None
        self.Source <- bitmap
        self.Focusable <- true

    member _.WidthPixels = width
    member _.HeightPixels = height

    /// Copies the back buffer into the bitmap when it is dirty and requests a repaint.
    member _.Refresh(buffer: ScreenBuffer) : unit =
        if buffer.IsDirty then
            use fb = bitmap.Lock()
            Marshal.Copy(buffer.Pixels, 0, fb.Address, buffer.Pixels.Length)
            buffer.ClearDirty()
            self.InvalidateVisual()

    interface IDisposable with
        member _.Dispose() = bitmap.Dispose()
