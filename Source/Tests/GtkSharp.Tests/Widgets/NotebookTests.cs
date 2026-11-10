using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class NotebookTests
{
    [GtkFact]
    public void AppendPage_IncreasesPageCount()
    {
        using var notebook = new Notebook();
        using var page = new Label("page");
        using var tab = new Label("tab");

        notebook.AppendPage(page, tab);

        Assert.Equal(1, notebook.NPages);
    }

    [GtkFact]
    public void AppendPage_ReturnsSequentialIndexes()
    {
        using var notebook = new Notebook();
        using var first = new Label("first");
        using var second = new Label("second");

        var firstIndex = notebook.AppendPage(first, new Label("tab"));
        var secondIndex = notebook.AppendPage(second, new Label("tab"));

        Assert.Equal(0, firstIndex);
        Assert.Equal(1, secondIndex);
    }

    [GtkFact]
    public void PageNum_ReturnsThePageIndex()
    {
        using var notebook = new Notebook();
        using var first = new Label("first");
        using var second = new Label("second");
        notebook.AppendPage(first, new Label("tab"));
        notebook.AppendPage(second, new Label("tab"));

        Assert.Equal(1, notebook.PageNum(second));
    }

    [GtkFact]
    public void CurrentPage_CanBeChanged()
    {
        using var notebook = new Notebook();
        notebook.AppendPage(new Label("first"), new Label("tab"));
        notebook.AppendPage(new Label("second"), new Label("tab"));
        notebook.ShowAll();

        notebook.CurrentPage = 1;

        Assert.Equal(1, notebook.CurrentPage);
    }

    [GtkFact]
    public void CurrentPageWidget_MatchesCurrentPage()
    {
        using var notebook = new Notebook();
        using var first = new Label("first");
        using var second = new Label("second");
        notebook.AppendPage(first, new Label("tab"));
        notebook.AppendPage(second, new Label("tab"));
        notebook.ShowAll();

        notebook.CurrentPage = 1;

        Assert.Equal(second.Handle, notebook.CurrentPageWidget.Handle);
    }

    [GtkFact]
    public void RemovePage_DecreasesPageCount()
    {
        using var notebook = new Notebook();
        notebook.AppendPage(new Label("first"), new Label("tab"));
        notebook.AppendPage(new Label("second"), new Label("tab"));

        notebook.RemovePage(0);

        Assert.Equal(1, notebook.NPages);
    }

    [GtkFact]
    public void NextPage_AdvancesToTheNextPage()
    {
        using var notebook = new Notebook();
        notebook.AppendPage(new Label("first"), new Label("tab"));
        notebook.AppendPage(new Label("second"), new Label("tab"));
        notebook.ShowAll();
        notebook.CurrentPage = 0;

        notebook.NextPage();

        Assert.Equal(1, notebook.CurrentPage);
    }

    [GtkFact]
    public void PrevPage_GoesBackToThePreviousPage()
    {
        using var notebook = new Notebook();
        notebook.AppendPage(new Label("first"), new Label("tab"));
        notebook.AppendPage(new Label("second"), new Label("tab"));
        notebook.ShowAll();
        notebook.CurrentPage = 1;

        notebook.PrevPage();

        Assert.Equal(0, notebook.CurrentPage);
    }
}
