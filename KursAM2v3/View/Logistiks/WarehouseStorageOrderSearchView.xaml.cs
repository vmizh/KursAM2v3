using System.ComponentModel;
using System.Windows;
using LayoutManager;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Interaction logic for WarehouseStorageOrderSearchView.xaml
    /// </summary>
    public partial class WarehouseStorageOrderSearchView : ILayout
    {
        public WarehouseStorageOrderSearchView()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Loaded += SearchBaseView_Loaded;
            Closing += SearchBaseView_Closing;
        }

        public LayoutManagerBase LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void SearchBaseView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void SearchBaseView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }
    }
}