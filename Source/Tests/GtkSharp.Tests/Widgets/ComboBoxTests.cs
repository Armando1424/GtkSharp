using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class ComboBoxTests
{
    [GtkFact]
    public void AppendText_AddsAnItem()
    {
        using var combo = new ComboBoxText();

        combo.AppendText("One");
        combo.Active = 0;

        Assert.Equal("One", combo.ActiveText);
    }

    [GtkFact]
    public void Append_StoresIdAndText()
    {
        using var combo = new ComboBoxText();

        combo.Append("id-one", "One");
        combo.Active = 0;

        Assert.Equal("id-one", combo.ActiveId);
        Assert.Equal("One", combo.ActiveText);
    }

    [GtkFact]
    public void Active_DefaultsToNoSelection()
    {
        using var combo = new ComboBoxText();
        combo.AppendText("One");

        Assert.Equal(-1, combo.Active);
    }

    [GtkFact]
    public void ActiveId_AfterRemoveAll_IsEmpty()
    {
        using var combo = new ComboBoxText();
        combo.Append("id-one", "One");
        combo.Active = 0;

        combo.RemoveAll();

        Assert.True(string.IsNullOrEmpty(combo.ActiveId));
    }

    [GtkFact]
    public void Remove_RemovesTheItemAtPosition()
    {
        using var combo = new ComboBoxText();
        combo.AppendText("One");
        combo.AppendText("Two");

        combo.Remove(0);
        combo.Active = 0;

        Assert.Equal("Two", combo.ActiveText);
    }

    [GtkFact]
    public void PopupShown_IsFalseByDefault()
    {
        using var combo = new ComboBoxText();

        Assert.False(combo.PopupShown);
    }

    [GtkFact]
    public void EntryTextColumn_RoundTrips()
    {
        using var combo = new ComboBoxText();

        combo.EntryTextColumn = 0;

        Assert.Equal(0, combo.EntryTextColumn);
    }
}
