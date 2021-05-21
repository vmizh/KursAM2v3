using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.LayoutControl;
using DevExpress.XtraEditors.DXErrorProvider;
using Dock = System.Windows.Controls.Dock;

namespace Helper
{
    public class ViewFluentHelper
    {
        public static MemoEdit SetDefaultMemoEdit(DataLayoutItem item,
            HorizontalAlignment hrzAlign = HorizontalAlignment.Stretch, float? width = null, float height = 80)
        {
            var oldContent = item.Content as BaseEdit;
            var defaultMemo = new MemoEdit
            {
                ShowIcon = false,
                VerticalAlignment = VerticalAlignment.Stretch,
                ShowEditorButtons = false,
                TextWrapping = TextWrapping.Wrap
            };
            defaultMemo.EditValueChanged += delegate(object o, EditValueChangedEventArgs args)
            {
                if (!(args.Source is MemoEdit editor)) return;
                if (editor.EditValue != null && !string.IsNullOrWhiteSpace((string) editor.EditValue))
                    editor.Height = 50;
            };
            BindingHelper.CopyBinding(oldContent, defaultMemo, BaseEdit.EditValueProperty);
            item.Content = defaultMemo;
            if (defaultMemo.EditValue != null && !string.IsNullOrWhiteSpace((string) defaultMemo.EditValue))
                item.Height = height;
            item.HorizontalAlignment = hrzAlign;
            if (width != null)
                item.Width = width.Value;
            return defaultMemo;
        }

        public static TextEdit SetDefaultTextEdit(DataLayoutItem item,
            HorizontalAlignment hrzAlign = HorizontalAlignment.Left,
            float? width = null, float? height = null)
        {
            var oldContent = item.Content as BaseEdit;
            var defaultText = new TextEdit
            {
                TextWrapping = TextWrapping.Wrap
            };
            BindingHelper.CopyBinding(oldContent, defaultText, BaseEdit.EditValueProperty);
            item.Content = defaultText;
            item.HorizontalAlignment = hrzAlign;
            if (width != null)
                item.Width = width.Value;
            if (height != null)
                item.Height = height.Value;
            return defaultText;
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

        public static ComboBoxEdit SetComboBoxEditNotNull(DataLayoutItem item, object field, string nameField,
            object list,
            string displayMember = "Name", string displayValue = "DocCode",
            double width = 300)
        {
            //item.c = width;
            var dockPanel = new DockPanel
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
                Width = width,
                ValidateOnTextInput = true,
                ValidateOnEnterKeyPressed = true,
                ShowError = true
            };
            cbCashs.Validate += (s, e) =>
            {
                if (e.Value != null) return;
                e.IsValid = false;
                e.ErrorType = ErrorType.Critical;
                e.ErrorContent = "Поле не может быть пустым";
            };
            DockPanel.SetDock(cbCashs, Dock.Left);
            cbCashs.SetBinding(LookUpEditBase.SelectedItemProperty,
                new Binding {Path = new PropertyPath(nameField)});
            dockPanel.Children.Add(cbCashs);
            item.Content = dockPanel;
            item.Width = dockPanel.Width + 150;
            return cbCashs;
        }

        public static ComboBoxEdit SetComboBoxEdit(DataLayoutItem item, object field, string nameField, object list,
            string displayMember = "Name", string displayValue = "DocCode",
            double width = 300)
        {
            //item.c = width;
            var dockPanel = new DockPanel
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
                Width = width,
                ValidateOnTextInput = true,
                ValidateOnEnterKeyPressed = true,
                ShowError = true
            };
            DockPanel.SetDock(cbCashs, Dock.Left);
            cbCashs.SetBinding(LookUpEditBase.SelectedItemProperty,
                new Binding {Path = new PropertyPath(nameField)});
            dockPanel.Children.Add(cbCashs);
            item.Content = dockPanel;
            item.Width = dockPanel.Width + 150;
            return cbCashs;
        }
    }
}