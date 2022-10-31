using System.Collections;
using System.Windows;
using DevExpress.Data;
using DevExpress.Xpf.Grid;

namespace Core.ViewModel.Base.Column
{
    public class Column
    {
        public Column()
        {
            Header = FieldName;
            DefaultWidth = true;
            Width = 0;
            ReadOnly = false;
        }

        public string FieldName { get; set; }
        public string Header { get; set; }
        public bool DefaultWidth { get; set; }
        public double Width { get; set; }
        public string Name { get; set; }
        public bool ReadOnly { set; get; }
        public IList ListSource { set; get; }
        public SettingsType Settings { get; set; }
    }

    public class ComboColumn : Column
    {
        public IList Source { get; set; }
    }

    public enum SettingsType
    {
        Default,
        Decimal,
        Decimal4,
        Combo,
        ImageCombo
    }

    public class Summary
    {
        public SummaryItemType Type { get; set; }
        public string FieldName { get; set; }
        public string DisplayFormat { set; get; }
    }

    public static class ColumnHelper
    {
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.RegisterAttached("Name", typeof(string), typeof(ColumnHelper),
                new PropertyMetadata(string.Empty, OnNameChanged));

        public static string GetName(DependencyObject obj)
        {
            return (string) obj.GetValue(NameProperty);
        }

        public static void SetName(DependencyObject obj, string value)
        {
            obj.SetValue(NameProperty, value);
        }

        private static void OnNameChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var c = (GridColumn) sender;
            if (e.NewValue != null)
                c.Name = e.NewValue.ToString();
        }
    }
}
