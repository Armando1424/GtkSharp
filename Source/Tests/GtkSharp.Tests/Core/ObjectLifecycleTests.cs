using System;
using Gtk;
using GtkSharp.Tests.Infrastructure;
using Xunit;

namespace GtkSharp.Tests.Core;

/// <summary>
/// Covers the managed wrapper lifetime: handles, dispose semantics and GType
/// registration. Leaks and premature finalization are frequent regressions.
/// </summary>
[Collection(GtkCollection.Name)]
public sealed class ObjectLifecycleTests
{
    [GtkFact]
    public void NewWidget_HasNonZeroHandle()
    {
        using var label = new Label("hello");

        Assert.NotEqual(IntPtr.Zero, label.Handle);
    }

    [GtkFact]
    public void Dispose_ReleasesTheHandle()
    {
        var label = new Label("hello");
        var originalHandle = label.Handle;

        label.Dispose();

        Assert.NotEqual(IntPtr.Zero, originalHandle);
        Assert.Equal(IntPtr.Zero, label.Handle);
    }

    [GtkFact]
    public void DisposeTwice_IsSafe()
    {
        var label = new Label("hello");

        label.Dispose();

        var exception = Record.Exception(label.Dispose);

        Assert.Null(exception);
    }

    [GtkFact]
    public void DistinctWidgets_HaveDistinctHandles()
    {
        using var first = new Label("first");
        using var second = new Label("second");

        Assert.NotEqual(first.Handle, second.Handle);
    }

    [GtkFact]
    public void GType_IsRegistered()
    {
        Assert.NotEqual(GLib.GType.Invalid, Label.GType);
        Assert.NotEqual(GLib.GType.Invalid, Button.GType);
        Assert.NotEqual(GLib.GType.Invalid, Window.GType);
    }

    [GtkFact]
    public void GType_IsStableAcrossCalls()
    {
        Assert.Equal(Label.GType, Label.GType);
    }
}
