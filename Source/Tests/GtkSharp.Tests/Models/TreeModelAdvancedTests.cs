using System.Linq;
using GLib;
using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Models;

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class TreeStoreAdvancedTests
{
    private static TreeStore Create() => new(GLib.GType.String, GLib.GType.Int, GLib.GType.Boolean);

    [GtkFact]
    public void NodeOperations_BuildAndRemoveChildren()
    {
        using var store = Create();

        var root = store.AppendNode();
        store.SetValue(root, 0, "root");
        store.SetValue(root, 1, 1);
        store.SetValue(root, 2, true);

        var first = store.AppendNode(root);
        store.SetValue(first, 0, "first");

        var prepended = store.PrependNode(root);
        store.SetValue(prepended, 0, "prepended");

        var inserted = store.InsertNode(root, 1);
        store.SetValue(inserted, 0, "inserted");

        Assert.Equal(3, store.IterNChildren(root));
        store.IterChildren(out _);
        store.IterNthChild(out _, 1);

        store.Remove(ref inserted);
        Assert.Equal(2, store.IterNChildren(root));
    }

    [GtkFact]
    public void SiblingInsertion_UsesTheSiblingPosition()
    {
        using var store = Create();
        var root = store.AppendValues("root");
        var first = store.AppendValues(root, "first");
        var second = store.AppendValues(root, "second");

        var before = store.InsertNodeBefore(root, second);
        store.SetValue(before, 0, "before");

        var after = store.InsertNodeAfter(root, first);
        store.SetValue(after, 0, "after");

        Assert.Equal(4, store.IterNChildren(root));
    }

    [GtkFact]
    public void SetValueOverloads_Work()
    {
        using var store = new TreeStore(GLib.GType.String, GLib.GType.Int, GLib.GType.UInt, GLib.GType.Double, GLib.GType.Float, GLib.GType.Boolean);
        var iter = store.AppendNode();

        store.SetValue(iter, 0, "text");
        store.SetValue(iter, 1, 7);
        store.SetValue(iter, 2, 8u);
        store.SetValue(iter, 3, 1.5);
        store.SetValue(iter, 4, 2.5f);
        store.SetValue(iter, 5, true);

        Assert.Equal("text", store.GetValue(iter, 0));
        Assert.Equal(7, store.GetValue(iter, 1));
        Assert.Equal(8u, store.GetValue(iter, 2));
        Assert.Equal(1.5, store.GetValue(iter, 3));
        Assert.Equal(2.5f, store.GetValue(iter, 4));
        Assert.Equal(true, store.GetValue(iter, 5));
    }

    [GtkFact]
    public void WalkingAndParentNavigation_Work()
    {
        using var store = Create();
        var root = store.AppendValues("root");
        var child = store.AppendValues(root, "child");

        Assert.True(store.IterParent(out var parent, child));
        Assert.True(store.IterIsValid(parent));
        Assert.True(store.IterHasChild(parent));
        Assert.Equal(1, store.IterDepth(child));
    }
}

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class ListStoreAdvancedTests
{
    [GtkFact]
    public void InsertSetAndRemove_RoundTrip()
    {
        using var store = new ListStore(GLib.GType.String, GLib.GType.Int, GLib.GType.Int64);

        var first = store.AppendValues("first", 1, 10L);
        var second = store.InsertWithValues(0, "second", 2, 20L);

        Assert.Equal(2, store.IterNChildren());
        Assert.Equal("second", store.GetValue(second, 0));

        store.SetValues(first, "updated", 3, 30L);
        Assert.Equal("updated", store.GetValue(first, 0));

        store.SetValue(first, 1, 4);
        store.SetValue(first, 2, 40L);
        Assert.Equal(4, store.GetValue(first, 1));
        Assert.Equal(40L, store.GetValue(first, 2));

        store.Remove(ref second);
        Assert.Equal(1, store.IterNChildren());
    }

    [GtkFact]
    public void EnumeratorAndChildren_WalkTheRows()
    {
        using var store = new ListStore(GLib.GType.String);
        store.AppendValues("a");
        store.AppendValues("b");

        Assert.Equal(2, store.Cast<object>().Count());
        Assert.True(store.IterChildren(out var iter));
        Assert.True(store.IterNthChild(out var second, 1));
        Assert.Equal("b", store.GetValue(second, 0));

        store.Clear();
        Assert.Equal(0, store.IterNChildren());
    }

    [GtkFact]
    public void SetValueOverloads_Work()
    {
        using var store = new ListStore(GLib.GType.Boolean, GLib.GType.Double, GLib.GType.Float, GLib.GType.UInt);
        var iter = store.AppendValues(false, 0.0, 0.0f, 0u);

        store.SetValue(iter, 0, true);
        store.SetValue(iter, 1, 1.25);
        store.SetValue(iter, 2, 2.5f);
        store.SetValue(iter, 3, 3u);

        Assert.Equal(true, store.GetValue(iter, 0));
        Assert.Equal(1.25, store.GetValue(iter, 1));
        Assert.Equal(2.5f, store.GetValue(iter, 2));
        Assert.Equal(3u, store.GetValue(iter, 3));
    }
}
