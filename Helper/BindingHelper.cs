using System.Windows;
using DevExpress.Xpf.Editors;

namespace Helper
{
    public class BindingHelper
    {
        public static void CopyBinding(BaseEdit oldContent, BaseEdit newContent, DependencyProperty property)
        {
            var binding = oldContent?.GetBindingExpression(property)?.ParentBinding;
            if (binding != null) newContent.SetBinding(property, binding);
        }

        private static void CopyValue(BaseEdit oldContent, BaseEdit newContent, DependencyProperty property)
        {
            newContent.SetValue(BaseEdit.IsReadOnlyProperty, oldContent.GetValue(property));
        }
    }
}