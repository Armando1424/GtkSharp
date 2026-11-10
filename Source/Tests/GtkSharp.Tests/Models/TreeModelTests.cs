using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Models;

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class TreeModelTests
{
    private static object? Read(Gtk.ITreeModel model, TreeIter iter, int column)
    {
        var value = GLib.Value.Empty;
        model.GetValue(iter, column, ref value);
        var result = value.Val;
        value.Dispose();
        return result;
    }

    private static ListStore CreateIntStore(params int[] values)
    {
        var store = new ListStore(GLib.GType.Int);
        foreach (var value in values)
            store.AppendValues(value);
        return store;
    }

    [GtkFact]
    public void TreePath_Depth_MatchesSegments()
    {
        using var path = new TreePath("1:2:3");

        Assert.Equal(3, path.Depth);
    }

    [GtkFact]
    public void TreePath_RoundTripsThroughString()
    {
        using var path = new TreePath("0:4:1");

        Assert.Equal("0:4:1", path.ToString());
    }

    [GtkFact]
    public void TreePath_Indices_ExposeSegments()
    {
        using var path = new TreePath("2:7");

        Assert.Equal(new[] { 2, 7 }, path.Indices);
    }

    [GtkFact]
    public void TreeModelSort_SortsByColumn()
    {
        using var store = CreateIntStore(3, 1, 2);
        using var sort = new TreeModelSort(store);

        sort.SetSortColumnId(0, SortType.Ascending);

        Assert.True(sort.GetIterFirst(out var iter));
        Assert.Equal(1, Read(sort, iter, 0));
    }

    [GtkFact]
    public void TreeModelSort_ConvertChildIterToIter_KeepsTheValue()
    {
        using var store = CreateIntStore(5);
        using var sort = new TreeModelSort(store);
        sort.SetSortColumnId(0, SortType.Ascending);

        Assert.True(store.GetIterFirst(out var childIter));
        Assert.True(sort.ConvertChildIterToIter(out var sortIter, childIter));

        Assert.Equal(5, Read(sort, sortIter, 0));
    }

    [GtkFact]
    public void TreeModelSort_ConvertIterToChildIter_KeepsTheValue()
    {
        using var store = CreateIntStore(9);
        using var sort = new TreeModelSort(store);
        sort.SetSortColumnId(0, SortType.Ascending);

        Assert.True(sort.GetIterFirst(out var sortIter));
        var childIter = sort.ConvertIterToChildIter(sortIter);

        Assert.Equal(9, Read(store, childIter, 0));
    }

    [GtkFact]
    public void TreeModelFilter_HidesRows()
    {
        using var store = CreateIntStore(1, 2, 3, 4);
        using var filter = new TreeModelFilter(store, null);
        filter.VisibleFunc = (model, iter) => (int)Read(model, iter, 0)! % 2 == 0;

        filter.Refilter();

        var values = new List<int>();
        var valid = filter.GetIterFirst(out var iter);
        while (valid)
        {
            values.Add((int)Read(filter, iter, 0)!);
            valid = filter.IterNext(ref iter);
        }

        Assert.Equal(new[] { 2, 4 }, values);
    }

    [GtkFact]
    public void TreeModelFilter_CanShowEverything()
    {
        using var store = CreateIntStore(1, 2, 3);
        using var filter = new TreeModelFilter(store, null);
        filter.VisibleFunc = (_, _) => true;

        filter.Refilter();

        var count = 0;
        var valid = filter.GetIterFirst(out var iter);
        while (valid)
        {
            count++;
            valid = filter.IterNext(ref iter);
        }

        Assert.Equal(3, count);
    }
}
