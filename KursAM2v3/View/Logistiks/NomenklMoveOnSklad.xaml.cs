using System.ComponentModel;
using System.Windows;
using LayoutManager;

namespace KursAM2.View.Logistiks
{
    /// <summary>
    ///     Interaction logic for NomenklMoveOnSklad.xaml
    /// </summary>
    public partial class NomenklMoveOnSklad : ILayout
    {
        public NomenklMoveOnSklad()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Loaded += NomenklMove_Loaded;
            Closing += NomenklMove_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void ResetLayot()
        {
            throw new System.NotImplementedException();
        }

        private void NomenklMove_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void NomenklMove_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }
    }
}