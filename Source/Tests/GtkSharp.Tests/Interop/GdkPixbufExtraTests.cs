using System;
using Gdk;
using Xunit;

namespace GtkSharp.Tests.Interop;

public sealed class GdkPixbufExtraTests
{
    private static byte[] CreateRgbData(int width, int height)
    {
        var data = new byte[width * height * 3];

        for (var i = 0; i < data.Length; i++)
            data[i] = (byte)(i % 255);

        return data;
    }

    [Fact]
    public void RawBuffer_ExposesTheImageProperties()
    {
        const int width = 4;
        const int height = 4;
        var data = CreateRgbData(width, height);

        using var pixbuf = new Pixbuf(data, Colorspace.Rgb, false, 8, width, height, width * 3, null);

        Assert.Equal(width, pixbuf.Width);
        Assert.Equal(height, pixbuf.Height);
        Assert.Equal(3, pixbuf.NChannels);
        Assert.False(pixbuf.HasAlpha);
        Assert.Equal(width * 3, pixbuf.Rowstride);
    }

    [Fact]
    public void FillScaleAndCopy_Work()
    {
        const int width = 4;
        const int height = 4;
        var data = CreateRgbData(width, height);

        using var pixbuf = new Pixbuf(data, Colorspace.Rgb, false, 8, width, height, width * 3, null);

        pixbuf.Fill(0x112233);
        using var scaled = pixbuf.ScaleSimple(2, 2, InterpType.Bilinear);
        Assert.Equal(2, scaled.Width);

        using var copy = pixbuf.Copy();
        Assert.Equal(pixbuf.Width, copy.Width);
    }

    [Fact]
    public void AddAlpha_And_SaveToBuffer_Work()
    {
        const int width = 2;
        const int height = 2;

        using var pixbuf = new Pixbuf(CreateRgbData(width, height), Colorspace.Rgb, false, 8, width, height, width * 3, null);
        using var withAlpha = pixbuf.AddAlpha(true, 0, 0, 0);

        Assert.True(withAlpha.HasAlpha);
        var png = withAlpha.SaveToBuffer("png");
        Assert.True(png.Length > 0);
    }
}
