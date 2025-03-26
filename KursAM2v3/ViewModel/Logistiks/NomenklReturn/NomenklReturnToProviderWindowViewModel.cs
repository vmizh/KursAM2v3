using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.LayoutControl;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Repositories.InvoicesRepositories;
using KursAM2.Repositories.NomenklReturn;
using KursAM2.Repositories.RedisRepository;
using KursAM2.Repositories.WarehousesRepository;
using KursAM2.View.DialogUserControl;
using KursAM2.View.Logistiks.NomenklReturn;
using KursAM2.ViewModel.Management.Calculations;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.NomenklReturn;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace KursAM2.ViewModel.Logistiks.NomenklReturn;

public class NomenklReturnToProviderWindowViewModel : RSWindowViewModelBase, IDataErrorInfo
{
    #region Constructors

    public NomenklReturnToProviderWindowViewModel(Guid? id, bool isCopy = false)
    {
        myRepository = new NomenklReturnToProviderRepository(myContext);

        try
        {
            redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
            mySubscriber = redis.GetSubscriber();
            if (mySubscriber.IsConnected())
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.NomenklReturnToProvider, RedisChannel.PatternMode.Auto),
                    // ReSharper disable once UnusedParameter.Local
                    (channel, message) =>
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

        mySubscriber = redis?.GetSubscriber();
        LeftMenuBar = GlobalOptions.UserInfo.IsAdmin
            ? MenuGenerator.DocWithCustomizeFormDocumentLeftBar(this)
            : MenuGenerator.DocWithRowsLeftBar(this);
        RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
        if (id is null)
        {
            Document = myRepository.CrateNewEmpty() as NomenklReturnToProviderViewModel;
            // ReSharper disable once PossibleNullReferenceException
            Document.State = RowStatus.NewRow;
        }
        else
        {
            if (!isCopy)
            {
                Document = myRepository.Get(id.Value) as NomenklReturnToProviderViewModel;
                if (Document is not null)
                    foreach (var r in Document.Rows)
                        r.MaxQuantity = r.Quantity;
            }
            else
            {
                Document = myRepository.CrateNewRequisite(id.Value) as NomenklReturnToProviderViewModel;
                if(Document is not null)
                    Document.myState = RowStatus.NewRow;
            }
            LoadRefernces();
            setUnchangedStatus();
        }
    }

    #endregion

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

    public void LoadRefernces()
    {
        var prices = myRepository.GetNomenklLastPrices(Document.Rows.Select(_ => _.NomenklDC).ToList(),
            Document.DocDate);
        foreach (var row in Document.Rows)
        {
            row.CalcCost = prices.FirstOrDefault(_ => _.Key == row.NomenklDC).Value;
            if (row.InvoiceRowId is not null)
            {
                var s1 = myInvoiceProviderRepository.GetInfoByRowId(row.InvoiceRowId.Value);
                row.InvoiceText = s1.IndexOf('№') > 0 ? s1.Remove(0, s1.IndexOf('№')) : s1;
            }
        }
    }

    private void setUnchangedStatus()
    {
        foreach (var r in Document.Rows)
            r.myState = RowStatus.NotEdited;
        Document.myState = RowStatus.NotEdited;
        Document.RaisePropertyChanged("State");
    }

    #endregion

    #region Fields

    private readonly ALFAMEDIAEntities myContext = GlobalOptions.GetEntities();

    private readonly INomenklReturnToProviderRepository myRepository;

    private readonly IInvoiceProviderRepository myInvoiceProviderRepository =
        new InvoiceProviderRepository(GlobalOptions.GetEntities());

    private readonly IPrihodOrderRepository myPrihodOrderRepository =
        new PrihodOrderRepository(GlobalOptions.GetEntities());

    private NomenklReturnToProviderViewModel myDocument;

    private readonly ConnectionMultiplexer redis;
    private readonly ISubscriber mySubscriber;

    private readonly Guid HeadViewLayoutId = Guid.Parse("{605675EF-76A1-4055-9AB8-D310B7783805}");
    private string myHeadViewLayoutBase;
    private NomenklReturnToProviderRowViewModel myCurrentRow;

    #endregion

    #region Properties

    public string NotifyInfo { get; set; }

    public override bool IsCanSaveData => (Document != null && Document.State != RowStatus.NotEdited
                                                            && Document.Kontragent is not null &&
                                                            Document.Warehouse is not null) ||
                                          ((Document?.State ?? RowStatus.NewRow) != RowStatus.NewRow &&
                                           DeletedRows.Count > 0);

    public override string WindowName => "Возврат товара поставщику";
    public override string LayoutName => "NomenklReturnToProviderWindow";

