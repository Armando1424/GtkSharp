using Xunit;

namespace GtkSharp.Tests.Core;

/// <summary>
/// KeyFile is pure GLib: it validates string marshalling, list handling and
/// the byte[] round-trip of ToData/LoadFromData.
/// </summary>
public sealed class GLibKeyFileTests
{
    [Fact]
    public void SetString_GetString_RoundTrips()
    {
        using var keyFile = new GLib.KeyFile();

        keyFile.SetString("main", "name", "GtkSharp");
        keyFile.SetString("main", "other", "value");

        Assert.Equal("GtkSharp", keyFile.GetString("main", "name"));
        Assert.Equal("value", keyFile.GetString("main", "other"));
    }

    [Fact]
    public void SetInteger_GetInteger_RoundTrips()
    {
        using var keyFile = new GLib.KeyFile();

        keyFile.SetInteger("main", "count", 42);

        Assert.Equal(42, keyFile.GetInteger("main", "count"));
    }

    [Fact]
    public void SetBoolean_GetBoolean_RoundTrips()
    {
        using var keyFile = new GLib.KeyFile();

        keyFile.SetBoolean("main", "enabled", true);

        Assert.True(keyFile.GetBoolean("main", "enabled"));
    }

    [Fact]
    public void SetDouble_GetDouble_RoundTrips()
    {
        using var keyFile = new GLib.KeyFile();

        keyFile.SetDouble("main", "ratio", 0.75);

        Assert.Equal(0.75, keyFile.GetDouble("main", "ratio"), precision: 6);
    }

    [Fact]
    public void SetStringList_GetStringList_RoundTrips()
    {
        using var keyFile = new GLib.KeyFile();

        keyFile.SetStringList("main", "items", new[] { "a", "b", "c" });

        Assert.Equal(new[] { "a", "b", "c" }, keyFile.GetStringList("main", "items"));
    }

    [Fact]
    public void ToData_ProducesContentThatCanBeLoadedBack()
    {
        using var keyFile = new GLib.KeyFile();
        keyFile.SetString("main", "name", "GtkSharp");

        var data = keyFile.ToData();

        using var reloaded = new GLib.KeyFile();
        Assert.True(reloaded.LoadFromData(data, GLib.KeyFileFlags.None));
        Assert.Equal("GtkSharp", reloaded.GetString("main", "name"));
    }

    [Fact]
    public void HasGroup_AndHasKey_ReportExistence()
    {
        using var keyFile = new GLib.KeyFile();
        keyFile.SetString("main", "name", "GtkSharp");

        Assert.True(keyFile.HasGroup("main"));
        Assert.True(keyFile.HasKey("main", "name"));
        Assert.False(keyFile.HasGroup("missing"));
        Assert.False(keyFile.HasKey("main", "missing"));
    }
}
