using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class DialogTests
{
    [GtkFact]
    public void MessageDialog_ExposesTypeAndText()
    {
        using var dialog = new MessageDialog(
            null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, "Everything went fine");

        Assert.Equal(MessageType.Info, dialog.MessageType);
        Assert.Equal("Everything went fine", dialog.Text);
    }

    [GtkFact]
    public void MessageDialog_SecondaryText_RoundTrips()
    {
        using var dialog = new MessageDialog(
            null, DialogFlags.Modal, MessageType.Warning, ButtonsType.Ok, "Title");

        dialog.SecondaryText = "More details";

        Assert.Equal("More details", dialog.SecondaryText);
    }

    [GtkFact]
    public void MessageDialog_AddButton_ReturnsAButton()
    {
        using var dialog = new MessageDialog(
            null, DialogFlags.Modal, MessageType.Question, ButtonsType.None, "Question");

        var button = dialog.AddButton("Cancel", ResponseType.Cancel);

        Assert.NotNull(button);
    }

    [GtkFact]
    public void Dialog_ActionArea_IsAvailable()
    {
        using var dialog = new MessageDialog(
            null, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, "Text");

        Assert.NotNull(dialog.ActionArea);
    }

    [GtkFact]
    public void Dialog_DefaultResponse_RoundTrips()
    {
        using var dialog = new MessageDialog(
            null, DialogFlags.Modal, MessageType.Info, ButtonsType.OkCancel, "Text");

        var exception = Record.Exception(() => dialog.DefaultResponse = ResponseType.Ok);

        Assert.Null(exception);
    }

    [GtkFact]
    public void FileChooserDialog_ExposesActionAndTitle()
    {
        using var dialog = new FileChooserDialog(
            "Open file", null, FileChooserAction.Open,
            "Cancel", ResponseType.Cancel,
            "Open", ResponseType.Accept);

        Assert.Equal(FileChooserAction.Open, dialog.Action);
        Assert.Equal("Open file", dialog.Title);
    }

    [GtkFact]
    public void AboutDialog_ProgramInformation_RoundTrips()
    {
        using var dialog = new AboutDialog();

        dialog.ProgramName = "GtkSharp Test";
        dialog.Version = "1.0";
        dialog.Comments = "Testing";

        Assert.Equal("GtkSharp Test", dialog.ProgramName);
        Assert.Equal("1.0", dialog.Version);
        Assert.Equal("Testing", dialog.Comments);
    }
}
