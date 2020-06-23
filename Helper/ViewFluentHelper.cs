using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using Dock = System.Windows.Controls.Dock;

namespace Helper
{
    public class ViewFluentHelper
    {
        public static MemoEdit SetDefaultMemoEdit(DataLayoutItem item)
        {
            var oldContent = item.Content as BaseEdit;
            var defaultMemo = new MemoEdit
            {
                ShowIcon = false
            };
            defaultMemo.EditValueChanged += delegate(object o, EditValueChangedEventArgs args)
            {
                if (!(args.Source is MemoEdit editor)) return;
                if (editor.EditValue != null && !string.IsNullOrWhiteSpace((string) editor.EditValue))
                    editor.Height = 50;
            };
            BindingHelper.CopyBinding(oldContent, defaultMemo, BaseEdit.EditValueProperty);
            item.Content = defaultMemo;
            if (defaultMemo.EditValue != null && !string.IsNullOrWhiteSpace((string)defaultMemo.EditValue))
                item.Height = 80;
            item.HorizontalAlignment = HorizontalAlignment.Stretch;
            return defaultMemo;
        }

        public static void SetModeUpdateProperties(object Document, DataLayoutItem item, string propertyName)
        {
            var canWrite = Document.GetType().GetProperties().FirstOrDefault(_ => _.Name == propertyName)
                ?.CanWrite;
            if (canWrite == null || !(bool) canWrite) return;
            {
                if (!(item.Content is BaseEdit editor)) return;
                // ReSharper disable once PossibleNullReferenceException
                var expr = editor.GetBindingExpression(BaseEdit.EditValueProperty);
                if (expr != null)
                {
                    var binding = expr.ParentBinding;
                    editor.SetBinding(BaseEdit.EditValueProperty,
                        new Binding
                        {
                            Path = binding.Path,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        });
                }
            }
        }

        public static void SetModeUpdateProperties(object Document, ColumnBase item, string propertyName)
        {
            var canWrite = Document.GetType().GetProperties().FirstOrDefault(_ => _.Name == propertyName)
                ?.CanWrite;
            if (canWrite == null || !(bool) canWrite) return;
            {
                if (!(item.EditSettings is BaseEditSettings editor)) return;
                // ReSharper disable once PossibleNullReferenceException
                var expr = editor.GetBindingExpression(BaseEdit.EditValueProperty);
                if (expr != null)
                {
                    var binding = expr.ParentBinding;
                    editor.SetBinding(BaseEdit.EditValueProperty,
                        new Binding
                        {
                            Path = binding.Path,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                        });
                }
            }
        }

        public static ComboBoxEdit SetComboBoxEdit(DataLayoutItem item, object field, string nameField, object list,
            string displayMember = "Name", string displayValue = "DocCode",
            double width = 300)
        {
            //item.c = width;
            var dockPanel = new DockPanel()
            {
                LastChildFill = false,
                Width = width,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var cbCashs = new ComboBoxEdit
            {
                Name = nameField,
                EditValue = field,
                ItemsSource = list,
                DisplayMember = displayMember,
                ValueMember = displayValue,
                AutoComplete = true,
                Width = width
            };
            DockPanel.SetDock(cbCashs, Dock.Left);
            cbCashs.SetBinding(LookUpEditBase.SelectedItemProperty,
                new Binding { Path = new PropertyPath(nameField) });
            dockPanel.Children.Add(cbCashs);
            item.Content = dockPanel;
            item.Width = dockPanel.Width + 150;
            return cbCashs;
        }
    }
}