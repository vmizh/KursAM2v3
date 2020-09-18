using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using DevExpress.Data;

namespace Core.ViewModel.Base.Column
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GridColumnView : Attribute
    {
        public GridColumnView(string header)
        {
            Header = header;
            DefaultWidth = true;
            Width = 0;
            Settings = SettingsType.Default;
            ReadOnly = false;
        }

        public GridColumnView(string header, SettingsType settings)
            : this(header)
        {
            Settings = settings;
        }

        public GridColumnView(string header, SettingsType settings, Summary[] summaries)
            : this(header, settings)
        {
            Summaries = summaries;
        }

        public string Header { get; set; }
        public bool DefaultWidth { get; set; }
        public double Width { get; set; }
        public bool ReadOnly { get; set; }
        public SettingsType Settings { get; set; }
        public Summary[] Summaries { set; get; }
        public IList ListSource { set; get; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class GridColumnSummary : Attribute
    {
        public GridColumnSummary(SummaryItemType summaryType)
        {
            Type = summaryType;
            IsTotalSummary = true;
            DisplayFormat = "n0";
        }

        public GridColumnSummary(SummaryItemType summaryType, string displayFormat)
            : this(summaryType)
        {
            DisplayFormat = displayFormat ?? "n0";
        }

        public bool IsTotalSummary { set; get; }
        public SummaryItemType Type { get; set; }
        public string DisplayFormat { set; get; }
    }

    public class GridTableViewInfo
    {
        public ObservableCollection<Column> Columns { get; } = new ObservableCollection<Column>();
        public ObservableCollection<Summary> TotalSummary { get; } = new ObservableCollection<Summary>();
        public ObservableCollection<Summary> GroupSummary { get; } = new ObservableCollection<Summary>();

        public void Generate(Type type)
        {
            foreach (var prop in type.GetProperties())
            {
                var attr = prop.GetCustomAttribute(typeof(GridColumnView)) as GridColumnView;
                if (attr != null)
                {
                    var c = new Column
                    {
                        Name = prop.Name,
                        Header = attr.Header,
                        Settings = attr.Settings,
                        FieldName = prop.Name,
                        ReadOnly = attr.ReadOnly
                    };
                    Columns.Add(c);
                }

                var attrSum = prop.GetCustomAttributes(typeof(GridColumnSummary)) as GridColumnSummary[];
                if (attrSum == null || !attrSum.Any()) continue;
                foreach (var summ in attrSum)
                {
                    var newSummary = new Summary
                    {
                        FieldName = prop.Name,
                        Type = summ.Type,
                        DisplayFormat = summ.DisplayFormat
                    };
                    if (summ.IsTotalSummary)
                        TotalSummary.Add(newSummary);
                    else
                        GroupSummary.Add(newSummary);
                }
            }
        }
    }
}