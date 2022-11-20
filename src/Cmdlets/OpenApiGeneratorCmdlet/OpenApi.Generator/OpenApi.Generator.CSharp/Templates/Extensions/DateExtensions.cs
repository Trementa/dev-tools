namespace Templates.Extensions;
using Types;

public static class DateExtensions
{
    public static Date Tomorrow() =>
        Date.Today.AddDays(1);
}
