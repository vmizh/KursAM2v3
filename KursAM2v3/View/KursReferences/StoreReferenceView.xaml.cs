using System.ComponentModel;
using System.Windows;
using DevExpress.Xpf.Grid;

namespace KursAM2.View.KursReferences
{
    /// <summary>
    ///     Interaction logic for StoreReferenceView.xaml
    /// </summary>
    public partial class StoreReferenceView
    {
        public StoreReferenceView()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, gridControl);
            //Loaded += StoreReferenceView_Loaded;
            //Closing += StoreReferenceView_Closing;
        }

        public LayoutManager.LayoutManager LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }

        public void SaveLayout()
        {
            LayoutManager.Save();
        }

        private void StoreReferenceView_Closing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void StoreReferenceView_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        private void GridControlUsers_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }
    }
}