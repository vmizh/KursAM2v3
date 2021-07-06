using DevExpress.Xpf.Core;
using DevExpress.Xpf.Grid;


namespace KursAM2.View.AktSpisaniya
{
    /// <summary>
    /// Interaction logic for AktSpisaniyaSearchView.xaml
    /// </summary>
    public partial class AktSpisaniyaSearchView : ThemedWindow
    {
        public AktSpisaniyaSearchView()
        {
            InitializeComponent();
        }


        private void GridAktSpisaniya_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
            switch (e.Column.FieldName)
            {
                case "State":
                    e.Column.Visible = false;
                    break;
            }
        }
    }
}
