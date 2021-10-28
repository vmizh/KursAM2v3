using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DevExpress.Xpf.Grid;
using KursAM2.ViewModel.Finance.AccruedAmount;

namespace KursAM2.ViewModel.Finance
{
    /// <summary>
    ///     Interaction logic for SelectCashBankDialogView.xaml
    /// </summary>
    public partial class SelectCashBankDialogView
    {
        public SelectCashBankDialogView()
        {
            InitializeComponent();
        }

        private void DataControlBase_OnAutoGeneratingColumn(object sender, AutoGeneratingColumnEventArgs e)
        {
            e.Column.ReadOnly = true;
            e.Column.Name = e.Column.FieldName;
            switch (e.Column.Name)
            {
                case "Name":
                    e.Column.Width = new GridColumnWidth(100, GridColumnUnitType.Auto);
                    break;
            }
        }

        private void TableView_OnFocusedRowChanging(object sender, CanceledEventArgs e)
        {
            var c = gridControl.GetRow(e.NewRowHandle) as SelectCashBankDialogViewModel.CashBankItem;
            if (c == null)
                return;
            if (c.Summa <= 5)
                e.Cancel = true;
        }
    }

    public delegate void CanceledEventHandler(object sender, CanceledEventArgs e);

    public class MyTableView : TableView
    {
        static MyTableView()
        {
            FocusedRowHandleProperty.OverrideMetadata(typeof(MyTableView), new FrameworkPropertyMetadata(
                DataControlBase.InvalidRowHandle, null,
                (d, e) => ((MyTableView)d).CoerceFocusedRowHandle((int)e)));
        }

        public event CanceledEventHandler FocusedRowChanging;

        private object CoerceFocusedRowHandle(int value)
        {
            if (FocusedRowHandle == value) return value;
            if (FocusedRowChanging != null)
            {
                var e = new CanceledEventArgs(FocusedRowHandle, value);
                FocusedRowChanging(this, e);
                if (e.Cancel)
                    return FocusedRowHandle;
            }

            return value;
        }
    }

    public class CanceledEventArgs : EventArgs
    {
        public CanceledEventArgs(int oldRowHandle, int newRowHandle)
        {
            OldRowHandle = oldRowHandle;
            NewRowHandle = newRowHandle;
            Cancel = false;
        }

        public int NewRowHandle { get; }
        public int OldRowHandle { get; }
        public bool Cancel { get; set; }
    }
}