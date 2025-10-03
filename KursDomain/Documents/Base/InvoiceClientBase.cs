using Core.ViewModel.Base;
using Data;
using KursDomain.IDocuments.Finance;
using KursDomain.IReferences;
using KursDomain.References;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace KursDomain.Documents.Base;

[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
public sealed class InvoiceClientBase : RSViewModelBase, IInvoiceClient
{
    private Kontragent myClient;
    private PayCondition myPayCondition;

    public InvoiceClientBase(SD_84 doc)
    {
        if (doc == null)
            throw new ArgumentNullException(nameof(doc));

        DocCode = doc.DOC_CODE;
        Id = doc.Id;
        DocDate = doc.SF_DATE;
        InnerNumber = doc.SF_IN_NUM;
        OuterNumber = doc.SF_OUT_NUM;

        Currency = GlobalOptions.ReferencesCache.GetCurrency(doc.SF_CRS_DC) as References.Currency;
        Client = GlobalOptions.ReferencesCache.GetKontragent(doc.SF_CLIENT_DC) as Kontragent;
        CO = GlobalOptions.ReferencesCache.GetCentrResponsibility(doc.SF_CENTR_OTV_DC) as CentrResponsibility;
        Receiver = GlobalOptions.ReferencesCache.GetKontragent(doc.SF_RECEIVER_KONTR_DC) as Kontragent;
        FormRaschet = GlobalOptions.ReferencesCache.GetPayForm(doc.SF_FORM_RASCH_DC) as PayForm;
        PayCondition = GlobalOptions.ReferencesCache.GetPayCondition(doc.SD_179?.DOC_CODE) as PayCondition;
        VzaimoraschetType =
            GlobalOptions.ReferencesCache.GetNomenklProductType(doc.SD_77?.DOC_CODE) as NomenklProductType;
        PersonaResponsible =
            GlobalOptions.ReferencesCache.GetEmployee(doc.PersonalResponsibleDC) as References.Employee;
        
        foreach (var t84 in doc.TD_84)
        {
            var q = t84.TD_24.Sum(t24 => t24.DDT_KOL_RASHOD);
            if (q > 0)
                SummaOtgruz += (t84.SFT_SUMMA_K_OPLATE_KONTR_CRS ?? 0) * (decimal)t84.SFT_KOL / q;
        }


        //DilerSumma = Math.Round(doc.DilerSumma, 2);
        Note = doc.SF_NOTE;
        //Diler = GlobalOptions.ReferencesCache.GetKontragent(doc.DilerDC) as Kontragent;
        IsAccepted = doc.SF_ACCEPTED == 1;
        Summa = Math.Round(doc.TD_84.Sum(_ => _.SFT_SUMMA_K_OPLATE_KONTR_CRS) ?? 0, 2);
        CREATOR = doc.CREATOR;
        IsNDSIncludeInPrice = doc.SF_NDS_1INCLUD_0NO == 1;
        decimal pSum = 0;
        using (var ctx = GlobalOptions.GetEntities())
        {
            pSum = ctx.SD_33.Where(_ => _.SFACT_DC == DocCode).Sum(_ => _.CRS_SUMMA) ?? 0;
            pSum += ctx.TD_101.Include(_ => _.SD_101)
                .Include(_ => _.SD_101.SD_114)
                .Where(_ => _.VVT_SFACT_CLIENT_DC == DocCode).Sum(_ => _.VVT_VAL_PRIHOD) ?? 0;
        }
        PaySumma = Math.Round(pSum, 2);

        Rows = new ObservableCollection<IInvoiceClientRow>();
        foreach (var r in doc.TD_84) Rows.Add(new InvoiceClientRowBase(r));
    }

    public InvoiceClientBase(IEnumerable<InvoiceClientQuery> invList, bool isLoadDetails = false)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            if (invList == null || !invList.Any())
                throw new ArgumentNullException();
            var doc = invList.First();
            DocCode = doc.DocCode;
            Id = doc.Id;
            DocDate = doc.DocDate;
            Receiver = GlobalOptions.ReferencesCache.GetKontragent(doc.ReceiverDC) as Kontragent;
            CO = GlobalOptions.ReferencesCache.GetCentrResponsibility(doc.CentOtvetstDC) as CentrResponsibility;
            VzaimoraschetType =
                GlobalOptions.ReferencesCache.GetNomenklProductType(doc.VzaimoraschetTypeDC) as NomenklProductType;
            PayCondition = GlobalOptions.ReferencesCache.GetPayCondition(doc.PayConditionDC) as PayCondition;
            InnerNumber = doc.InnerNumber;
            OuterNumber = doc.OuterNumber;
            Client = GlobalOptions.ReferencesCache.GetKontragent(doc.ClientDC) as Kontragent;
            Currency = GlobalOptions.ReferencesCache.GetCurrency(doc.CurrencyDC) as References.Currency;
            SummaOtgruz = Math.Round(invList.Sum(_ => _.SummaOtgruz), 2);
            DilerSumma = Math.Round(doc.DilerSumma, 2);
            Note = doc.Note;
            Diler = GlobalOptions.ReferencesCache.GetKontragent(doc.DilerDC) as Kontragent;
            IsAccepted = doc.IsAccepted ?? false;
            Summa = Math.Round(doc.Summa ?? 0, 2);
            CREATOR = doc.CREATOR;
            FormRaschet = GlobalOptions.ReferencesCache.GetPayForm(doc.FormRaschetDC) as PayForm;
            IsNDSIncludeInPrice = doc.IsNDSIncludeInPrice ?? false;
            PaySumma = Math.Round(doc.PaySumma ?? 0, 2);
            VzaimoraschetType =
                GlobalOptions.ReferencesCache.GetNomenklProductType(doc.VzaimoraschetTypeDC) as NomenklProductType;
            var r = ctx.SD_84.FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
            if (r != null)
            {
                PersonaResponsible =
                    GlobalOptions.ReferencesCache.GetEmployee(r.PersonalResponsibleDC) as References.Employee;
            }
            else
            {
                var k = ctx.SD_43.FirstOrDefault(_ => _.DOC_CODE == doc.ClientDC);
                if (k.OTVETSTV_LICO != null)
                    PersonaResponsible =
                        GlobalOptions.ReferencesCache.GetEmployee(k.OTVETSTV_LICO.Value) as References.Employee;
            }
        }

        if (!isLoadDetails) return;
        Rows = new ObservableCollection<IInvoiceClientRow>();
        foreach (var r in invList) Rows.Add(new InvoiceClientRowBase(r));
    }

    public PayCondition PayCondition {get => myPayCondition;
        set
        {
            if (Equals(value, myPayCondition) && value?.Name == myPayCondition?.Name) return;
            myPayCondition = value;
            RaisePropertyChanged();
        } }

    public Kontragent Receiver { get; set; }

    public Kontragent Client
    {
        get => myClient;
        set
        {
            if (Equals(value, myClient)) return;
            myClient = value;
            RaisePropertyChanged();
        }
    }

    public Kontragent Diler { get; set; }

    public override decimal DocCode { get; set; }
    public override Guid Id { get; set; }
    public CentrResponsibility CO { get; set; }
    public NomenklProductType VzaimoraschetType { get; set; }
    public PayForm FormRaschet { get; set; }
    public DateTime DocDate { get; set; }
    public int InnerNumber { get; set; }
    public string OuterNumber { get; set; }
    public References.Currency Currency { get; set; }
    public decimal SummaOtgruz { get; set; }
    public decimal DilerSumma { get; set; }
    public override string Note { get; set; }
    public bool IsAccepted { get; set; }
    public decimal Summa { get; set; }
    public string CREATOR { get; set; }
    public bool IsNDSIncludeInPrice { get; set; }
    public decimal PaySumma { get; set; }
    public References.Employee PersonaResponsible { get; set; }
    public bool? IsExcludeFromPays { get; set; } = false;
    public string LastChanger { get; set; }
    public DateTime LastChangerDate { get; set; }
    public decimal? VazaimoraschetTypeDC { get; set; }
    public bool IsLinkProject { get; set; }
    public string LinkPrjectNames { get; set; }
    public ObservableCollection<IInvoiceClientRow> Rows { get; set; }
}

