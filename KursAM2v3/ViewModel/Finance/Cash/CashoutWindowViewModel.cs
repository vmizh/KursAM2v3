using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using Core.Helper;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.Finance.AccruedAmount;
using KursAM2.View.Finance.Cash;
using KursAM2.View.Helper;
using KursAM2.ViewModel.Finance.AccruedAmount;
using KursAM2.ViewModel.Management.Calculations;
using KursDomain;
using KursDomain.Documents.Cash;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace KursAM2.ViewModel.Finance.Cash
{
    public sealed class CashOutWindowViewModel : RSWindowViewModelBase
    {
        #region Methods

        private void ShowNotify(string notify)
        {
            if (string.IsNullOrWhiteSpace(notify)) return;
            var msg = JsonConvert.DeserializeObject<RedisMessage>(notify);
            if (msg == null || msg.UserId == GlobalOptions.UserInfo.KursId) return;
            if (msg.DocCode == Document.DocCode)
            {
                NotifyInfo = msg.Message;
                var notification = KursNotyficationService.CreateCustomNotification(this);
                notification.ShowAsync();
            }
        }


        public void CreateMenu()
        {
            LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
        }

        public void SelectStockHolder()
        {
            var service = this.GetService<IDialogService>("DialogServiceUI");
            var sh = StandartDialogs.SelectStockHolder(service);
            if (sh != null)
            {
                Document.KONTRAGENT_DC = null;
                Document.BankAccount = null;
                Document.CashTo = null;
                Document.Employee = null;
                Document.StockHolder = sh;
                Document.NAME_ORD = sh.Name;
                Document.SPostName = null;
                Document.SPOST_DC = null;
                Document.RaisePropertyChanged("Kontragent");
            }
        }

        #endregion

        #region Fields

        private CashOut myDocument;
        public Window BookView;
        private readonly DateTime oldDate = DateTime.MaxValue;
        public decimal OldSumma;
        private readonly decimal? oldKontrDC;

        private readonly ConnectionMultiplexer redis;
        private readonly ISubscriber mySubscriber;

        #endregion

        #region Constructors

        public CashOutWindowViewModel()
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = false;
            IsDocDeleteAllow = true;
            IsCanSaveData = false;
            WindowName = $"Расходный кассовый ордер от {Document?.Kontragent} в {Document?.Cash?.Name}";
            try
            {
                redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
                mySubscriber = redis.GetSubscriber();

                if (mySubscriber.IsConnected())
                    mySubscriber.Subscribe(new RedisChannel(RedisMessageChannels.CashIn, RedisChannel.PatternMode.Auto),
                        (_, message) =>
                        {
                            if (KursNotyficationService != null)
                            {
                                Console.WriteLine($@"Redis - {message}");
                                Form.Dispatcher.Invoke(() => ShowNotify(message));
                            }
                        });
            }
            catch
            {
                Console.WriteLine($@"Redis {ConfigurationManager.AppSettings["redis.connection"]} не обнаружен");
            }
        }

        public CashOutWindowViewModel(decimal dc) : this()
        {
            RefreshData(dc);
            // ReSharper disable once PossibleInvalidOperationException
            oldDate = (DateTime)Document.DATE_ORD;
            oldKontrDC = Document.KONTRAGENT_DC;
            LastDocumentManager.SaveLastOpenInfo(DocumentType.CashOut, null, Document.DocCode,
                Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
        }

        #endregion

        #region Properties

        public string NotifyInfo { get; set; }

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

        public override string LayoutName => "CashOutView";

        public CashOut Document
        {
            get => myDocument;
            set
            {
                if (Equals(myDocument, value)) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override void OnWindowClosing(object obj)
        {
            mySubscriber?.UnsubscribeAll();
            base.OnWindowClosing(obj);
        }


        public override bool IsDocNewCopyRequisiteAllow => State != RowStatus.NewRow;

        public override void SaveData(object data)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var context = GlobalOptions.GetEntities())
                {
                    var old = context.SD_34.FirstOrDefault(_ => _.DOC_CODE == Document.DocCode);
                    var sfDC = old?.SPOST_DC;
                    if (Document.State == RowStatus.Edited)
                        CashManager.UpdateDocument(CashDocumentType.CashOut, Document, oldDate);
                    if (Document.State == RowStatus.NewRow)
                        CashManager.InsertDocument(CashDocumentType.CashOut, Document);
                    //CashManager.InsertDocument(CashDocumentType.CashIn, Document);
                    if (Document.SPOST_DC != null)
                        context.Database.ExecuteSqlCommand(
                            $"EXEC [dbo].[GenerateSFProviderCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(Document.SPOST_DC.Value)}");
                    if (sfDC != null && Document.SPOST_DC != sfDC)
                        context.Database.ExecuteSqlCommand(
                            $"EXEC [dbo].[GenerateSFProviderCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(sfDC)}");
                }
            }

            if (BookView is CashBookView bw)
                if (bw.DataContext is CashBookWindowViewModel ctx)
                    ctx.RefreshActual(Document);

            DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.CashOut), null,
                Document.DocCode, null, (string)Document.ToJson());
            if (Document.KONTRAGENT_DC != null)
                // ReSharper disable once PossibleInvalidOperationException
                RecalcKontragentBalans.CalcBalans((decimal)Document.KONTRAGENT_DC, (DateTime)Document.DATE_ORD);
            LastDocumentManager.SaveLastOpenInfo(DocumentType.CashOut, null, Document.DocCode,
                Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
            if (mySubscriber != null && mySubscriber.IsConnected())
            {
                var str = Document.State == RowStatus.NewRow ? "создал" : "сохранил";
                var message = new RedisMessage
                {
                    DocumentType = DocumentType.CashOut,
                    DocCode = Document.DocCode,
                    Id = Document.Id,
                    DocDate = Document.DATE_ORD,
                    IsDocument = true,
                    OperationType = Document.myState == RowStatus.NewRow
                        ? RedisMessageDocumentOperationTypeEnum.Create
                        : RedisMessageDocumentOperationTypeEnum.Update,
                    Message =
                        $"Пользователь '{GlobalOptions.UserInfo.Name}' {str} расходный кассовый ордер {Document.Description}"
                };
                message.ExternalValues.Add("KontragentDC", Document.KONTRAGENT_DC);
                message.ExternalValues.Add("PersonaDC", Document.Employee?.DocCode);
                message.ExternalValues.Add("CashToDC", Document.CASH_TO_DC);
                message.ExternalValues.Add("InvoiceDC", Document.SPOST_DC);
                message.ExternalValues.Add("BankAccountDC", Document.BANK_RASCH_SCHET_DC);
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                mySubscriber.Publish(new RedisChannel(RedisMessageChannels.CashOut, RedisChannel.PatternMode.Auto),
                    json);
            }
        }

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("Вы уверены, что хотите удалить данный документ?", "Запрос",
                MessageBoxButton.YesNo,
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
                                if (Document.SPOST_DC != null)
                                    using (var context = GlobalOptions.GetEntities())
                                    {
                                        context.Database.ExecuteSqlCommand(
                                            $"EXEC [dbo].[GenerateSFProviderCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(Document.SPOST_DC.Value)}");
                                    }

                                if (Document.KONTRAGENT_DC != null)
                                    RecalcKontragentBalans.CalcBalans((decimal)Document.KONTRAGENT_DC,
                                        // ReSharper disable once PossibleInvalidOperationException
                                        (DateTime)Document.DATE_ORD);
                                ctx?.RefreshActual(Document);
                                if (mySubscriber != null && mySubscriber.IsConnected())
                                {
                                    var message = new RedisMessage
                                    {
                                        DocumentType = DocumentType.CashOut,
                                        DocCode = Document.DocCode,
                                        Id = Document.Id,
                                        DocDate = Document.DATE_ORD,
                                        IsDocument = true,
                                        OperationType = RedisMessageDocumentOperationTypeEnum.Delete,
                                        Message =
                                            $"Пользователь '{GlobalOptions.UserInfo.Name}' удалил расходный кассовый ордер {Document.Description}"
                                    };
                                    message.ExternalValues.Add("KontragentDC", Document.KONTRAGENT_DC);
                                    message.ExternalValues.Add("PersonaDC", Document.Employee?.DocCode);
                                    message.ExternalValues.Add("CashToDC", Document.CASH_TO_DC);
                                    message.ExternalValues.Add("InvoiceDC", Document.SPOST_DC);
                                    message.ExternalValues.Add("BankAccountDC", Document.BANK_RASCH_SCHET_DC);

                                    var jsonSerializerSettings = new JsonSerializerSettings
                                    {
                                        TypeNameHandling = TypeNameHandling.All
                                    };
                                    var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                                    if (Document.State != RowStatus.NewRow)
                                        mySubscriber.Publish(
                                            new RedisChannel(RedisMessageChannels.CashOut,
                                                RedisChannel.PatternMode.Auto),
                                            json);
                                }

                                CloseWindow(Form);
                                return;
                            case MessageBoxResult.No:
                                return;
                        }
                    }

                    var ctx1 = BookView?.DataContext as CashBookWindowViewModel;
                    CashManager.DeleteDocument(CashDocumentType.CashOut, Document);
                    if (Document.SPOST_DC != null)
                        using (var context = GlobalOptions.GetEntities())
                        {
                            context.Database.ExecuteSqlCommand(
                                $"EXEC [dbo].[GenerateSFProviderCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(Document.SPOST_DC.Value)}");
                        }

                    ctx1?.RefreshActual(Document);
                    CloseWindow(Form);
                    if (BookView is AccruedAmountOfSupplierView bw)
                        if (bw.DataContext is AccruedAmountOfSupplierWindowViewModel vm)
                            vm.RefreshData(false);
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
                    var winManager = new WindowManager();
                    winManager.ShowWinUIMessageBox($"Не найден документ с кодом {dc}!",
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
