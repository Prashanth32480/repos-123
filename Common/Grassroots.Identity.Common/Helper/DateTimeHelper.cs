using System;
using System.Globalization;

namespace Grassroots.Identity.Common.Helper
{
    public static class DateTimeHelper
    {
        public static string GetDate(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}