using System;
using System.IO;
using System.Runtime.InteropServices;
using GLib;
using Xunit;

namespace GtkSharp.Tests.Core;

/// <summary>
/// Behaviour of the hand-written <see cref="GioStream"/> adapter over GIO
/// streams. Everything here uses in-memory streams, so no display or files are
/// needed.
/// </summary>
public sealed class GioStreamTests
{
    private static byte[] ReadNative(IntPtr data, int length)
    {
        var bytes = new byte[length];
        Marshal.Copy(data, bytes, 0, length);
        return bytes;
    }

    [Fact]
    public void Write_ToMemoryOutputStream_StoresTheBytes()
    {
        using var output = new MemoryOutputStream();
        using var stream = new GioStream(output);
        byte[] payload = { 0x10, 0x20, 0x30 };

        stream.Write(payload, 0, payload.Length);

        Assert.Equal((ulong)payload.Length, output.DataSize);
        Assert.Equal(payload, ReadNative(output.Data, payload.Length));
    }

    [Fact]
    public void Write_WithOffset_StoresOnlyTheRequestedSlice()
    {
        using var output = new MemoryOutputStream();
        using var stream = new GioStream(output);
        byte[] payload = { 0x10, 0x20, 0x30, 0x40 };

        stream.Write(payload, 1, 2);

        Assert.Equal(2UL, output.DataSize);
        Assert.Equal(new byte[] { 0x20, 0x30 }, ReadNative(output.Data, 2));
    }

    [Fact]
    public void Read_FromMemoryInputStream_ReturnsTheBytes()
    {
        using var bytes = new Bytes(new byte[] { 4, 5, 6 });
        using var input = new MemoryInputStream(bytes);
        using var stream = new GioStream(input);
        var buffer = new byte[3];

        int read = stream.Read(buffer, 0, buffer.Length);

        Assert.Equal(3, read);
        Assert.Equal(new byte[] { 4, 5, 6 }, buffer);
    }

    [Fact]
    public void Read_WithOffset_CopiesIntoTheRequestedPosition()
    {
        using var bytes = new Bytes(new byte[] { 4, 5, 6 });
        using var input = new MemoryInputStream(bytes);
        using var stream = new GioStream(input);
        var buffer = new byte[5];
        buffer[0] = 0xAA;
        buffer[1] = 0xBB;

        int read = stream.Read(buffer, 2, 3);

        Assert.Equal(3, read);
        Assert.Equal(new byte[] { 0xAA, 0xBB, 4, 5, 6 }, buffer);
    }

    [Fact]
    public void SeekAndPosition_AllowRereadingFromTheStart()
    {
        using var bytes = new Bytes(new byte[] { 7, 8, 9 });
        using var input = new MemoryInputStream(bytes);
        using var stream = new GioStream(input);
        var first = new byte[2];

        Assert.True(stream.CanSeek);
        Assert.Equal(2, stream.Read(first, 0, first.Length));
        Assert.Equal(2, stream.Position);

        Assert.Equal(0, stream.Seek(0, SeekOrigin.Begin));

        var again = new byte[2];
        Assert.Equal(2, stream.Read(again, 0, again.Length));
        Assert.Equal(first, again);
    }

    [Fact]
    public void Read_OnAWriteOnlyStream_ThrowsNotSupported()
    {
        using var output = new MemoryOutputStream();
        using var stream = new GioStream(output);

        Assert.Throws<NotSupportedException>(() => stream.Read(new byte[1], 0, 1));
    }

    [Fact]
    public void Write_OnAReadOnlyStream_ThrowsNotSupported()
    {
        using var bytes = new Bytes(new byte[] { 1 });
        using var input = new MemoryInputStream(bytes);
        using var stream = new GioStream(input);

        Assert.Throws<NotSupportedException>(() => stream.Write(new byte[1], 0, 1));
    }

    [Fact]
    public void Read_WithNullBuffer_ThrowsArgumentNullException()
    {
        using var bytes = new Bytes(new byte[] { 1 });
        using var input = new MemoryInputStream(bytes);
        using var stream = new GioStream(input);

        Assert.Throws<ArgumentNullException>(() => stream.Read(null!, 0, 1));
    }

    [Fact]
    public void Read_WithNegativeOffset_ThrowsArgumentOutOfRange()
    {
        using var bytes = new Bytes(new byte[] { 1 });
        using var input = new MemoryInputStream(bytes);
        using var stream = new GioStream(input);

        Assert.Throws<ArgumentOutOfRangeException>(() => stream.Read(new byte[1], -1, 1));
    }

    [Fact]
    public void Close_ThenWrite_ThrowsObjectDisposed()
    {
        using var output = new MemoryOutputStream();
        using var stream = new GioStream(output);
        stream.Close();

        Assert.Throws<ObjectDisposedException>(() => stream.Write(new byte[1], 0, 1));
    }
}
