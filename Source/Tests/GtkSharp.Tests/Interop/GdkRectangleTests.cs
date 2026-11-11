using System;
using Gdk;
using Xunit;

namespace GtkSharp.Tests.Interop;

public sealed class GdkRectangleTests
{
    [Fact]
    public void Constructors_And_Properties_Work()
    {
        var rectangle = new Rectangle(1, 2, 3, 4);

        Assert.Equal(1, rectangle.X);
        Assert.Equal(2, rectangle.Y);
        Assert.Equal(3, rectangle.Width);
        Assert.Equal(4, rectangle.Height);
        Assert.Equal(new Rectangle(1, 2, 3, 4), Rectangle.FromLTRB(1, 2, 4, 6));
        Assert.Equal(new Rectangle(new Point(1, 2), new Size(3, 4)), rectangle);
    }

    [Fact]
    public void EqualityAndHashCode_Work()
    {
        var first = new Rectangle(1, 2, 3, 4);
        var second = new Rectangle(1, 2, 3, 4);
        var other = new Rectangle(5, 6, 7, 8);

        Assert.True(first == second);
        Assert.False(first != second);
        Assert.True(first != other);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.True(first.Equals((object)second));
    }

    [Fact]
    public void Contains_And_Intersects_Work()
    {
        var rectangle = new Rectangle(0, 0, 10, 10);

        Assert.True(rectangle.Contains(5, 5));
        Assert.True(rectangle.Contains(new Point(5, 5)));
        Assert.True(rectangle.Contains(new Rectangle(1, 1, 2, 2)));
        Assert.False(rectangle.Contains(20, 20));
        Assert.True(rectangle.IntersectsWith(new Rectangle(5, 5, 10, 10)));
        Assert.False(rectangle.IntersectsWith(new Rectangle(20, 20, 1, 1)));
    }

    [Fact]
    public void Union_ReturnsTheBoundingBox()
    {
        var union = Rectangle.Union(new Rectangle(0, 0, 5, 5), new Rectangle(10, 10, 5, 5));

        Assert.Equal(0, union.X);
        Assert.Equal(0, union.Y);
        Assert.Equal(14, union.Width);
        Assert.Equal(14, union.Height);
    }

    [Fact]
    public void ValueBoxing_RoundTrips()
    {
        var rectangle = new Rectangle(1, 2, 3, 4);
        using var value = (GLib.Value)rectangle;
        var unboxed = (Rectangle)value;

        Assert.Equal(rectangle, unboxed);
        Assert.Contains("1", rectangle.ToString());
    }
}
