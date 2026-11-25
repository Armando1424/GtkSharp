using Cairo;
using Xunit;

namespace GtkSharp.Tests.Rendering;

/// <summary>
/// Drawing behaviour of <see cref="Context"/> verified through the pixels of an
/// image surface. These tests do not need a display.
/// </summary>
public sealed class CairoDrawingTests
{
    private static (byte B, byte G, byte R, byte A) Pixel(ImageSurface surface, int x, int y)
    {
        byte[] data = surface.Data;
        int offset = y * surface.Stride + x * 4;
        return (data[offset], data[offset + 1], data[offset + 2], data[offset + 3]);
    }

    [Fact]
    public void FillRectangle_UsesTheSourceColor()
    {
        using var surface = new ImageSurface(Format.ARGB32, 8, 8);
        using var context = new Context(surface);

        context.SetSourceRGB(1, 0, 0);
        context.Rectangle(0, 0, 8, 8);
        context.Fill();
        surface.Flush();

        var pixel = Pixel(surface, 4, 4);
        Assert.Equal(255, pixel.R);
        Assert.Equal(0, pixel.G);
        Assert.Equal(0, pixel.B);
        Assert.Equal(255, pixel.A);
    }

    [Fact]
    public void Translate_OffsetsTheDrawing()
    {
        using var surface = new ImageSurface(Format.ARGB32, 8, 8);
        using var context = new Context(surface);

        context.SetSourceRGB(1, 0, 0);
        context.Translate(4, 4);
        context.Rectangle(0, 0, 4, 4);
        context.Fill();
        surface.Flush();

        var painted = Pixel(surface, 5, 5);
        var untouched = Pixel(surface, 1, 1);
        Assert.Equal(255, painted.R);
        Assert.Equal(0, untouched.A);
    }

    [Fact]
    public void Clip_RestrictsPaintingToTheRegion()
    {
        using var surface = new ImageSurface(Format.ARGB32, 8, 8);
        using var context = new Context(surface);

        context.Rectangle(2, 2, 4, 4);
        context.Clip();
        context.SetSourceRGB(0, 0, 1);
        context.Paint();
        surface.Flush();

        var inside = Pixel(surface, 3, 3);
        var outside = Pixel(surface, 0, 0);
        Assert.Equal(255, inside.B);
        Assert.Equal(0, outside.A);
    }

    [Fact]
    public void SaveRestore_RestoresTheSourceColor()
    {
        using var surface = new ImageSurface(Format.ARGB32, 8, 8);
        using var context = new Context(surface);

        context.SetSourceRGB(1, 0, 0);
        context.Save();
        context.SetSourceRGB(0, 1, 0);
        context.Restore();
        context.Paint();
        surface.Flush();

        var pixel = Pixel(surface, 4, 4);
        Assert.Equal(255, pixel.R);
        Assert.Equal(0, pixel.G);
    }

    [Fact]
    public void StrokeLine_UsesTheSourceColor()
    {
        using var surface = new ImageSurface(Format.ARGB32, 8, 8);
        using var context = new Context(surface);

        context.SetSourceRGB(0, 0, 1);
        context.LineWidth = 2;
        context.MoveTo(0, 4);
        context.LineTo(8, 4);
        context.Stroke();
        surface.Flush();

        var pixel = Pixel(surface, 4, 4);
        Assert.Equal(255, pixel.B);
        Assert.Equal(0, pixel.R);
    }

    [Fact]
    public void PathAndStateOperations_RoundTrip()
    {
        using var surface = new ImageSurface(Format.ARGB32, 8, 8);
        using var context = new Context(surface);

        context.LineWidth = 1.5;
        context.LineCap = LineCap.Round;
        context.LineJoin = LineJoin.Round;
        context.FillRule = FillRule.EvenOdd;
        context.Operator = Operator.Over;
        context.Tolerance = 0.5;
        context.Antialias = Antialias.Gray;
        context.SetDash(new double[] { 2, 2 }, 0);

        context.MoveTo(0, 0);
        context.CurveTo(1, 1, 2, 2, 3, 3);
        context.Arc(4, 4, 2, 0, System.Math.PI);
        context.ClosePath();
        context.Stroke();
        surface.Flush();

        Assert.Equal(LineCap.Round, context.LineCap);
        Assert.Equal(LineJoin.Round, context.LineJoin);
        Assert.Equal(FillRule.EvenOdd, context.FillRule);
        Assert.Equal(Operator.Over, context.Operator);
        Assert.Equal(0.5, context.Tolerance, precision: 6);
        Assert.Equal(Antialias.Gray, context.Antialias);
    }
}
