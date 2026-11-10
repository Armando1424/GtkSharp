using System;
using System.IO;
using Cairo;
using Xunit;

namespace GtkSharp.Tests.Rendering;

/// <summary>
/// Pure Cairo tests. They do not need a display, so they run even on headless
/// machines without Xvfb.
/// </summary>
public sealed class CairoTests
{
    [Fact]
    public void ImageSurface_ExposesDimensions()
    {
        using var surface = new ImageSurface(Format.ARGB32, 64, 48);

        Assert.Equal(64, surface.Width);
        Assert.Equal(48, surface.Height);
    }

    [Fact]
    public void DrawingOperations_DoNotThrow()
    {
        using var surface = new ImageSurface(Format.ARGB32, 64, 64);
        using var context = new Cairo.Context(surface);

        context.SetSourceRGB(1, 0, 0);
        context.Paint();
        context.SetSourceRGB(0, 0, 1);
        context.Rectangle(10, 10, 20, 20);
        context.Stroke();

        surface.Flush();

        Assert.Equal(64, surface.Width);
    }

    [Fact]
    public void WriteToPng_ProducesANonEmptyFile()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"gtksharp-cairo-{Guid.NewGuid():N}.png");

        try
        {
            using (var surface = new ImageSurface(Format.ARGB32, 32, 32))
            using (var context = new Context(surface))
            {
                context.SetSourceRGB(0, 1, 0);
                context.Paint();
                context.MoveTo(0, 0);
                context.LineTo(32, 32);
                context.Stroke();

                surface.Flush();
                surface.WriteToPng(path);
            }

            Assert.True(File.Exists(path));
            Assert.True(new FileInfo(path).Length > 0);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
