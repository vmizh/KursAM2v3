using System.ComponentModel;
using System.Windows;
using LayoutManager;

namespace KursAM2.View.KursReferences
{
    /// <summary>
    ///     Interaction logic for OffBalanseSheetTypeReferenceView.xaml
    /// </summary>
    public partial class OffBalanseSheetTypeReferenceView : ILayout
    {
        public OffBalanseSheetTypeReferenceView()
        {
            InitializeComponent();
            LayoutManager = new LayoutManager.LayoutManager(GetType().Name, this, null);
            Loaded += myLoaded;
            Closing += myClosing;
        }
        private void myClosing(object sender, CancelEventArgs e)
        {
            LayoutManager.Save();
        }

        private void myLoaded(object sender, RoutedEventArgs e)
        {
            LayoutManager.Load();
        }

        public LayoutManagerBase LayoutManager { get; set; }
        public string LayoutManagerName { get; set; }
    }
}