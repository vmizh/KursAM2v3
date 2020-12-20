using System.ComponentModel;
using System.Windows;
using LayoutManager;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Interaction logic for InventorySheetView.xaml
    /// </summary>
    public partial class InventorySheetView : ILayout
    {
        public InventorySheetView()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Loaded += InventorySheetView_Loaded;
            Closing += InventorySheetView_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void InventorySheetView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void InventorySheetView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }
    }
}