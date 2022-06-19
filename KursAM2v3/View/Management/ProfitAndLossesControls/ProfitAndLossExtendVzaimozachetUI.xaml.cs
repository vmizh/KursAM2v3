using System.Windows;
using DevExpress.Xpf.Core;
using LayoutManager;

namespace KursAM2.View.Management.ProfitAndLossesControls
{
    /// <summary>
    ///     Interaction logic for ProfitAndLossExtendBaseUI.xaml
    /// </summary>
    public partial class ProfitAndLossExtendVzaimozchetUI : ILayout
    {
        public ProfitAndLossExtendVzaimozchetUI()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            Loaded += BalansCompareFinanseUI_Loaded;
            Unloaded += BalansCompareFinanseUI_Unloaded;
            LayoutManager = new LayoutManager.LayoutManager(
                "KursAM2.View.Management.ProfitAndLossesControls.ProfitAndLossExtendVzaimozchetUI",
                GridControlExtend);
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void BalansCompareFinanseUI_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void BalansCompareFinanseUI_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load(true);
        }
    }
}
