using System.Windows;
using LayoutManager;

namespace KursAM2.View.Personal
{
    /// <summary>
    ///     Interaction logic for PersonaAddUserRightsUC.xaml
    /// </summary>
    public partial class PersonaAddUserRightsUC : ILayout
    {
        public PersonaAddUserRightsUC()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, gridControl);
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