public class InvoiceClientRowBase : IInvoiceClientRow
{
    public InvoiceClientRowBase(InvoiceClientQuery row)
    {
        DocCode = row.DocCode;
        Code = row.RowCode ?? 0;
        Id = row.Row2d ?? Guid.Empty;
        DocId = row.DocId ?? Guid.Empty;
        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(row.NomenklDC) as Nomenkl;
        NomNomenkl = Nomenkl?.NomenklNumber;
        IsUsluga = Nomenkl?.IsUsluga ?? false;
        Quantity = row.Quantity ?? 0;
        Price = row.Price ?? 0;
        Summa = Quantity * Price;
        SFT_NACENKA_DILERA = row.SFT_NACENKA_DILERA;
        Shipped = row.Shipped;
        Rest = Quantity - Shipped;
        CurrentRemains = 0;
        NDSPercent = row.NDSPercent ?? 0;
        SFT_SUMMA_NDS = row.SFT_SUMMA_NDS;
    }

    public InvoiceClientRowBase()
    {
    }

    public InvoiceClientRowBase(TD_84 entity)
    {
        DocCode = entity.DOC_CODE;
        Code = entity.CODE;
        Id = entity.Id;
        DocId = entity.DocId;
        Nomenkl = GlobalOptions.ReferencesCache.GetNomenkl(entity.SFT_NEMENKL_DC) as Nomenkl;
        NomNomenkl = Nomenkl?.NomenklNumber;
        IsUsluga = Nomenkl?.IsUsluga ?? false;
        Quantity = (decimal)entity.SFT_KOL;
        Price = entity.SFT_ED_CENA ?? 0;
        Summa = Quantity * Price;
        SFT_NACENKA_DILERA = entity.SFT_NACENKA_DILERA;
        Shipped = entity.TD_24?.Sum(_ => _.DDT_KOL_RASHOD) ?? 0;
        Rest = Quantity - Shipped;
        CurrentRemains = 0;
        //NDSPercent = row.NDSPercent ?? 0;
        //SFT_SUMMA_NDS = row.SFT_SUMMA_NDS;
    }

