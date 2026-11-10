using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class WindowTests
{
    [GtkFact]
    public void Title_RoundTrips()
    {
        using var window = new Window("initial");

        window.Title = "Documento";

        Assert.Equal("Documento", window.Title);
    }

    [GtkFact]
    public void Title_RoundTripsUnicode()
    {
        using var window = new Window("initial");

        window.Title = "Añadir — 日本語 🌍";

        Assert.Equal("Añadir — 日本語 🌍", window.Title);
    }

    [GtkFact]
    public void DefaultWidthAndHeight_RoundTrip()
    {
        using var window = new Window("sizes");

        window.DefaultWidth = 1024;
        window.DefaultHeight = 768;

        Assert.Equal(1024, window.DefaultWidth);
        Assert.Equal(768, window.DefaultHeight);
    }

    [GtkFact]
    public void SetDefaultSize_UpdatesTheDefaultSize()
    {
        using var window = new Window("sizes");

        window.SetDefaultSize(320, 240);

        Assert.Equal(320, window.DefaultWidth);
        Assert.Equal(240, window.DefaultHeight);
    }

    [GtkFact]
    public void WindowPosition_RoundTrips()
    {
        using var window = new Window("position");

        window.WindowPosition = WindowPosition.Center;

        Assert.Equal(WindowPosition.Center, window.WindowPosition);
    }

    [GtkFact]
    public void ShowAndHide_TogglesVisibility()
    {
        using var window = new Window("visibility");

        window.Show();
        Assert.True(window.Visible);

        window.Hide();
        Assert.False(window.Visible);
    }
}
