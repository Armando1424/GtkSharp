using System;
using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Core;

/// <summary>
/// Verifies that managed events are wired to the native GTK signals and that
/// connecting/disconnecting handlers behaves correctly. This is the most
/// common source of regressions in binding changes.
/// </summary>
[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class SignalTests
{
    [GtkFact]
    public void ButtonClick_FiresClickedOnce()
    {
        using var button = Button.NewWithLabel("click");
        var count = 0;
        button.Clicked += (_, _) => count++;

        button.Click();

        Assert.Equal(1, count);
    }

    [GtkFact]
    public void ButtonClick_AfterHandlerIsRemoved_DoesNotInvokeIt()
    {
        using var button = Button.NewWithLabel("click");
        var count = 0;
        EventHandler handler = (_, _) => count++;

        button.Clicked += handler;
        button.Click();
        button.Clicked -= handler;
        button.Click();

        Assert.Equal(1, count);
    }

    [GtkFact]
    public void ButtonClick_EmittingTheSignalByName_InvokesManagedHandler()
    {
        using var button = Button.NewWithLabel("click");
        var fired = false;
        button.Clicked += (_, _) => fired = true;

        GLib.Signal.Emit(button, "clicked");

        Assert.True(fired);
    }

    [GtkFact]
    public void EntryChanged_FiresWhenTextIsSet()
    {
        using var entry = new Entry();
        var count = 0;
        entry.Changed += (_, _) => count++;

        entry.Text = "hola";

        Assert.Equal(1, count);
        Assert.Equal("hola", entry.Text);
    }

    [GtkFact]
    public void ToggleButtonClick_TogglesActiveAndFiresToggled()
    {
        using var toggle = new ToggleButton();
        var toggled = 0;
        toggle.Toggled += (_, _) => toggled++;

        Assert.False(toggle.Active);

        toggle.Click();

        Assert.True(toggle.Active);
        Assert.Equal(1, toggled);
    }

    [GtkFact]
    public void WidgetShown_FiresWhenWindowIsShown()
    {
        using var window = new Window("signals");
        var shown = false;
        window.Shown += (_, _) => shown = true;

        window.Show();

        Assert.True(shown);
    }
}
