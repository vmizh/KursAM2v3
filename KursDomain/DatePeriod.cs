using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Core.Helper;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.Annotations;

namespace Core
{
    public enum DatePeriodType
    {
        [Display(Name = "Год")] Year,
        [Display(Name = "Месяц")] Month,
        [Display(Name = "День")] Day
    }

    public interface IPeriod
    {
        DateTime PeriodDate { set; get; }
    }

    public enum PeriodIerarhy
    {
        YearMonthDay = 0,
        YearMonth = 1,
        YearMonthWeekDay = 2,
        YearQuartMonthDay = 3,
        YearQuartMonth = 4,
        YearQuartWeek = 5,
        YearQuart = 6,
        YearWeek = 7
    }

    public enum PeriodType
    {
        [Display(Name = "Год")] Year = 0,
        [Display(Name = "Месяц")] Month = 1,
        [Display(Name = "День")] Day = 2,
        [Display(Name = "Неделя")] Week = 3,
        [Display(Name = "Квартал")] Quart = 4
    }

    [MetadataType(typeof(DataAnnotationsDataPeriod))]
    [DataContract]
    public class DatePeriod : ViewModelBase, IComparable<DatePeriod>,
        IComparable, IEquatable<DatePeriod>
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once CollectionNeverQueried.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public IList<string> NotifyProtocol = new List<string>();

        [DataMember] public PeriodType PeriodType { set; get; }

        [DataMember] public DateTime DateStart { set; get; }

        [DataMember] public DateTime DateEnd { set; get; }

        public object ParentId { get; set; }
        public Guid Id { get; set; }
        public virtual string Name { get; set; }

        public int CompareTo(object obj)
        {
            if (!(obj is DatePeriod o)) return -1;
            return Id.CompareTo(o.Id);
        }

        public int CompareTo(DatePeriod other)
        {
            return Id.CompareTo(other?.Id);
        }

        public bool Equals(DatePeriod other)
        {
            return other?.Id == Id;
        }

        /// <summary>
        ///     Событие изменения значения свойства представления
        /// </summary>
        public new virtual event PropertyChangedEventHandler PropertyChanged;

        private static string GetMonthName(int month)
        {
            if (month < 1 || month > 12) return null;
            string ret = null;
            switch (month)
            {
                case 1:
                    ret = "Январь";
                    break;
                case 2:
                    ret = "Февраль";
                    break;
                case 3:
                    ret = "Март";
                    break;
                case 4:
                    ret = "Апрель";
                    break;
                case 5:
                    ret = "Май";
                    break;
                case 6:
                    ret = "Июнь";
                    break;
                case 7:
                    ret = "Июль";
                    break;
                case 8:
                    ret = "Август";
                    break;
                case 9:
                    ret = "Сентябрь";
                    break;
                case 10:
                    ret = "Октябрь";
                    break;
                case 11:
                    ret = "Ноябрь";
                    break;
                case 12:
                    ret = "Декабрь";
                    break;
            }

            return ret;
        }

