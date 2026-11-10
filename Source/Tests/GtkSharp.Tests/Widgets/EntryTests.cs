using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

[Collection(GtkCollection.Name)]
public sealed class EntryTests
{
    [GtkFact]
    public void Text_RoundTrips()
    {
        using var entry = new Entry();

        entry.Text = "hello";

        Assert.Equal("hello", entry.Text);
    }

    [GtkFact]
    public void Text_RoundTripsUnicode()
    {
        using var entry = new Entry();

        entry.Text = "Hola, señor 🌍 こんにちは";

        Assert.Equal("Hola, señor 🌍 こんにちは", entry.Text);
    }

    [GtkFact]
    public void Text_EmptyString_RoundTrips()
    {
        using var entry = new Entry { Text = "something" };

        entry.Text = string.Empty;

        Assert.Equal(string.Empty, entry.Text);
    }

    [GtkFact]
    public void Visibility_RoundTrips()
    {
        using var entry = new Entry();

        entry.Visibility = false;

        Assert.False(entry.Visibility);
    }

    [GtkFact]
    public void MaxLength_TruncatesAssignedText()
    {
        using var entry = new Entry { MaxLength = 5 };

        entry.Text = "123456789";

        Assert.Equal("12345", entry.Text);
    }

    [GtkFact]
    public void WidthChars_RoundTrips()
    {
        using var entry = new Entry();

        entry.WidthChars = 24;

        Assert.Equal(24, entry.WidthChars);
    }

    [GtkFact]
    public void PlaceholderText_RoundTrips()
    {
        using var entry = new Entry();

        entry.PlaceholderText = "Escribe tu nombre";

        Assert.Equal("Escribe tu nombre", entry.PlaceholderText);
    }
}
