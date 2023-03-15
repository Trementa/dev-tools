#nullable disable

using System;

namespace OpenApi.Library.Extensions;

public static class DateTimeExtensions
{
    public static long ToUnixTime(this DateTime dt)
    {
        var offset = new DateTimeOffset(dt);
        return offset.ToUnixTimeMilliseconds();
    }
    public static long ToUnixTime(this DateTime? dt)
    {
        if (dt == null)
            return 0;
        var offset = new DateTimeOffset(dt.Value);
        return offset.ToUnixTimeMilliseconds();
    }
    public static DateTime FromUnixTime(this long timestamp)
        => DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
}