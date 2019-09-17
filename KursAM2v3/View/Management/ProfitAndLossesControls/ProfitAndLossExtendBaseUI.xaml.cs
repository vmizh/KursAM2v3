using System.Windows;
using System.Windows.Controls;
using LayoutManager;

namespace KursAM2.View.Management.ProfitAndLossesControls
{
    /// <summary>
    ///     Interaction logic for ProfitAndLossExtendBaseUI.xaml
    /// </summary>
    public partial class ProfitAndLossExtendBaseUI : ILayout
    {
        public ProfitAndLossExtendBaseUI()
        {
            InitializeComponent();
            Loaded += BalansCompareFinanseUI_Loaded;
            Unloaded += BalansCompareFinanseUI_Unloaded;
            LayoutManager = new LayoutManager.LayoutManager("KursAM2.View.Management.ProfitAndLossesControls.ProfitAndLossExtendBaseUI",
                GridControlExtend);
        }

        private void BalansCompareFinanseUI_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void BalansCompareFinanseUI_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load(true);
        }

        public LayoutManagerBase LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
    }
}