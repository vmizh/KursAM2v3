using System;
using System.Collections.ObjectModel;
using System.Configuration;
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
using KursAM2.View.Finance.Cash;
using KursAM2.View.Helper;
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
    public sealed class CashInWindowViewModel : RSWindowViewModelBase
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
                Document.TABELNUMBER = null;
                Document.BankAccount = null;
                Document.RASH_ORDER_FROM_DC = null;
                Document.StockHolder = sh;
                Document.KONTRAGENT_DC = null;
                Document.RaisePropertyChanged("Kontragent");
            }
        }

        #endregion

        #region Fields

        public CashBookView BookView;
        private CashInViewModel myDocument;
        private readonly DateTime oldDate = DateTime.MaxValue;
        public decimal OldSumma;
        private readonly decimal? oldKontrDC;

        private readonly ConnectionMultiplexer redis;
        private readonly ISubscriber mySubscriber;

        #endregion

        #region Constructors

        public CashInWindowViewModel()
        {
            IsDocNewCopyAllow = true;
            IsDocNewCopyRequisiteAllow = false;
            IsDocDeleteAllow = true;
            IsCanSaveData = false;
            WindowName = $"Приходный кассовый ордер от {Document?.Kontragent} в {Document?.Cash?.Name}";
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

        public CashInWindowViewModel(decimal dc) : this()
        {
            RefreshData(dc);
            // ReSharper disable once PossibleInvalidOperationException
            oldDate = (DateTime)Document.DATE_ORD;
            oldKontrDC = Document.KONTRAGENT_DC;
        }

        #endregion

        #region Properties

        public string NotifyInfo { get; set; }

        public bool IsAccuredOpenEnable => !string.IsNullOrWhiteSpace(Document?.AccuredInfo);

        public override string LayoutName => "CashInView";

        public override bool IsCanRefresh => Document != null && Document.State != RowStatus.NewRow;

        public bool IsSummaEnabled => (Document != null && Document.State == RowStatus.NewRow) || (Document != null &&
            Document.BANK_RASCH_SCHET_DC == null
            && Document.RASH_ORDER_FROM_DC == null);

        public bool IsKontrSelectEnable => Document != null && Document.KontragentType != CashKontragentType.NotChoice
                                                            && Document.SFACT_DC == null &&
                                                            Document.State != RowStatus.NewRow;

        public ObservableCollection<Currency> CurrencyList { get; set; } = new ObservableCollection<Currency>();

        public CashInViewModel Document
        {
            get => myDocument;
            set
            {
                if (Equals(myDocument, value)) return;
                myDocument = value;
                RaisePropertyChanged();
            }
        }

        public override RowStatus State => Document?.State ?? RowStatus.NewRow;

        public override bool IsCanSaveData
        {
            get
            {
                if (Form is CashInView frm)
                {
                    frm.KontrSelectButton.IsEnabled =
                        Document?.Cash != null && Document.KontragentType != CashKontragentType.NotChoice;
                    frm.SFactNameItem.IsEnabled =
                        Document != null && Document.KontragentType == CashKontragentType.Kontragent;
                    frm.NCodeItem.IsReadOnly =
                        Document != null && Document.KontragentType != CashKontragentType.Employee;
                    if (Document != null && Document.KontragentType != CashKontragentType.Employee)
                        Document.NCODE = null;
                    if (Document != null)
                        frm.Sumordcont.IsEnabled =
                            (Document.BANK_RASCH_SCHET_DC == null && Document.RASH_ORDER_FROM_DC == null)
                            || Document.State == RowStatus.NewRow;
                }

                return Document != null && Document.State != RowStatus.NotEdited &&
                       CashManager.CheckCashIn(Document);
            }
        }

        public override bool IsDocDeleteAllow => Document != null && Document.State != RowStatus.NewRow;
        public override bool IsNewDocument => Document != null && Document.State == RowStatus.NewRow;

        #endregion

        #region Command

        public override bool IsAllowHistoryShow => State != RowStatus.NewRow;

        public override void ShowHistory(object data)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            DocumentHistoryManager.LoadHistory(DocumentType.CashIn, null, Document.DocCode, null);
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

        public override void SaveData(object data)
        {
            if (Document.State == RowStatus.NewRow)
            {
                using (var context = GlobalOptions.GetEntities())
                {
                    CashManager.InsertDocument(CashDocumentType.CashIn, Document);
                    context.Database.ExecuteSqlCommand(
                        $"EXEC [dbo].[GenerateSFClientCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(Document.SFACT_DC ?? 0)}");
                    if ((BookView?.DataContext is CashBookWindowViewModel ctx))
                        ctx.RefreshActual(Document);
                }
            }

            if (Document.State == RowStatus.Edited)
            {
                CashManager.UpdateDocument(CashDocumentType.CashIn, Document, oldDate);
                using (var context = GlobalOptions.GetEntities())
                {
                    context.Database.ExecuteSqlCommand(
                        $"EXEC [dbo].[GenerateSFClientCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(Document.SFACT_DC ?? 0)}");
                    if (BookView?.DataContext is CashBookWindowViewModel ctx)
                        ctx.RefreshActual(Document);
                }
            }
            DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.CashIn), null,
                Document.DocCode, null, (string)Document.ToJson());

            if (Document.KONTRAGENT_DC != null)
                RecalcKontragentBalans.CalcBalans((decimal)Document.KONTRAGENT_DC,
                    // ReSharper disable once PossibleInvalidOperationException
                    (DateTime)(Document.DATE_ORD > oldDate ? oldDate : Document.DATE_ORD));
            LastDocumentManager.SaveLastOpenInfo(DocumentType.CashIn, null, Document.DocCode,
                Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
            if (mySubscriber != null && mySubscriber.IsConnected())
            {
                var str = Document.State == RowStatus.NewRow ? "создал" : "сохранил";
                var message = new RedisMessage
                {
                    DocumentType = DocumentType.CashIn,
                    DocCode = Document.DocCode,
                    Id = Document.Id,
                    DocDate = Document.DATE_ORD,
                    IsDocument = true,
                    OperationType = Document.myState == RowStatus.NewRow
                        ? RedisMessageDocumentOperationTypeEnum.Create
                        : RedisMessageDocumentOperationTypeEnum.Update,
                    Message =
                        $"Пользователь '{GlobalOptions.UserInfo.Name}' {str} приходный кассовый ордер {Document.Description}"
                };
                message.ExternalValues.Add("KontragentDC", Document.KONTRAGENT_DC);
                message.ExternalValues.Add("PersonaDC", Document.Employee?.DocCode);
                message.ExternalValues.Add("CashOutDC", Document.RASH_ORDER_FROM_DC);
                message.ExternalValues.Add("InvoiceDC", Document.SFACT_DC);
                message.ExternalValues.Add("BankAccountDC", Document.BANK_RASCH_SCHET_DC);
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                mySubscriber.Publish(new RedisChannel(RedisMessageChannels.CashIn, RedisChannel.PatternMode.Auto),
                    json);
            }
        }

        public override void OnWindowClosing(object obj)
        {
            mySubscriber?.UnsubscribeAll();
            base.OnWindowClosing(obj);
        }

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("Вы уверены, что хотите удалить данный документ?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    var ctx = BookView?.DataContext as CashBookWindowViewModel;
                    CashManager.DeleteDocument(CashDocumentType.CashIn, Document);
                    using (var context = GlobalOptions.GetEntities())
                    {
                        context.Database.ExecuteSqlCommand(
                            $"EXEC [dbo].[GenerateSFClientCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(Document.SFACT_DC ?? 0)}");
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
                            DocumentType = DocumentType.CashIn,
                            DocCode = Document.DocCode,
                            Id = Document.Id,
                            DocDate = Document.DATE_ORD,
                            IsDocument = true,
                            OperationType = RedisMessageDocumentOperationTypeEnum.Delete,
                            Message =
                                $"Пользователь '{GlobalOptions.UserInfo.Name}' удалил приходный кассовый ордер {Document.Description}"
                        };
                        message.ExternalValues.Add("KontragentDC", Document.KONTRAGENT_DC);
                        message.ExternalValues.Add("PersonaDC", Document.Employee?.DocCode);
                        message.ExternalValues.Add("CashOutDC", Document.RASH_ORDER_FROM_DC);
                        message.ExternalValues.Add("InvoiceDC", Document.SFACT_DC);
                        message.ExternalValues.Add("BankAccountDC", Document.BANK_RASCH_SCHET_DC);

                        var jsonSerializerSettings = new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All
                        };
                        var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                        if (Document.State != RowStatus.NewRow)
                            mySubscriber.Publish(
                                new RedisChannel(RedisMessageChannels.CashIn,
                                    RedisChannel.PatternMode.Auto),
                                json);
                    }

                    CloseWindow(Form);
                    break;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }
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
                Document = CashManager.LoadCashIn(Document.DocCode);
                LastDocumentManager.SaveLastOpenInfo(DocumentType.CashIn, null, Document.DocCode,
                    Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
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
                    case CashInViewModel model:
                        dc = model.DocCode;
                        break;
                }

                Document = CashManager.LoadCashIn(dc);
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
            OldSumma = Document.SUMM_ORD ?? 0m;
        }

        public override void DocNewEmpty(object form)
        {
            var vm = new CashInWindowViewModel
            {
                Document = CashManager.NewCashIn()
            };
            vm.Document.Cash = Document.Cash;
            vm.BookView = BookView;
            DocumentsOpenManager.Open(DocumentType.CashIn, vm, BookView);
        }

        public override void DocNewCopy(object form)
        {
            if (Document == null) return;
            var vm = new CashInWindowViewModel
            {
                Document = CashManager.NewCopyCashIn(Document.DocCode)
            };
            vm.Document.Cash = Document.Cash;
            vm.BookView = BookView;
            DocumentsOpenManager.Open(DocumentType.CashIn, vm, BookView);
        }

        public override void DocNewCopyRequisite(object form)
        {
            if (Document == null) return;
            var vm = new CashInWindowViewModel
            {
                Document = CashManager.NewRequisiteCashIn(Document.DocCode)
            };
            //vm.Document.Cash = Document.Cash;
            vm.BookView = BookView;
            DocumentsOpenManager.Open(DocumentType.CashIn, vm, BookView);
        }

        #endregion
    }
}
