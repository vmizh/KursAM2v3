using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using KursDomain.Documents.Bank;
using KursDomain.Documents.Cash;

namespace KursAM2.View.DialogUserControl
{
    public sealed class CashOrdersForBankSelectDialog : RSWindowViewModelBase, IDataUserControl
    {
        private CashOrdersForBankSelectDialogUC myDataUserControl;
        private readonly SelectCashOrdersDelegate selectCashOrders;
        private readonly BankAccount bankAcc;

        public CashOrdersForBankSelectDialog(BankAccount bankAcc, SelectCashOrdersDelegate selectMethod)
        {
            this.bankAcc = bankAcc;
            DateEnd = DateTime.Today;
            DateStart = DateTime.Today.AddDays(-30);
            selectCashOrders = selectMethod;
            LayoutControl = myDataUserControl = new CashOrdersForBankSelectDialogUC();
            RefreshData(null);
            WindowName = "Выбор кассовых ордеров для банка";
            RightMenuBar = MenuGenerator.RefreshOnlyRightBar(this);
        }
        
        private DateTime myDateStart;
        public DateTime DateStart
        {
            get => myDateStart;
            set
            {
                if (myDateStart == value) return;
                myDateStart = value;
                RaisePropertyChanged();
            }
        }
        
        private DateTime myDateEnd;
        public DateTime DateEnd
        {
            get => myDateEnd;
            set
            {
                if (myDateEnd == value) return;
                myDateEnd = value;
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public List<CashOrder> Documents { set; get; } = new List<CashOrder>();

        public ObservableCollection<CashOrder> SelectedDocuments { set; get; } = new ObservableCollection<CashOrder>();


        private CashOrder myCurrentDocument;
        public CashOrder CurrentDocument
        {
            get => myCurrentDocument;
            set
            {
                if (myCurrentDocument == value) return;
                myCurrentDocument = value;
                RaisePropertyChanged();
            }
        }


        public CashOrdersForBankSelectDialogUC DataUserControl
        {
            get => myDataUserControl;
            set
            {
                if (myDataUserControl == value) return;
                myDataUserControl = value;
                RaisePropertyChanged();
            }
        }
        public DependencyObject LayoutControl { get; }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            Documents.Clear();
            Documents.AddRange(selectCashOrders(bankAcc.DocCode, DateStart, DateEnd));
            myDataUserControl.gridDocument.ItemsSource = Documents;
            myDataUserControl.gridDocument.RefreshData();
        }
    }
}
