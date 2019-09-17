using System.Windows;
using LayoutManager;

namespace KursAM2.View.Finance.UC
{
    /// <summary>
    ///     Interaction logic for SchetFacturaProviderSelectUC.xaml
    /// </summary>
    public partial class SchetFacturaProviderSelectUC : ILayout
    {
        public SchetFacturaProviderSelectUC()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, mainLyoutControl);
        }

        public LayoutManagerBase LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }
    }
}