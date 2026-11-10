using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Models;

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class TreeStoreTests
{
    private static TreeStore CreateStore() => new(GLib.GType.String);

    private static object? Read(Gtk.ITreeModel model, TreeIter iter, int column)
    {
        var value = GLib.Value.Empty;
        model.GetValue(iter, column, ref value);
        var result = value.Val;
        value.Dispose();
        return result;
    }

    [GtkFact]
    public void AppendValues_AddsRootRow()
    {
        using var store = CreateStore();

        var root = store.AppendValues("root");

        Assert.True(store.IterIsValid(root));
        Assert.Equal("root", Read(store, root, 0));
    }

    [GtkFact]
    public void AppendValues_WithParent_CreatesChild()
    {
        using var store = CreateStore();
        var root = store.AppendValues("root");

        var child = store.AppendValues(root, "child");

        Assert.Equal("child", Read(store, child, 0));
        Assert.True(store.IterHasChild(root));
        Assert.Equal(1, store.IterNChildren(root));
    }

    [GtkFact]
    public void IterChildren_EnumeratesChildren()
    {
        using var store = CreateStore();
        var root = store.AppendValues("root");
        store.AppendValues(root, "one");
        store.AppendValues(root, "two");

        var count = 0;
        var valid = store.IterChildren(out var iter, root);
        while (valid)
        {
            count++;
            valid = store.IterNext(ref iter);
        }

        Assert.Equal(2, count);
    }

    [GtkFact]
    public void IterNChildren_OnEmptyStore_IsZero()
    {
        using var store = CreateStore();

        Assert.Equal(0, store.IterNChildren(TreeIter.Zero));
    }

    [GtkFact]
    public void IterHasChild_OnLeaf_IsFalse()
    {
        using var store = CreateStore();
        var root = store.AppendValues("root");

        Assert.False(store.IterHasChild(root));
    }

    [GtkFact]
    public void SetValue_UpdatesCell()
    {
        using var store = CreateStore();
        var root = store.AppendValues("root");

        store.SetValue(root, 0, "updated");

        Assert.Equal("updated", Read(store, root, 0));
    }

    [GtkFact]
    public void Remove_RemovesSubtree()
    {
        using var store = CreateStore();
        var root = store.AppendValues("root");
        store.AppendValues(root, "child");

        store.Remove(ref root);
        Assert.False(store.GetIterFirst(out _));
    }

    [GtkFact]
    public void Clear_RemovesEverything()
    {
        using var store = CreateStore();
        var root = store.AppendValues("root");
        store.AppendValues(root, "child");

        store.Clear();

        Assert.False(store.GetIterFirst(out _));
    }

    [GtkFact]
    public void MultipleColumns_RoundTrip()
    {
        using var store = new TreeStore(GLib.GType.String, GLib.GType.Int);

        var root = store.AppendValues("name", 42);

        Assert.Equal("name", Read(store, root, 0));
        Assert.Equal(42, Read(store, root, 1));
    }

    [GtkFact]
    public void IterNthChild_WithValidParent_ReturnsRequestedRow()
    {
        using var store = CreateStore();
        var root = store.AppendValues("root");
        store.AppendValues(root, "one");
        store.AppendValues(root, "two");

        Assert.True(store.IterNthChild(out var iter, root, 1));
        Assert.Equal("two", Read(store, iter, 0));
    }

    [GtkFact]
    public void IterNthChild_WithZeroParent_ReturnsTopLevelRow()
    {
        using var store = CreateStore();
        store.AppendValues("first");
        store.AppendValues("second");

        Assert.True(store.IterNthChild(out var iter, TreeIter.Zero, 1));
        Assert.Equal("second", Read(store, iter, 0));
    }

    [GtkFact]
    public void IterNChildren_WithZeroParent_CountsRoots()
    {
        using var store = CreateStore();
        var first = store.AppendValues("first");
        store.AppendValues("second");
        store.AppendValues(first, "child");

        Assert.Equal(2, store.IterNChildren(TreeIter.Zero));
        Assert.Equal(1, store.IterNChildren(first));
    }

    [GtkFact]
    public void IterParent_ReturnsTheParentRow()
    {
        using var store = CreateStore();
        var root = store.AppendValues("root");
        var child = store.AppendValues(root, "child");

        Assert.True(store.IterParent(out var parent, child));
        Assert.Equal("root", Read(store, parent, 0));
    }
}
