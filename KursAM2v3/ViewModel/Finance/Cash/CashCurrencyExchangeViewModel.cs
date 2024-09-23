using System;
using System.Collections.ObjectModel;
using System.Windows;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Helper;
using KursAM2.Managers;
using KursAM2.View.Finance.Cash;
using KursAM2.View.Helper;
using KursDomain;
using KursDomain.Documents.Cash;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Finance.Cash
{
    public sealed class CashCurrencyExchangeWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        public CashBookView BookView;
        private CashCurrencyExchange myDocument;
        public ObservableCollection<Currency> CurrencyList { get; set; } = new ObservableCollection<Currency>();
        private readonly DateTime oldDate = DateTime.MaxValue;
        private readonly DateTime oldDate2 = DateTime.MaxValue;

        #endregion

        #region Constructors

        public CashCurrencyExchangeWindowViewModel()
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = false;
            IsDocDeleteAllow = true;
            IsCanSaveData = false;
            WindowName = $"Обмен валюты для {Document?.Kontragent} в {Document?.Cash.Name}";
        }

        public CashCurrencyExchangeWindowViewModel(decimal dc) : this()
        {
            DocCode = dc;
            RefreshData(dc);
            oldDate = Document.CH_DATE_IN ?? DateTime.Today;
            oldDate2 = Document.CH_DATE_OUT ?? DateTime.Today;
            LastDocumentManager.SaveLastOpenInfo(DocumentType.CurrencyChange, null, Document.DocCode,
                Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
        }

        #endregion

        #region Properties

        public override bool IsCanSaveData => Document != null && Document.State != RowStatus.NotEdited &&
                                              CashManager.CheckCashCurrencyExchange(Document);

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;

        public CashCurrencyExchange Document
        {
            get => myDocument;
            set
            {
                if (Equals(myDocument,value)) return;
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
            DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.CurrencyChange), null,
                Document.DocCode, null, (string)Document.ToJson());
            LastDocumentManager.SaveLastOpenInfo(DocumentType.CurrencyChange, null, Document.DocCode,
                Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
           
        }

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("Вы уверены, что хотите удалить данный документ?", "Запрос",
                MessageBoxButton.YesNo,
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
            var vm = new CashCurrencyExchangeWindowViewModel
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
            var vm = new CashCurrencyExchangeWindowViewModel
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

        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            DocumentHistoryManager.LoadHistory(DocumentType.CurrencyChange, null, Document.DocCode, null);
        }

        public override void RefreshData(object obj)
        {
            base.RefreshData(obj);
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
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
                    case CashCurrencyExchange model:
                        dc = model.DocCode;
                        break;
                }

                Document = CashManager.LoadCurrencyExchange(dc);
                if (Document == null)
                {
                    var winManager = new WindowManager();
                    winManager.ShowWinUIMessageBox($"Не найден документ с кодом {dc}!",
                        "Ошибка обращения к базе данных", MessageBoxButton.OK, MessageBoxImage.Error,
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
