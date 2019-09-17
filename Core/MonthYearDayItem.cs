using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Core
{
    [DataContract]
    public class MonthYearDayItem
    {
        [DataMember]
        public Guid Id { set; get; }
        [DataMember]
        public Guid ParentId { set; get; }
        [DataMember]
        public DatePeriodType PeriodType { set; get; }
        [DataMember]
        public string PeriodName { set; get; }
        [DataMember]
        public string PeriodNameYear { set; get; }
        [DataMember]
        public string PeriodNameMonth { set; get; }
        [DataMember]
        public DateTime PeriodStart { set; get; }
        [DataMember]
        public DateTime PeriodEnd { set; get; }

        public static string MonthName(int month)
        {
            if (month < 1 || month > 12)
                return "Неправильный номер месяца";
            switch (month)
            {
                case 1:
                    return "Январь";
                case 2:
                    return "Февраль";
                case 3:
                    return "Март";
                case 4:
                    return "Апрель";
                case 5:
                    return "Май";
                case 6:
                    return "Июнь";
                case 7:
                    return "Июль";
                case 8:
                    return "Август";
                case 9:
                    return "Сентябрь";
                case 10:
                    return "Октябрь";
                case 11:
                    return "Ноябрь";
                case 12:
                    return "Декабрь";
            }
            return "";
        }

        public static List<MonthYearDayItem> Generate(IEnumerable<IPeriod> listDate)
        {
            var res = new List<MonthYearDayItem>();
            var uniqdates = new List<DateTime>((from dt in listDate select dt.PeriodDate).Distinct());
            foreach (var y in uniqdates.Select(_ => _.Year).Distinct())
            {
                var newYear = new MonthYearDayItem
                {
                    Id = Guid.NewGuid(),
                    ParentId = Guid.Empty,
                    PeriodType = DatePeriodType.Year,
                    PeriodStart = new DateTime(y, 1, 1),
                    PeriodEnd = new DateTime(y, 12, 31),
                    PeriodName = y.ToString()
                };
                res.Add(newYear);
                var y1 = y;
                foreach (var m in uniqdates.Where(_ => _.Year == y1).Select(_ => _.Month))
                {
                    var newMonth = new MonthYearDayItem
                    {
                        Id = Guid.NewGuid(),
                        ParentId = newYear.Id,
                        PeriodType = DatePeriodType.Month,
                        PeriodStart = new DateTime(y1, m, 1),
                        PeriodEnd = new DateTime(y1, m, 1).AddMonths(1).AddDays(-1),
                        PeriodName = string.Format("{0} {1}", MonthName(m), y)
                    };
                    res.Add(newMonth);
                    var y2 = y;
                    var m1 = m;
                    res.AddRange(
                        uniqdates.Where(_ => _.Year == y2 && _.Month == m1)
                            .Select(_ => _.Day)
                            .Select(d => new MonthYearDayItem
                            {
                                Id = Guid.NewGuid(),
                                ParentId = newMonth.Id,
                                PeriodType = DatePeriodType.Day,
                                PeriodStart = new DateTime(y2, m1, d),
                                PeriodEnd = new DateTime(y2, m1, d),
                                PeriodName = new DateTime(y2, m1, d).ToShortDateString()
                            }));
                }
            }
            return res;
        }

        public static IEnumerable<MonthYearDayItem> Generate(IEnumerable<DateTime> listDate)
        {
            var res = new List<MonthYearDayItem>();
            var uniqdates = new List<DateTime>((from dt in listDate select dt).Distinct());
            foreach (var y in uniqdates.Select(_ => _.Year).Distinct())
            {
                var newYear = new MonthYearDayItem
                {
                    Id = Guid.NewGuid(),
                    ParentId = Guid.Empty,
                    PeriodType = DatePeriodType.Year,
                    PeriodStart = new DateTime(y, 1, 1),
                    PeriodEnd = new DateTime(y, 12, 31),
                    PeriodName = y.ToString(),
                    PeriodNameYear = y.ToString(),
                    PeriodNameMonth = null
                };
                res.Add(newYear);
                var y1 = y;
                foreach (var m in uniqdates.Where(_ => _.Year == y1).Select(_ => _.Month).Distinct())
                {
                    var newMonth = new MonthYearDayItem
                    {
                        Id = Guid.NewGuid(),
                        ParentId = newYear.Id,
                        PeriodType = DatePeriodType.Month,
                        PeriodStart = new DateTime(y1, m, 1),
                        PeriodEnd = new DateTime(y1, m, 1).AddMonths(1).AddDays(-1),
                        PeriodName = string.Format("{0} {1}", MonthName(m), y),
                        PeriodNameYear = y1.ToString(),
                        PeriodNameMonth = MonthName(m)
                    };
                    res.Add(newMonth);
                    var y2 = y1;
                    var m1 = m;
                    res.AddRange(
                        uniqdates.Where(_ => _.Year == y2 && _.Month == m1)
                            .Select(_ => _.Day).Distinct()
                            .Select(d => new MonthYearDayItem
                            {
                                Id = Guid.NewGuid(),
                                ParentId = newMonth.Id,
                                PeriodType = DatePeriodType.Day,
                                PeriodStart = new DateTime(y2, m1, d),
                                PeriodEnd = new DateTime(y2, m1, d),
                                PeriodName = new DateTime(y2, m1, d).ToShortDateString()
                            }));
                }
            }
            return res;
        }

        public static List<MonthYearDayItem> Generate(DateTime[] listDate)
        {
            var res = new List<MonthYearDayItem>();
            var uniqdates = new List<DateTime>((from dt in listDate select dt).Distinct());
            foreach (var y in uniqdates.Select(_ => _.Year).Distinct())
            {
                var newYear = new MonthYearDayItem
                {
                    Id = Guid.NewGuid(),
                    ParentId = Guid.Empty,
                    PeriodType = DatePeriodType.Year,
                    PeriodStart = new DateTime(y, 1, 1),
                    PeriodEnd = new DateTime(y, 12, 31),
                    PeriodName = y.ToString()
                };
                res.Add(newYear);
                var y1 = y;
                foreach (var m in uniqdates.Where(_ => _.Year == y1).Select(_ => _.Month))
                {
                    if (res.Exists(_ => _.PeriodStart.Year == y1 && _.PeriodStart.Month == m))
                        continue;
                    var newMonth = new MonthYearDayItem
                    {
                        Id = Guid.NewGuid(),
                        ParentId = newYear.Id,
                        PeriodType = DatePeriodType.Month,
                        PeriodStart = new DateTime(y1, m, 1),
                        PeriodEnd = new DateTime(y1, m, 1).AddMonths(1).AddDays(-1),
                        PeriodName = string.Format("{0} {1}", MonthName(m), y)
                    };
                    res.Add(newMonth);
                    var y2 = y1;
                    var m1 = m;
                    res.AddRange(
                        uniqdates.Where(_ => _.Year == y2 && _.Month == m1)
                            .Select(_ => _.Day)
                            .Select(d => new MonthYearDayItem
                            {
                                Id = Guid.NewGuid(),
                                ParentId = newMonth.Id,
                                PeriodType = DatePeriodType.Day,
                                PeriodStart = new DateTime(y2, m1, d),
                                PeriodEnd = new DateTime(y2, m1, d),
                                PeriodName = new DateTime(y2, m1, d).ToShortDateString()
                            }));
                }
            }
            return res;
        }
    }

    public class PeriodInfo
    {
        private readonly List<DateTime> dates = new List<DateTime>();

        public PeriodInfo()
        {
            Data = new List<MonthYearDayItem>();
        }

        public PeriodInfo(IEnumerable<IPeriod> listDate)
            : this()
        {
            Data = new List<MonthYearDayItem>();
            Data = MonthYearDayItem.Generate(listDate);
        }

        public PeriodInfo(IEnumerable<DateTime> listDate)
            : this()
        {
            dates.Clear();
            var dateTimes = listDate as DateTime[] ?? listDate.ToArray();
            foreach (var d in dateTimes)
                dates.Add(d);
            Data = new List<MonthYearDayItem>();
            Data = MonthYearDayItem.Generate(dateTimes).ToList();
        }

        public List<MonthYearDayItem> Data { set; get; }

        public List<int> GetYears()
        {
            return (List<int>) Data.Where(_ => _.PeriodType == DatePeriodType.Year).Select(y => y.PeriodStart.Year);
        }

        public List<string> GetMonths(int year)
        {
            return
                (List<string>)
                Data.Where(_ => _.PeriodType == DatePeriodType.Year && _.PeriodStart.Year == year)
                    .Select(m => m.PeriodNameMonth);
        }

        public List<DateTime> GetDates()
        {
            return dates;
        }

        public List<DateTime> GetDates(int year, int month)
        {
            return
                new List<DateTime>(
                    Data.Where(
                        _ =>
                            _.PeriodType == DatePeriodType.Day && _.PeriodStart.Year == year &&
                            _.PeriodStart.Month == month).Select(d => d.PeriodStart));
        }
    }
}