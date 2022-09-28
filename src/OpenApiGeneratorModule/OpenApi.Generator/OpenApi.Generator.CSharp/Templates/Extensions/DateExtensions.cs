using GK.WebLib.Types;

namespace GK.WebLib.Extensions
{
    public static class DateExtensions
    {
        public static Date Tomorrow() =>
            Date.Today.AddDays(1);
    }
}
