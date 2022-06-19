using System.Windows;
using DevExpress.Xpf.Core;
using LayoutManager;

namespace KursAM2.View.KursReferences.UC
{
    /// <summary>
    ///     Interaction logic for UsersUC.xaml
    /// </summary>
    public partial class UsersUC : ILayout
    {
        public UsersUC()
        {
            InitializeComponent(); 
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, mainControl);
            Loaded += UserControl_Loaded;
            Unloaded += UserControl_Unloaded;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
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
