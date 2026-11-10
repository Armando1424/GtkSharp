using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

[Collection(GtkCollection.Name)]
public sealed class LabelTests
{
    [GtkFact]
    public void Text_RoundTrips()
    {
        using var label = new Label("hello");

        Assert.Equal("hello", label.Text);
    }

    [GtkFact]
    public void Text_RoundTripsUnicode()
    {
        using var label = new Label();

        label.Text = "Línea 1\nLínea 2 — ñandú 🐦";

        Assert.Equal("Línea 1\nLínea 2 — ñandú 🐦", label.Text);
    }

    [GtkFact]
    public void Markup_IsParsedIntoPlainText()
    {
        using var label = new Label();

        label.Markup = "<b>bold</b> and <i>italic</i>";

        Assert.Contains("bold", label.Text);
        Assert.Contains("italic", label.Text);
        Assert.DoesNotContain("<b>", label.Text);
    }

    [GtkFact]
    public void LineWrap_RoundTrips()
    {
        using var label = new Label("some long text");

        label.LineWrap = true;

        Assert.True(label.LineWrap);
    }

    [GtkFact]
    public void Justify_RoundTrips()
    {
        using var label = new Label("some text");

        label.Justify = Justification.Center;

        Assert.Equal(Justification.Center, label.Justify);
    }

    [GtkFact]
    public void Selectable_RoundTrips()
    {
        using var label = new Label("some text");

        label.Selectable = true;

        Assert.True(label.Selectable);
    }
}
