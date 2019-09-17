using System.ComponentModel;
using System.Windows;
using LayoutManager;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Interaction logic for NomenklTransferSearchView.xaml
    /// </summary>
    public partial class NomenklTransferSearchView : ILayout
    {
        public NomenklTransferSearchView()
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
    }
}