using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

/// <summary>
/// Behaviour of the hand-written <see cref="TextBuffer"/> helpers on top of the
/// generated GTK binding.
/// </summary>
[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class TextBufferTests
{
    [GtkFact]
    public void Text_SetAndGet_RoundTrips()
    {
        using var buffer = new TextBuffer(null!);

        buffer.Text = "hola";

        Assert.Equal("hola", buffer.Text);
    }

    [GtkFact]
    public void Insert_AtEnd_AppendsText()
    {
        using var buffer = new TextBuffer(null!);
        buffer.Text = "hola";
        TextIter iter = buffer.EndIter;

        buffer.Insert(ref iter, " mundo");

        Assert.Equal("hola mundo", buffer.Text);
    }

    [GtkFact]
    public void InsertAtCursor_AfterPlacingCursorAtEnd_AppendsText()
    {
        using var buffer = new TextBuffer(null!);
        buffer.Text = "hola";
        buffer.PlaceCursor(buffer.EndIter);

        buffer.InsertAtCursor("!");

        Assert.Equal("hola!", buffer.Text);
    }

    [GtkFact]
    public void Clear_RemovesAllText()
    {
        using var buffer = new TextBuffer(null!);
        buffer.Text = "hola";

        buffer.Clear();

        Assert.Equal(string.Empty, buffer.Text);
    }

    [GtkFact]
    public void InsertInteractive_WhenAllowed_InsertsText()
    {
        using var buffer = new TextBuffer(null!);
        buffer.Text = "a";
        TextIter iter = buffer.EndIter;

        bool inserted = buffer.InsertInteractive(ref iter, "b", true);

        Assert.True(inserted);
        Assert.Equal("ab", buffer.Text);
    }

    [GtkFact]
    public void InsertInteractiveAtCursor_WhenAllowed_InsertsText()
    {
        using var buffer = new TextBuffer(null!);
        buffer.Text = "a";
        buffer.PlaceCursor(buffer.EndIter);

        bool inserted = buffer.InsertInteractiveAtCursor("b", true);

        Assert.True(inserted);
        Assert.Equal("ab", buffer.Text);
    }
}
