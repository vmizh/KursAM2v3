﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Core.ViewModel.Base;
using KursDomain.WindowsManager.WindowsManager;
using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using Helper;
using KursAM2.Dialogs;
using KursAM2.Managers;
using KursAM2.Managers.Base;
using KursAM2.Managers.Invoices;
using KursAM2.Repositories.RedisRepository;
using KursAM2.View.DialogUserControl;
using KursAM2.View.Finance;
using KursAM2.View.Helper;
using KursAM2.ViewModel.Management.Calculations;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.Documents.Vzaimozachet;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;
using KursRepositories.Repositories.MutualAccounting;
using Newtonsoft.Json;
using StackExchange.Redis;
using static KursAM2.ViewModel.Finance.MutualAccountingDebitorCreditors;

namespace KursAM2.ViewModel.Finance;

public sealed class MutualAcountingWindowViewModel : RSWindowViewModelBase
{
    //public readonly MutualAccountingManager Manager = new MutualAccountingManager();
    private readonly IMutualAccountingRepository myMutAccRepository;
    private readonly ISubscriber mySubscriber;

    private readonly ConnectionMultiplexer redis;
    private MutualAccountingCreditorViewModel myCurrentCreditor;

    private CrossCurrencyRate myCurrentCurrencyRate;
    private MutualAccountingDebitorViewModel myCurrentDebitor;
    private bool myIsCurrencyConvert;


