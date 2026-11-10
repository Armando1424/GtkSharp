using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class StackAndExpanderTests
{
    [GtkFact]
    public void StackAddNamed_SetsTheVisibleChildName()
    {
        using var stack = new Stack();
        using var first = new Label("first");
        stack.AddNamed(first, "first");
        stack.ShowAll();

        stack.VisibleChildName = "first";

        Assert.Equal("first", stack.VisibleChildName);
    }

    [GtkFact]
    public void StackVisibleChildName_CanSwitchPages()
    {
        using var stack = new Stack();
        using var first = new Label("first");
        using var second = new Label("second");
        stack.AddNamed(first, "first");
        stack.AddNamed(second, "second");
        stack.ShowAll();

        stack.VisibleChildName = "second";

        Assert.Equal("second", stack.VisibleChildName);
    }

    [GtkFact]
    public void StackSetVisibleChildFull_ChangesThePage()
    {
        using var stack = new Stack();
        using var first = new Label("first");
        using var second = new Label("second");
        stack.AddNamed(first, "first");
        stack.AddNamed(second, "second");
        stack.ShowAll();

        stack.SetVisibleChildFull("first", StackTransitionType.None);

        Assert.Equal("first", stack.VisibleChildName);
    }

    [GtkFact]
    public void StackTransitionType_RoundTrips()
    {
        using var stack = new Stack();

        stack.TransitionType = StackTransitionType.Crossfade;

        Assert.Equal(StackTransitionType.Crossfade, stack.TransitionType);
    }

    [GtkFact]
    public void Expander_Label_RoundTrips()
    {
        using var expander = new Expander("More");

        Assert.Equal("More", expander.Label);
    }

    [GtkFact]
    public void Expander_CanChangeLabel()
    {
        using var expander = new Expander("More");

        expander.Label = "Less";

        Assert.Equal("Less", expander.Label);
    }

    [GtkFact]
    public void Expander_Expanded_RoundTrips()
    {
        using var expander = new Expander("More");

        expander.Expanded = true;

        Assert.True(expander.Expanded);
    }

    [GtkFact]
    public void Expander_AcceptsAChild()
    {
        using var expander = new Expander("More");
        using var child = new Label("content");

        expander.Add(child);

        Assert.Contains(child.Handle, expander.Children.Select(c => c.Handle));
    }
}
