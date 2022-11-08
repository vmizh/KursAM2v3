using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Core.ViewModel.Base;
using KursDomain.Documents.Bank;
using KursDomain.Documents.Cash;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.View.DialogUserControl
{
    public sealed class CashOrdersForBankSelectDialog : RSWindowViewModelBase, IDataUserControl
    {
        private readonly BankAccount bankAcc;
        private readonly SelectCashOrdersDelegate selectCashOrders;


        private CashOrder myCurrentDocument;
        private CashOrdersForBankSelectDialogUC myDataUserControl;

        private DateTime myDateEnd;

        private DateTime myDateStart;

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
