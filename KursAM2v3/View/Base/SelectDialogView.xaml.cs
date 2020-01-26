using System.ComponentModel;
using System.IO;
using System.Windows;
using Core.ViewModel.Base;
using DevExpress.Xpf.Grid;
using LayoutManager;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for SelectDialogView.xaml
    /// </summary>
    public partial class SelectDialogView : ILayout
    {
        public SelectDialogView()
        {
            InitializeComponent();
            DataContextChanged += SelectDialogView_DataContextChanged;
            Closing += SelectDialogView_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void ResetLayot()
        {
            throw new System.NotImplementedException();
        }

        private void SelectDialogView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void SelectDialogView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = DataContext as IDataUserControl;
            LayoutManager = new LayoutManager.LayoutManager($"{GetType().Name}.{DataContext.GetType()}", this,
                ctrl?.LayoutControl);
            LayoutManager.Load();
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is RSWindowViewModelBase ctx)
            {
                DialogResult = true;
                ctx.DialogResult = true;
                Close();
            }
            else
            {
                DialogResult = false;
                Close();
            }
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is RSWindowViewModelBase ctx)
            {
                DialogResult = false;
                ctx.DialogResult = false;
                Close();
            }
            else
            {
                DialogResult = false;
                Close();
            }
        }

        private void Reset_OnClick(object sender, RoutedEventArgs e)
        {
            var ctrl = DataContext as IDataUserControl;
            var layout = ctrl?.LayoutControl as ILayout;
            layout?.LayoutManager.ResetLayout();
        }
    }
}