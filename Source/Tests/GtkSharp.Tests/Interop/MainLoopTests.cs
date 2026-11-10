using Xunit;

namespace GtkSharp.Tests.Interop;

/// <summary>
/// Validates the GLib main loop plumbing. Callbacks are a critical interop
/// surface: a wrong delegate signature makes the process crash instead of
/// failing a test.
/// </summary>
public sealed class MainLoopTests
{
    [Fact]
    public void IdleHandler_RunsWhenTheMainLoopRuns()
    {
        var loop = new GLib.MainLoop();
        var ran = false;

        GLib.Idle.Add(() =>
        {
            ran = true;
            loop.Quit();
            return false;
        });

        loop.Run();

        Assert.True(ran);
    }

    [Fact]
    public void Idle_ReturnsNonZeroSourceId()
    {
        var id = GLib.Idle.Add(() => false);

        try
        {
            Assert.NotEqual(0u, id);
        }
        finally
        {
            GLib.Source.Remove(id);
        }
    }

    [Fact]
    public void MainContext_Default_IsAvailable()
    {
        var context = GLib.MainContext.Default;

        Assert.NotNull(context);
    }

    [Fact]
    public void MainLoop_CanBeCreated()
    {
        var loop = new GLib.MainLoop();

        Assert.NotNull(loop);
    }
}