    public MutualAcountingWindowViewModel()
    {
        myMutAccRepository = new MutualAccountingRepository(GlobalOptions.GetEntities());
        // ReSharper disable once VirtualMemberCallInConstructor
        IsDocNewCopyAllow = true;
        IsDocNewCopyRequisiteAllow = true;
        DebitorCollection.CollectionChanged += DebitorCollection_CollectionChanged;
        CreditorCollection.CollectionChanged += CreditorCollection_CollectionChanged;
        try
        {
            redis = ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["redis.connection"]);
            mySubscriber = redis.GetSubscriber();

            if (mySubscriber.IsConnected())
                mySubscriber.Subscribe(
                    new RedisChannel(RedisMessageChannels.MutualAccounting, RedisChannel.PatternMode.Auto),
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

        UpdateVisualData();
    }

    public MutualAcountingWindowViewModel(decimal dc) : this()
    {
        RefreshData(dc);
        CalcItogoSumma();
    }

    public new string WindowName => Document != null && Document.State != RowStatus.NewRow ? Document.Description :
        IsCurrencyConvert ? "Новый акт конвертации" : "Новый акт взаимозачета";

    public bool IsTypeVzaimEnabled => !IsCurrencyConvert;

    public decimal CurrencyConvertRate
    {
        get
        {
            if (!IsCurrencyConvert)
                return 1;
            var sumCreditor = Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1).Sum(_ => _.VZT_CRS_SUMMA) ??
                              0;
            var sumDebitor = Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0).Sum(_ => _.VZT_CRS_SUMMA) ?? 0;
            if (Equals(Document.CreditorCurrency, GlobalOptions.SystemProfile.NationalCurrency))
                return sumDebitor == 0 ? 0 : sumCreditor / sumDebitor;
            if (Equals(Document.DebitorCurrency, GlobalOptions.SystemProfile.NationalCurrency))
                return sumCreditor == 0 ? 0 : sumDebitor / sumCreditor;
            return sumDebitor == 0 ? 0 : sumCreditor / sumDebitor;
        }
    }

    public SD_110ViewModel Document { set; get; }
    public bool IsNotOld => !Document.IsOld;
    public bool IsCanDebitorCrsChanged => DebitorCollection.Count == 0;
    public bool IsCanCreditorCrsChanged => IsCurrencyConvert && CreditorCollection.Count == 0;

    public string NotifyInfo { get; set; }

    public CrossCurrencyRate CurrentCurrencyRate
    {
        get => myCurrentCurrencyRate;
        set
        {
            if (Equals(myCurrentCurrencyRate, value)) return;
            myCurrentCurrencyRate = value;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<CrossCurrencyRate> CurrencyRates { set; get; } =
        new ObservableCollection<CrossCurrencyRate>();

    public ObservableCollection<MutualAccountingDebitorViewModel> DebitorCollection { set; get; } =
        new ObservableCollection<MutualAccountingDebitorViewModel>();

    public ObservableCollection<MutualAccountingCreditorViewModel> CreditorCollection { set; get; } =
        new ObservableCollection<MutualAccountingCreditorViewModel>();

    public MutualAccountingDebitorViewModel CurrentDebitor
    {
        get => myCurrentDebitor;
        set
        {
            if (Equals(myCurrentDebitor, value)) return;
            myCurrentDebitor = value;
            RaisePropertyChanged();
        }
    }

    public MutualAccountingCreditorViewModel CurrentCreditor
    {
        get => myCurrentCreditor;
        set
        {
            if (Equals(myCurrentCreditor, value)) return;
            myCurrentCreditor = value;
            RaisePropertyChanged();
        }
    }

    public bool IsCurrencyConvert
    {
        get => myIsCurrencyConvert;
        set
        {
            if (myIsCurrencyConvert == value) return;
            myIsCurrencyConvert = value;
            RaisePropertyChanged();
        }
    }

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

    public override void UpdateDocumentOpen()
    {
    }

    /// <summary>
    ///     Зпускает RaisePropertyChanged
    ///     для объектов на экране
    /// </summary>
    public void UpdateVisualData()
    {
        UpdateDebitorCreditorCollections(null);
        RaisePropertyChanged(nameof(IsCurrencyConvert));
        RaisePropertyChanged(nameof(Document));
        RaisePropertyChanged(nameof(DebitorCollection));
        RaisePropertyChanged(nameof(CreditorCollection));
    }

    private void CreditorCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(IsCanCreditorCrsChanged));
    }

    private void DebitorCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(IsCanDebitorCrsChanged));
    }

    public void CreateMenu()
    {
        LeftMenuBar = MenuGenerator.DocWithRowsLeftBar(this);
        RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
    }

    #region Commands

    //<MenuItem Header="Открыть счет-фактуру" Command="{Binding OpenDebitorSFCommand}" />
    //<Separator />

    public ICommand OpenDebitorSFCommand
    {
        get { return new Command(OpenDebitorSF, _ => CurrentDebitor?.SfClient != null); }
    }

    private void OpenDebitorSF(object obj)
    {
        DocumentsOpenManager.Open(DocumentType.InvoiceClient, CurrentDebitor.SfClient.DocCode);
    }

    public ICommand OpenCreditorSFCommand
    {
        get { return new Command(OpenCreditorSF, _ => CurrentCreditor?.SFProvider != null); }
    }

    private void OpenCreditorSF(object obj)
    {
        DocumentsOpenManager.Open(DocumentType.InvoiceProvider, CurrentCreditor.SFProvider.DocCode);
    }


    public ICommand SetDebitorSFCommand
    {
        get { return new Command(SetDebitorSF, _ => CurrentDebitor != null); }
    }

    public ICommand SetNewDebitorSFCommand
    {
        get { return new Command(SetNewDebitorSF, _ => Document.MutualAccountingOldType != null); }
    }

    private void SetNewDebitorSF(object obj)
    {
        var item = StandartDialogs.SelectInvoiceClient(true, true,
            DebitorCollection.Select(_ => _.SfClient?.DocCode ?? 0).Distinct().ToList(), Document.DebitorCurrency);
        if (item == null) return;
        NomenklProductType vzdefault = null;
        var vzdefDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(_ =>
            _.SECTION == "MUTUAL_ACCOUNTING" && _.ITEM == "DEFAULT_TYPE_PRODUCT");
        if (vzdefDC != null)
            vzdefault =
                GlobalOptions.ReferencesCache.GetNomenklProductType(Convert.ToDecimal(vzdefDC.ITEM_VALUE)) as
                    NomenklProductType;
        var m = DebitorCollection.Any() ? DebitorCollection.Max(_ => _.Code) + 1 : 0;
        var m2 = CreditorCollection.Any() ? CreditorCollection.Max(_ => _.Code) + 1 : 0;
        var _code = Math.Max(m, m2) + 1;
        var newdeb = new MutualAccountingDebitorViewModel
        {
            DocCode = Document.DocCode,
            Code = _code,
            VZT_DOC_DATE = Document.VZ_DATE,
            VZT_DOC_NUM = (Document.Rows.Count + 1).ToString(),
            VZT_1MYDOLZH_0NAMDOLZH = 0,
            VZT_CRS_POGASHENO = item.Summa - item.PaySumma,
            VZT_UCH_CRS_POGASHENO = item.Summa - item.PaySumma,
            VZT_CRS_SUMMA = item.Summa - item.PaySumma,
            VZT_KONTR_CRS_SUMMA = -(item.Summa - item.PaySumma),
            VZT_UCH_CRS_RATE = 1,
            State = RowStatus.NewRow,
            VzaimoraschType = vzdefault,
            Parent = Document,
            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(item.Entity.SF_CLIENT_DC) as Kontragent,
            SfClient = item,
            MaxSumma = item.Summa - item.PaySumma
        };
        if (Document.DebitorCurrency == null)
            Document.DebitorCurrency = newdeb.Currency;
        Document.Rows.Add(newdeb);
        DebitorCollection.Add(newdeb);
        CurrentDebitor = newdeb;
        // ReSharper disable once PossibleNullReferenceException
        KontragentManager.UpdateSelectCount(newdeb.Kontragent.DocCode);
        CalcItogoSumma();
    }

    private void SetCreditorSF(object obj)
    {
        var item = StandartDialogs.SelectInvoiceProvider(CurrentCreditor.VZT_KONTR_DC, true, true, true);
        if (item == null) return;
        CurrentCreditor.VZT_DOC_NUM = (Document.Rows.Count + 1).ToString();
        CurrentCreditor.VZT_CRS_POGASHENO = item.Summa - item.PaySumma;
        CurrentCreditor.VZT_UCH_CRS_POGASHENO = item.Summa - item.PaySumma;
        CurrentCreditor.VZT_CRS_SUMMA = item.Summa - item.PaySumma;
        CurrentCreditor.VZT_KONTR_CRS_SUMMA = item.Summa - item.PaySumma;
        CurrentCreditor.VZT_UCH_CRS_RATE = 1;
        CurrentCreditor.VZT_SPOST_DC = item.DocCode;
        CurrentCreditor.Kontragent =
            GlobalOptions.ReferencesCache.GetKontragent(item.Entity.SF_POST_DC) as Kontragent;
        CurrentCreditor.SFProvider = item;
        CurrentCreditor.MaxSumma = item.Summa - item.PaySumma;
        if (Document.CreditorCurrency == null)
            Document.CreditorCurrency = CurrentCreditor.Currency;
        if (CurrentCreditor.State == RowStatus.NotEdited) CurrentCreditor.myState = RowStatus.Edited;
        // ReSharper disable once PossibleNullReferenceException
        KontragentManager.UpdateSelectCount(CurrentCreditor.Kontragent.DocCode);
        CalcItogoSumma();
    }


    private void SetNewCreditorSF(object obj)
    {
        var item = StandartDialogs.SelectInvoiceProvider(true, true,
            CreditorCollection.Select(_ => _.SFProvider?.DocCode ?? 0).Distinct().ToList(),
            true, Document.CreditorCurrency);
        if (item == null) return;
        NomenklProductType vzdefault = null;
        var vzdefDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(_ =>
            _.SECTION == "MUTUAL_ACCOUNTING" && _.ITEM == "DEFAULT_TYPE_PRODUCT");
        if (vzdefDC != null)
            vzdefault =
                GlobalOptions.ReferencesCache.GetNomenklProductType(Convert.ToDecimal(vzdefDC.ITEM_VALUE)) as
                    NomenklProductType;
        var m = DebitorCollection.Any() ? DebitorCollection.Max(_ => _.Code) + 1 : 0;
        var m2 = CreditorCollection.Any() ? CreditorCollection.Max(_ => _.Code) + 1 : 0;
        var _code = Math.Max(m, m2) + 1;
        var newcred = new MutualAccountingCreditorViewModel
        {
            DocCode = Document.DocCode,
            Code = _code,
            VZT_DOC_DATE = Document.VZ_DATE,
            VZT_DOC_NUM = (Document.Rows.Count + 1).ToString(),
            VZT_1MYDOLZH_0NAMDOLZH = 1,
            VZT_CRS_POGASHENO = item.Summa - item.PaySumma,
            VZT_UCH_CRS_POGASHENO = item.Summa - item.PaySumma,
            VZT_CRS_SUMMA = item.Summa - item.PaySumma,
            VZT_KONTR_CRS_SUMMA = item.Summa - item.PaySumma,
            VZT_UCH_CRS_RATE = 1,
            VZT_SPOST_DC = item.DocCode,
            State = RowStatus.NewRow,
            VzaimoraschType = vzdefault,
            Parent = Document,
            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(item.Entity.SF_POST_DC) as Kontragent,
            SFProvider = item,
            MaxSumma = item.Summa - item.PaySumma
        };
        Document.Rows.Add(newcred);
        if (Document.CreditorCurrency == null)
            Document.CreditorCurrency = newcred.Currency;
        CreditorCollection.Add(newcred);
        CurrentCreditor = newcred;
        // ReSharper disable once PossibleNullReferenceException
        KontragentManager.UpdateSelectCount(newcred.Kontragent.DocCode);
        CalcItogoSumma();
    }

    public ICommand SetCreditorSFCommand
    {
        get { return new Command(SetCreditorSF, _ => CurrentCreditor != null); }
    }

    public ICommand SetNewCreditorSFCommand
    {
        get { return new Command(SetNewCreditorSF, _ => Document.MutualAccountingOldType != null); }
    }

    private void SetDebitorSF(object obj)
    {
        var item = StandartDialogs.SelectInvoiceClient(CurrentDebitor.VZT_KONTR_DC, true, true);
        if (item == null) return;
        CurrentDebitor.VZT_DOC_NUM = (Document.Rows.Count + 1).ToString();
        CurrentDebitor.VZT_CRS_POGASHENO = item.Summa - item.PaySumma;
        CurrentDebitor.VZT_UCH_CRS_POGASHENO = item.Summa - item.PaySumma;
        CurrentDebitor.VZT_CRS_SUMMA = item.Summa - item.PaySumma;
        CurrentDebitor.VZT_KONTR_CRS_SUMMA = -(item.Summa - item.PaySumma);
        CurrentDebitor.VZT_UCH_CRS_RATE = 1;
        CurrentDebitor.VZT_SFACT_DC = item.DocCode;
        CurrentDebitor.Kontragent =
            GlobalOptions.ReferencesCache.GetKontragent(item.Entity.SF_CLIENT_DC) as Kontragent;
        CurrentDebitor.SfClient = item;
        if (CurrentDebitor.State == RowStatus.NotEdited) CurrentDebitor.myState = RowStatus.Edited;
        CurrentDebitor.MaxSumma = item.Summa - item.PaySumma;
        // ReSharper disable once PossibleNullReferenceException
        KontragentManager.UpdateSelectCount(CurrentDebitor.Kontragent.DocCode);
        UpdateVisualData();
        CalcItogoSumma();
    }

    public ICommand UpdateCreditorCalcSummaCommand
    {
        get { return new Command(UpdateCreditorCalcSumma, _ => true); }
    }

    public ICommand UpdateDebitorCalcSummaCommand
    {
        get { return new Command(UpdateDebitorCalcSumma, _ => true); }
    }

    private decimal CalcItogoSumma()
    {
        decimal sumLeft = 0, sumRight = 0;
        foreach (var l in DebitorCollection)
            if (l.Kontragent?.IsBalans == true)
                // ReSharper disable once PossibleInvalidOperationException
                sumLeft += Math.Abs((decimal)l.VZT_CRS_SUMMA);
        foreach (var l in CreditorCollection)
            if (l.Kontragent?.IsBalans == true)
                // ReSharper disable once PossibleInvalidOperationException
                sumRight += (decimal)l.VZT_CRS_SUMMA;
        if (!IsCurrencyConvert)
        {
            Document.VZ_RIGHT_UCH_CRS_SUM = sumRight;
            Document.VZ_LEFT_UCH_CRS_SUM = sumLeft;
            Document.VZ_PRIBIL_UCH_CRS_SUM = sumRight - sumLeft;
            return sumRight - sumLeft;
        }

        var d = CurrencyConvertRate != 0
            ? (Document.CreditorCurrency.DocCode == CurrencyCode.RUB ? sumRight / CurrencyConvertRate : sumRight) -
              (Document.DebitorCurrency.DocCode == CurrencyCode.RUB ? sumLeft / CurrencyConvertRate : sumLeft)
            : 0;
        return d;
    }

    private void UpdateDebitorCalcSumma(object obj)
    {
        var param = obj as CellValueChangedEventArgs;
        if (param == null || param.Column.Name != "VZT_CRS_SUMMA") return;
        if (Document == null) return;
        CurrentDebitor.VZT_CRS_SUMMA = (decimal)param.Value;
        if (IsNotOld)
        {
            CalcItogoSumma(); //Document.VZ_RIGHT_UCH_CRS_SUM - Document.VZ_LEFT_UCH_CRS_SUM;
        }
        else
        {
            Document.VZ_LEFT_UCH_CRS_SUM = Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0)
                .Sum(_ => _.VZT_UCH_CRS_POGASHENO);
            Document.VZ_RIGHT_UCH_CRS_SUM = Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1)
                .Sum(_ => _.VZT_UCH_CRS_POGASHENO);
            Document.VZ_PRIBIL_UCH_CRS_SUM =
                CalcItogoSumma(); //Document.VZ_RIGHT_UCH_CRS_SUM - Document.VZ_LEFT_UCH_CRS_SUM;
        }

        //Document.myState = state;
        RaisePropertyChanged(nameof(Document));
        RaisePropertyChanged(nameof(CurrencyConvertRate));
    }

    private void UpdateCreditorCalcSumma(object obj)
    {
        var param = obj as CellValueChangedEventArgs;
        if (param == null || param.Column.Name != "VZT_CRS_SUMMA") return;
        if (Document == null) return;
        CurrentCreditor.VZT_CRS_SUMMA = (decimal)param.Value;
        if (IsNotOld)
        {
            CalcItogoSumma(); //Document.VZ_RIGHT_UCH_CRS_SUM - Document.VZ_LEFT_UCH_CRS_SUM;
        }
        else
        {
            Document.VZ_LEFT_UCH_CRS_SUM = Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 0)
                .Sum(_ => _.VZT_UCH_CRS_POGASHENO);
            Document.VZ_RIGHT_UCH_CRS_SUM = Document.Rows.Where(_ => _.VZT_1MYDOLZH_0NAMDOLZH == 1)
                .Sum(_ => _.VZT_UCH_CRS_POGASHENO);
            Document.VZ_PRIBIL_UCH_CRS_SUM =
                CalcItogoSumma(); //Document.VZ_RIGHT_UCH_CRS_SUM - Document.VZ_LEFT_UCH_CRS_SUM;
        }

        //Document.myState = state;
        RaisePropertyChanged(nameof(Document));
        RaisePropertyChanged(nameof(CurrencyConvertRate));
    }

    public override void ShowHistory(object data)
    {
        // ReSharper disable once RedundantArgumentDefaultValue
        DocumentHistoryManager.LoadHistory(DocumentType.MutualAccounting, null, Document.DocCode);
    }

    public override void RefreshData(object obj)
    {
        var crsrate = new CrossCurrencyRate();
        crsrate.SetRates(DateTime.Today);
        foreach (var c in crsrate.CurrencyList)
            CurrencyRates.Add(c);
        CurrentCurrencyRate = CurrencyRates[0];
        var WinManager = new WindowManager();
        try
        {
            base.RefreshData(obj);
            if (Document != null && IsCanSaveData)
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
                Document = myMutAccRepository.Load(Document.DocCode);
                IsCurrencyConvert = Document.Entity.SD_111.IsCurrencyConvert;
                RaisePropertyChanged(nameof(Document));
            }
            else
            {
                if (obj == null) return;
                var dc = (decimal)obj;
                Document = myMutAccRepository.Load(dc);
                IsCurrencyConvert = Document.Entity.SD_111.IsCurrencyConvert;
                if (Document == null)
                {
                    WinManager.ShowWinUIMessageBox($"Не найден документ с кодом {dc}!",
                        "Ошибка обращения к базе данных", MessageBoxButton.OK, MessageBoxImage.Error,
                        MessageBoxResult.None, MessageBoxOptions.None);
                    return;
                }

                Document.IsOld = myMutAccRepository.CheckDocumentForOld(Document.DocCode);
                RaisePropertyChanged(nameof(Document));
            }

            foreach (var r in Document.Rows)
            {
                r.Parent = Document;
                if (r.VZT_SFACT_DC != null)
                {
                    r.SfClient = InvoicesManager.GetInvoiceClient((decimal)r.VZT_SFACT_DC);
                    r.MaxSumma = r.SfClient.Summa - r.SfClient.PaySumma + (r.VZT_CRS_SUMMA ?? 0);
                }

                if (r.VZT_SPOST_DC != null)
                {
                    r.SFProvider = InvoicesManager.GetInvoiceProvider((decimal)r.VZT_SPOST_DC);
                    r.MaxSumma = r.SFProvider.Summa - r.SFProvider.PaySumma + (r.VZT_CRS_SUMMA ?? 0);
                }

                r.myState = RowStatus.NotEdited;
            }

            UpdateDebitorCreditorCollections(null);
            Document.myState = RowStatus.NotEdited;
            RaisePropertyChanged(nameof(IsCanSaveData));
            CalcItogoSumma();
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    public override void DocDelete(object form)
    {
        IsDeleting = true;
        var res = MessageBox.Show("Вы уверены, что хотите удалить документ?", "Запрос",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        switch (res)
        {
            case MessageBoxResult.Yes:
                myMutAccRepository.Delete(Document);
                if (mySubscriber != null && mySubscriber.IsConnected())
                {
                    var message = new RedisMessage
                    {
                        DocumentType = DocumentType.MutualAccounting,
                        DocCode = Document.DocCode,
                        DocDate = Document.VZ_DATE,
                        IsDocument = true,
                        OperationType = RedisMessageDocumentOperationTypeEnum.Delete,
                        Message =
                            $"Пользователь '{GlobalOptions.UserInfo.Name}' удалил акт зачета (конвертации) {Document.Description}"
                    };
                    message.ExternalValues.Add("KontragentDCList", Document.Rows.Where(_ => _.Kontragent is not null)
                        .Select(r => r.Kontragent.DocCode).Distinct().ToList());
                    message.ExternalValues.Add("ClientInvoiceDCList", Document.Rows.Where(_ => _.SfClient is not null)
                        .Select(r => r.SfClient.DocCode).Distinct().ToList());
                    message.ExternalValues.Add("ProviderInvoiceDCList", Document.Rows
                        .Where(_ => _.SFProvider is not null)
                        .Select(r => r.SFProvider.DocCode).Distinct().ToList());
                    var jsonSerializerSettings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    };
                    var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
                    if (Document.State != RowStatus.NewRow)
                        mySubscriber.Publish(
                            new RedisChannel(RedisMessageChannels.MutualAccounting,
                                RedisChannel.PatternMode.Auto),
                            json);
                }

                Document.State = RowStatus.NotEdited;
                CloseWindow(null);
                break;
            case MessageBoxResult.No:
                return;
        }
    }

    public override void SaveData(object data)
    {
        if (IsCurrencyConvert && Document.CreditorCurrency == null)
            Document.CreditorCurrency = Document.DebitorCurrency;
        RaisePropertyChanged(nameof(Document));
        var res = myMutAccRepository.Save(Document);
        if (IsCurrencyConvert)
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var cred in CreditorCollection)
                    if (cred.SFProvider is not null)
                    {
                        var pay = ctx.ProviderInvoicePay.FirstOrDefault(_ =>
                            _.VZDC == Document.DocCode && _.VZCode == cred.Code);
                        if (pay != null) pay.Rate = CurrencyConvertRate;
                    }

                ctx.SaveChanges();
            }

        DocumentHistoryHelper.SaveHistory(CustomFormat.GetEnumName(DocumentType.MutualAccounting), null,
            Document.DocCode, null, (string)Document.ToJson());
        if (res != null)
        {
            var dcs = new List<decimal>();
            foreach (var d in Document.DeletedRows)
                if (dcs.All(_ => _ != d.VZT_KONTR_DC))
                    dcs.Add(d.VZT_KONTR_DC);
            foreach (var d in Document.Rows)
                if (dcs.All(_ => _ != d.VZT_KONTR_DC))
                    dcs.Add(d.VZT_KONTR_DC);
            foreach (var d in dcs)
                RecalcKontragentBalans.CalcBalans(d, Document.VZ_DATE);
            Document.DeletedRows.Clear();
        }

        LastDocumentManager.SaveLastOpenInfo(
            IsCurrencyConvert ? DocumentType.CurrencyConvertAccounting : DocumentType.MutualAccounting, null,
            Document.DocCode,
            Document.CREATOR, GlobalOptions.UserInfo.NickName, Document.Description);
        if (mySubscriber != null && mySubscriber.IsConnected())
        {
            var str = Document.State == RowStatus.NewRow ? "создал" : "сохранил";
            var message = new RedisMessage
            {
                DocumentType = DocumentType.MutualAccounting,
                DocCode = Document.DocCode,
                DocDate = Document.VZ_DATE,
                IsDocument = true,
                OperationType = Document.myState == RowStatus.NewRow
                    ? RedisMessageDocumentOperationTypeEnum.Create
                    : RedisMessageDocumentOperationTypeEnum.Update,
                Message =
                    $"Пользователь '{GlobalOptions.UserInfo.Name}' {str} акт зачета (конвертации) {Document.Description}"
            };
            message.ExternalValues.Add("KontragentDCList", Document.Rows.Where(_ => _.Kontragent is not null)
                .Select(r => r.Kontragent.DocCode).Distinct().ToList());
            message.ExternalValues.Add("ClientInvoiceDCList", Document.Rows.Where(_ => _.SfClient is not null)
                .Select(r => r.SfClient.DocCode).Distinct().ToList());
            message.ExternalValues.Add("ProviderInvoiceDCList", Document.Rows.Where(_ => _.SFProvider is not null)
                .Select(r => r.SFProvider.DocCode).Distinct().ToList());
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var json = JsonConvert.SerializeObject(message, jsonSerializerSettings);
            mySubscriber.Publish(new RedisChannel(RedisMessageChannels.MutualAccounting, RedisChannel.PatternMode.Auto),
                json);
        }

        CalcItogoSumma();
        foreach (var r in Document.Rows) r.myState = RowStatus.NotEdited;
        Document.myState = RowStatus.NotEdited;
        Document.RaisePropertyChanged("State");
        RaisePropertyChanged(nameof(WindowName));
    }

    private void UpdateDebitorCreditorCollections(TD_110ViewModel vm, bool isDebitor = true)
    {
        DebitorCollection.Clear();
        CreditorCollection.Clear();
        if (!(Document?.Rows.Count > 0)) return;
        // ReSharper disable once PossibleNullReferenceException
        foreach (var r in Document.Rows)
            if (r.VZT_1MYDOLZH_0NAMDOLZH == 0)
            {
                var n = new MutualAccountingDebitorViewModel(r)
                {
                    Kontragent = r.Kontragent,
                    SfClient = r.SfClient,
                    Parent = r.Parent
                };
                DebitorCollection.Add(n);
            }
            else
            {
                var n = new MutualAccountingCreditorViewModel(r)
                {
                    Kontragent = r.Kontragent,
                    SFProvider = r.SFProvider,
                    Parent = r.Parent
                };
                CreditorCollection.Add(n);
            }

        if (vm == null) return;
        if (isDebitor)
            CurrentDebitor = DebitorCollection.FirstOrDefault(_ => _.Code == vm.Code);
        else
            CurrentCreditor = CreditorCollection.FirstOrDefault(_ => _.Code == vm.Code);
    }

    public override bool IsDocDeleteAllow => Document?.State != RowStatus.NewRow;

    public override bool IsCanSaveData => Document != null && (Document.State != RowStatus.NotEdited
                                                               || Document.DeletedRows.Count > 0 ||
                                                               Document.Rows.Any(_ => _.State != RowStatus.NotEdited))
                                                           && Document.DebitorCurrency != null &&
                                                           Document.MutualAccountingOldType != null;

    //private bool isCanSave()
    //{
    //    return Document.State != RowStatus.NotEdited && manager.IsChecked(Document);
    //}

    public override bool IsCanRefresh => Document.State != RowStatus.NewRow;

    public ICommand AddNewCreditorCommand
    {
        get
        {
            return new Command(AddNewCreditor,
                _ => Document?.CreditorCurrency != null ||
                     (!IsCurrencyConvert && Document?.DebitorCurrency != null));
        }
    }

    public ICommand AddNewDebitorCommand
    {
        get { return new Command(AddNewDebitor, _ => Document?.DebitorCurrency != null); }
    }

    public ICommand RemoveCreditorCommand
    {
        get { return new Command(RemoveCreditor, _ => CurrentCreditor != null); }
    }

    public void RemoveCreditor(object obj)
    {
        if (CurrentCreditor != null)
        {
            if (CurrentCreditor.State == RowStatus.NewRow)
            {
                Document.Rows.Remove(CurrentCreditor);
                CreditorCollection.Remove(CurrentCreditor);
                CalcItogoSumma();
                return;
            }

            Document.DeletedRows.Add(CurrentCreditor);
            Document.Rows.Remove(CurrentCreditor);
            if (CurrentCreditor.Parent is RSViewModelBase prnt)
                prnt.State = RowStatus.Edited;
            CreditorCollection.Remove(CurrentCreditor);
            RaisePropertyChanged(nameof(CurrencyConvertRate));
            CalcItogoSumma();
        }
    }

    public ICommand RemoveDebitorCommand
    {
        get { return new Command(RemoveDebitor, _ => CurrentDebitor != null); }
    }

    private void RemoveDebitor(object obj)
    {
        //manager.DeleteRow(Document.Rows, Document.DeletedRows, CurrentDebitor);
        if (CurrentDebitor != null)
        {
            if (CurrentDebitor.State == RowStatus.NewRow)
            {
                Document.Rows.Remove(CurrentDebitor);
                DebitorCollection.Remove(CurrentDebitor);
                CalcItogoSumma();
                return;
            }

            Document.DeletedRows.Add(CurrentDebitor);
            Document.Rows.Remove(CurrentDebitor);
            if (CurrentDebitor.Parent is RSViewModelBase prnt)
                prnt.State = RowStatus.Edited;
            DebitorCollection.Remove(CurrentDebitor);
            RaisePropertyChanged(nameof(CurrencyConvertRate));
            CalcItogoSumma();
        }

        //DebitorCollection.Remove(CurrentDebitor);
    }

    public void AddNewDebitor(object obj)
    {
        try
        {
            NomenklProductType vzdefault = null;
            var vzdefDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(_ =>
                _.SECTION == "MUTUAL_ACCOUNTING" && _.ITEM == "DEFAULT_TYPE_PRODUCT");
            if (vzdefDC != null)
                vzdefault =
                    GlobalOptions.ReferencesCache.GetNomenklProductType(Convert.ToDecimal(vzdefDC.ITEM_VALUE)) as
                        NomenklProductType;
            var k = StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions
                { Currency = Document.IsOld ? null : Document.DebitorCurrency });
            if (k == null) return;
            var m = DebitorCollection.Any() ? DebitorCollection.Max(_ => _.Code) + 1 : 0;
            var m2 = CreditorCollection.Any() ? CreditorCollection.Max(_ => _.Code) + 1 : 0;
            var _code = Math.Max(m, m2) + 1;
            var newdeb = new MutualAccountingDebitorViewModel
            {
                DocCode = Document.DocCode,
                Code = _code,
                VZT_DOC_DATE = Document.VZ_DATE,
                VZT_DOC_NUM = (Document.Rows.Count + 1).ToString(),
                VZT_1MYDOLZH_0NAMDOLZH = 0,
                VZT_CRS_POGASHENO = 0,
                VZT_UCH_CRS_POGASHENO = 0,
                VZT_CRS_SUMMA = 0,
                VZT_KONTR_CRS_RATE = 1,
                VZT_KONTR_CRS_SUMMA = 0,
                VZT_UCH_CRS_RATE = 1,
                VZT_KONTR_DC = k.DocCode,
                State = RowStatus.NewRow,
                VzaimoraschType = vzdefault,
                Parent = Document,
                Kontragent = GlobalOptions.ReferencesCache.GetKontragent(k.DocCode) as Kontragent
            };
            Document.Rows.Add(newdeb);
            DebitorCollection.Add(newdeb);
            CurrentDebitor = newdeb;
            KontragentManager.UpdateSelectCount(k.DocCode);
            RaisePropertyChanged(nameof(CurrencyConvertRate));
            CalcItogoSumma();
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    public void AddNewCreditor(object obj)
    {
        var k = StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions
            { Currency = Document.IsOld ? null : Document.CreditorCurrency });
        if (k == null) return;
        NomenklProductType vzdefault = null;
        var vzdefDC = GlobalOptions.SystemProfile.Profile.FirstOrDefault(_ =>
            _.SECTION == "MUTUAL_ACCOUNTING" && _.ITEM == "DEFAULT_TYPE_PRODUCT");
        if (vzdefDC != null)
            vzdefault =
                GlobalOptions.ReferencesCache.GetNomenklProductType(Convert.ToDecimal(vzdefDC.ITEM_VALUE)) as
                    NomenklProductType;
        var m = DebitorCollection.Any() ? DebitorCollection.Max(_ => _.Code) + 1 : 0;
        var m2 = CreditorCollection.Any() ? CreditorCollection.Max(_ => _.Code) + 1 : 0;
        var _code = Math.Max(m, m2) + 1;
        var newcred = new MutualAccountingCreditorViewModel
        {
            DocCode = Document.DocCode,
            Code = _code,
            VZT_DOC_DATE = Document.VZ_DATE,
            VZT_DOC_NUM = (Document.Rows.Count + 1).ToString(),
            VZT_1MYDOLZH_0NAMDOLZH = 1,
            VZT_CRS_POGASHENO = 0,
            VZT_UCH_CRS_POGASHENO = 0,
            VZT_CRS_SUMMA = 0,
            VZT_KONTR_CRS_RATE = 1,
            VZT_KONTR_CRS_SUMMA = 0,
            VZT_UCH_CRS_RATE = 1,
            VZT_KONTR_DC = k.DocCode,
            VzaimoraschType = vzdefault,
            State = RowStatus.NewRow,
            Parent = Document,
            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(k.DocCode) as Kontragent
        };
        Document.Rows.Add(newcred);
        CreditorCollection.Add(newcred);
        CurrentCreditor = newcred;
        KontragentManager.UpdateSelectCount(k.DocCode);
        RaisePropertyChanged(nameof(CurrencyConvertRate));
        CalcItogoSumma();
    }

    public override string SaveInfo => myMutAccRepository.CheckedInfo;

    public override void DocNewEmpty(object form)
    {
        var frm = new MutualAccountingView { Owner = Application.Current.MainWindow };
        var ctx = new MutualAcountingWindowViewModel
        {
            IsCurrencyConvert = IsCurrencyConvert,
            Form = frm
        };
        ctx.CreateMenu();
        ctx.Document = myMutAccRepository.New();
        if (IsCurrencyConvert)
            frm.typeVzaimozachetComboBoxEdit.SelectedIndex = 0;
        frm.Show();
        frm.DataContext = ctx;
    }

    public override void DocNewCopy(object form)
    {
        var frm = new MutualAccountingView { Owner = Application.Current.MainWindow };
        var ctx = new MutualAcountingWindowViewModel
        {
            IsCurrencyConvert = IsCurrencyConvert,
            Form = frm
        };
        ctx.CreateMenu();
        ctx.Document = myMutAccRepository.NewFullCopy(Document);
        ctx.UpdateVisualData();
        frm.Show();
        frm.DataContext = ctx;
    }

    public override void DocNewCopyRequisite(object form)
    {
        var frm = new MutualAccountingView { Owner = Application.Current.MainWindow };
        var ctx = new MutualAcountingWindowViewModel
        {
            IsCurrencyConvert = IsCurrencyConvert,
            Form = frm
        };
        ctx.CreateMenu();
        ctx.Document = myMutAccRepository.NewRequisity(Document);
        if (IsCurrencyConvert)
            frm.typeVzaimozachetComboBoxEdit.SelectedIndex = 0;
        frm.Show();
        frm.DataContext = ctx;
    }

    public ICommand ChangeDebitorKontragentCommand
    {
        get { return new Command(ChangeDebitorKontragent, _ => CurrentDebitor != null); }
    }

    private void ChangeDebitorKontragent(object obj)
    {
        var k = obj as Kontragent ??
                StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions
                    { Currency = Document.IsOld ? null : Document.DebitorCurrency });
        if (k == null) return;
        CurrentDebitor.Kontragent = GlobalOptions.ReferencesCache.GetKontragent(k.DocCode) as Kontragent;
        CurrentDebitor.VZT_KONTR_CRS_RATE = 1;
        CurrentDebitor.VZT_UCH_CRS_RATE = CurrencyRate.GetCBRate(CurrentDebitor.Currency,
            GlobalOptions.SystemProfile.MainCurrency, Document.VZ_DATE);
        var r = Document.Rows.FirstOrDefault(_ => _.Code == CurrentDebitor.Code);
        if (r != null)
        {
            r.VZT_CRS_DC = ((IDocCode)k.Currency).DocCode;
            r.VZT_KONTR_CRS_DC = ((IDocCode)k.Currency).DocCode;
            r.VZT_KONTR_DC = k.DocCode;
        }

        if (!(Form is MutualAccountingView f)) return;
        if (f.gridViewDebitor.ActiveEditor != null)
        {
            f.gridViewDebitor.ActiveEditor.EditValuePostMode = PostMode.Immediate;
            f.gridViewDebitor.ActiveEditor.EditValue = k;
        }
    }

    public ICommand ChangeCreditorKontragentCommand
    {
        get { return new Command(ChangeCreditorKontragent, _ => CurrentCreditor != null); }
    }

    public ICommand RemoveDebitorSFCommand
    {
        get { return new Command(RemoveDebitorSF, _ => CurrentDebitor != null && CurrentDebitor.SfClient != null); }
    }

    private void RemoveDebitorSF(object obj)
    {
        CurrentDebitor.SfClient = null;
        if (!(Form is MutualAccountingView f)) return;
        if (f.gridViewDebitor.ActiveEditor != null)
        {
            f.gridViewDebitor.ActiveEditor.EditValuePostMode = PostMode.Immediate;
            f.gridViewDebitor.ActiveEditor.EditValue = null;
        }
    }

    private void ChangeCreditorKontragent(object obj)
    {
        var k = obj as Kontragent ??
                StandartDialogs.SelectKontragent(new KontragentSelectDialogOptions
                    { Currency = Document.IsOld ? null : Document.CreditorCurrency });
        if (k == null) return;
        CurrentCreditor.Kontragent = k;
        CurrentCreditor.VZT_KONTR_CRS_RATE = 1;
        CurrentCreditor.VZT_UCH_CRS_RATE = CurrencyRate.GetCBRate(CurrentCreditor.Currency,
            GlobalOptions.SystemProfile.MainCurrency, Document.VZ_DATE);
        var r = Document.Rows.FirstOrDefault(_ => _.Code == CurrentCreditor.Code);
        if (r != null)
        {
            r.VZT_KONTR_DC = k.DocCode;
            r.VZT_CRS_DC = ((IDocCode)k.Currency).DocCode;
            r.VZT_KONTR_CRS_DC = ((IDocCode)k.Currency).DocCode;
        }

        if (!(Form is MutualAccountingView f)) return;
        if (f.gridViewCreditor.ActiveEditor != null)
        {
            f.gridViewCreditor.ActiveEditor.EditValuePostMode = PostMode.Immediate;
            f.gridViewCreditor.ActiveEditor.EditValue = k;
        }
    }

    public override void OnWindowClosing(object obj)
    {
        mySubscriber?.UnsubscribeAll();
        base.OnWindowClosing(obj);
    }

    #endregion
}
