using System.Linq;
using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

/// <summary>
/// Container relationships are asserted on native handles rather than on
/// managed references: GtkSharp may legitimately hand back a different wrapper
/// instance for the same native object.
/// </summary>
[Collection(GtkCollection.Name)]
public sealed class ContainerTests
{
    [GtkFact]
    public void WindowAdd_SetsChildParent()
    {
        using var window = new Window("container");
        using var button = Button.NewWithLabel("ok");

        window.Add(button);

        Assert.NotNull(button.Parent);
        Assert.Equal(window.Handle, button.Parent!.Handle);
    }

    [GtkFact]
    public void WindowAdd_ExposesTheChild()
    {
        using var window = new Window("container");
        using var button = Button.NewWithLabel("ok");

        window.Add(button);

        Assert.Contains(button.Handle, window.Children.Select(child => child.Handle));
    }

    [GtkFact]
    public void WindowRemove_DetachesChild()
    {
        using var window = new Window("container");
        using var button = Button.NewWithLabel("ok");

        window.Add(button);
        window.Remove(button);

        Assert.Empty(window.Children);
    }

    [GtkFact]
    public void BoxPackStart_KeepsInsertionOrder()
    {
        using var box = new Box(Orientation.Horizontal, 0);
        using var first = new Label("first");
        using var second = new Label("second");

        box.PackStart(first, false, false, 0);
        box.PackStart(second, false, false, 0);

        var children = box.Children;

        Assert.Equal(2, children.Length);
        Assert.Equal(first.Handle, children[0].Handle);
        Assert.Equal(second.Handle, children[1].Handle);
    }

    [GtkFact]
    public void BoxPackEnd_PlacesWidgetLast()
    {
        using var box = new Box(Orientation.Horizontal, 0);
        using var first = new Label("first");
        using var second = new Label("second");

        box.PackStart(first, false, false, 0);
        box.PackEnd(second, false, false, 0);

        var children = box.Children;

        Assert.Equal(2, children.Length);
        Assert.Equal(first.Handle, children[0].Handle);
        Assert.Equal(second.Handle, children[1].Handle);
    }
}
