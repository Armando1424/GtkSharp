using System;
using System.Collections;
using Xunit;

namespace GtkSharp.Tests.Core;

public sealed class GLibPtrArrayTests
{
    [Fact]
    public void AddIndexAndRemove_Work()
    {
        using var array = new GLib.PtrArray(typeof(IntPtr), true, false);

        array.Add(new IntPtr(1));
        array.Add(new IntPtr(2));

        Assert.Equal(2, array.Count);
        Assert.Equal(new IntPtr(1), (IntPtr)array[0]);

        array.Remove(new IntPtr(1));
        Assert.Equal(1, array.Count);

        array.Add(new IntPtr(3));
        array.RemoveRange(new IntPtr(2), 0, 1);
        Assert.Equal(new IntPtr(3), (IntPtr)array[0]);
    }

}
