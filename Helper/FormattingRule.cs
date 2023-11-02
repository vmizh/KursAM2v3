using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm;

namespace Helper
{
    public enum FormattingType
    {
        Background,
        Foreground,
        Font,
        Icon
    }

    public enum ConditionRule
    {
        Less,
        Greater,
        Equal
    }

    public class FormattingRule : BindableBase
    {
        public FormattingRule(string fieldname, ConditionRule valuerule, int value, bool applytorow,
            FormattingType type)
        {
            FieldName = fieldname;
            ValueRule = valuerule;
            Value = value;
            ApplyToRow = applytorow;
            Type = type;
        }

        public virtual string FieldName { get; }
        public virtual ConditionRule ValueRule { get; }
        public virtual int Value { get; }
        public virtual bool ApplyToRow { get; }
        public virtual FormattingType Type { get; }
    }

    public class FormatConditionSelector : DataTemplateSelector
    {

        public DataTemplate BackgroundTemplate { get; set; }

        public DataTemplate ForegroundTemplate { get; set; }
        public DataTemplate FontTemplate { get; set; }
        public DataTemplate IconTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FormattingRule rule = item as FormattingRule;
            if (rule == null) return null;
            switch (rule.Type)
            {
                case FormattingType.Font:
                    return FontTemplate;
                case FormattingType.Foreground:
                    return ForegroundTemplate;
                case FormattingType.Background:
                    return BackgroundTemplate;
                case FormattingType.Icon:
                    return IconTemplate;
            }

            return null;
        }
    }
}
