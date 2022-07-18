using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;
using LayoutManager;

namespace KursAM2.View.KursReferences
{
    /// <summary>
    ///     Interaction logic for KontragentReference2View.xaml
    /// </summary>
    public partial class KontragentReference2View : ILayout
    {
        public KontragentReference2View()
        {
            InitializeComponent();
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, mainLayoutControl);
            Loaded += KontragentReferenceView_Loaded;
            Closing += KontragentReferenceView_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void KontragentReferenceView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void KontragentReferenceView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void GridControlKontragent_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
        }
    }
}
