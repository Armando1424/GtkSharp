using System;
using System.IO;
using Gdk;
using Xunit;

namespace GtkSharp.Tests.Rendering;

/// <summary>
/// Gdk.Pixbuf is pure Gdk and does not need a display.
/// </summary>
public sealed class GdkPixbufTests
{
    [Fact]
    public void NewPixbuf_ExposesDimensionsAndChannels()
    {
        using var pixbuf = new Pixbuf(Colorspace.Rgb, true, 8, 8, 8);

        Assert.Equal(8, pixbuf.Width);
        Assert.Equal(8, pixbuf.Height);
        Assert.Equal(4, pixbuf.NChannels);
        Assert.True(pixbuf.Rowstride >= 8 * 4);
    }

    [Fact]
    public void NewPixbuf_WithoutAlpha_HasThreeChannels()
    {
        using var pixbuf = new Pixbuf(Colorspace.Rgb, false, 8, 4, 4);

        Assert.Equal(3, pixbuf.NChannels);
    }

    [Fact]
    public void Fill_DoesNotThrow()
    {
        using var pixbuf = new Pixbuf(Colorspace.Rgb, true, 8, 4, 4);

        var exception = Record.Exception(() => pixbuf.Fill(0xFF0000FF));

        Assert.Null(exception);
    }

    [Fact]
    public void SaveToBuffer_ProducesPngBytes()
    {
        using var pixbuf = new Pixbuf(Colorspace.Rgb, true, 8, 4, 4);
        pixbuf.Fill(0x00FF00FF);

        var bytes = pixbuf.SaveToBuffer("png");

        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void Save_WritesAFile()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"gtksharp-pixbuf-{Guid.NewGuid():N}.png");

        try
        {
            using (var pixbuf = new Pixbuf(Colorspace.Rgb, true, 8, 4, 4))
            {
                pixbuf.Fill(0x0000FFFF);
                Assert.True(pixbuf.Save(path, "png"));
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

    [Fact]
    public void Copy_PreservesDimensions()
    {
        using var pixbuf = new Pixbuf(Colorspace.Rgb, true, 8, 16, 12);

        using var copy = pixbuf.Copy();

        Assert.Equal(16, copy.Width);
        Assert.Equal(12, copy.Height);
    }
}
