using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Managers;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.Management;
using KursAM2.ViewModel.Management.Calculations;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using KursDomain.Repository.DocHistoryRepository;
using KursDomain.WindowsManager.WindowsManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace KursAM2.ViewModel.Management;

[DataContract]
public class KontragentBalansWindowViewModel : RSWindowViewModelBase
{
    private readonly DocHistoryRepository lastDocumentRopository = new(GlobalOptions.GetEntities());

    private readonly ISubscriber mySubscriber;

    private readonly ConnectionMultiplexer redis;

    private KontragentBalansRowViewModel myCurrentDocument;
    private KontragentPeriod myCurrentPeriod;
    private Kontragent myKontragent;
    private decimal myLastBalansSumma;
    private DateTime myLastOperationDate;

    public KontragentBalansWindowViewModel()
    {
        Operations = new ObservableCollection<KontragentBalansRowViewModel>();
        Periods = new ObservableCollection<KontragentPeriod>();
        Documents = new ObservableCollection<KontragentBalansRowViewModel>();
        LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
        {
            [MenuGeneratorItemVisibleEnum.AddSearchlist] = true
        });
        RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
        try
        {
            redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
            mySubscriber = redis.GetSubscriber();

            if (mySubscriber.IsConnected())
            {
                mySubscriber.Subscribe(new RedisChannel(RedisMessageChannels.Bank, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        Console.WriteLine($@"Redis - {message}");
                        Form.Dispatcher.Invoke(() => Update(RedisMessageChannels.Bank, message));
                    });
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.MutualAccounting, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        Console.WriteLine($@"Redis - {message}");
                        Form.Dispatcher.Invoke(() => Update(RedisMessageChannels.MutualAccounting, message));
                    });
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.WarehouseOrderIn, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        Console.WriteLine($@"Redis - {message}");
                        Form.Dispatcher.Invoke(() => Update(RedisMessageChannels.WarehouseOrderIn, message));
                    });
                mySubscriber.Subscribe(new RedisChannel(RedisMessageChannels.CashIn, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        Console.WriteLine($@"Redis - {message}");
                        Form.Dispatcher.Invoke(() => Update(RedisMessageChannels.CashIn, message));
                    });
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.WayBill, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        Console.WriteLine($@"Redis - {message}");
                        Form.Dispatcher.Invoke(() => Update(RedisMessageChannels.WayBill, message));
                    });
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.CashOut, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        Console.WriteLine($@"Redis - {message}");
                        Form.Dispatcher.Invoke(() => Update(RedisMessageChannels.CashOut, message));
                    });
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.InvoiceClient, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        Console.WriteLine($@"Redis - {message}");
                        Form.Dispatcher.Invoke(() => Update(RedisMessageChannels.InvoiceClient, message));
                    });
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.InvoiceProvider, RedisChannel.PatternMode.Auto),
                    (_, message) =>
                    {
                        Console.WriteLine($@"Redis - {message}");
                        Form.Dispatcher.Invoke(() => Update(RedisMessageChannels.InvoiceProvider, message));
                    });
            }
        }
        catch
        {
            Console.WriteLine($@"Redis {ConfigurationManager.AppSettings["redis.connection"]} не обнаружен");
        }
    }

    public KontragentBalansWindowViewModel(decimal doccode) : this()
    {
        Kontragent = Kontragents.Single(_ => _.DocCode == doccode);
    }

    public override string LayoutName => "KontragentBalansView";
    public override string WindowName => "Лицевой счет контрагента";

    public string NotifyInfo { get; set; }

    public ObservableCollection<KontragentBalansRowViewModel> SelectedDocs { set; get; } = new();

    public Kontragent StartKontragent { get; set; }

    // ReSharper disable once CollectionNeverQueried.Global
    public ObservableCollection<KontragentBalansRowViewModel> Documents { set; get; }


    public string PeriodName
    {
        set
        {
            if (Equals(myPeriodName, value)) return;
            myPeriodName = value;
            RaisePropertyChanged();
        }
        get => myPeriodName;
    }

    public string ResponsibleName
    {
        set
        {
            if (value == myResponsibleName) return;
            myResponsibleName = value;
            RaisePropertyChanged();
        }
        get => myResponsibleName;
    }

    public KontragentPeriod CurrentPeriod
    {
        get => myCurrentPeriod;
        set
        {
            if (Equals(myCurrentPeriod, value)) return;
            myCurrentPeriod = value;
            switch (myCurrentPeriod?.PeriodType)
            {
                case PeriodType.Year:
                    PeriodName = myCurrentPeriod.Name;
                    break;
                case PeriodType.Month:
                    PeriodName = $"{myCurrentPeriod.Name} {myCurrentPeriod.DateStart.Year} г.";
                    break;
                case PeriodType.Day:
                    PeriodName = myCurrentPeriod.DateStart.ToLongDateString();
                    break;
                default:
                    PeriodName = null;
                    break;
            }

            if (myCurrentPeriod != null)
                LoadDocumentsForPeriod();
            else
                Documents.Clear();
            RaisePropertyChanged();
        }
    }

    public KontragentBalansRowViewModel CurrentDocument
    {
        get => myCurrentDocument;
        set
        {
            if (myCurrentDocument == value) return;
            myCurrentDocument = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(IsDocumentOpenAllow));
        }
    }

    public List<Kontragent> Kontragents => KontragentManager.GetAllKontragentSortedByUsed();

    [DataMember]
    public Kontragent Kontragent
    {
        get => myKontragent != null
            ? GlobalOptions.ReferencesCache.GetKontragent(myKontragent.DocCode) as Kontragent
            : null;
        set
        {
            if (Equals(myKontragent, value)) return;
            myKontragent = value;
            if (myKontragent != null)
            {
                KontragentManager.UpdateSelectCount(myKontragent.DocCode);
                LoadOperations(myKontragent.DocCode);
                ResponsibleName = ((IName)Kontragent.ResponsibleEmployee)?.Name;
                RaisePropertyChanged(nameof(Kontragents));
            }
            else
            {
                ResponsibleName = null;
            }

            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Periods));
            RaisePropertyChanged(nameof(CrsName));
            RaisePropertyChanged(nameof(LastBalansSumma));
            RaisePropertyChanged(nameof(LastOperationDate));
        }
    }

    public string CrsName => Kontragent != null && Kontragent.Currency != null
        ? ((IName)Kontragent.Currency).Name
        : null;

    [DataMember]
    public decimal LastBalansSumma
    {
        get => myLastBalansSumma;
        set
        {
            if (myLastBalansSumma == value) return;
            myLastBalansSumma = value;
            RaisePropertyChanged();
        }
    }

    [DataMember]
    public DateTime LastOperationDate
    {
        get => myLastOperationDate;
        set
        {
            if (myLastOperationDate == value) return;
            myLastOperationDate = value;
            RaisePropertyChanged();
        }
    }

    [DataMember] public ObservableCollection<KontragentBalansRowViewModel> Operations { set; get; }

    [DataMember]
    public ObservableCollection<KontragentPeriod> Periods { set; get; } =
        // ReSharper disable once MemberInitializerValueIgnored
        new();

    public Brush BalansBrush
        => LastBalansSumma < 0 ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);

    public override void AddSearchList(object obj)
    {
        var form = new KontragentBalansView
        {
            Owner = Application.Current.MainWindow
        };
        var ctxk = new KontragentBalansWindowViewModel
        {
            Form = form
        };
        form.DataContext = ctxk;
        form.Show();
    }

    private void LoadDocumentsForPeriod()
    {
        Documents.Clear();
        if (CurrentPeriod == null)
        {
            Documents.Clear();
            RaisePropertyChanged(nameof(Documents));
            return;
        }

        foreach (
            var item in
            Operations.Where(_ => _.DocDate >= CurrentPeriod.DateStart && _.DocDate <= CurrentPeriod.DateEnd))
        {
            if (item.DocName.Contains("начало")) item.Nakopit = item.CrsKontrOut - item.CrsKontrIn;

            Documents.Add(item);
        }

        if (Documents.Count > 0)
        {
            var lasts = lastDocumentRopository.GetLastChanges(Documents.Select(_ => _.DocCode).Distinct());
            foreach (var r in Documents)
                if (lasts.ContainsKey(r.DocCode))
                {
                    var last = lasts[r.DocCode];
                    r.LastChanger = last.Item1;
                    r.LastChangerDate = last.Item2;
                }
                else
                {
                    r.LastChanger = null;
                    r.LastChangerDate = r.DocDate;
                }
        }

        RaisePropertyChanged(nameof(Documents));
    }

    private void LoadAllDocuments()
    {
        Documents.Clear();
        foreach (var item in Operations)
            Documents.Add(item);
        RaisePropertyChanged(nameof(Documents));
    }

    private void Update(string channels, RedisValue message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        var msg = JsonConvert.DeserializeObject<RedisMessage>(message);
        if (msg == null || msg.DbId != GlobalOptions.DataBaseId) return;
        switch (channels)
        {
            case RedisMessageChannels.InvoiceProvider:
            case RedisMessageChannels.InvoiceClient:
            case RedisMessageChannels.CashIn:
            case RedisMessageChannels.CashOut:
            case RedisMessageChannels.Bank:
            case RedisMessageChannels.WarehouseOrderIn:
            case RedisMessageChannels.WayBill:
                if (!msg.ExternalValues.ContainsKey("KontragentDC") ||
                    msg.ExternalValues["KontragentDC"] is null) return;
                break;
            case RedisMessageChannels.MutualAccounting:
                var arr = msg.ExternalValues["KontragentDCList"] as JArray;
                if (arr is null) return;
                if (!arr.Contains(Kontragent.DocCode)) return;
                break;
        }

        var period = CurrentPeriod;
        var doc = CurrentDocument;
        CurrentPeriod = null;
        CurrentDocument = null;
        LoadOperations(myKontragent.DocCode);
        CurrentPeriod = period;
        CurrentDocument = doc;
        if (KursNotyficationService is not null)
        {
            NotifyInfo = msg.Message;
            var notification = KursNotyficationService.CreateCustomNotification(this);
            notification.ShowAsync();
        }
    }

    private void LoadPeriod(IEnumerable<KontragentBalansRowViewModel> rows)
    {
        if (rows == null) return;
        var periods = DatePeriod.GenerateIerarhy(rows.Select(_ => _.DocDate).Distinct(), PeriodIerarhy.YearMonth);
        Periods.Clear();
        var localperiods = periods.Select(d => new KontragentPeriod
            {
                DateStart = d.DateStart,
                DateEnd = d.DateEnd,
                Name = d.Name,
                Id = d.Id,
                ParentId = d.ParentId,
                PeriodType = d.PeriodType,
                EndPeriodSumma = Operations.Last(_ => _.DocDate >= d.DateStart && _.DocDate <= d.DateEnd).Nakopit
            })
            .ToList();
        Periods = new ObservableCollection<KontragentPeriod>(localperiods);
        RaisePropertyChanged(nameof(Periods));
    }

    public void LoadOperations(decimal doccode)
    {
        Load(doccode);
        if (!Operations.Any(o =>
                Periods.Count == 0 || Periods.Where(_ => _.PeriodType == PeriodType.Day)
                    .All(_ => _.DateStart != o.DocDate))) return;
        LoadPeriod(Operations);
    }

    public ObservableCollection<KontragentBalansRowViewModel> Load(decimal dc)
    {
        try
        {
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = false;
            using (var ent = GlobalOptions.GetEntities())
            {
                if (GlobalOptions.ReferencesCache.GetKontragent(dc) == null)
                    throw new Exception($"Контрагент {dc} не найден. RecalcKontragentBalans.CalcBalans");
                RecalcKontragentBalans.CalcBalans(dc,
                    GlobalOptions.ReferencesCache.GetKontragent(dc).StartBalans);
                var sql = "SELECT DOC_NAME as DocName, " +
                          "CASE WHEN DOC_TYPE_CODE = 101 THEN cast(td_101.CODE as varchar) ELSE DOC_NUM END AS DocNum, " +
                          "DOC_DATE as DocDate, " +
                          "cast(CRS_KONTR_IN as numeric(18,4)) as CrsKontrIn, " +
                          "cast(CRS_KONTR_OUT as numeric(18,4)) as CrsKontrOut, " +
                          "DOC_DC as DocDC, " +
                          "DOC_ROW_CODE as DocRowCode, " +
                          "DOC_TYPE_CODE as DocTypeCode, " +
                          "cast(CRS_OPER_IN as numeric(18,4)) as CrsOperIn, " +
                          "cast(CRS_OPER_OUT as numeric(18,4)) as CrsOperOut, " +
                          "OPER_CRS_DC as CrsOperDC, " +
                          "cast(OPER_CRS_RATE as numeric(18,4)) as CrsOperRate, " +
                          "cast(UCH_CRS_RATE as numeric(18,4)) as CrsUchRate, " +
                          "KONTR_DC as DocCode, " +
                          "CASE WHEN DOC_TYPE_CODE = 101 THEN '' ELSE DOC_EXT_NUM END as DocExtNum, " +
                          " ISNULL(sd_24.DD_NOTES,'') + ISNULL(Sd_26.SF_NOTES,'') + ISNULL(SD_33.NOTES_ORD,'') + ISNULL(SD_34.NOTES_ORD,'') + " +
                          "ISNULL(sd_84.SF_NOTE,'') + ISNULL(td_101.VVT_DOC_NUM,'') + ISNULL(td_110.VZT_DOC_NOTES,'') + " +
                          "ISNULL(sd_430.ASV_NOTES, '') +" +
                          "CASE WHEN s26.DOC_CODE IS NOT NULL THEN 'С/ф поставщика №' + convert(varchar,s26.SF_IN_NUM) + ' от ' + convert(varchar,s26.SF_POSTAV_DATE,110) ELSE '' end +" +
                          "CASE WHEN s84.DOC_CODE IS NOT NULL THEN 'С/ф клиенту №' + convert(varchar,s84.SF_IN_NUM) + ' от ' + convert(varchar,s84.SF_DATE,110) ELSE '' end AS Notes " +
                          "FROM dbo.KONTR_BALANS_OPER_ARC kboa " +
                          "LEFT OUTER JOIN sd_24 ON DOC_DC = sd_24.DOC_CODE " +
                          "LEFT OUTER JOIN sd_26 ON DOC_DC = sd_26.DOC_CODE " +
                          "LEFT OUTER JOIN SD_33 ON DOC_DC = sd_33.DOC_CODE " +
                          "LEFT OUTER JOIN SD_34 ON DOC_DC = sd_34.DOC_CODE " +
                          "LEFT OUTER JOIN SD_84 ON DOC_DC = sd_84.DOC_CODE " +
                          "LEFT OUTER JOIN td_101 ON DOC_ROW_CODE = td_101.code " +
                          "LEFT OUTER JOIN td_110 ON DOC_DC = td_110.DOC_CODE AND kboa.DOC_ROW_CODE = td_110.code " +
                          "LEFT OUTER JOIN sd_26 s26 ON td_110.VZT_SPOST_DC = s26.DOC_CODE " +
                          "LEFT OUTER JOIN sd_84 s84 ON td_110.VZT_SFACT_DC = s84.DOC_CODE " +
                          "LEFT OUTER JOIN sd_430 ON doc_dc = sd_430.DOC_CODE " +
                          $"WHERE KONTR_DC = {CustomFormat.DecimalToSqlDecimal(dc)} " +
                          "ORDER BY kboa.DOC_DATE;";
                var data1 = ent.Database.SqlQuery<KontragentBalansRowViewModel>(sql).ToList();
                decimal sum = 0;
                Operations.Clear();
                var sn = data1.FirstOrDefault(_ => _.DocName == " На начало учета");
                if (sn != null)
                {
                    if (sn.CrsKontrIn > 0)
                        sum = -sn.CrsKontrIn;
                    else
                        sum = sn.CrsKontrOut;
                    //Operations.Add(KontragentBalansRowViewModel.DbToViewModel(sn, (decimal) Math.Round(sum, 2)));
                    Operations.Add(sn);
                    data1.Remove(sn);
                }

                foreach (var d in data1)
                {
                    sum += d.CrsKontrOut - d.CrsKontrIn;
                    d.Nakopit = sum;
                    Operations.Add(d);
                }

                LastBalansSumma = Math.Round(sum, 2);
                LastOperationDate = Operations.Count > 0 ? Operations.Max(_ => _.DocDate) : DateTime.MinValue;
                //var prjcts = ent.ProjectsDocs.Include(_ => _.Projects).ToList();
                //foreach (var o in Operations)
                //    switch (o.DocName)
                //    {
                //        case "Акт в/з ":
                //        case "Акт конвертации ":
                //            var prj1 = prjcts.FirstOrDefault(_ => _.DocDC == o.DocDC && _.DocRowId == o.DocRowCode);
                //            if (prj1 != null)
                //                o.Project = GlobalOptions.ReferencesCache.GetProject(prj1.Projects.Id) as Project;
                //            break;
                //        default:
                //            var prj = prjcts.FirstOrDefault(_ => _.DocDC == o.DocDC);
                //            if (prj != null)
                //                o.Project = GlobalOptions.ReferencesCache.GetProject(prj.Projects.Id) as Project;
                //            break;
                //    }
            }

            return Operations;
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
            return null;
        }
        finally
        {
            GlobalOptions.ReferencesCache.IsChangeTrackingOn = true;
        }
    }

    public override void RefreshData(object obj)
    {
        base.RefreshData(obj);
        if (myKontragent == null) return;
        CurrentPeriod = null;
        Periods.Clear();
        Operations.Clear();
        LoadOperations(myKontragent.DocCode);
    }

    #region Command

    public ICommand DocumentLinkSFOpenCommand
    {
        get { return new Command(DocumentLinkSFOpen, _ => CurrentDocument != null && CurrentDocument.Notes.Contains("С/ф")); }
    }
    
    private void DocumentLinkSFOpen(object obj)
    {
        decimal? dc = null;
        using (var ctx = GlobalOptions.GetEntities())
        {
            switch (CurrentDocument.DocTypeCode)
            {
                case DocumentType.Bank:
                    
                    if (CurrentDocument.CrsKontrIn > 0)
                    {
                        dc = ctx.TD_101.FirstOrDefault(_ => _.CODE == CurrentDocument.DocRowCode)?.VVT_SFACT_CLIENT_DC;
                        if (dc != null) DocumentsOpenManager.Open(DocumentType.InvoiceClient, dc.Value);
                    }
                    if (CurrentDocument.CrsKontrOut > 0)
                    {
                        dc = ctx.TD_101.FirstOrDefault(_ => _.CODE == CurrentDocument.DocRowCode)?.VVT_SFACT_POSTAV_DC;
                        if(dc != null) DocumentsOpenManager.Open(DocumentType.InvoiceProvider,dc.Value);
                    }
                    break;
                case DocumentType.MutualAccounting: 
                case DocumentType.CurrencyConvertAccounting:
                    var row = ctx.TD_110.FirstOrDefault(_ =>
                        _.DOC_CODE == CurrentDocument.DocDC && _.CODE == CurrentDocument.DocRowCode);
                    if (row == null) return;
                    if(row.VZT_SFACT_DC != null)
                        DocumentsOpenManager.Open(DocumentType.InvoiceClient, row.VZT_SFACT_DC.Value);
                    if (row.VZT_SPOST_DC != null)
                        DocumentsOpenManager.Open(DocumentType.InvoiceProvider, row.VZT_SPOST_DC.Value);
                    break;
            }
        }
    }

    public ICommand DocumentLinkToProjectCommand
    {
        get { return new Command(DocumentLinkToProject, _ => SelectedDocs != null && SelectedDocs.Count > 0); }
    }

    //public override void ResetLayout(object form)
    //{
    //    var curKontr = Kontragent;
    //    base.ResetLayout(form);
    //    Kontragent = curKontr;
    //}

    private void DocumentLinkToProject(object obj)
    {
        //try
        //{
        //    using (var ctx = GlobalOptions.GetEntities())
        //    {
        //        var prj = StandartDialogs.SelectProject();
        //        if (prj == null) return;
        //        foreach (var d in SelectedDocs)
        //        {
        //            switch (d.DocName)
        //            {
        //                case "Акт конвертации ":
        //                    ctx.ProjectsDocs.Add(new ProjectsDocs
        //                    {
        //                        Id = Guid.NewGuid(),
        //                        ProjectId = prj.Id,
        //                        DocDC = d.DocDC,
        //                        DocTypeName = "Акт конвертации",
        //                        DocInfo = d.DocName,
        //                        DocRowId = d.DocRowCode
        //                    });
        //                    d.Project = prj;
        //                    break;
        //                case "Акт в/з ":
        //                    ctx.ProjectsDocs.Add(new ProjectsDocs
        //                    {
        //                        Id = Guid.NewGuid(),
        //                        ProjectId = prj.Id,
        //                        DocDC = d.DocDC,
        //                        DocTypeName = "Акт взаимозачета",
        //                        DocInfo = d.DocName,
        //                        DocRowId = d.DocRowCode
        //                    });
        //                    d.Project = prj;
        //                    break;
        //                case "Банковская выписка":
        //                    ctx.ProjectsDocs.Add(new ProjectsDocs
        //                    {
        //                        Id = Guid.NewGuid(),
        //                        ProjectId = prj.Id,
        //                        DocDC = d.DocDC,
        //                        DocTypeName = "Банковская выписка",
        //                        DocInfo = d.DocName,
        //                        DocRowId = d.DocRowCode
        //                    });
        //                    d.Project = prj;
        //                    break;
        //            }

        //            if (d.DocName.Contains("С/Ф") &&
        //                !d.DocName.ToUpper().Contains("УСЛУГИ")
        //                && d.DocName.ToUpper().Contains("КЛИЕНТУ"))
        //            {
        //                ctx.ProjectsDocs.Add(new ProjectsDocs
        //                {
        //                    Id = Guid.NewGuid(),
        //                    ProjectId = prj.Id,
        //                    DocDC = d.DocDC,
        //                    DocTypeName = "Отгрузка товара",
        //                    DocInfo = d.DocName
        //                });
        //                d.Project = prj;
        //            }

        //            if (d.DocName.ToUpper().Contains("С/Ф") &&
        //                d.DocName.ToUpper().Contains("ПОСТАВЩИКА")
        //                && !d.DocName.ToUpper().Contains("УСЛУГИ"))
        //            {
        //                ctx.ProjectsDocs.Add(new ProjectsDocs
        //                {
        //                    Id = Guid.NewGuid(),
        //                    ProjectId = prj.Id,
        //                    DocDC = d.DocDC,
        //                    DocTypeName = "Услуги поставщиков",
        //                    DocInfo = d.DocName
        //                });
        //                d.Project = prj;
        //            }

        //            if (d.DocName.ToUpper().Contains("С/Ф") &&
        //                d.DocName.ToUpper().Contains("ПОСТАВЩИКА")
        //                && d.DocName.ToUpper().Contains("ТМЦ"))
        //            {
        //                ctx.ProjectsDocs.Add(new ProjectsDocs
        //                {
        //                    Id = Guid.NewGuid(),
        //                    ProjectId = prj.Id,
        //                    DocDC = d.DocDC,
        //                    DocTypeName = "Приход товара",
        //                    DocInfo = d.DocName
        //                });
        //                d.Project = prj;
        //            }

        //            if (d.DocName.ToUpper().Contains("УСЛУГИ")
        //                && d.DocName.ToUpper().Contains("КЛИЕНТУ"))
        //            {
        //                ctx.ProjectsDocs.Add(new ProjectsDocs
        //                {
        //                    Id = Guid.NewGuid(),
        //                    ProjectId = prj.Id,
        //                    DocDC = d.DocDC,
        //                    DocTypeName = "Услуги клиенту",
        //                    DocInfo = d.DocName,
        //                    DocRowId = d.DocRowCode
        //                });
        //                d.Project = prj;
        //            }

        //            if (d.DocName.ToUpper().Contains("ПРИХОДНЫЙ")
        //                && d.DocName.ToUpper().Contains("ОРДЕР"))
        //            {
        //                ctx.ProjectsDocs.Add(new ProjectsDocs
        //                {
        //                    Id = Guid.NewGuid(),
        //                    ProjectId = prj.Id,
        //                    DocDC = CurrentDocument.DocDC,
        //                    DocTypeName = "Кассовые документы",
        //                    DocInfo = CurrentDocument.DocName
        //                });
        //                d.Project = prj;
        //            }

        //            if (d.DocName.ToUpper().Contains("РАСХОДНЫЙ")
        //                && d.DocName.ToUpper().Contains("ОРДЕР"))
        //            {
        //                ctx.ProjectsDocs.Add(new ProjectsDocs
        //                {
        //                    Id = Guid.NewGuid(),
        //                    ProjectId = prj.Id,
        //                    DocDC = d.DocDC,
        //                    DocTypeName = "Кассовые документы",
        //                    DocInfo = d.DocName
        //                });
        //                d.Project = prj;
        //            }
        //        }

        //        ctx.SaveChanges();
        //    }
        //}
        //catch (Exception ex)
        //{
        //    WindowManager.ShowError(ex);
        //}
    }

    public ICommand DocumentUnLinkProjectCommand
    {
        get { return new Command(DocumentUnLinkProject, _ => SelectedDocs != null && SelectedDocs.Count > 0); }
    }

    private void DocumentUnLinkProject(object obj)
    {
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                //foreach (var d in SelectedDocs.Where(_ => _.Project != null))
                //{
                //    var p = ctx.ProjectsDocs.FirstOrDefault(_ =>
                //        _.ProjectId == d.Project.Id && _.DocDC == d.DocDC);
                //    if (p != null) ctx.ProjectsDocs.Remove(p);
                //    d.Project = null;
                //}

                //ctx.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    public new Command DocumentOpenCommand
    {
        get { return new Command(DocumentOpen, _ => IsDocumentOpenAllow); }
    }

    public override bool IsDocumentOpenAllow => CurrentDocument != null &&
                                                DocumentsOpenManager.IsDocumentOpen(CurrentDocument.DocTypeCode);

    protected override void DocumentOpen(object obj)
    {
        var WinManager = new WindowManager();
        if (CurrentDocument.DocTypeCode == DocumentType.Bank)
        {
            DocumentsOpenManager.Open(CurrentDocument.DocTypeCode, CurrentDocument.DocRowCode);
            return;
        }

        if (CurrentDocument.DocTypeCode == DocumentType.NomenklReturnOfClient)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var s = CurrentDocument.DocNum.Split('/');
                var num = Convert.ToInt32(s[0]);
                var retDoc = ctx.NomenklReturnOfClient.FirstOrDefault(_ => _.DocDate == CurrentDocument.DocDate
                                                                           && _.DocNum == num);
                if (retDoc is not null)
                    DocumentsOpenManager.Open(CurrentDocument.DocTypeCode, retDoc.Id);
            }

            return;
        }

        if (CurrentDocument.DocTypeCode == DocumentType.AccruedAmountForClient ||
            CurrentDocument.DocTypeCode == DocumentType.AccruedAmountOfSupplier
            || CurrentDocument.DocTypeCode == DocumentType.NomenklReturnToProvider
            || CurrentDocument.DocTypeCode == DocumentType.NomenklReturnOfClient)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var s = CurrentDocument.DocNum.Split('/');
                var num = Convert.ToInt32(s[0]);
                if (CurrentDocument.DocTypeCode == DocumentType.AccruedAmountForClient)
                {
                    var doc = ctx.AccruedAmountForClient.FirstOrDefault(_ => _.DocInNum == num);
                    if (doc != null)
                        DocumentsOpenManager.Open(CurrentDocument.DocTypeCode, 0, doc.Id);
                    else
                        WinManager.ShowWinUIMessageBox("Документ не найден.", "Сообщение");
                }

                if (CurrentDocument.DocTypeCode == DocumentType.AccruedAmountOfSupplier)
                {
                    var doc = ctx.AccruedAmountOfSupplier.FirstOrDefault(_ => _.DocInNum == num);
                    if (doc != null)
                        DocumentsOpenManager.Open(CurrentDocument.DocTypeCode, 0, doc.Id);
                    else
                        WinManager.ShowWinUIMessageBox("Документ не найден.", "Сообщение");
                }

                if (CurrentDocument.DocTypeCode == DocumentType.NomenklReturnToProvider)
                {
                    var doc = ctx.NomenklReturnToProvider.FirstOrDefault(_ => _.DocNum == num);
                    if (doc != null)
                        DocumentsOpenManager.Open(CurrentDocument.DocTypeCode, 0, doc.Id);
                    else
                        WinManager.ShowWinUIMessageBox("Документ не найден.", "Сообщение");
                }

                if (CurrentDocument.DocTypeCode == DocumentType.NomenklReturnOfClient)
                {
                    var doc = ctx.NomenklReturnOfClient.FirstOrDefault(_ => _.DocNum == num);
                    if (doc != null)
                        DocumentsOpenManager.Open(CurrentDocument.DocTypeCode, 0, doc.Id);
                    else
                        WinManager.ShowWinUIMessageBox("Документ не найден.", "Сообщение");
                }
            }

            return;
        }

        DocumentsOpenManager.Open(CurrentDocument.DocTypeCode, CurrentDocument.DocDC);
    }

    public ICommand SetAllPeriodsCommand
    {
        get { return new Command(SetAllPeriods, _ => true); }
    }

    private void SetAllPeriods(object obj)
    {
        IsPeriodsEnabled = !IsPeriodsEnabled;
        PeriodModeName = IsPeriodsEnabled ? "Показать по всем периодам" : "Показать по выбранному периоду";
        if (IsPeriodsEnabled)
            LoadDocumentsForPeriod();
        else
            LoadAllDocuments();
    }

    public ICommand PeriodTreeClickCommand
    {
        get { return new Command(PeriodTreeClick, _ => true); }
    }

    private void PeriodTreeClick(object obj)
    {
        IsPeriodsEnabled = true;
        LoadDocumentsForPeriod();
        PeriodModeName = "Показать по всем периодам";
    }

    private bool myIsPeriodsEnabled = true;

    public bool IsPeriodsEnabled
    {
        get => myIsPeriodsEnabled;
        set
        {
            if (myIsPeriodsEnabled == value) return;
            myIsPeriodsEnabled = value;
            RaisePropertyChanged();
        }
    }

    private string myPeriodModeName = "Показать по всем периодам";
    private string myPeriodName;
    private string myResponsibleName;

    public string PeriodModeName
    {
        get => myPeriodModeName;
        set
        {
            if (myPeriodModeName == value) return;
            myPeriodModeName = value;
            RaisePropertyChanged();
        }
    }

    public override void OnWindowClosing(object obj)
    {
        mySubscriber?.UnsubscribeAll();
        base.OnWindowClosing(obj);
    }

    protected override void OnWindowLoaded(object obj)
    {
        base.OnWindowLoaded(obj);
        var frm = Form as KontragentBalansView;
        if (frm == null) return;
        frm.treeListPeriodView.ShowTotalSummary = false;
        var nakSummaries = new List<GridSummaryItem>();
        foreach (var s in frm.KontrOperGrid.TotalSummary)
            if (s.FieldName == "Nakopit" || s.FieldName == "CrsOperRate")
                nakSummaries.Add(s);

        foreach (var c in nakSummaries) frm.KontrOperGrid.TotalSummary.Remove(c);
    }

    #endregion
}

[MetadataType(typeof(DataAnnotationKontragentPeriod))]
public class KontragentPeriod : DatePeriod
{
    [DataMember] public decimal EndPeriodSumma { set; get; }
}

public class DataAnnotationKontragentPeriod : DataAnnotationForFluentApiBase, IMetadataProvider<KontragentPeriod>
{
    void IMetadataProvider<KontragentPeriod>.BuildMetadata(MetadataBuilder<KontragentPeriod> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.PeriodType).NotAutoGenerated();
        //builder.Property(_ => _.DateEnd).NotAutoGenerated().DisplayName("Конец");
        //builder.Property(_ => _.DateStart).NotAutoGenerated().DisplayName("Начало");
        builder.Property(_ => _.EndPeriodSumma).AutoGenerated().LocatedAt(1).ReadOnly()
            .DisplayName("Сумма на конец").DisplayFormatString("n2");
        builder.Property(_ => _.Name).AutoGenerated().LocatedAt(0).ReadOnly().DisplayName("Период");
    }
}
