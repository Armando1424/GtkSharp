using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Core;

/// <summary>
/// Round-trips primitive values through the native layer. These tests catch
/// marshalling bugs (string encoding, bool width, double precision, enum
/// conversion) that break consumers even when the public API is unchanged.
/// </summary>
[Collection(GtkCollection.Name)]
public sealed class MarshallingTests
{
    [GtkFact]
    public void String_WithEmoji_RoundTrips()
    {
        using var entry = new Entry();

        entry.Text = "Hola 🌍 👋";

        Assert.Equal("Hola 🌍 👋", entry.Text);
    }

    [GtkFact]
    public void String_WithCjk_RoundTrips()
    {
        using var entry = new Entry();

        entry.Text = "こんにちは世界";

        Assert.Equal("こんにちは世界", entry.Text);
    }

    [GtkFact]
    public void String_LongValue_RoundTrips()
    {
        using var label = new Label();
        var text = new string('x', 10_000) + " fin";

        label.Text = text;

        Assert.Equal(text, label.Text);
    }

    [GtkFact]
    public void String_EmptyValue_RoundTrips()
    {
        using var window = new Window("initial");

        window.Title = string.Empty;

        Assert.Equal(string.Empty, window.Title);
    }

    [GtkFact]
    public void Int_RoundTrips()
    {
        using var window = new Window("int");

        window.DefaultWidth = 1280;

        Assert.Equal(1280, window.DefaultWidth);
    }

    [GtkFact]
    public void Double_RoundTrips()
    {
        using var bar = new ProgressBar();

        bar.Fraction = 0.25;

        Assert.Equal(0.25, bar.Fraction, precision: 6);
    }

    [GtkFact]
    public void Bool_RoundTrips()
    {
        using var label = new Label("text");

        label.Selectable = true;

        Assert.True(label.Selectable);
    }

    [GtkFact]
    public void Enum_RoundTrips()
    {
        using var button = Button.NewWithLabel("ok");

        button.Relief = ReliefStyle.None;

        Assert.Equal(ReliefStyle.None, button.Relief);
    }
}
