using System;
using System.Collections.ObjectModel;
using System.Windows;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using KursAM2.Managers;
using KursAM2.View.Finance.Cash;

namespace KursAM2.ViewModel.Finance.Cash
{
    public class CashCurrencyExchangeViewModel : RSWindowViewModelBase
    {
        #region Fields

        public CashBookView BookView;
        private SD_251ViewModel myDocument;
        public ObservableCollection<Currency> CurrencyList { get; set; } = new ObservableCollection<Currency>();
        private readonly DateTime oldDate = DateTime.MaxValue;
        private readonly DateTime oldDate2 = DateTime.MaxValue;

        #endregion

        #region Constructors

        public CashCurrencyExchangeViewModel()
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = false;
            IsDocDeleteAllow = true;
            IsCanSaveData = false;
            WindowName = $"����� ������ ��� {Document?.Kontragent} � {Document?.Cash.Name}";
        }

        public CashCurrencyExchangeViewModel(decimal dc) : this()
        {
            DocCode = dc;
            RefreshData(dc);
            oldDate = Document.CH_DATE_IN ?? DateTime.Today;
            oldDate2 = Document.CH_DATE_OUT ?? DateTime.Today;
        }

        #endregion

        #region Properties


        public override bool IsCanSaveData => Document != null && Document.State != RowStatus.NotEdited &&
                                              CashManager.CheckCashCurrencyExchange(Document);

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;

        public SD_251ViewModel Document
        {
            get => myDocument;
            set
            {
                if (myDocument != null && myDocument.Equals(value)) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

        public override void SaveData(object data)
        {
            if (Document.State == RowStatus.Edited)
                CashManager.UpdateDocument(CashDocumentType.CurrencyExchange, Document, oldDate, oldDate2);
            if (Document.State == RowStatus.NewRow)
                CashManager.InsertDocument(CashDocumentType.CurrencyExchange, Document);
            if (BookView?.DataContext is CashBookWindowViewModel ctx)
                ctx.RefreshActual(Document);
        }

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("�� �������, ��� ������ ������� ������ ��������?", "������",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    var ctx = BookView.DataContext as CashBookWindowViewModel;
                    CashManager.DeleteDocument(CashDocumentType.CurrencyExchange, Document);
                    ctx?.RefreshActual(Document);
                    CloseWindow(Form);
                    break;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }
        }

        public override void DocNewEmpty(object form)
        {
            var vm = new CashCurrencyExchangeViewModel
            {
                Document = CashManager.NewCashCurrencyEchange()
            };
            vm.Document.Cash = Document.Cash;
            vm.BookView = BookView;
            DocumentsOpenManager.Open(DocumentType.CashIn, vm, BookView);
        }

        public override void DocNewCopy(object form)
        {
            if (Document == null) return;
            var vm = new CashCurrencyExchangeViewModel
            {
                Document = CashManager.NewCopyCashCurrencyExchange(Document.DocCode)
            };
            vm.Document.Cash = Document.Cash;
            vm.BookView = BookView;
            DocumentsOpenManager.Open(DocumentType.CashIn, vm, BookView);
        }

        public void CreateMenu()
        {
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("� �������� ���� ������� ���������, ���������?", "������",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }

            if (Document?.DocCode > 0)
            {
                Document = CashManager.LoadCurrencyExchange(Document.DOC_CODE);
                RaisePropertyChanged(nameof(Document));
            }
            else
            {
                if (obj == null) return;
                decimal dc = 0;
                switch (obj)
                {
                    case decimal dec:
                        dc = dec;
                        break;
                    case SD_251ViewModel model:
                        dc = model.DocCode;
                        break;
                }

                Document = CashManager.LoadCurrencyExchange(dc);
                if (Document == null)
                {
                    WinManager.ShowWinUIMessageBox($"�� ������ �������� � ����� {dc}!",
                        "������ ��������� � ���� ������", MessageBoxButton.OK, MessageBoxImage.Error,
                        MessageBoxResult.None, MessageBoxOptions.None);
                    return;
                }

                RaisePropertyChanged(nameof(Document));
            }

            Document.myState = RowStatus.NotEdited;
        }

        #endregion
    }
}