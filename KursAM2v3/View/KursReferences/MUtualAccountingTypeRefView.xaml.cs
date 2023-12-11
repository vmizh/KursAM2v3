using System.Windows;
using DevExpress.Xpf.Core;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.KursReferences
{
    /// <summary>
    ///     Interaction logic for MUtualAccountingTypeRefView.xaml
    /// </summary>
    public partial class MUtualAccountingTypeRefView : ILayout
    {
        public MUtualAccountingTypeRefView()
        {
            InitializeComponent();
            
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, gridControl);
            Loaded += MUtualAccountingTypeRefView_Loaded;
            Unloaded += MUtualAccountingTypeRefView_Unloaded;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void MUtualAccountingTypeRefView_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void MUtualAccountingTypeRefView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }
    }
}
