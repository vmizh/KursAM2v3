using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm;
using DevExpress.Xpf.Grid;

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
        Equal,
        NotEqual
    }

    public sealed class FormattingRule : BindableBase
    {
        public FormattingRule(string fieldname, ConditionRule valuerule, decimal value, bool applytorow,
            FormattingType type)
        {
            FieldName = fieldname;
            ValueRule = valuerule;
            Value = value;
            ApplyToRow = applytorow;
            Type = type;
        }

        public string FieldName { get; }
        public ConditionRule ValueRule { get; }
        public decimal Value { get; }
        public bool ApplyToRow { get; }
        public FormattingType Type { get; }

        public FormatCondition GetFormatCondition()
        {
            var ret = new FormatCondition
            {
                Value1 = Value,
                ApplyToRow = ApplyToRow,
                TypeName = "FormatCondition",
                FieldName = FieldName
            };
            switch (ValueRule)
            {
                case ConditionRule.Less:
                    ret.ValueRule = DevExpress.Xpf.Core.ConditionalFormatting.ConditionRule.Less;
                    break;
                case ConditionRule.Greater:
                    ret.ValueRule = DevExpress.Xpf.Core.ConditionalFormatting.ConditionRule.Greater;
                    break;
                case ConditionRule.Equal:
                    ret.ValueRule = DevExpress.Xpf.Core.ConditionalFormatting.ConditionRule.Equal;
                    break;
                case ConditionRule.NotEqual:
                    ret.ValueRule = DevExpress.Xpf.Core.ConditionalFormatting.ConditionRule.NotEqual;
                    break;
            }
            return ret;
        }
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
