using System;
using System.Collections.Generic;
using GLib;
using Xunit;

namespace GtkSharp.Tests.Core;

public sealed class GLibVariantTests
{
    [Fact]
    public void ScalarConstructors_RoundTrip()
    {
        using var b = new GLib.Variant(true);
        using var by = new GLib.Variant((byte)1);
        using var s = new GLib.Variant((short)-2);
        using var us = new GLib.Variant((ushort)2);
        using var i = new GLib.Variant(3);
        using var ui = new GLib.Variant(4u);
        using var l = new GLib.Variant(5L);
        using var ul = new GLib.Variant(6UL);
        using var d = new GLib.Variant(1.5);

        Assert.True((bool)b);
        Assert.Equal((byte)1, (byte)by);
        Assert.Equal((short)-2, (short)s);
        Assert.Equal((ushort)2, (ushort)us);
        Assert.Equal(3, (int)i);
        Assert.Equal(4u, (uint)ui);
        Assert.Equal(5L, (long)l);
        Assert.Equal(6UL, (ulong)ul);
        Assert.Equal(1.5, (double)d);
    }

    [Fact]
    public void StringAndStringArray_RoundTrip()
    {
        using var text = new GLib.Variant("hello");
        Assert.Equal("hello", (string)text);

        using var array = new GLib.Variant(new[] { "a", "b" });
        var values = array.ToArray();
        Assert.Equal(2, values.Length);

        foreach (var value in values)
            value.Dispose();
    }

    [Fact]
    public void TupleArrayAndDictionary_Work()
    {
        using var first = new GLib.Variant(1);
        using var second = new GLib.Variant(2);

        using var tuple = GLib.Variant.NewTuple(new[] { first, second });
        Assert.True(tuple.ToArray().Length >= 2);

        using var array = GLib.Variant.NewArray(new[] { first, second });
        Assert.True(array.ToArray().Length >= 2);

        using var entry = GLib.Variant.NewDictEntry(new GLib.Variant("key"), new GLib.Variant("value"));
        Assert.NotNull(entry);

        using var dictionary = new GLib.Variant(new Dictionary<string, GLib.Variant>
        {
            ["key"] = new GLib.Variant("value"),
        });
        var asv = dictionary.ToAsv();
        Assert.True(asv.ContainsKey("key"));
    }

    [Fact]
    public void PrintAndType_ExposeTheValue()
    {
        using var variant = new GLib.Variant(42);

        Assert.NotNull(variant.Type);
        Assert.Contains("42", variant.Print(true));
        Assert.Contains("42", variant.Print(false));
        Assert.NotNull(GLib.Variant.NewVariant(variant));
    }

    [Fact]
    public void VariantType_ParsesAndInspects()
    {
        using var type = new GLib.VariantType("as");

        Assert.True(GLib.VariantType.StringIsValid("as"));
        Assert.False(GLib.VariantType.StringIsValid("not a type"));
        Assert.True(type.IsArray);
        Assert.False(type.IsBasic);
        Assert.True(type.IsContainer);
        Assert.True(type.IsDefinite);

        using var element = type.Element();
        Assert.True(element.IsBasic);

        using var copy = type.Copy();
        Assert.True(type.Equals(copy));
        Assert.Equal(type.GetHashCode(), copy.GetHashCode());
        Assert.Contains("as", type.ToString());

        using var basic = new GLib.VariantType("s");
        Assert.True(basic.IsBasic);
        Assert.True(type.IsSubtypeOf(type));
    }
}
