using System.Windows;
using LayoutManager;

namespace KursAM2.View.Logistiks.UC
{
    /// <summary>
    ///     Interaction logic for NomenklAddFromNotShippedUC.xaml
    /// </summary>
    public partial class NomenklAddFromNotShippedUC : ILayout
    {
        public NomenklAddFromNotShippedUC()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, mainControl);
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