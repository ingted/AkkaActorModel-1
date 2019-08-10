﻿module Fractal.Shared

open System
open System.Drawing
open System.Threading
open System.Threading.Tasks
open System.IO
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open Akka.FSharp
open SixLabors.ImageSharp.Formats


type RenderedTile = {
    Bytes: byte array
    X: int
    Y: int
}

type RenderTile = {
    X: int
    Y: int
    Height: int
    Width: int
}

type BitmapConverter() =

    static member toByteArray (imageIn:Image<Rgba32>) =
        use mem = new MemoryStream()
        imageIn.Save(mem, SixLabors.ImageSharp.Formats.Png.PngEncoder())
        mem.ToArray()

    static member toBitmap(byteArray:byte array) =
        use mem = new MemoryStream(byteArray)
        let image = Image.Load(mem)
        image

let mandelbrot (xp:int, yp:int, w:int, h:int,width:int, height:int ,maxr:float, minr:float, maxi:float, mini:float) =
    let img = new Image<Rgba32>(w, h)

    let mutable zx = 0.
    let mutable zy = 0.
    let mutable cx = 0.
    let mutable cy = 0.
    let xjump = ((maxr - minr) / Convert.ToDouble(width))
    let yjump = ((maxi - mini) / Convert.ToDouble(height))
    let mutable tempzx = 0.
    let loopmax = 1000
    let mutable loopgo = 0
    for x = xp to (xp + w - 1) do //(int x = xp; x < xp + w; x++)
        cx <- (xjump * (float x)) - Math.Abs(minr)
        for y = yp to (yp + h - 1) do //(int y = yp; y < yp + h; y++)
            zx <- 0.
            zy <- 0.
            cy <- (yjump * (float y)) - Math.Abs(mini)
            loopgo <- 0

            while zx * zx + zy * zy <= 4. && loopgo < loopmax do
                loopgo <- loopgo + 1
                tempzx <- zx
                zx <- (zx * zx) - (zy * zy) + cx
                zy <- (2. * tempzx * zy) + cy

            if loopgo <> loopmax then
                img.[x - xp, y - yp] <- Rgba32(byte(loopgo % 32 * 7), byte(loopgo % 128 * 2), byte(loopgo % 16 * 14))
            else
                img.[x - xp, y - yp] <- Rgba32.Black
    img

let tileRenderer (mailbox: Actor<_>) (render:RenderTile) =
    logInfof mailbox "%A rendering %d , %d" mailbox.Self render.X render.Y

    let res = mandelbrot(render.X, render.Y,render.Width,render.Height, 4000, 4000, 0.5, -2.5, 1.5, -1.5)
    //let res = MandelbrotSet.Mandelbrot.Set(render.X, render.Y,render.Width,render.Height, 4000, 4000, 0.5, -2.5, 1.5, -1.5)
    let bytes = BitmapConverter.toByteArray(res)
    mailbox.Sender() <! {Bytes = bytes; X = render.X; Y = render.Y}
