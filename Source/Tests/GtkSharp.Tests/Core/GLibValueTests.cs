using Gtk;
using Xunit;

namespace GtkSharp.Tests.Core;

/// <summary>
/// Exercises GLib.Value boxing and unboxing without a display.
/// </summary>
public sealed class GLibValueTests
{
    [Fact]
    public void Int_RoundTrips()
    {
        using var value = new GLib.Value(42);

        Assert.Equal(42, value.Val);
    }

    [Fact]
    public void String_RoundTrips()
    {
        using var value = new GLib.Value("hello");

        Assert.Equal("hello", value.Val);
    }

    [Fact]
    public void Double_RoundTrips()
    {
        using var value = new GLib.Value(3.5);

        Assert.Equal(3.5, value.Val);
    }

    [Fact]
    public void Boolean_RoundTrips()
    {
        using var value = new GLib.Value(true);

        Assert.Equal(true, value.Val);
    }

    [Fact]
    public void DefaultValue_ForInt_IsZero()
    {
        using var value = new GLib.Value(GLib.GType.Int);

        Assert.Equal(0, value.Val);
    }

    [Fact]
    public void DefaultValue_ForString_IsEmpty()
    {
        using var value = new GLib.Value(GLib.GType.String);

        Assert.True(value.Val is null || (string)value.Val == string.Empty);
    }

    [Fact]
    public void ValueFromObject_BoxesTheUnderlyingType()
    {
        object boxed = 7;
        using var value = new GLib.Value(boxed);

        Assert.Equal(7, value.Val);
    }

    [Fact]
    public void UInt_RoundTrips()
    {
        using var value = new GLib.Value(4000000000u);

        Assert.Equal(4000000000u, value.Val);
    }

    [Fact]
    public void Float_RoundTrips()
    {
        using var value = new GLib.Value(1.25f);

        Assert.Equal(1.25f, value.Val);
    }

    [Fact]
    public void Long_RoundTrips()
    {
        using var value = new GLib.Value(9_000_000_000L);

        Assert.Equal(9_000_000_000L, value.Val);
    }
}

public sealed class GLibValueExtraTests
{
    [Fact]
    public void Byte_SByte_UShort_UInt_ULong_RoundTrip()
    {
        using var b = new GLib.Value((byte)200);
        using var sb = new GLib.Value((sbyte)-5);
        using var us = new GLib.Value((ushort)65000);
        using var ui = new GLib.Value(4_000_000_000u);
        using var ul = new GLib.Value(9_000_000_000UL);

        Assert.Equal((byte)200, (byte)b);
        Assert.Equal((sbyte)-5, (sbyte)sb);
        Assert.Equal((ushort)65000, (ushort)us);
        Assert.Equal(4_000_000_000u, (uint)ui);
        Assert.Equal(9_000_000_000UL, (ulong)ul);
    }

    [Fact]
    public void Pointer_RoundTrips()
    {
        var pointer = new IntPtr(0x1234);
        using var value = new GLib.Value(pointer);

        Assert.Equal(pointer, (IntPtr)value);
    }

    [Fact]
    public void StringArray_RoundTrips()
    {
        using var value = new GLib.Value(new[] { "a", "b" });
        var result = (string[])value;

        Assert.Equal(new[] { "a", "b" }, result);
    }

    [Fact]
    public void Variant_RoundTrips()
    {
        using var variant = new GLib.Variant("hello");
        using var value = new GLib.Value(variant);

        Assert.Equal("hello", (string)(GLib.Variant)value);
    }

    [Fact]
    public void Enum_BoxedAsInt_CanBeRead()
    {
        using var value = new GLib.Value((int)DayOfWeek.Friday);

        Assert.Equal((int)DayOfWeek.Friday, (int)value);
    }
}
