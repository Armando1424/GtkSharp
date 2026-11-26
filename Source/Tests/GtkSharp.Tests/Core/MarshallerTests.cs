using System;
using GLib;
using Xunit;

namespace GtkSharp.Tests.Core;

/// <summary>
/// Round-trips the public <see cref="Marshaller"/> helpers used by the
/// generated bindings for strings, string arrays and byte arrays.
/// </summary>
public sealed class MarshallerTests
{
    [Fact]
    public void StringPtr_RoundTripsUtf8()
    {
        IntPtr ptr = Marshaller.StringToPtrGStrdup("hola 🌍");
        try
        {
            Assert.Equal("hola 🌍", Marshaller.Utf8PtrToString(ptr));
        }
        finally
        {
            Marshaller.Free(ptr);
        }
    }

    [Fact]
    public void Strv_RoundTripsAndFrees()
    {
        string[] values = { "uno", "dos", "tres" };
        IntPtr ptr = Marshaller.StringArrayToStrvPtr(values);

        Assert.Equal(values, Marshaller.PtrToStringArrayGFree(ptr));
    }

    [Fact]
    public void NullTerminatedPointerArray_RoundTrips()
    {
        string[] values = { "a", "b" };
        IntPtr[] pointers = Marshaller.StringArrayToNullTermPointer(values);
        try
        {
            Assert.Equal(values, Marshaller.Utf8PtrToString(pointers));
        }
        finally
        {
            foreach (IntPtr pointer in pointers)
                if (pointer != IntPtr.Zero)
                    Marshaller.Free(pointer);
        }
    }

    [Fact]
    public void GUnicharConversions_RoundTrip()
    {
        Assert.Equal('ñ', Marshaller.GUnicharToChar(0x00F1));
        Assert.Equal("ñ", Marshaller.GUnicharToString(0x00F1));
        Assert.Equal(0x00F1u, Marshaller.CharToGUnichar('ñ'));
    }

    [Fact]
    public void FilenamePtr_RoundTrips()
    {
        IntPtr ptr = Marshaller.StringToFilenamePtr("/tmp/archivo.txt");
        try
        {
            Assert.Equal("/tmp/archivo.txt", Marshaller.FilenamePtrToString(ptr));
        }
        finally
        {
            Marshaller.Free(ptr);
        }
    }

    [Fact]
    public void StringFormat_FormatsAndEscapesPercent()
    {
        Assert.Equal("x=3", Marshaller.StringFormat("x={0}", 3));
        Assert.Equal("100%% de 3", Marshaller.StringFormat("100% de {0}", 3));
    }

    [Fact]
    public void ByteArrayPtr_RoundTrips()
    {
        byte[] values = { 1, 2, 3, 4 };
        IntPtr ptr = Marshaller.ArrayToArrayPtr(values);
        try
        {
            Assert.Equal(values, Marshaller.ArrayPtrToArray<byte>(ptr, values.Length, false));
        }
        finally
        {
            Marshaller.Free(ptr);
        }
    }

    [Fact]
    public void MallocAndFree_Work()
    {
        IntPtr ptr = Marshaller.Malloc(16);

        Assert.NotEqual(IntPtr.Zero, ptr);

        Marshaller.Free(ptr);
    }
}
