using System.Linq;
using Core;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.StockHolder;

namespace KursAM2.View.StockHolder
{
    /// <summary>
    ///     Interaction logic for StockHolderAccrualsView.xaml
    /// </summary>
    public partial class StockHolderAccrualsView
    {
        public StockHolderAccrualsView()
        {
            InitializeComponent();
        }

        private void GridControlStockAccruals_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            var dtx = DataContext as StockHolderAccrualWindowViewModel;
            switch (e.Column.FieldName)
            {
                case "Currency":
                    e.Column.EditSettings = new ComboBoxEditSettings
                    {
                        IsTextEditable = false,
                        ItemsSource = MainReferences.Currencies.Values.ToList()
                    };
                    break;
                case "AccrualType":
                    if (dtx != null)
                    {
                        e.Column.EditSettings = new ComboBoxEditSettings
                        {
                            IsTextEditable = false,
                            ItemsSource = dtx.StockHolderAccruelTypes
                        };
                    }
                    break;
                case "StockHolder":
                    e.Column.Visible = false;
                    break;
            }
        }

        private void GridControlStockHolder_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            //switch (e.Column.FieldName)
            //{
                
            //}
        }

        private void TableViewControlStockAccruals_CellValueChanging(object sender, CellValueChangedEventArgs e)
        {
            if (DataContext is StockHolderAccrualWindowViewModel dtx)
            {
                foreach (var sh in dtx.StockHolders)
                {
                    sh.RaisePropertyAllChanged();
                }
                dtx.SetVisibleColums();
            } 
        }
    }
}