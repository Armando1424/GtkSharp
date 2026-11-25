using System;
using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Models;

/// <summary>
/// Behaviour of the hand-written TreeModel adapters: filtering and sorting a
/// child model without touching the underlying store.
/// </summary>
[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class TreeModelFilterSortTests
{
    private static TreeStore CreateStore(params string[] values)
    {
        var store = new TreeStore(GLib.GType.String);
        foreach (string value in values)
            store.AppendValues(value);
        return store;
    }

    private static object? Read(ITreeModel model, TreeIter iter, int column)
    {
        var value = GLib.Value.Empty;
        model.GetValue(iter, column, ref value);
        var result = value.Val;
        value.Dispose();
        return result;
    }

    [GtkFact]
    public void Filter_WithVisibleFunc_ShowsOnlyMatchingRows()
    {
        using var store = CreateStore("keep", "drop");
        using var filter = new TreeModelFilter(store, null);

        filter.VisibleFunc = (model, iter) => (string?)Read(model, iter, 0) == "keep";
        filter.Refilter();

        Assert.Equal(1, filter.IterNChildren());
        Assert.True(filter.IterChildren(out var iter));
        Assert.Equal("keep", filter.GetValue(iter, 0));
    }

    [GtkFact]
    public void Filter_WithoutVisibleFunc_ShowsAllRows()
    {
        using var store = CreateStore("a", "b");
        using var filter = new TreeModelFilter(store, null);

        Assert.Equal(2, filter.IterNChildren());
        Assert.True(filter.IterNthChild(out var second, 1));
        Assert.Equal("b", filter.GetValue(second, 0));
    }

    [GtkFact]
    public void Filter_ConvertChildIterToIter_ReturnsZeroForHiddenRow()
    {
        using var store = CreateStore("keep", "drop");
        using var filter = new TreeModelFilter(store, null);
        filter.VisibleFunc = (model, iter) => (string?)Read(model, iter, 0) == "keep";
        filter.Refilter();

        Assert.True(store.IterNthChild(out var visibleChild, TreeIter.Zero, 0));
        Assert.True(store.IterNthChild(out var hiddenChild, TreeIter.Zero, 1));

        TreeIter visible = filter.ConvertChildIterToIter(visibleChild);
        TreeIter hidden = filter.ConvertChildIterToIter(hiddenChild);

        Assert.Equal("keep", filter.GetValue(visible, 0));
        Assert.True(hidden.Equals(TreeIter.Zero));
    }

    [GtkFact]
    public void Filter_SetValue_IsNotSupported()
    {
        using var store = CreateStore("a");
        using var filter = new TreeModelFilter(store, null);

        Assert.Throws<NotImplementedException>(() => filter.SetValue(TreeIter.Zero, 0, "x"));
    }

    [GtkFact]
    public void Sort_Ascending_OrdersRowsByColumn()
    {
        using var store = CreateStore("b", "a", "c");
        using var sort = new TreeModelSort(store);

        sort.SetSortColumnId(0, SortType.Ascending);

        Assert.Equal(3, sort.IterNChildren());
        Assert.True(sort.IterNthChild(out var first, 0));
        Assert.Equal("a", sort.GetValue(first, 0));
    }

    [GtkFact]
    public void Sort_Descending_OrdersRowsByColumn()
    {
        using var store = CreateStore("b", "a", "c");
        using var sort = new TreeModelSort(store);

        sort.SetSortColumnId(0, SortType.Descending);

        Assert.True(sort.IterNthChild(out var first, 0));
        Assert.Equal("c", sort.GetValue(first, 0));
    }

    [GtkFact]
    public void Sort_ConvertChildIterToIter_ReturnsTheSortedIter()
    {
        using var store = CreateStore("b", "a");
        using var sort = new TreeModelSort(store);
        sort.SetSortColumnId(0, SortType.Ascending);

        Assert.True(store.IterNthChild(out var child, TreeIter.Zero, 0));
        TreeIter sorted = sort.ConvertChildIterToIter(child);

        Assert.Equal("b", sort.GetValue(sorted, 0));
    }

    [GtkFact]
    public void Sort_SetValue_IsNotSupported()
    {
        using var store = CreateStore("a");
        using var sort = new TreeModelSort(store);

        Assert.Throws<NotImplementedException>(() => sort.SetValue(TreeIter.Zero, 0, "x"));
    }
}
