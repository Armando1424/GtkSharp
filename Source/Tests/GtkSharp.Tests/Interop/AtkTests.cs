using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Interop;

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class AtkTests
{
    [GtkFact]
    public void Widget_ExposesAnAccessible()
    {
        using var label = new Label("accessible");

        Assert.NotNull(label.Accessible);
    }

    [GtkFact]
    public void Accessible_Name_RoundTrips()
    {
        using var label = new Label("accessible");

        label.Accessible.Name = "My label";

        Assert.Equal("My label", label.Accessible.Name);
    }

    [GtkFact]
    public void Accessible_Description_RoundTrips()
    {
        using var button = Button.NewWithLabel("ok");

        button.Accessible.Description = "Confirms the dialog";

        Assert.Equal("Confirms the dialog", button.Accessible.Description);
    }

    [GtkFact]
    public void Accessible_Role_IsValid()
    {
        using var button = Button.NewWithLabel("ok");

        Assert.NotEqual(Atk.Role.Invalid, button.Accessible.Role);
    }
}
