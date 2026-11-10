using Cairo;
using Pango;
using Xunit;

namespace GtkSharp.Tests.Rendering;

/// <summary>
/// Extra Pango coverage: markup, font descriptions and layout context.
/// </summary>
public sealed class PangoExtrasTests
{
    [Fact]
    public void SetMarkup_ProducesMeasurableText()
    {
        using var surface = new ImageSurface(Format.ARGB32, 300, 100);
        using var context = new Cairo.Context(surface);
        using var layout = CairoHelper.CreateLayout(context);

        layout.SetMarkup("<b>bold</b> and <i>italic</i>");
        layout.GetPixelSize(out var width, out var height);

        Assert.True(width > 0);
        Assert.True(height > 0);
    }

    [Fact]
    public void FontDescription_ParsesWeight()
    {
        using var description = FontDescription.FromString("Sans Bold 14");

        Assert.Equal(Weight.Bold, description.Weight);
    }

    [Fact]
    public void FontDescription_Merge_AppliesTheOtherDescription()
    {
        using var baseDescription = FontDescription.FromString("Sans 10");
        using var bold = new FontDescription { Weight = Weight.Bold };

        baseDescription.Merge(bold, true);

        Assert.Equal(Weight.Bold, baseDescription.Weight);
        Assert.Equal("Sans", baseDescription.Family);
    }

    [Fact]
    public void Layout_ExposesAContext()
    {
        using var surface = new ImageSurface(Format.ARGB32, 100, 100);
        using var context = new Cairo.Context(surface);
        using var layout = CairoHelper.CreateLayout(context);

        Assert.NotNull(layout.Context);
    }

    [Fact]
    public void AttrList_CanBeCreated()
    {
        using var attributes = new AttrList();

        Assert.NotNull(attributes);
    }

    [Fact]
    public void Layout_Alignment_RoundTrips()
    {
        using var surface = new ImageSurface(Format.ARGB32, 100, 100);
        using var context = new Cairo.Context(surface);
        using var layout = CairoHelper.CreateLayout(context);

        layout.Alignment = Alignment.Center;

        Assert.Equal(Alignment.Center, layout.Alignment);
    }
}
