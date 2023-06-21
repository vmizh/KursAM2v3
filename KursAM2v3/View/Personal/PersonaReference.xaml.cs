using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Core;
using KursDomain;
using LayoutManager;

namespace KursAM2.View.Personal
{
    /// <summary>
    ///     Interaction logic for PersonaReference.xaml
    /// </summary>
    public partial class PersonaReference : ILayout
    {
        public PersonaReference()
        {
            InitializeComponent(); 
            LayoutManager = new LayoutManager.LayoutManager(GlobalOptions.KursSystem(),GetType().Name, this, gridControl);
            Loaded += PersonaReference_Loaded;
            Closing += PersonaReference_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void PersonaReference_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void PersonaReference_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }
    }
}
