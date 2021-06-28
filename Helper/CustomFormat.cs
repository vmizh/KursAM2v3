using System;
using System.Globalization;

namespace Helper
{
    public static class CustomFormat
    {
        public static string DateToString(DateTime date)
        {
            return
                $"{date.Year}{(date.Month < 10 ? "0" + date.Month : date.Month.ToString())}{(date.Day < 10 ? "0" + date.Day : date.Day.ToString())}";
        }

        public static string DateWithTimeToString(DateTime date)
        {
            var ret =
                $"{date.Year}{(date.Month < 10 ? "0" + date.Month : date.Month.ToString())}{(date.Day < 10 ? "0" + date.Day : date.Day.ToString())}";
            ret += $" {date.Hour}:{date.Minute}:{date.Second}.{date.Millisecond}";
            return ret;
        }

        public static string DecimalToSqlDecimal(decimal d)
        {
            return Convert.ToString(d, CultureInfo.InvariantCulture).Replace(",", ".");
        }

        public static string DecimalToSqlDecimal(decimal? d)
        {
            return d == null ? "null" : Convert.ToString(d.Value, CultureInfo.InvariantCulture).Replace(",", ".");
        }

        public static string GuidToSqlString(Guid d)
        {
            return Convert.ToString(d, CultureInfo.InvariantCulture)?.Replace("{", "").Replace("}", "");
        }

        /// <summary>
        /// Перевожит строку в decimal, не зависимо от токчки или запятой, в качестве разделения
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static decimal StringToDecimal(string d)
        {
            return Convert.ToDecimal(d, new NumberFormatInfo
            {
                CurrencyDecimalSeparator = ".,"
            });
        }
    }
}