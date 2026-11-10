using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class MenuTests
{
    [GtkFact]
    public void MenuItem_Label_RoundTrips()
    {
        using var item = new MenuItem("Open");

        Assert.Equal("Open", item.Label);
    }

    [GtkFact]
    public void MenuItem_Label_CanBeChanged()
    {
        using var item = new MenuItem("Open");

        item.Label = "Abrir";

        Assert.Equal("Abrir", item.Label);
    }

    [GtkFact]
    public void MenuItem_Activated_FiresOnActivateSignal()
    {
        using var item = new MenuItem("Open");
        var fired = false;
        item.Activated += (_, _) => fired = true;

        GLib.Signal.Emit(item, "activate");

        Assert.True(fired);
    }

    [GtkFact]
    public void Menu_Append_AddsTheItem()
    {
        using var menu = new Menu();
        using var item = new MenuItem("Open");

        menu.Append(item);

        Assert.Contains(item.Handle, menu.Children.Select(c => c.Handle));
    }

    [GtkFact]
    public void MenuBar_Append_AddsTheMenuItem()
    {
        using var bar = new MenuBar();
        using var item = new MenuItem("File");

        bar.Append(item);

        Assert.Contains(item.Handle, bar.Children.Select(c => c.Handle));
    }

    [GtkFact]
    public void MenuItem_Submenu_CanBeAssigned()
    {
        using var item = new MenuItem("File");
        using var menu = new Menu();

        item.Submenu = menu;

        Assert.NotNull(item.Submenu);
        Assert.Equal(menu.Handle, item.Submenu!.Handle);
    }

    [GtkFact]
    public void CheckMenuItem_Active_RoundTrips()
    {
        using var item = new CheckMenuItem("Show toolbar");

        item.Active = true;

        Assert.True(item.Active);
    }

    [GtkFact]
    public void SeparatorMenuItem_IsAMenuItem()
    {
        using var separator = new SeparatorMenuItem();

        Assert.IsAssignableFrom<MenuItem>(separator);
    }

    [GtkFact]
    public void MenuItem_Sensitive_RoundTrips()
    {
        using var item = new MenuItem("Open");

        item.Sensitive = false;

        Assert.False(item.Sensitive);
    }
}
