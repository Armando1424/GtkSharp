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
