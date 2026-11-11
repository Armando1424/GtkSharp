using System;
using GLib;
using Xunit;

namespace GtkSharp.Tests.Core;

public sealed class GLibDateTimeTests
{
    private static GLib.DateTime Create() =>
        new(GLib.TimeZone.NewUtc(), 2024, 3, 15, 10, 30, 15.5);

    [Fact]
    public void Properties_ExposeTheComponents()
    {
        using var dateTime = Create();

        Assert.Equal(2024, dateTime.Year);
        Assert.Equal(3, dateTime.Month);
        Assert.Equal(15, dateTime.DayOfMonth);
        Assert.Equal(10, dateTime.Hour);
        Assert.Equal(30, dateTime.Minute);
        Assert.Equal(15, (int)dateTime.Second);
        Assert.True(dateTime.DayOfYear > 0);
        Assert.True(dateTime.DayOfWeek >= 0);
        Assert.True(dateTime.WeekOfYear >= 0);
        Assert.True(dateTime.WeekNumberingYear >= 2024);
    }

    [Fact]
    public void Format_UsesTheGivenPattern()
    {
        using var dateTime = Create();

        Assert.Equal("2024-03-15", dateTime.Format("%Y-%m-%d"));
        Assert.Equal("10:30", dateTime.Format("%H:%M"));
    }

    [Fact]
    public void AddMethods_MoveTheInstant()
    {
        using var dateTime = Create();

        using var plusDay = dateTime.AddDays(1);
        Assert.NotNull(plusDay);

        using var plusMonth = dateTime.AddMonths(1);
        Assert.NotNull(plusMonth);

        using var plusYear = dateTime.AddYears(1);
        Assert.NotNull(plusYear);

        using var plusHour = dateTime.AddHours(1);
        Assert.NotNull(plusHour);

        using var plusMinute = dateTime.AddMinutes(1);
        Assert.NotNull(plusMinute);

        using var plusSecond = dateTime.AddSeconds(1);
        Assert.NotNull(plusSecond);

        using var plusWeek = dateTime.AddWeeks(1);
        Assert.NotNull(plusWeek);

        using var full = dateTime.AddFull(1, 1, 1, 1, 1, 1);
        Assert.NotNull(full);

    }

    [Fact]
    public void UnixAndLocalConversions_Work()
    {
        using var dateTime = Create();
        var unix = dateTime.ToUnix();

        Assert.True(unix > 0);
        Assert.Equal(unix, dateTime.ToLocal().ToUnix());
        Assert.True(dateTime.IsDaylightSavings || !dateTime.IsDaylightSavings);
    }

    [Fact]
    public void DefaultAndTimeValConstructors_Work()
    {
        using var empty = new GLib.DateTime();
        Assert.True(empty.Format("%Y") is { Length: 4 });

        var timeVal = new GLib.TimeVal { TvSec = 1_700_000_000 };
        using var fromTimeVal = new GLib.DateTime(timeVal);
        Assert.True(fromTimeVal.Year >= 2023);

        using var fromUnix = new GLib.DateTime(1_700_000_000L);
        Assert.True(fromUnix.Year >= 2023);

        using var fromZone = new GLib.DateTime(GLib.TimeZone.NewUtc());
        Assert.True(fromZone.Year >= 2024);
    }
}

public sealed class GLibDateTests
{
    [Fact]
    public void Properties_ExposeTheComponents()
    {
        var date = new GLib.Date(15, 3, 2024);

        Assert.True(date.Valid());
        Assert.Equal(15, date.Day);
        Assert.Equal(3, date.Month);
        Assert.Equal(2024, date.Year);
        Assert.True(date.DayOfYear > 0);
        Assert.True(date.Iso8601WeekOfYear > 0);
    }

    [Fact]
    public void AddAndSubtract_MoveTheDate()
    {
        var date = new GLib.Date(15, 3, 2024);

        date.AddDays(1);
        Assert.Equal(16, date.Day);

        date.SubtractDays(1);
        Assert.Equal(15, date.Day);

        date.AddMonths(1);
        Assert.Equal(4, date.Month);

        date.SubtractMonths(1);
        Assert.Equal(3, date.Month);

        date.AddYears(1);
        Assert.Equal(2025, date.Year);

        date.SubtractYears(1);
        Assert.Equal(2024, date.Year);
    }

    [Fact]
    public void DaysBetween_ReturnsTheDifference()
    {
        var first = new GLib.Date(1, 1, 2024);
        var second = new GLib.Date(10, 1, 2024);

        Assert.Equal(9, first.DaysBetween(second));
    }

    [Fact]
    public void FirstAndLastOfMonth_AreDetected()
    {
        Assert.True(new GLib.Date(1, 1, 2024).IsFirstOfMonth);
        Assert.True(new GLib.Date(31, 1, 2024).IsLastOfMonth);
        Assert.False(new GLib.Date(15, 1, 2024).IsFirstOfMonth);
    }

    [Fact]
    public void StaticHelpers_ValidateDates()
    {
        Assert.True(GLib.Date.IsLeapYear(2024));
        Assert.False(GLib.Date.IsLeapYear(2023));
        Assert.True(GLib.Date.ValidDay(31));
        Assert.False(GLib.Date.ValidDay(0));
        Assert.True(GLib.Date.ValidDmy(31, 1, 2024));
        Assert.True(GLib.Date.ValidMonth(12));
        Assert.False(GLib.Date.ValidMonth(13));
        Assert.True(GLib.Date.ValidJulian(1));
        Assert.Equal(29, GLib.Date.GetDaysInMonth(2, 2024));
        Assert.True(GLib.Date.GetMondayWeeksInYear(2024) >= 52);
        Assert.True(GLib.Date.GetSundayWeeksInYear(2024) >= 52);
    }

}
