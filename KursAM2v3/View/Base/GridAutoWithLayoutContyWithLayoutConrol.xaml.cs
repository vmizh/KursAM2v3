using System.Windows;
using DevExpress.Xpf.Core;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for GridAutoWithLayoutContyWithLayoutConrol.xaml
    /// </summary>
    public partial class GridAutoWithLayoutContyWithLayoutConrol : ILayout
    {

        public GridAutoWithLayoutContyWithLayoutConrol()
        {
            InitializeComponent(); 
            
            Unloaded += GridAutoWithLayoutConrol_Unloaded;
            Loaded += GridAutoWithLayoutConrol_Loaded;
        }

        public GridAutoWithLayoutContyWithLayoutConrol(string layoutName, object data, object currentItem) : this()
        {
            LayoutManagerName = layoutName;
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),LayoutManagerName, grid);
            grid.ItemsSource = data;
            grid.CurrentItem = currentItem;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void GridAutoWithLayoutConrol_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager?.Load();
        }

        private void GridAutoWithLayoutConrol_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager?.Save();
        }
    }
}
