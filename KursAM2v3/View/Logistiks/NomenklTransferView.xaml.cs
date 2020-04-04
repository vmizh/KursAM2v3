using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.Logistiks;
using LayoutManager;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Interaction logic for NomenklTransferView.xaml
    /// </summary>
    public partial class NomenklTransferView : ILayout
    {
        public NomenklTransferView()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Closing += NomenklTransferView_Closing;
            Loaded += NomenklTransferView_Loaded;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void NomenklTransferView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
       }

        private void NomenklTransferView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void tableView_CellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            if (e.Column.FieldName == "IsAccepted")
            {
                RaiseIsRowReadonly((bool)e.Value);
            }
        }

        private void RaiseIsRowReadonly(bool isAccepted)
        {
            //ctx.RaisePropertyChanged("IsRowReadOnly");
            col2.ReadOnly = isAccepted;
            col5.ReadOnly = isAccepted;
            col7.ReadOnly = isAccepted;
            col8.ReadOnly = isAccepted;
            col10.ReadOnly = isAccepted;
            col17_1.ReadOnly = isAccepted;
            col18.ReadOnly = isAccepted;
            tableView.UpdateLayout();
        }

        private void GridControl_OnCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            if (!(DataContext is NomenklTransferWindowViewModel ctx)) return;
            RaiseIsRowReadonly(ctx.CurrentRow.IsAccepted);
        }

        private void ButtonInfo_Click(object sender, RoutedEventArgs e)
        {
            tableView.CloseEditor();
        }
    }
}