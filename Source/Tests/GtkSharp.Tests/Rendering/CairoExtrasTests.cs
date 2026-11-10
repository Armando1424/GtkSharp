using Cairo;
using Xunit;

namespace GtkSharp.Tests.Rendering;

/// <summary>
/// Additional Cairo coverage: matrices, regions and context state.
/// </summary>
public sealed class CairoExtrasTests
{
    [Fact]
    public void Matrix_InitScale_SetsScaleFactors()
    {
        var matrix = new Matrix();

        matrix.InitScale(2, 3);

        Assert.Equal(2, matrix.Xx);
        Assert.Equal(3, matrix.Yy);
    }

    [Fact]
    public void Matrix_Translate_SetsTranslation()
    {
        var matrix = new Matrix();

        matrix.Translate(10, 20);

        Assert.Equal(10, matrix.X0);
        Assert.Equal(20, matrix.Y0);
    }

    [Fact]
    public void Region_ExposesRectangleExtents()
    {
        using var region = new Region(new RectangleInt { X = 1, Y = 2, Width = 30, Height = 40 });

        var extents = region.Extents;

        Assert.Equal(1, extents.X);
        Assert.Equal(2, extents.Y);
        Assert.Equal(30, extents.Width);
        Assert.Equal(40, extents.Height);
    }

    [Fact]
    public void Context_LineWidth_RoundTrips()
    {
        using var surface = new ImageSurface(Format.ARGB32, 10, 10);
        using var context = new Context(surface);

        context.LineWidth = 3.5;

        Assert.Equal(3.5, context.LineWidth, precision: 6);
    }

    [Fact]
    public void Context_SaveRestore_RestoresState()
    {
        using var surface = new ImageSurface(Format.ARGB32, 10, 10);
        using var context = new Context(surface);
        context.LineWidth = 1;

        context.Save();
        context.LineWidth = 9;
        context.Restore();

        Assert.Equal(1, context.LineWidth, precision: 6);
    }

    [Fact]
    public void Surface_CreateSimilar_PreservesFormatAndSize()
    {
        using var surface = new ImageSurface(Format.ARGB32, 32, 24);

        using var similar = surface.CreateSimilar(Content.Color, 32, 24);

        Assert.Equal(Content.Color, similar.Content);
    }
}
