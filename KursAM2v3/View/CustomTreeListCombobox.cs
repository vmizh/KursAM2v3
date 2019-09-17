// Developer Express Code Central Example:
// Implement a custom ComboBoxEdit with TreeView as a PopupContent
// 
// Show how to implement a custom ComboBoxEdit with TreeView as a PopupContent and
// use it inside DXGrid as a custom column.
// 
// You can find sample updates and versions for different programming languages here:
// http://www.devexpress.com/example=E3106

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Editors.EditStrategy;
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Editors.Internal;
using DevExpress.Xpf.Editors.Native;
using DevExpress.Xpf.Editors.Settings;

namespace KursAM2.View
{
    //public partial class MainWindow : Window {
    //    public MainWindow() {
    //        InitializeComponent();
    //    }

    //    private void Button_Click(object sender, RoutedEventArgs e)
    //    {
    //        TestData staticVar = ((customControl.ItemsSource as IList)[3] as TestData);
    //        customControl.EditValue = staticVar.Item1;          
    //    }
    //}

    public class TreeViewComboBoxEdit : ComboBoxEdit
    {
        static TreeViewComboBoxEdit()
        {
            var ownerType = typeof(TreeViewComboBoxEdit);
            DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
            EditorSettingsProvider.Default.RegisterUserEditor(
                typeof(TreeViewComboBoxEdit),
                typeof(TreeViewComboBoxEditSettings),
                () => new TreeViewComboBoxEdit(),
                () => new TreeViewComboBoxEditSettings());
        }

        protected override EditStrategyBase CreateEditStrategy()
        {
            return new TreeViewComboBoxEditStrategy(this);
        }
    }

    public class TreeViewComboBoxEditSettings : ComboBoxEditSettings
    {
        static TreeViewComboBoxEditSettings()
        {
            EditorSettingsProvider.Default.RegisterUserEditor(
                typeof(TreeViewComboBoxEdit),
                typeof(TreeViewComboBoxEditSettings),
                () => new TreeViewComboBoxEdit(),
                () => new TreeViewComboBoxEditSettings());
        }
    }

    public class TreeViewComboBoxEditStrategy : ComboBoxEditStrategy
    {
        public TreeViewComboBoxEditStrategy(ComboBoxEdit editor) : base(editor)
        {
        }

        protected VisualClientOwner CreateVisualClient()
        {
            return new TreeViewVisualClientOwner((TreeViewComboBoxEdit) Editor);
        }
    }

    public class TreeViewVisualClientOwner : VisualClientOwner
    {
        public TreeViewVisualClientOwner(ComboBoxEdit editor) : base(editor)
        {
        }

        private new ComboBoxEdit Editor => base.Editor as ComboBoxEdit;
        private TreeView TreeView => InnerEditor as TreeView;

        protected override FrameworkElement FindEditor()
        {
            if (LookUpEditHelper.GetPopupContentOwner(Editor).Child == null)
                return null;
            return LayoutHelper.FindElementByName(LookUpEditHelper.GetPopupContentOwner(Editor).Child, "PART_Content");
        }

        public override bool ProcessKeyDownInternal(KeyEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override object GetSelectedItem()
        {
            if (!IsLoaded)
                return null;
            return TreeView.SelectedItem;
        }

        public override IEnumerable GetSelectedItems()
        {
            if (!IsLoaded)
                return null;
            return new List<object> {TreeView.SelectedItem};
        }

        protected override void SetupEditor()
        {
            if (!IsLoaded)
                return;
            SyncProperties(true);
        }

        public override void SyncProperties(bool syncDataSource)
        {
            if (!IsLoaded)
                return;
            if (syncDataSource)
                TreeView.ItemsSource = (IEnumerable) ((ISelectorEdit) Editor).GetPopupContentItemsSource();
            TreeView.SelectedValuePath = Editor.ValueMember;
            TreeView.DisplayMemberPath = Editor.DisplayMember;
        }

        public override void PopupOpened()
        {
            base.PopupOpened();
            // ReSharper disable once RedundantDelegateCreation
            TreeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(TreeView_SelectedItemChanged);
        }

        public override void PopupClosed()
        {
            // ReSharper disable once RedundantDelegateCreation
            TreeView.SelectedItemChanged -= new RoutedPropertyChangedEventHandler<object>(TreeView_SelectedItemChanged);
            base.PopupClosed();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Editor.ClosePopup();
        }
    }
}