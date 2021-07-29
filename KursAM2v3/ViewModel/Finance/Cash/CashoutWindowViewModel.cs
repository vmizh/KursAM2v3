using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Core;
using Core.EntityViewModel.Cash;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Core.Menu;
using Core.ViewModel.Base;
using Helper;
using KursAM2.Managers;
using KursAM2.View.Finance.Cash;
using KursAM2.View.Helper;
using KursAM2.ViewModel.Management.Calculations;

namespace KursAM2.ViewModel.Finance.Cash
{
    public sealed class CashOutWindowViewModel : RSWindowViewModelBase
    {
        #region Methods

        public void CreateMenu()
        {
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
        }

        #endregion

        #region Fields

        private CashOut myDocument;
        public CashBookView BookView;
        private readonly DateTime oldDate = DateTime.MaxValue;
        public decimal OldSumma;

        #endregion

        #region Constructors

        public CashOutWindowViewModel()
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = false;
            IsDocDeleteAllow = true;
            IsCanSaveData = false;
            WindowName = $"Расходный кассовый ордер от {Document?.Kontragent} в {Document?.Cash?.Name}";
        }

        public CashOutWindowViewModel(decimal dc) : this()
        {
            RefreshData(dc);
            // ReSharper disable once PossibleInvalidOperationException
            oldDate = (DateTime) Document.DATE_ORD;
        }

        #endregion

        #region Properties

        public ObservableCollection<Currency> CurrencyList { get; set; } = new ObservableCollection<Currency>();

        public override bool IsCanSaveData
        {
            get
            {
                if (Form is CashOutView frm)
                {
                    if (frm.KontrSelectButton != null)
                        frm.KontrSelectButton.IsEnabled =
                            Document?.Cash != null && Document.KontragentType != CashKontragentType.NotChoice;
                    if (Document != null)
                    {
                        frm.NCODEItem.IsEnabled = Document.KontragentType == CashKontragentType.Employee;
                        frm.SFactNameItem.IsEnabled = Document.KontragentType == CashKontragentType.Kontragent;
                        if (Document.KontragentType != CashKontragentType.Employee)
                            Document.NCODE = null;
                        if (Document.CASH_TO_DC != null)
                        {
                            frm.Sumordcont.IsEnabled = true;
                            var din = GlobalOptions.GetEntities().SD_33
                                .FirstOrDefault(_ => _.RASH_ORDER_FROM_DC == Document.DocCode);
                            if (din != null)
                                frm.Sumordcont.IsEnabled = false;
                        }
                        if (Document.IsBackCalc)
                        {
                            frm.Sumordcont.IsEnabled = false;
                            frm.SumKontrcont.IsEnabled = true;
                        }
                        else
                        {
                            frm.SumKontrcont.IsEnabled = false;
                        }
                    }
                }

                return Document != null && Document.State != RowStatus.NotEdited &&
                       CashManager.CheckCashOut(Document) && Document.State != RowStatus.Deleted;
            }
        }


        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;

        public CashOut Document
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

        #region Commands

        public override bool IsDocNewCopyRequisiteAllow => State != RowStatus.NewRow;

        public override void SaveData(object data)
        {
            if (Document.State == RowStatus.Edited)
                CashManager.UpdateDocument(CashDocumentType.CashOut, Document, oldDate);
            if (Document.State == RowStatus.NewRow)
                CashManager.InsertDocument(CashDocumentType.CashOut, Document);
            if (BookView?.DataContext is CashBookWindowViewModel ctx)
                ctx.RefreshActual(Document);
            DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.CashOut), null,
                Document.DocCode, null, (string)Document.ToJson());
            if (Document.KONTRAGENT_DC != null)
                // ReSharper disable once PossibleInvalidOperationException
                RecalcKontragentBalans.CalcBalans((decimal) Document.KONTRAGENT_DC, (DateTime) Document.DATE_ORD);
            DocumentsOpenManager.SaveLastOpenInfo(DocumentType.CashOut, Document.Id, Document.DocCode, Document.CREATOR,
                "", Document.Description);
        }

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("Вы уверены, что хотите удалить данный документ?", "Запрос",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;
            switch (res)
            {
                case MessageBoxResult.Yes:
                    if (Document.SPOST_DC != null)
                    {
                        var res1 = MessageBox.Show("Есть связь с оплаченным счетом? Удалить?", "Запрос",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                        switch (res1)
                        {
                            case MessageBoxResult.Yes:
                                var ctx = BookView?.DataContext as CashBookWindowViewModel;
                                CashManager.DeleteDocument(CashDocumentType.CashOut, Document);
                                if (Document.KONTRAGENT_DC != null)
                                    RecalcKontragentBalans.CalcBalans((decimal) Document.KONTRAGENT_DC,
                                        // ReSharper disable once PossibleInvalidOperationException
                                        (DateTime) Document.DATE_ORD);
                                ctx?.RefreshActual(Document);
                                CloseWindow(Form);
                                return;
                            case MessageBoxResult.No:
                                return;
                        }
                    }

                    var ctx1 = BookView?.DataContext as CashBookWindowViewModel;
                    CashManager.DeleteDocument(CashDocumentType.CashOut, Document);
                    ctx1?.RefreshActual(Document);
                    CloseWindow(Form);
                    return;
                // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
                case MessageBoxResult.No:
                    return;
            }
        }

        public override void CloseWindow(object form)
        {
            if (IsCanSaveData && Document?.State != RowStatus.Deleted)
            {
                var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            if (Form != null)
            {
                Form.Close();
                return;
            }

            var frm = form as Window;
            frm?.Close();
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
                Document = CashManager.LoadCashOut(Document.DocCode);
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
                    case CashOut model:
                        dc = model.DocCode;
                        break;
                }

                Document = CashManager.LoadCashOut(dc);
                if (Document == null)
                {
                    WinManager.ShowWinUIMessageBox($"Не найден документ с кодом {dc}!",
                        "Ошибка обращения к базе данных", MessageBoxButton.OK, MessageBoxImage.Error,
                        MessageBoxResult.None, MessageBoxOptions.None);
                    return;
                }

                RaisePropertyChanged(nameof(Document));
            }

            Document.myState = RowStatus.NotEdited;
            // ReSharper disable once PossibleInvalidOperationException
            OldSumma = Document.SUMM_ORD ?? 0;
        }

        public override void DocNewEmpty(object form)
        {
            var vm = new CashOutWindowViewModel
            {
                Document = CashManager.NewCashOut()
            };
            vm.Document.Cash = Document.Cash;
            vm.BookView = BookView;
            DocumentsOpenManager.Open(DocumentType.CashOut, vm, BookView);
        }

        public override void DocNewCopy(object form)
        {
            if (Document == null) return;
            var vm = new CashOutWindowViewModel
            {
                Document = CashManager.NewCopyCashOut(Document.DocCode)
            };
            vm.Document.Cash = Document.Cash;
            vm.BookView = BookView;
            DocumentsOpenManager.Open(DocumentType.CashOut, vm, BookView);
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (Document == null) return;
            var vm = new CashOutWindowViewModel
            {
                Document = CashManager.NewRequisisteCashOut(Document.DocCode)
            };
            vm.Document.Cash = Document.Cash;
            vm.BookView = BookView;
            DocumentsOpenManager.Open(DocumentType.CashOut, vm, BookView);
        }

        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            DocumentHistoryManager.LoadHistory(DocumentType.CashOut, null, Document.DocCode, null);
        }

        #endregion
    }
}