using Cairo;
using Pango;
using Xunit;

namespace GtkSharp.Tests.Rendering;

/// <summary>
/// Pure Pango/PangoCairo tests. They validate text shaping and measurement
/// without needing a GTK display.
/// </summary>
public sealed class PangoTests
{
    [Fact]
    public void CreateLayout_MeasuresNonEmptyText()
    {
        using var surface = new ImageSurface(Format.ARGB32, 200, 100);
        using var context = new Cairo.Context(surface);
        using var layout = CairoHelper.CreateLayout(context);

        layout.SetText("Hello");

        layout.GetPixelSize(out var width, out var height);

        Assert.True(width > 0, $"Expected a positive width, got {width}.");
        Assert.True(height > 0, $"Expected a positive height, got {height}.");
    }

    [Fact]
    public void LongerText_ProducesAWiderLayout()
    {
        using var surface = new ImageSurface(Format.ARGB32, 400, 100);
        using var context = new Cairo.Context(surface);
        using var layout = CairoHelper.CreateLayout(context);

        layout.SetText("Hi");
        layout.GetPixelSize(out var shortWidth, out _);

        layout.SetText("Hi, this is a much longer piece of text");
        layout.GetPixelSize(out var longWidth, out _);

        Assert.True(longWidth > shortWidth, $"Expected {longWidth} > {shortWidth}.");
    }

    [Fact]
    public void FontDescription_FromString_ParsesFamilyAndSize()
    {
        using var description = FontDescription.FromString("Sans 12");

        Assert.Equal("Sans", description.Family);
        Assert.True(description.Size > 0);
    }

    [Fact]
    public void SettingAFontDescription_ChangesLayoutMetrics()
    {
        using var surface = new ImageSurface(Format.ARGB32, 400, 200);
        using var context = new Cairo.Context(surface);
        using var layout = CairoHelper.CreateLayout(context);
        layout.SetText("Measure me");

        layout.GetPixelSize(out var defaultWidth, out _);

        using var big = FontDescription.FromString("Sans 32");
        layout.FontDescription = big;
        layout.GetPixelSize(out var bigWidth, out _);

        Assert.True(bigWidth > defaultWidth, $"Expected {bigWidth} > {defaultWidth}.");
    }
}
