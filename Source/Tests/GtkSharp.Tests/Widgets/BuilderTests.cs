using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Widgets;

[Collection(GtkCollection.Name)]
[Trait("Category", "UI")]
public sealed class BuilderTests
{
    private const string SimpleButton = """
        <interface>
          <object class="GtkButton" id="btn">
            <property name="label">Hello</property>
          </object>
        </interface>
        """;

    private const string WindowWithChild = """
        <interface>
          <object class="GtkWindow" id="win">
            <property name="title">Built</property>
            <child>
              <object class="GtkButton" id="child">
                <property name="label">Inside</property>
              </object>
            </child>
          </object>
        </interface>
        """;

    [GtkFact]
    public void AddFromString_ValidXml_ReturnsPositive()
    {
        using var builder = new Builder();

        var result = builder.AddFromString(SimpleButton);

        // gtk_builder_add_from_string returns a positive value on success.
        Assert.True(result > 0);
    }

    [GtkFact]
    public void AddFromString_InvalidXml_ThrowsGException()
    {
        using var builder = new Builder();

        Assert.Throws<GLib.GException>(() =>
            builder.AddFromString("""<interface><object class="NoSuchWidget" id="x"/></interface>"""));
    }

    [GtkFact]
    public void GetObject_ReturnsTypedWidget()
    {
        using var builder = new Builder();
        builder.AddFromString(SimpleButton);

        var button = builder.GetObject("btn") as Button;

        Assert.NotNull(button);
        Assert.Equal("Hello", button!.Label);
    }

    [GtkFact]
    public void GetObject_UnknownId_ReturnsNull()
    {
        using var builder = new Builder();
        builder.AddFromString(SimpleButton);

        Assert.Null(builder.GetObject("missing"));
    }

    [GtkFact]
    public void Builder_BuildsWindowHierarchy()
    {
        using var builder = new Builder();
        builder.AddFromString(WindowWithChild);

        var window = builder.GetObject("win") as Window;
        var child = builder.GetObject("child") as Button;

        Assert.NotNull(window);
        Assert.NotNull(child);
        Assert.Equal("Built", window!.Title);
        Assert.Contains(child!.Handle, window.Children.Select(c => c.Handle));
    }

    [GtkFact]
    public void AddFromString_CanBeCalledTwice()
    {
        using var builder = new Builder();

        Assert.True(builder.AddFromString(SimpleButton) > 0);
        Assert.True(builder.AddFromString("""
            <interface>
              <object class="GtkLabel" id="label">
                <property name="label">Second</property>
              </object>
            </interface>
            """) > 0);
    }

    [GtkFact]
    public void GetObject_AfterSecondAdd_ReturnsBothWidgets()
    {
        using var builder = new Builder();
        builder.AddFromString(SimpleButton);
        builder.AddFromString("""
            <interface>
              <object class="GtkLabel" id="label">
                <property name="label">Second</property>
              </object>
            </interface>
            """);

        Assert.IsType<Button>(builder.GetObject("btn"));
        Assert.IsType<Label>(builder.GetObject("label"));
    }
}