    public readonly List<NomenklReturnToProviderRowViewModel> DeletedRows =
        new List<NomenklReturnToProviderRowViewModel>();

    public NomenklReturnToProviderViewModel Document
    {
        get => myDocument;
        set
        {
            if (Equals(myDocument, value)) return;
            myDocument = value;
            RaisePropertyChanged();
        }
    }

    public NomenklReturnToProviderRowViewModel CurrentRow
    {
        get => myCurrentRow;
        set
        {
            if (Equals(value, myCurrentRow)) return;
            myCurrentRow = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region ErrirInfo

    public string this[string columnName] => throw new NotImplementedException();

    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public string Error { get; }

    #endregion

    #region Command

    public ICommand OpenSFCommand
    {
        get { return new Command(OpenSF, _ => CurrentRow != null && CurrentRow.InvoiceRowId is not null); }
    }

    private void OpenSF(object obj)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var dc = ctx.TD_26.FirstOrDefault(_ => _.Id == CurrentRow.InvoiceRowId);
            if (dc != null)
            {
                DocumentsOpenManager.Open(DocumentType.InvoiceProvider, dc.DOC_CODE);
            }
        }
    }

    public ICommand OpenOrderCommand
    {
        get { return new Command(OpenOrder, _ => CurrentRow != null && CurrentRow.PrihodOrderId is not null); }
    }

