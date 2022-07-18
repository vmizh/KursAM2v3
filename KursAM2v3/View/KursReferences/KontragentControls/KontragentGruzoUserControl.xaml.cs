using System.Windows;
using DevExpress.Xpf.Core;
using LayoutManager;

namespace KursAM2.View.KursReferences.KontragentControls
{
    /// <summary>
    ///     Interaction logic for KontragentGruzoUserControl.xaml
    /// </summary>
    public partial class KontragentGruzoUserControl : ILayout
    {
        public KontragentGruzoUserControl()
        {
            InitializeComponent();
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            LayoutManager = new LayoutManager.LayoutManager("KontragentGruzoUserControl", LayoutControl);
            Loaded += KontragentGruzo_Loaded;
            Unloaded += KontragentGruzo_Unloaded;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            //throw new System.NotImplementedException();
        }

        private void KontragentGruzo_Unloaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Save();
        }

        private void KontragentGruzo_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }
    }
}
