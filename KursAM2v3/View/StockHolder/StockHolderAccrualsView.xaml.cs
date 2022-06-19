using System.Linq;
using Core;
using Core.ViewModel.Base;
using DevExpress.Xpf.Core;
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
            ApplicationThemeHelper.ApplicationThemeName = Theme.MetropolisLightName;
        }

        private void GridControlStockAccruals_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.Name = e.Column.FieldName;
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
            e.Column.Name = e.Column.FieldName;
            e.Column.ReadOnly = true;
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
                if (dtx.State != RowStatus.NewRow)
                {
                    dtx.Document.State = RowStatus.Edited;
                    dtx.Document.RaisePropertyChanged("State");
                    GridControlStockHolder.UpdateTotalSummary();
                }
            } 
        }

        private void TableViewControlStockAccruals_OnCellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            GridControlStockHolder.UpdateTotalSummary();
            if (DataContext is StockHolderAccrualWindowViewModel ctx)
            {
                foreach (var sh in ctx.StockHolders)
                {
                    sh.RaisePropertyAllChanged();
                }
            }
        }

        private void DateEdit_EditValueChanging(object sender, DevExpress.Xpf.Editors.EditValueChangingEventArgs e)
        {
            if (DataContext is StockHolderAccrualWindowViewModel dtx)
            {
                dtx.Document.RaisePropertyChanged("State");
                dtx.RaisePropertyChanged("IsCanSaveData");
                //if (dtx.State != RowStatus.NewRow)
                //{
                //    dtx.Document.State = RowStatus.Edited;
                //    dtx.Document.RaisePropertyChanged("State");
                //    
                //}
            } 
        }
    }
}
