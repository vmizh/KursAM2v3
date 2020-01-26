using System.Windows;
using LayoutManager;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Interaction logic for NomAddNomenklToMoveUC.xaml
    /// </summary>
    public partial class NomAddNomenklToMoveUC : ILayout
    {
        public NomAddNomenklToMoveUC()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, mainLayoutControl);
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void ResetLayot()
        {
            throw new System.NotImplementedException();
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