using System.Windows;
using System.Windows.Input;
using KursAM2.ViewModel.Logistiks;
using LayoutManager;

namespace KursAM2.View.Logistiks.UC
{
    /// <summary>
    ///     Interaction logic for SelectNomenkls.xaml
    /// </summary>
    public partial class SelectWarehouse : ILayout
    {
        public SelectWarehouse()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, treeListWarehouse);
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void ButtonEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(DataContext is WarehouseSelectDialogViewModel ctx)) return;
            if (e.Key == Key.Enter)
                ctx.SearchExecute(null);
        }
    }
}