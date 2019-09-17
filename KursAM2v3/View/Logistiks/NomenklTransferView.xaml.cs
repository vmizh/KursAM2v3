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

        public LayoutManagerBase LayoutManager { get; set; }
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
            RaiseIsRowReadonly();
        }

        private void RaiseIsRowReadonly()
        {
            var ctx = DataContext as NomenklTransferWindowViewModel;
            // ReSharper disable once NotResolvedInText
            ctx?.RaisePropertyChanged("IsRowReadOnly");
        }

        private void GridControl_OnCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
        {
            RaiseIsRowReadonly();
        }

        private void ButtonInfo_Click(object sender, RoutedEventArgs e)
        {
            tableView.CloseEditor();
        }
    }
}