    public InvoiceClientRowBase(decimal docCode, int code, Guid id, Guid docId, Nomenkl nomenkl, string nomNomenkl,
        bool isUsluga,
        decimal quantity, decimal price, decimal summa, decimal? sFT_NACENKA_DILERA, decimal shipped,
        decimal rest, decimal currentRemains, string note, decimal nDSPercent, decimal? sFT_SUMMA_NDS,
        SDRSchet sDRSchet, string gruzoDeclaration)
    {
        DocCode = docCode;
        Code = code;
        Id = id;
        DocId = docId;
        Nomenkl = nomenkl;
        NomNomenkl = nomNomenkl;
        IsUsluga = isUsluga;
        Quantity = quantity;
        Price = price;
        Summa = summa;
        SFT_NACENKA_DILERA = sFT_NACENKA_DILERA;
        Shipped = shipped;
        Rest = rest;
        CurrentRemains = currentRemains;
        Note = note;
        NDSPercent = nDSPercent;
        SFT_SUMMA_NDS = sFT_SUMMA_NDS;
        SDRSchet = sDRSchet;
        GruzoDeclaration = gruzoDeclaration;
    }

    public Nomenkl Nomenkl { get; set; }

    public decimal DocCode { get; set; }
    public int Code { get; set; }
    public Guid Id { get; set; }
    public Guid DocId { get; set; }
    public string NomNomenkl { get; set; }
    public Unit Unit { get; set; }
    public bool IsUsluga { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal PriceWithNDS { get; }
    public decimal Summa { get; set; }
    public decimal? SFT_NACENKA_DILERA { get; set; }
    public decimal Shipped { get; set; }
    public decimal Rest { get; set; }
    public decimal CurrentRemains { get; set; }
    public string Note { get; set; }
    public decimal NDSPercent { get; set; }
    public decimal? SFT_SUMMA_NDS { get; set; }
    public SDRSchet SDRSchet { get; set; }
    public string GruzoDeclaration { get; set; }
}

public class InvoiceClientRemains : IInvoiceClientRemains
{
    public InvoiceClientRemains(IInvoiceClient inv)
    {
        DocCode = inv.DocCode;
        Id = inv.Id;
        Receiver = inv.Receiver;
        CO = inv.CO;
        VzaimoraschetType = inv.VzaimoraschetType;
        FormRaschet = inv.FormRaschet;
        PayCondition = inv.PayCondition;
        DocDate = inv.DocDate;
        InnerNumber = inv.InnerNumber;
        OuterNumber = inv.OuterNumber;
        Client = inv.Client;
        Currency = inv.Currency;
        SummaOtgruz = inv.SummaOtgruz;
        DilerSumma = inv.DilerSumma;
        Note = inv.Note;
        Diler = inv.Diler;
        IsAccepted = inv.IsAccepted;
        Summa = inv.Summa;
        CREATOR = inv.CREATOR;
        IsNDSIncludeInPrice = inv.IsNDSIncludeInPrice;
        PaySumma = inv.PaySumma;
        PersonaResponsible = inv.PersonaResponsible;
    }

