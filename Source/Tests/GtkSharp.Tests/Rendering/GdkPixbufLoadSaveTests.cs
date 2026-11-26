using System;
using System.IO;
using Gdk;
using GLib;
using Xunit;

namespace GtkSharp.Tests.Rendering;

/// <summary>
/// Loading and saving pixbufs through the file, byte array and stream
/// constructors. Gdk.Pixbuf is pure Gdk and does not need a display.
/// </summary>
public sealed class GdkPixbufLoadSaveTests
{
    private static Pixbuf CreatePixbuf()
    {
        var pixbuf = new Pixbuf(Colorspace.Rgb, true, 8, 4, 3);
        pixbuf.Fill(0xFF0000FF);
        return pixbuf;
    }

    private static string TempPath() =>
        System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"gtksharp-pixbuf-{Guid.NewGuid():N}.png");

    [Fact]
    public void LoadFromByteArray_RestoresDimensions()
    {
        using var original = CreatePixbuf();
        byte[] png = original.SaveToBuffer("png");

        using var loaded = new Pixbuf(png);

        Assert.Equal(4, loaded.Width);
        Assert.Equal(3, loaded.Height);
    }

    [Fact]
    public void LoadFromByteArrayWithSize_ScalesTheImage()
    {
        using var original = CreatePixbuf();
        byte[] png = original.SaveToBuffer("png");

        using var scaled = new Pixbuf(png, 2, 2);

        Assert.Equal(2, scaled.Width);
        Assert.Equal(2, scaled.Height);
    }

    [Fact]
    public void LoadFromStream_RestoresDimensions()
    {
        using var original = CreatePixbuf();
        byte[] png = original.SaveToBuffer("png");
        using var stream = new MemoryStream(png);

        using var loaded = new Pixbuf(stream);

        Assert.Equal(4, loaded.Width);
        Assert.Equal(3, loaded.Height);
    }

    [Fact]
    public void LoadFromStreamWithSize_ScalesTheImage()
    {
        using var original = CreatePixbuf();
        byte[] png = original.SaveToBuffer("png");
        using var stream = new MemoryStream(png);

        using var scaled = new Pixbuf(stream, 2, 2);

        Assert.Equal(2, scaled.Width);
        Assert.Equal(2, scaled.Height);
    }

    [Fact]
    public void LoadFromFile_RestoresDimensions()
    {
        string path = TempPath();
        try
        {
            using (var original = CreatePixbuf())
                Assert.True(original.Save(path, "png"));

            using var loaded = new Pixbuf(path);
            Assert.Equal(4, loaded.Width);
            Assert.Equal(3, loaded.Height);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void LoadFromFileWithSize_ScalesTheImage()
    {
        string path = TempPath();
        try
        {
            using (var original = CreatePixbuf())
                Assert.True(original.Save(path, "png"));

            using var scaled = new Pixbuf(path, 2, 2);
            Assert.Equal(2, scaled.Width);
            Assert.Equal(2, scaled.Height);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void SaveToStream_WritesPngBytes()
    {
        using var pixbuf = CreatePixbuf();
        using var output = new MemoryOutputStream();

        bool saved = pixbuf.SaveToStream(output, "png", null);

        Assert.True(saved);
        Assert.True(output.DataSize > 0);
    }

    [Fact]
    public void SaveToStreamv_WritesPngBytes()
    {
        using var pixbuf = CreatePixbuf();
        using var output = new MemoryOutputStream();

        bool saved = pixbuf.SaveToStreamv(
            output,
            "png",
            Array.Empty<string>(),
            Array.Empty<string>(),
            null);

        Assert.True(saved);
        Assert.True(output.DataSize > 0);
    }

    [Fact]
    public void Savev_WritesAFile()
    {
        string path = TempPath();
        try
        {
            using var pixbuf = CreatePixbuf();

            bool saved = pixbuf.Savev(path, "png", Array.Empty<string>(), Array.Empty<string>());

            Assert.True(saved);
            Assert.True(File.Exists(path));
            Assert.True(new System.IO.FileInfo(path).Length > 0);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void Formats_IncludePng()
    {
        Assert.Contains(Pixbuf.Formats, format => format.Name == "png");
    }
}