        /// <summary>
        ///     Метод обработки события изменения значения свойства представления
        /// </summary>
        /// <param name="propertyName">Идентификатор свойства</param>
        [NotifyPropertyChangedInvocator("propertyName")]
        public new virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            NotifyProtocol.Add(propertyName);
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static IEnumerable<DatePeriod> GenerateIerarhy(IEnumerable<DateTime> dates, PeriodIerarhy ierarhyType)
        {
            var years = new List<DatePeriod>();
            var months = new List<DatePeriod>();
            var days = new List<DatePeriod>();
            foreach (var d in dates.Select(_ => _.Date.Year).Distinct())
                years.Add(new DatePeriod
                {
                    Id =
                        Guid.NewGuid(),
                    ParentId = null,
                    PeriodType =
                        PeriodType
                            .Year,
                    DateStart =
                        new DateTime(
                            d, 1, 1),
                    DateEnd =
                        new DateTime(
                            d, 12,
                            31),
                    Name =
                        d.ToString()
                });
            foreach (var y1 in years)
            foreach (var d in dates.Where(_ => _.Date.Year == y1.DateStart.Year))
            {
                var mm = months.FirstOrDefault(_ =>
                    _.DateStart.Year == y1.DateStart.Year && _.DateStart.Month == d.Date.Month);
                if (mm == null)
                    months.Add(new DatePeriod
                    {
                        Id = Guid.NewGuid(),
                        ParentId = y1.Id,
                        PeriodType = PeriodType.Month,
                        DateStart = new DateTime(y1.DateStart.Year, d.Month, 1),
                        DateEnd = new DateTime(y1.DateStart.Year, d.Month, 1).AddMonths(1).AddDays(-1),
                        Name = GetMonthName(d.Month)
                    });
            }

            foreach (var m1 in months)
            foreach (var d1 in dates.Where(_ =>
                    _.Date.Year == m1.DateStart.Year && _.Date.Month == m1.DateStart.Month).Select(_ => _.Date)
                .Distinct())

                days.Add(new DatePeriod
                {
                    Id = Guid.NewGuid(),
                    ParentId = m1.Id,
                    PeriodType = PeriodType.Day,
                    DateStart = d1,
                    DateEnd = d1,
                    Name = d1.ToShortDateString()
                });
            return years.Union(months).Union(days).OrderByDescending(_ => _.DateStart);
        }

        public static IEnumerable<DatePeriod> GenerateIerarhy(DateTime start, DateTime end, PeriodIerarhy ierarhyType)
        {
            var ret = new List<DatePeriod>();
            var dates = new List<DateTime>();
            var dtemp = start;
            while (dtemp <= end)
            {
                dates.Add(dtemp);
                dtemp = dtemp.AddDays(1);
            }

            foreach (var d in dates)
            {
                if (!ret.Any(t => t.PeriodType == PeriodType.Year && t.DateStart.Year == d.Year))
                    ret.Add(new DatePeriod
                    {
                        Id =
                            Guid.NewGuid(),
                        ParentId = null,
                        PeriodType =
                            PeriodType
                                .Year,
                        DateStart =
                            new DateTime(
                                d.Year, 1, 1),
                        DateEnd =
                            new DateTime(
                                d.Year, 12,
                                31),
                        Name =
                            d.Year
                                .ToString()
                    });
                if (ret.Any(
                    t =>
                        t.PeriodType == PeriodType.Month && t.DateStart.Year == d.Year && t.DateStart.Month == d.Month))
                    continue;
                var newMonthPeriod = new DatePeriod
                {
                    Id = Guid.NewGuid(),
                    ParentId = null,
                    PeriodType = PeriodType.Month,
                    DateStart = new DateTime(d.Year, d.Month, 1),
                    DateEnd = new DateTime(d.Year, d.Month, 1).AddMonths(1).AddDays(-1),
                    Name = GetMonthName(d.Month)
                };
                newMonthPeriod.ParentId =
                    ret.Find(
                            t => t.PeriodType == PeriodType.Year && t.DateStart.Year == newMonthPeriod.DateStart.Year)
                        .Id;
                ret.Add(newMonthPeriod);
            }

            return ret;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class DataAnnotationsDataPeriod : DataAnnotationForFluentApiBase, IMetadataProvider<DatePeriod>
    {
        void IMetadataProvider<DatePeriod>.BuildMetadata(MetadataBuilder<DatePeriod> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.PeriodType).NotAutoGenerated();
            builder.Property(_ => _.DateEnd).DisplayName("Конец");
            builder.Property(_ => _.DateStart).DisplayName("Начало");
            builder.Property(_ => _.Name).AutoGenerated().DisplayName("Период");
            builder.Group("Период")
                .ContainsProperty(_ => _.Name)
                .ContainsProperty(_ => _.DateStart)
                .ContainsProperty(_ => _.DateEnd);
        }
    }
}
