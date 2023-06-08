using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace Helper
{
    public static class CustomFormat
    {
        public static string DateToString(DateTime date)
        {
            return
                $"{date.Year}{(date.Month < 10 ? "0" + date.Month : date.Month.ToString())}{(date.Day < 10 ? "0" + date.Day : date.Day.ToString())}";
        }

        public static string DateToString(DateTime? date)
        {
            return date == null ? "20000101" : DateToString(date.Value);
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
        ///     Перевожит строку в decimal, не зависимо от токчки или запятой, в качестве разделения
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

        public static string GetFullExceptionTextMessage(Exception ex, string methodName = null)
        {
            var errText = new StringBuilder(ex.Message);
            var ex1 = ex;
            while (ex1.InnerException != null)
            {
                ex1 = ex1.InnerException;
                errText.Append(ex1.Message + "\n");
                if (ex1.InnerException != null)
                    errText.Append(ex1.InnerException.Message);
            }

            return string.IsNullOrWhiteSpace(methodName) ? errText.ToString() : $"{methodName} -> {errText}";
        }

        public static string GetEnumName(Enum enumElement)
        {
            var type = enumElement.GetType();

            var memInfo = type.GetMember(enumElement.ToString());
            if (memInfo.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                if (attrs.Length > 0)
                    return ((DisplayAttribute) attrs[0]).Name;
            }

            return enumElement.ToString();
        }
    }
}