    [Display(AutoGenerateField = false)] public decimal DocCode { get; set; }

    [Display(AutoGenerateField = false)] public Guid Id { get; set; }

    [Display(AutoGenerateField = true, Name = "Поставщик", Order = 9)]
    public Kontragent Receiver { get; set; }

    [Display(AutoGenerateField = true, Name = "Центр ответственности", Order = 12)]
    public CentrResponsibility CO { get; set; }

    [Display(AutoGenerateField = true, Name = "Тип продукции", Order = 13)]
    public NomenklProductType VzaimoraschetType { get; set; }

    [Display(AutoGenerateField = true, Name = "Форма расчетов", Order = 15)]
    public PayForm FormRaschet { get; set; }

    [Display(AutoGenerateField = true, Name = "Статус", Order = 14)]
    public PayCondition PayCondition { get; set; }

    [Display(AutoGenerateField = true, Name = "Дата", Order = 1)]
    public DateTime DocDate { get; set; }

    [Display(AutoGenerateField = true, Name = "№", Order = 2)]
    public int InnerNumber { get; set; }

    [Display(AutoGenerateField = true, Name = "Внешний №", Order = 3)]
    public string OuterNumber { get; set; }

    [Display(AutoGenerateField = true, Name = "Клиент", Order = 4)]
    public Kontragent Client { get; set; }

    [Display(AutoGenerateField = true, Name = "Валюта", Order = 6)]
    public References.Currency Currency { get; set; }

    [Display(AutoGenerateField = true, Name = "Отгружено", Order = 8)]
    public decimal SummaOtgruz { get; set; }

    [Display(AutoGenerateField = true, Name = "Сумма дилера", Order = 17)]
    public decimal DilerSumma { get; set; }

    [Display(AutoGenerateField = true, Name = "Примечание", Order = 10)]
    public string Note { get; set; }

    [Display(AutoGenerateField = true, Name = "Дилер", Order = 16)]
    public Kontragent Diler { get; set; }

    [Display(AutoGenerateField = true, Name = "Акцептован", Order = 18)]
    public bool IsAccepted { get; set; }

    [Display(AutoGenerateField = true, Name = "Сумма", Order = 5)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Summa { get; set; }

    [Display(AutoGenerateField = true, Name = "Создатель", Order = 11)]
    public string CREATOR { get; set; }

    [Display(AutoGenerateField = true, Name = "НДС в цене", Order = 19)]
    public bool IsNDSIncludeInPrice { get; set; }

    [Display(AutoGenerateField = true, Name = "Оплачено", Order = 7)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal PaySumma { get; set; }

    [Display(AutoGenerateField = true, Name = "Ответственный", Order = 20)]
    public References.Employee PersonaResponsible { get; set; }

    [Display(AutoGenerateField = true, Name = "Искл. из поиска", Order = 20,
        Description = "Исключить из диалогов поиска для оплаты")]
    public bool? IsExcludeFromPays { get; set; }

    [Display(AutoGenerateField = true, Name = "Посл.изменил", Order = 22)]
    public string LastChanger { get; set; }

    [Display(AutoGenerateField = true, Name = "Дата посл.изм.", Order = 23)]
    public DateTime LastChangerDate { get; set; }

    [Display(AutoGenerateField = false)] public decimal? VazaimoraschetTypeDC { get; set; }

    public bool IsLinkProject { get; set; }
    public string LinkPrjectNames { get; set; }
    public ObservableCollection<IInvoiceClientRow> Rows { get; set; }

    [Display(AutoGenerateField = true, Name = "Кол-во", Order = 21)]
    public decimal NomQuantity { get; set; }
}
