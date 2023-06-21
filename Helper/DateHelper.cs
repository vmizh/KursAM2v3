using System;


namespace Helper
{
    public class DateHelper
    {
        public static DateTime GetFirstDate()
        {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month == 1 ? 1 :  DateTime.Today.Month-1, 1);
        }
    }
}
