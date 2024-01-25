using System;
using System.Windows.Threading;
using DevExpress.Xpf.Grid;
using KursDomain.ViewModel.Base2;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for TableAddDeleteUserControl.xaml
    /// </summary>
    public partial class TableAddDeleteUserControl
    {
        public TableAddDeleteUserControl(GridControlColumnsAutogenerating onColumnsAutogenerating)
        {
            OnColumnsAutogenerating = onColumnsAutogenerating;
            InitializeComponent();
        }

        private GridControlColumnsAutogenerating OnColumnsAutogenerating { get; }

        private void OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            OnColumnsAutogenerating?.Invoke(sender, e);
        }

        private void tableView_CellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            tableView.PostEditor();
        }

        private void tableView_ShownEditor(object sender, EditorEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(e.Editor.SelectAll), DispatcherPriority.Input);
        }
    }
}
