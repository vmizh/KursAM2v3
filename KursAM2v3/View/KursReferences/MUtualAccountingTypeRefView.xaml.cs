using System.Windows;
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
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, gridControl);
            Loaded += MUtualAccountingTypeRefView_Loaded;
            Unloaded += MUtualAccountingTypeRefView_Unloaded;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
 
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