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

// Extended coverage for the hand-written KeyFile wrapper.
public sealed class GLibKeyFileExtraTests
{
    private static GLib.KeyFile Create() => new();

    [Fact]
    public void Groups_And_Keys_AreListed()
    {
        using var keyFile = Create();
        keyFile.SetString("one", "a", "1");
        keyFile.SetString("one", "b", "2");
        keyFile.SetString("two", "c", "3");

        Assert.Equal(new[] { "one", "two" }, keyFile.Groups.OrderBy(g => g).ToArray());
        Assert.Equal(new[] { "a", "b" }, keyFile.GetKeys("one").OrderBy(k => k).ToArray());
    }

    [Fact]
    public void GetValue_ReturnsRawString()
    {
        using var keyFile = Create();
        keyFile.SetInteger("g", "k", 42);

        Assert.Equal("42", keyFile.GetValue("g", "k"));
    }

    [Fact]
    public void Int64_And_UInt64_RoundTrip()
    {
        using var keyFile = Create();
        keyFile.SetInt64("g", "i", long.MaxValue);
        keyFile.SetUInt64("g", "u", ulong.MaxValue);

        Assert.Equal(long.MaxValue, keyFile.GetInt64("g", "i"));
        Assert.Equal(ulong.MaxValue, keyFile.GetUInt64("g", "u"));
    }

    [Fact]
    public void Boolean_Integer_And_Double_Lists_RoundTrip()
    {
        using var keyFile = Create();
        keyFile.SetBooleanList("g", "b", new[] { true, false, true });
        keyFile.SetIntegerList("g", "i", new[] { 1, 2, 3 });
        keyFile.SetDoubleList("g", "d", new[] { 1.5, 2.5 });

        Assert.Equal(new[] { true, false, true }, keyFile.GetBooleanList("g", "b"));
        Assert.Equal(new[] { 1, 2, 3 }, keyFile.GetIntegerList("g", "i"));
        Assert.Equal(new[] { 1.5, 2.5 }, keyFile.GetDoubleList("g", "d"));
    }

    [Fact]
    public void Comment_CanBeSetAndRemoved()
    {
        using var keyFile = Create();
        keyFile.SetString("g", "k", "v");
        Assert.True(keyFile.SetComment("g", "k", "note"));
        Assert.Equal("note", keyFile.GetComment("g", "k"));

        Assert.True(keyFile.RemoveComment("g", "k"));
    }

    [Fact]
    public void LocaleStrings_RoundTrip()
    {
        using var keyFile = Create();
        keyFile.SetLocaleString("g", "k", "es", "hola");
        keyFile.SetString("g", "plain", "value");

        Assert.Equal("hola", keyFile.GetLocaleString("g", "k", "es"));
        Assert.Single(keyFile.GetLocaleStringList("g", "k", "es"));
    }

    [Fact]
    public void RemoveKey_And_RemoveGroup_DeleteContent()
    {
        using var keyFile = Create();
        keyFile.SetString("g", "k", "v");
        keyFile.SetString("g", "k2", "v2");

        Assert.True(keyFile.RemoveKey("g", "k"));
        Assert.False(keyFile.HasKey("g", "k"));
        Assert.True(keyFile.RemoveGroup("g"));
        Assert.False(keyFile.HasGroup("g"));
    }

    [Fact]
    public void Save_And_LoadFromFile_RoundTrip()
    {
        var path = Path.GetTempFileName();

        try
        {
            using (var keyFile = Create())
            {
                keyFile.SetString("g", "k", "v");
                keyFile.SetInteger("g", "i", 7);
                keyFile.Save(path);
            }

            using var loaded = new GLib.KeyFile(path);
            Assert.Equal("v", loaded.GetString("g", "k"));
            Assert.Equal(7, loaded.GetInteger("g", "i"));
            Assert.True(loaded.HasGroup("g"));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void LoadFromData_WithFlags_ParsesTheContent()
    {
        using var keyFile = Create();
        var data = System.Text.Encoding.UTF8.GetBytes("[g]\nk=v\n");

        Assert.True(keyFile.LoadFromData(data, GLib.KeyFileFlags.KeepComments));
        Assert.Equal("v", keyFile.GetString("g", "k"));
    }

    [Fact]
    public void LoadFromDirs_FindsTheFile()
    {
        var directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        var file = Path.Combine(directory.FullName, "sample.ini");
        File.WriteAllText(file, "[g]\nk=v\n");

        try
        {
            using var keyFile = Create();
            Assert.True(keyFile.LoadFromDirs("sample.ini", new[] { directory.FullName }, out var fullPath, GLib.KeyFileFlags.None));
            Assert.Equal(file, fullPath);
            Assert.Equal("v", keyFile.GetString("g", "k"));
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public void ListSeparator_CanBeChanged()
    {
        using var keyFile = Create();
        keyFile.SetListSeparator(';');
        keyFile.SetStringList("g", "list", new[] { "a", "b" });

        Assert.Equal("a;b;", keyFile.GetString("g", "list"));
    }
}
