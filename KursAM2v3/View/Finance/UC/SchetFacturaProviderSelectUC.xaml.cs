using System.Windows;
using DevExpress.Xpf.Core;
using KursDomain;
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
            
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, mainLyoutControl);
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

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
