using System.Linq;
using System.Windows;
using DevExpress.Xpf.Core;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Management.Controls
{
    /// <summary>
    ///     Interaction logic for ManagementBalansCompareMainUI.xaml
    /// </summary>
    public partial class ManagementBalansCompareMainUI : ILayout
    {
        public ManagementBalansCompareMainUI()
        {
            InitializeComponent(); 
            
            Loaded += ManagementBalansCompareMainUI_Loaded;
            Unloaded += ManagementBalansCompareMainUI_Unloaded;
            LayoutManager = new LayoutManager.LayoutManager(
                GlobalOptions.KursSystem(),"KursAM2.View.Management.Controls.ManagementBalansCompareMainUI", treeListBalans);
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void ManagementBalansCompareMainUI_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void ManagementBalansCompareMainUI_Loaded(object sender, RoutedEventArgs e)
        {
            var k = 0;
            foreach (var column in treeListBalans.Bands.Where(_ => string.IsNullOrEmpty(_.Name)))
                column.Name = "band" + k++;
            LayoutManager.Load();
        }
    }
}