    private void OpenOrder(object obj)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var dc = ctx.TD_24.FirstOrDefault(_ => _.Id == CurrentRow.PrihodOrderId);
            if (dc != null)
            {
                DocumentsOpenManager.Open(DocumentType.StoreOrderIn, dc.DOC_CODE);
            }
        }
    }


    public override void DocNewCopyRequisite(object form)
    {
        var frm = new NomenklReturnToProviderView
        {
            Owner = Application.Current.MainWindow
        };
        var ctx = new NomenklReturnToProviderWindowViewModel(Document.Id,true) { Form = frm };
        frm.DataContext = ctx;
        frm.Show();
    }

    public override void DocNewEmpty(object form)
    {
        var frm = new NomenklReturnToProviderView
        {
            Owner = Application.Current.MainWindow
        };
        var ctx = new NomenklReturnToProviderWindowViewModel(null) { Form = frm };
        frm.DataContext = ctx;
        frm.Show();
    }

    public override void DocDelete(object form)
    {
        var res = MessageBox.Show("Вы уверены, что хотите удалить данный документ?", "Запрос",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        switch (res)
        {
            case MessageBoxResult.Yes:
                var docdate = Document.DocDate;
                if (Document.State == RowStatus.NewRow)
                {
                    Form.Close();
                    return;
                }

                try
                {
                    myRepository.BeginTransaction();
                    myRepository.Delete(Document.Id);
                    myRepository.CommitTransaction();
                    if (mySubscriber != null && mySubscriber.IsConnected())
                    {
                        var message = new RedisMessage
                        {
                            DocumentType = DocumentType.NomenklReturnToProvider,
                            Id = Document.Id,
                            DocDate = Document.DocDate,
                            IsDocument = true,
                            OperationType = RedisMessageDocumentOperationTypeEnum.Delete,
                            Message =
                                $"Пользователь '{GlobalOptions.UserInfo.Name}' удалил возврат товара от клиента {Document.Description}"
                        };
                        message.ExternalValues.Add("KontragentDC", Document.Kontragent.DocCode);
                        message.ExternalValues.Add("WarehouseDC", Document.Warehouse.DocCode);
                        var jsonSerializerSettings = new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All
                        };
                        var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                        if (Document.State != RowStatus.NewRow)
                            mySubscriber.Publish(
                                new RedisChannel(RedisMessageChannels.NomenklReturnToProvider,
                                    RedisChannel.PatternMode.Auto),
                                json);
                    }
                }
                catch (Exception ex)
                {
                    myRepository.RollbackTransaction();
                    WindowManager.ShowError(ex);
                }

                RecalcKontragentBalans.CalcBalans(Document.Kontragent.DocCode, docdate);
                Form?.Close();
                break;
            case MessageBoxResult.No:
                break;
        }
    }

    public override void CloseWindow(object form)
    {
        if (CanCustomize)
        {
            var res = MessageBox.Show("Документ в статусе настройки шапки, сохранить текущий вид?", "Запрос",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes) SaveCustomizedFormDocument(null);
        }

        base.CloseWindow(form);
    }

    public ICommand KontragentSelectCommad
    {
        get { return new Command(KontragentSelect, _ => Document.Rows.Count == 0); }
    }

    private void KontragentSelect(object obj)
    {
        var kontr = StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions
        {
            IsProviderOnly = true
        });
        if (kontr == null) return;
        Document.Kontragent = kontr;
    }

    public ICommand WarehouseSelectCommand
    {
        get { return new Command(WarehouseSelect, _ => true); }
    }

    private void WarehouseSelect(object obj)
    {
        var warehouse = StandartDialogs.SelectWarehouseDialog(new List<KursDomain.References.Warehouse>(
            GlobalOptions.ReferencesCache.GetWarehousesAll().Cast<KursDomain.References.Warehouse>()
                .Where(_ => _.IsDeleted)));
        if (warehouse == null) return;
        Document.Warehouse = warehouse;
    }

    public ICommand InvoiceSelectCommad
    {
        get { return new Command(InvoiceSelect, _ => Document.Kontragent is not null); }
    }

    private void InvoiceSelect(object obj)
    {
    }

    public override void UpdateVisualObjects()
    {
        base.UpdateVisualObjects();
        if (Form is not NomenklReturnToProviderView frm) return;
        using (var ctx = GlobalOptions.KursSystem())
        {
            var layoutHead = ctx.FormLayout.FirstOrDefault(_ => _.Id == HeadViewLayoutId);
            if (layoutHead is null) return;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(layoutHead.Layout);
            writer.Flush();
            stream.Position = 0;
            using (var reader = XmlReader.Create(stream))
            {
                frm.DocumentHead.ReadFromXML(reader);
            }
        }

        foreach (var col in frm.gridRows.Columns)
        {
            switch (col.FieldName)
            {
                case nameof(NomenklReturnToProviderRowViewModel.Cost):
                    col.ReadOnly = true;
                    break;
            }
        }
    }

    protected override void SaveCustomizedFormDocument(object obj)
    {
        if (Form is NomenklReturnToProviderView frm)
            using (var ctx = GlobalOptions.KursSystem())
            {
                foreach (var o in LogicalTreeHelper.GetChildren(frm.DocumentHead))
                    if (o is LayoutGroup item)
                        if (item.Header is not null && ((string)item.Header).ToLower() == "group")
                            item.View = LayoutGroupView.Group;

                string layoutXML;
                var stream = new MemoryStream();

                using (var writer = XmlWriter.Create(stream))
                {
                    frm.DocumentHead.WriteToXML(writer);
                    writer.Close();
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        layoutXML = reader.ReadToEnd();
                    }
                }

                stream.Flush();


                var old = ctx.FormLayout.FirstOrDefault(_ => _.Id == HeadViewLayoutId);
                if (old != null)
                {
                    old.Layout = layoutXML;
                    old.UserId = GlobalOptions.UserInfo.KursId;
                    old.UpdateDate = DateTime.Now;
                }
                else
                {
                    ctx.FormLayout.Add(new FormLayout
                    {
                        Id = HeadViewLayoutId,
                        ControlName = $"{LayoutName}.DocumentHead",
                        FormName = $"{LayoutName}.DocumentHead",
                        Layout = layoutXML,
                        UserId = GlobalOptions.UserInfo.KursId,
                        UpdateDate = DateTime.Now,
                        WindowState = string.Empty
                    });
                }

                ctx.SaveChanges();
            }

        base.SaveCustomizedFormDocument(obj);
    }

    public override void ResetLayout(object form)
    {
        base.ResetLayout(form);
        if (Form is NomenklReturnToProviderView frm)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(myHeadViewLayoutBase);
            writer.Flush();
            stream.Position = 0;
            using (var reader = XmlReader.Create(stream))
            {
                frm.DocumentHead.ReadFromXML(reader);
            }
        }
    }

    protected override void OnLayoutInitial(object obj)
    {
        base.OnLayoutInitial(obj);
        if (Form is NomenklReturnToProviderView frm)
        {
            var stream = new MemoryStream();

            using (var writer = XmlWriter.Create(stream))
            {
                frm.DocumentHead.WriteToXML(writer);
                writer.Close();
                stream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    myHeadViewLayoutBase = reader.ReadToEnd();
                }
            }

            stream.Flush();
        }
    }

    public ICommand AddNomenklCommand
    {
        get
        {
            return new Command(AddNomenkl, _ => Document?.Kontragent is not null
                                                && Document?.Warehouse is not null);
        }
    }

    private void AddNomenkl(object obj)
    {
        var existsRows = new List<Guid>();
        foreach (var row in Document.Rows)
            if (row.Entity.PrihOrderId is not null)
                existsRows.Add(row.Entity.PrihOrderId.Value);
        var ctx = new NomenklReturnSelectPrihodOrderRowsDialog(Document.Kontragent, Document.Warehouse, existsRows);
        var service = this.GetService<IDialogService>("DialogServiceUI");
        if (service.ShowDialog(MessageButton.OKCancel, "Возврат товара поставщику. Выбор номенклатур.", ctx) ==
            MessageResult.Cancel) return;
        var prices = myRepository.GetNomenklLastPrices(ctx.SelectedRows.Select(_ => _.NomenklDC).ToList(),
            Document.DocDate);
        foreach (var item in ctx.SelectedRows)
        {
            if (Document.Rows.Any(_ => _.InvoiceRowId == item.InvoiceRowId)) continue;
            var newRow = new NomenklReturnToProviderRowViewModel(new NomenklReturnToProviderRow
            {
                Id = Guid.NewGuid(),
                Price = item.Price,
                Quantity = item.Quantity,
                DocId = Document.Id,
                InvoiceRowId = item.InvoiceRowId,
                PrihOrderId = item.Id
            }, Document)
            {
                State = RowStatus.NewRow,
                Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(item.NomenklDC) as Nomenkl,
                CalcCost = prices.FirstOrDefault(_ => _.Key == item.NomenklDC).Value,
                MaxQuantity = item.Quantity
            };
            Document.Entity.NomenklReturnToProviderRow.Add(newRow.Entity);
            newRow.Cost = newRow.CalcCost;
            if (newRow.PrihodOrderId is not null)
            {
                var s = myPrihodOrderRepository.GetInfoByRowId(newRow.PrihodOrderId.Value);
                newRow.PrihodOrderText = s.IndexOf('№') > 0 ? s.Remove(0, s.IndexOf('№')) : s;
            }

            if (newRow.InvoiceRowId is not null)
            {
                var s1 = myInvoiceProviderRepository.GetInfoByRowId(newRow.InvoiceRowId.Value);
                newRow.InvoiceText = s1.IndexOf('№') > 0 ? s1.Remove(0, s1.IndexOf('№')) : s1;
            }

            Document.Rows.Add(newRow);
        }
    }

    public ICommand DeleteRowCommand
    {
        get { return new Command(DeleteRow, _ => CurrentRow is not null); }
    }

    private void DeleteRow(object obj)
    {
        if (CurrentRow.State != RowStatus.NewRow)
            DeletedRows.Add(CurrentRow);
        Document.Rows.Remove(CurrentRow);
        //Document.Entity.NomenklReturnToProviderRow.Remove(CurrentRow.Entity);
        Document.State = RowStatus.Edited;
    }

    public override void SaveData(object data)
    {
        try
        {
            if (State == RowStatus.NewRow || Document.DocNum < 0)
            {
                var n = myRepository.GetNewNumber();
                Document.DocNum = n <= 0 ? 1 : n;
            }

            myRepository.BeginTransaction();
            myRepository.AddOrUpdate(Document, DeletedRows.Select(_ => _.Id).ToList());
            myRepository.CommitTransaction();
            DeletedRows.Clear();
            DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.NomenklReturnToProvider),
                Document.Id,
                null, null, (string)Document.ToJson());
            LastDocumentManager.SaveLastOpenInfo(DocumentType.NomenklReturnToProvider, Document.Id, null,
                Document.Creator, GlobalOptions.UserInfo.NickName, Document.Description);
            if (mySubscriber != null && mySubscriber.IsConnected())
            {
                var str = Document.State == RowStatus.NewRow ? "создал" : "сохранил";
                var message = new RedisMessage
                {
                    DocumentType = DocumentType.NomenklReturnToProvider,
                    Id = Document.Id,
                    DocDate = Document.DocDate,
                    IsDocument = true,
                    OperationType = Document.myState == RowStatus.NewRow
                        ? RedisMessageDocumentOperationTypeEnum.Create
                        : RedisMessageDocumentOperationTypeEnum.Update,
                    Message = $"Пользователь '{GlobalOptions.UserInfo.Name}' {str} ордер {Document.Description}"
                };
                message.ExternalValues.Add("KontragentDC", Document.Kontragent.DocCode);
                message.ExternalValues.Add("WarehouseDC", Document.Warehouse.DocCode);
                var jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                mySubscriber.Publish(
                    new RedisChannel(RedisMessageChannels.NomenklReturnToProvider, RedisChannel.PatternMode.Auto),
                    json);
            }

            setUnchangedStatus();
        }
        catch (Exception ex)
        {
            myRepository.RollbackTransaction();
            WindowManager.ShowError(ex);
        }
    }

    public override void RefreshData(object obj)
    {
        foreach (var dRow in DeletedRows) Document.Rows.Add(dRow);
        myRepository.UndoChanges();
        Document.RaisePropertyAllChanged();
        foreach (var row in Document.Rows) row.RaisePropertyAllChanged();
        DeletedRows.Clear();
        setUnchangedStatus();
    }

    #endregion
}
