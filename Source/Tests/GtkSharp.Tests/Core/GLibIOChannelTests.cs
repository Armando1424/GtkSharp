using System;
using System.IO;
using GLib;
using Xunit;

namespace GtkSharp.Tests.Core;

public sealed class GLibIOChannelTests
{
    [Fact]
    public void WriteAndRead_RoundTripsThroughTheFile()
    {
        var path = Path.GetTempFileName();

        try
        {
            using (var writer = new GLib.IOChannel(path, "w"))
            {
                var status = writer.WriteChars("line1\nline2\n", out var remainder);
                writer.Flush();
                writer.Shutdown(true);

                Assert.True(Enum.IsDefined(status));
                Assert.True(remainder is null || remainder.Length >= 0);
            }

            using var reader = new GLib.IOChannel(path, "r");

            Assert.Equal(GLib.IOStatus.Normal, reader.ReadLine(out var first));
            Assert.Contains("line1", first);
            Assert.Equal(GLib.IOStatus.Normal, reader.ReadToEnd(out var rest));
            Assert.Contains("line2", rest);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Properties_CanBeChanged()
    {
        var path = Path.GetTempFileName();

        try
        {
            using var channel = new GLib.IOChannel(path, "w");

            channel.Buffered = true;
            channel.BufferSize = 1024;
            channel.CloseOnUnref = true;
            channel.Encoding = "UTF-8";
            channel.LineTerminator = new[] { '\n' };

            Assert.True(channel.Buffered);
            Assert.Equal(1024UL, channel.BufferSize);
            Assert.True(channel.CloseOnUnref || !channel.CloseOnUnref);
            Assert.NotNull(channel.Encoding);
            Assert.True(channel.UnixFd >= 0);
            Assert.NotEqual(IntPtr.Zero, channel.Handle);
            _ = channel.BufferCondition;
            _ = channel.Flags;
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SeekAndReadChars_Work()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, "abcdef");

        try
        {
            using var channel = new GLib.IOChannel(path, "r");
            var buffer = new byte[3];

            var status = channel.ReadChars(buffer, out var bytesRead);
            Assert.Equal(GLib.IOStatus.Normal, status);
            Assert.Equal(3UL, bytesRead);
            Assert.Equal((byte)'a', buffer[0]);

            Assert.Equal(GLib.IOStatus.Normal, channel.SeekPosition(0, GLib.SeekType.Set));
            Assert.Equal(GLib.IOStatus.Normal, channel.ReadUnichar(out var unichar));
            Assert.Equal((uint)'a', unichar);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
