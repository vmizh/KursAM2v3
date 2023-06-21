using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Core;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Period
{
    /// <summary>
    ///     Interaction logic for PeriodCloseManagementView.xaml
    /// </summary>
    public partial class PeriodCloseManagementView : ILayout
    {
        public PeriodCloseManagementView()
        {
            InitializeComponent(); 
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, mainLayoutControl);
            Loaded += PeriodCloseManagemen_Loaded;
            Closing += PeriodCloseManagemen_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void PeriodCloseManagemen_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void PeriodCloseManagemen_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }
    }
}
