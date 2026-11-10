using System;
using Xunit;

namespace GtkSharp.Tests.Infrastructure;

/// <summary>
/// Groups every test that touches GTK. GTK is single threaded, so the whole
/// collection is serialized and GTK is initialized once by <see cref="GtkFixture"/>.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class GtkCollection : ICollectionFixture<GtkFixture>
{
    public const string Name = "Gtk";
}

/// <summary>
/// Ensures GTK is initialized before the first widget test runs.
/// </summary>
public sealed class GtkFixture
{
    public GtkFixture()
    {
        var args = Array.Empty<string>();

        if (!Gtk.Application.InitCheck("GtkSharp.Tests", ref args))
            throw new InvalidOperationException("GTK could not be initialized: no display available.");
    }
}
