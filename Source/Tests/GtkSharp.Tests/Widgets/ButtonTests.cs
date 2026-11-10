using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

[Collection(GtkCollection.Name)]
public sealed class ButtonTests
{
    [GtkFact]
    public void NewWithLabel_SetsLabel()
    {
        using var button = Button.NewWithLabel("Save");

        Assert.Equal("Save", button.Label);
    }

    [GtkFact]
    public void Label_CanBeChangedAfterCreation()
    {
        using var button = Button.NewWithLabel("Save");

        button.Label = "Guardar";

        Assert.Equal("Guardar", button.Label);
    }

    [GtkFact]
    public void Relief_RoundTrips()
    {
        using var button = Button.NewWithLabel("Save");

        // GTK 3.24 maps ReliefStyle.Half to Normal, so None is the only
        // non-default value that is guaranteed to round-trip.
        button.Relief = ReliefStyle.None;

        Assert.Equal(ReliefStyle.None, button.Relief);
    }

    [GtkFact]
    public void UseUnderline_RoundTrips()
    {
        using var button = Button.NewWithLabel("_Save");

        button.UseUnderline = true;

        Assert.True(button.UseUnderline);
    }

    [GtkFact]
    public void TooltipText_RoundTrips()
    {
        using var button = Button.NewWithLabel("Save");

        button.TooltipText = "Guarda el documento";

        Assert.Equal("Guarda el documento", button.TooltipText);
    }

    [GtkFact]
    public void Name_RoundTrips()
    {
        using var button = Button.NewWithLabel("Save");

        button.Name = "save-button";

        Assert.Equal("save-button", button.Name);
    }

    [GtkFact]
    public void Sensitive_RoundTrips()
    {
        using var button = Button.NewWithLabel("Save");

        button.Sensitive = false;

        Assert.False(button.Sensitive);
    }
}
