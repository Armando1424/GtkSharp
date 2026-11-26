using Cairo;
using Xunit;

namespace GtkSharp.Tests.Rendering;

/// <summary>
/// Advanced <see cref="Context"/> operations (paths, groups, masks, patterns
/// and extents) verified through image-surface pixels where possible.
/// </summary>
public sealed class CairoAdvancedTests
{
    private static (byte B, byte G, byte R, byte A) Pixel(ImageSurface surface, int x, int y)
    {
        byte[] data = surface.Data;
        int offset = y * surface.Stride + x * 4;
        return (data[offset], data[offset + 1], data[offset + 2], data[offset + 3]);
    }

    [Fact]
    public void RelativePath_FillsTheExpectedArea()
    {
        using var surface = new ImageSurface(Format.ARGB32, 8, 8);
        using var context = new Context(surface);

        context.NewPath();
        context.MoveTo(0, 0);
        context.RelMoveTo(1, 1);
        context.RelLineTo(4, 0);
        context.RelLineTo(0, 4);
        context.RelLineTo(-4, 0);
        context.ClosePath();

        Rectangle extents = context.PathExtents();
        Assert.Equal(1, extents.X, precision: 3);
        Assert.Equal(1, extents.Y, precision: 3);
        Assert.Equal(4, extents.Width, precision: 3);
        Assert.Equal(4, extents.Height, precision: 3);
        Assert.True(context.InFill(2.5, 2.5));

        context.SetSourceRGB(0, 1, 0);
        context.Fill();
        surface.Flush();

        var inside = Pixel(surface, 2, 2);
        var outside = Pixel(surface, 7, 7);
        Assert.Equal(255, inside.G);
        Assert.Equal(0, outside.A);
    }

    [Fact]
    public void GroupAndAlpha_ComposeTheDrawing()
    {
        using var surface = new ImageSurface(Format.ARGB32, 8, 8);
        using var context = new Context(surface);

        context.PushGroup();
        context.SetSourceRGB(0, 0, 1);
        context.Rectangle(0, 0, 8, 8);
        context.Fill();
        context.PopGroupToSource();
        context.PaintWithAlpha(0.5);
        surface.Flush();

        var pixel = Pixel(surface, 4, 4);
        Assert.True(pixel.B > 0);
        Assert.True(pixel.A > 0);
    }

    [Fact]
    public void ClipAndFillQueries_ReportInsidePoints()
    {
        using var surface = new ImageSurface(Format.ARGB32, 8, 8);
        using var context = new Context(surface);

        Assert.False(context.HasCurrentPoint);

        context.Rectangle(0, 0, 8, 8);

        Assert.True(context.InFill(4, 4));
        context.Clip();
        Assert.True(context.InClip(4, 4));
        Assert.False(context.InClip(20, 20));
    }

    [Fact]
    public void CopyAndAppendPath_ReuseThePath()
    {
        using var surface = new ImageSurface(Format.ARGB32, 8, 8);
        using var context = new Context(surface);

        context.Rectangle(2, 2, 4, 4);
        using var path = context.CopyPath();
        context.NewPath();
        context.AppendPath(path);
        context.SetSourceRGB(1, 0, 0);
        context.Fill();
        surface.Flush();

        var pixel = Pixel(surface, 3, 3);
        Assert.Equal(255, pixel.R);
    }

    [Fact]
    public void SourceSurface_IsPaintedAtTheGivenOffset()
    {
        using var source = new ImageSurface(Format.ARGB32, 4, 4);
        using (var sourceContext = new Context(source))
        {
            sourceContext.SetSourceRGB(1, 1, 0);
            sourceContext.Paint();
            source.Flush();
        }

        using var surface = new ImageSurface(Format.ARGB32, 8, 8);
        using var context = new Context(surface);
        context.SetSourceSurface(source, 2, 2);
        context.Rectangle(2, 2, 4, 4);
        context.Fill();
        surface.Flush();

        var painted = Pixel(surface, 3, 3);
        var untouched = Pixel(surface, 0, 0);
        Assert.Equal(255, painted.R);
        Assert.Equal(255, painted.G);
        Assert.Equal(0, untouched.A);
    }

    [Fact]
    public void SolidPattern_IsUsedAsSource()
    {
        using var surface = new ImageSurface(Format.ARGB32, 4, 4);
        using var context = new Context(surface);
        using var pattern = new SolidPattern(0, 1, 0);

        context.SetSource(pattern);
        context.Paint();
        surface.Flush();

        var pixel = Pixel(surface, 2, 2);
        Assert.Equal(255, pixel.G);
    }

    [Fact]
    public void Extents_ReportTheDrawnRectangle()
    {
        using var surface = new ImageSurface(Format.ARGB32, 8, 8);
        using var context = new Context(surface);

        context.Rectangle(2, 2, 4, 3);

        Rectangle fill = context.FillExtents();
        Assert.Equal(2, fill.X, precision: 6);
        Assert.Equal(2, fill.Y, precision: 6);
        Assert.Equal(4, fill.Width, precision: 6);
        Assert.Equal(3, fill.Height, precision: 6);
    }
}
