using System.Windows;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;

namespace KursAM2.View.StockHolder
{
    /// <summary>
    ///     Interaction logic for StockHolderAccrualTypeView.xaml
    /// </summary>
    public partial class StockHolderAccrualTypeView
    {
        public StockHolderAccrualTypeView()
        {
            InitializeComponent(); 
            
        }

        private void GridControlStockHolderAccrual_OnAutoGeneratingColumn(object sender,
            AutoGeneratingColumnEventArgs e)
        {
            switch (e.Column.FieldName)
            {
                case "Note":
                    e.Column.EditSettings = new TextEditSettings
                    {
                        AcceptsReturn = true,
                        TextWrapping = TextWrapping.Wrap
                    };
                    break;
            }
        }
    }
}
