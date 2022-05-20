using DevExpress.Xpf.Grid;

namespace KursAM2.View.Base
{
    /// <summary>
    ///     Interaction logic for StandartSearchView.xaml
    /// </summary>
    public partial class StandartSearchView
    {
        public StandartSearchView()
        {
            InitializeComponent();
        }

        private void GridDocuments_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
        }
    }
}
