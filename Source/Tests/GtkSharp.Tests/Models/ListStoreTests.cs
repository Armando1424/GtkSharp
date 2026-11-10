using System.Linq;
using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Models;

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class ListStoreTests
{
    private static ListStore CreateStore() => new(GLib.GType.String, GLib.GType.Int);

    private static object? Read(Gtk.ITreeModel model, TreeIter iter, int column)
    {
        var value = GLib.Value.Empty;
        model.GetValue(iter, column, ref value);
        var result = value.Val;
        value.Dispose();
        return result;
    }

    [GtkFact]
    public void NewStore_HasNoRows()
    {
        using var store = CreateStore();

        Assert.False(store.GetIterFirst(out _));
    }

    [GtkFact]
    public void AppendValues_AddsRowWithValues()
    {
        using var store = CreateStore();

        var iter = store.AppendValues("first", 1);

        Assert.True(store.IterIsValid(iter));
        Assert.Equal("first", Read(store, iter, 0));
        Assert.Equal(1, Read(store, iter, 1));
    }

    [GtkFact]
    public void IterFirst_WalksAllRows()
    {
        using var store = CreateStore();
        store.AppendValues("a", 1);
        store.AppendValues("b", 2);
        store.AppendValues("c", 3);

        var count = 0;
        var valid = store.GetIterFirst(out var iter);
        while (valid)
        {
            count++;
            valid = store.IterNext(ref iter);
        }

        Assert.Equal(3, count);
    }

    [GtkFact]
    public void IterNthChild_WithZeroParent_ReturnsTopLevelRow()
    {
        using var store = CreateStore();
        store.AppendValues("a", 1);
        store.AppendValues("b", 2);

        Assert.True(store.IterNthChild(out var iter, TreeIter.Zero, 1));
        Assert.Equal("b", Read(store, iter, 0));
    }

    [GtkFact]
    public void IterNthChild_WithZeroParent_OutOfRange_ReturnsFalse()
    {
        using var store = CreateStore();
        store.AppendValues("a", 1);

        Assert.False(store.IterNthChild(out _, TreeIter.Zero, 5));
    }

    [GtkFact]
    public void IterNChildren_WithZeroParent_CountsTopLevelRows()
    {
        using var store = CreateStore();
        store.AppendValues("a", 1);
        store.AppendValues("b", 2);

        Assert.Equal(2, store.IterNChildren(TreeIter.Zero));
    }

    [GtkFact]
    public void IterChildren_WithZeroParent_EnumeratesTopLevelRows()
    {
        using var store = CreateStore();
        store.AppendValues("a", 1);
        store.AppendValues("b", 2);

        var count = 0;
        var valid = store.IterChildren(out var iter, TreeIter.Zero);
        while (valid)
        {
            count++;
            valid = store.IterNext(ref iter);
        }

        Assert.Equal(2, count);
    }

    [GtkFact]
    public void ConvenienceOverloads_WorkWithoutAParent()
    {
        using var store = CreateStore();
        store.AppendValues("a", 1);

        Assert.Equal(1, store.IterNChildren());
        Assert.True(store.IterNthChild(out var iter, 0));
        Assert.Equal("a", Read(store, iter, 0));
    }

    [GtkFact]
    public void GetIterFromString_OutOfRange_ReturnsFalse()
    {
        using var store = CreateStore();
        store.AppendValues("a", 1);

        Assert.False(store.GetIterFromString(out _, "5"));
    }

    [GtkFact]
    public void SetValue_UpdatesCell()
    {
        using var store = CreateStore();
        var iter = store.AppendValues("a", 1);

        store.SetValue(iter, 0, "z");

        Assert.Equal("z", Read(store, iter, 0));
    }

    [GtkFact]
    public void Remove_RemovesRow()
    {
        using var store = CreateStore();
        var iter = store.AppendValues("a", 1);
        store.AppendValues("b", 2);

        Assert.True(store.Remove(ref iter));
        Assert.True(store.GetIterFirst(out var next));
        Assert.Equal("b", Read(store, next, 0));
    }

    [GtkFact]
    public void Clear_RemovesAllRows()
    {
        using var store = CreateStore();
        store.AppendValues("a", 1);
        store.AppendValues("b", 2);

        store.Clear();

        Assert.False(store.GetIterFirst(out _));
    }

    [GtkFact]
    public void GetIterFromString_ReturnsRow()
    {
        using var store = CreateStore();
        store.AppendValues("a", 1);
        store.AppendValues("b", 2);

        Assert.True(store.GetIterFromString(out var iter, "1"));
        Assert.Equal("b", Read(store, iter, 0));
    }

    [GtkFact]
    public void ColumnTypes_AreExposed()
    {
        using var store = CreateStore();

        Assert.Equal(GLib.GType.String, store.GetColumnType(0));
        Assert.Equal(GLib.GType.Int, store.GetColumnType(1));
    }

    [GtkFact]
    public void NColumns_MatchesConstructor()
    {
        using var store = new ListStore(typeof(string), typeof(int), typeof(bool));

        Assert.Equal(3, store.NColumns);
    }

    [GtkFact]
    public void Model_CanBeEnumerated()
    {
        using var store = CreateStore();
        store.AppendValues("a", 1);
        store.AppendValues("b", 2);

        Assert.Equal(2, store.Cast<object>().Count());
    }
}
