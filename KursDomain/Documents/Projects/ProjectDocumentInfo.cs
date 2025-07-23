using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using KursDomain.Documents.CommonReferences;
using KursDomain.References;

namespace KursDomain.Documents.Projects;

public class ProjectDocumentInfoBase : RSViewModelBase, IEntity<ProjectDocuments>
{
    private string _Note;

    public ProjectDocumentInfoBase()
    {
        Entity = DefaultValue();
    }

    public ProjectDocumentInfoBase(ProjectDocuments entity)
    {
        Entity = entity ?? DefaultValue();
    }

    [Display(AutoGenerateField = false, GroupName = "Основные данные")]
    public override Guid Id
    {
        set
        {
            if (Equals(Entity.Id, value)) return;
            Entity.Id = value;
            RaisePropertyChanged();
        }
        get => Entity.Id;
    }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Создатель")]
    public string Creator { set; get; }

    [Display(AutoGenerateField = false)]
    public Guid ProjectId
    {
        set
        {
            if (Equals(Entity.ProjectId, value)) return;
            Entity.ProjectId = value;
            RaisePropertyChanged();
        }
        get => Entity.ProjectId;
    }

    [Display(AutoGenerateField = false)]
    public int? BankCode
    {
        set
        {
            if (Equals(Entity.BankCode, value)) return;
            Entity.BankCode = value;
            RaisePropertyChanged();
        }
        get => Entity.BankCode;
    }

    [Display(AutoGenerateField = false)]
    public decimal? CashInDC
    {
        set
        {
            if (Equals(Entity.CashInDC, value)) return;
            Entity.CashInDC = value;
            RaisePropertyChanged();
        }
        get => Entity.CashInDC;
    }

    [Display(AutoGenerateField = false)]
    public decimal? CashOutDC
    {
        set
        {
            if (Equals(Entity.CashOutDC, value)) return;
            Entity.CashOutDC = value;
            RaisePropertyChanged();
        }
        get => Entity.CashOutDC;
    }

    [Display(AutoGenerateField = false)]
    public decimal? WarehouseOrderInDC
    {
        set
        {
            if (Equals(Entity.WarehouseOrderInDC, value)) return;
            Entity.WarehouseOrderInDC = value;
            RaisePropertyChanged();
        }
        get => Entity.WarehouseOrderInDC;
    }

    [Display(AutoGenerateField = false)]
    public Guid? CurrencyConvertId
    {
        set
        {
            if (Equals(Entity.CurrencyConvertId, value)) return;
            Entity.CurrencyConvertId = value;
            RaisePropertyChanged();
        }
        get => Entity.CurrencyConvertId;
    }

    [Display(AutoGenerateField = false)]
    public decimal? WaybillDC
    {
        set
        {
            if (Equals(Entity.WaybillDC, value)) return;
            Entity.WaybillDC = value;
            RaisePropertyChanged();
        }
        get => Entity.WaybillDC;
    }

    [Display(AutoGenerateField = false)]
    public Guid? UslugaProviderRowId
    {
        set
        {
            if (Equals(Entity.UslugaProviderRowId, value)) return;
            Entity.UslugaProviderRowId = value;
            RaisePropertyChanged();
        }
        get => Entity.UslugaProviderRowId;
    }

    [Display(AutoGenerateField = false)]
    public Guid? UslugaClientRowId
    {
        set
        {
            if (Equals(Entity.UslugaClientRowId, value)) return;
            Entity.UslugaClientRowId = value;
            RaisePropertyChanged();
        }
        get => Entity.UslugaClientRowId;
    }


    [Display(AutoGenerateField = false)]
    public Guid? AccruedClientRowId
    {
        set
        {
            if (Equals(Entity.AccruedClientRowId, value)) return;
            Entity.AccruedClientRowId = value;
            RaisePropertyChanged();
        }
        get => Entity.AccruedClientRowId;
    }

    [Display(AutoGenerateField = false)]
    public Guid? AccruedSupplierRowId
    {
        set
        {
            if (Equals(Entity.AccruedSupplierRowId, value)) return;
            Entity.AccruedSupplierRowId = value;
            RaisePropertyChanged();
        }
        get => Entity.AccruedSupplierRowId;
    }


    [Display(AutoGenerateField = false)]
    public Guid? InvoiceClientId
    {
        set
        {
            if (Equals(Entity.InvoiceClientId, value)) return;
            Entity.InvoiceClientId = value;
            RaisePropertyChanged();
        }
        get => Entity.InvoiceClientId;
    }

    [Display(AutoGenerateField = false)]
    public Guid? InvoiceProviderId
    {
        set
        {
            if (Equals(Entity.InvoiceProviderId, value)) return;
            Entity.InvoiceProviderId = value;
            RaisePropertyChanged();
        }
        get => Entity.InvoiceProviderId;
    }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Тип док-та", Order = 1)]
    public string DocType => DocumentType.GetDisplayAttributesFrom(DocumentType.GetType()).Name;

    [Display(AutoGenerateField = false, GroupName = "Основные данные", Name = "Тип док-та", Order = 1)]
    public DocumentType DocumentType
    {
        set
        {
            if (Equals(Entity.DocType, (int)value)) return;
            Entity.DocType = (int)value;
            RaisePropertyChanged();
        }
        get => (DocumentType)Entity.DocType;
    }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Внут.№", Order = 2)]
    public int? InnerNumber { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Внеш.№", Order = 3)]
    public string ExtNumber { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Дата", Order = 4)]
    public DateTime DocDate { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Контрагент", Order = 5)]
    public Kontragent Kontragent { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Дилер", Order = 12)]
    public Kontragent Diler { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Склад", Order = 13)]
    public References.Warehouse Warehouse { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Банк счет", Order = 14)]
    public string BankAccount { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Касса", Order = 15)]
    public CashBox CashBox { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Сотрудника", Order = 16)]
    public References.Employee Employee { set; get; }

    [Display(AutoGenerateField = false)] public Nomenkl Nomenkl { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Номенклатура", Order = 23)]
    public string NomenklName => Nomenkl is null ? null : $"{Nomenkl.NomenklNumber,-20} {Nomenkl.Name}";

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Вид продукции", Order = 22)]
    public string ProductTypeName { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Валюта", Order = 11)]
    public References.Currency Currency { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Сумма прихода", Order = 6)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaIn { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Сумма расхода", Order = 7)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaOut { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Оплачено", Order = 8)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaPay { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Отгружено", Order = 9)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaShipped { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Сумма дилера", Order = 10)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaDiler { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Описание", Order = 17)]
    public string DocInfo
    {
        set
        {
            if (Equals(Entity.DocInfo, value)) return;
            Entity.DocInfo = value;
            RaisePropertyChanged();
        }
        get => Entity.DocInfo;
    }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Примечание (проект)", Order = 18)]
    public string ProjectNote
    {
        set
        {
            if (Equals(Entity.Note, value)) return;
            Entity.Note = value;
            RaisePropertyChanged();
        }
        get => Entity.Note;
    }

    [Display(AutoGenerateField = true, GroupName = "Основные данные", Name = "Примечание (док-т)", Order = 19)]
    public new string Note
    {
        set
        {
            if (Equals(_Note, value)) return;
            _Note = value;
            RaisePropertyChanged();
        }
        get => _Note;
    }


    [Display(AutoGenerateField = true, GroupName = "Приход", Name = "Кол-во(док-т)", Order = 20)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal QuantityInDocument { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Приход", Name = "Кол-во(отгружено)", Order = 20)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal QuantityInShipped { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Приход", Name = "Остаток(кол-во)", Order = 20)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal QuantityInRemain { set; get; }


    [Display(AutoGenerateField = true, GroupName = "Приход", Name = "Остаток(сумма)", Order = 20)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaInRemain { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Расход", Name = "Кол-во(док-т)", Order = 20)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal QuantityOutDocument { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Расход", Name = "Кол-во(отгружено)", Order = 20)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal QuantityOutShipped { set; get; }

    [Display(AutoGenerateField = true, GroupName = "Расход", Name = "Остаток(кол-во)", Order = 20)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal QuantityOutRemain { set; get; }


    [Display(AutoGenerateField = true, GroupName = "Расход", Name = "Остаток(сумма)", Order = 20)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaOutRemain { set; get; }


    [Display(AutoGenerateField = false)] public ProjectDocuments Entity { get; set; }

    [Display(AutoGenerateField = true, Name = "Искл.строки")] public bool HasExcludeRow { get; set; }

    public ProjectDocuments DefaultValue()
    {
        return new ProjectDocuments
        {
            Id = Guid.NewGuid()
        };
    }
}

[DebuggerDisplay("{DocumentType} {InnerNumber}/{ExtNumber} {DocDate} ")]
public class ProjectDocumentInfo : ProjectDocumentInfoBase, IMultyWithDilerCurrency
{
    #region Constructors

    public ProjectDocumentInfo()
    {
    }


    public ProjectDocumentInfo(ProjectDocuments entity) : base(entity)
    {
    }

    #endregion

    #region Methods

    private void SetCurrencyToZero()
    {
        ProfitRUB = 0;
        ProfitCHF = 0;
        ProfitCNY = 0;
        ProfitEUR = 0;
        ProfitGBP = 0;
        ProfitSEK = 0;
        ProfitUSD = 0;

        LossRUB = 0;
        LossCHF = 0;
        LossCNY = 0;
        LossEUR = 0;
        LossGBP = 0;
        LossSEK = 0;
        LossUSD = 0;
    }

    public void SetCurrency()
    {
        SetCurrencyToZero();
        switch (Currency.DocCode)
        {
            case CurrencyCode.RUB:
                ProfitRUB = SummaIn;
                DilerRUB = SummaDiler;
                LossRUB = SummaOut;
                break;
            case CurrencyCode.EUR:
                ProfitEUR = SummaIn;
                DilerEUR = SummaDiler;
                LossEUR = SummaOut;
                break;
            case CurrencyCode.USD:
                ProfitUSD = SummaIn;
                DilerUSD = SummaDiler;
                LossUSD = SummaOut;
                break;
            case CurrencyCode.CHF:
                ProfitCHF = SummaIn;
                DilerCHF = SummaDiler;
                LossCHF = SummaOut;
                break;
            case CurrencyCode.CNY:
                ProfitCNY = SummaIn;
                DilerCNY = SummaDiler;
                LossCNY = SummaOut;
                break;
            case CurrencyCode.GBP:
                ProfitGBP = SummaIn;
                DilerGBP = SummaDiler;
                LossGBP = SummaOut;
                break;
            case CurrencyCode.SEK:
                ProfitSEK = SummaIn;
                DilerSEK = SummaDiler;
                LossSEK = SummaOut;
                break;
        }
    }

    #endregion

    #region Currencies

    [Display(AutoGenerateField = true, GroupName = "CHF", Name = "Приход")]
    public decimal ProfitCHF { get; set; }

    [Display(AutoGenerateField = true, GroupName = "EUR", Name = "Приход")]
    public decimal ProfitEUR { get; set; }

    [Display(AutoGenerateField = true, GroupName = "GBP", Name = "Приход")]
    public decimal ProfitGBP { get; set; }

    [Display(AutoGenerateField = true, GroupName = "RUR", Name = "Приход")]
    public decimal ProfitRUB { get; set; }

    [Display(AutoGenerateField = true, GroupName = "SEK", Name = "Приход")]
    public decimal ProfitSEK { get; set; }

    [Display(AutoGenerateField = true, GroupName = "USD", Name = "Приход")]
    public decimal ProfitUSD { get; set; }

    [Display(AutoGenerateField = true, GroupName = "CNY", Name = "Приход")]
    public decimal ProfitCNY { get; set; }

    [Display(AutoGenerateField = true, GroupName = "CHF", Name = "Расход")]
    public decimal LossCHF { get; set; }

    [Display(AutoGenerateField = true, GroupName = "EUR", Name = "Расход")]
    public decimal LossEUR { get; set; }

    [Display(AutoGenerateField = true, GroupName = "GBP", Name = "Расход")]
    public decimal LossGBP { get; set; }

    [Display(AutoGenerateField = true, GroupName = "RUR", Name = "Расход")]
    public decimal LossRUB { get; set; }

    [Display(AutoGenerateField = true, GroupName = "SEK", Name = "Расход")]
    public decimal LossSEK { get; set; }

    [Display(AutoGenerateField = true, GroupName = "USD", Name = "Расход")]
    public decimal LossUSD { get; set; }

    [Display(AutoGenerateField = true, GroupName = "CNY", Name = "Расход")]
    public decimal LossCNY { get; set; }

    [Display(AutoGenerateField = true, GroupName = "CHF", Name = "Дилер")]
    public decimal DilerCHF { get; set; }

    [Display(AutoGenerateField = true, GroupName = "EUR", Name = "Дилер")]
    public decimal DilerEUR { get; set; }

    [Display(AutoGenerateField = true, GroupName = "GBP", Name = "Дилер")]
    public decimal DilerGBP { get; set; }

    [Display(AutoGenerateField = true, GroupName = "RUR", Name = "Дилер")]
    public decimal DilerRUB { get; set; }

    [Display(AutoGenerateField = true, GroupName = "SEK", Name = "Дилер")]
    public decimal DilerSEK { get; set; }

    [Display(AutoGenerateField = true, GroupName = "USD", Name = "Дилер")]
    public decimal DilerUSD { get; set; }

    [Display(AutoGenerateField = true, GroupName = "CNY", Name = "Дилер")]
    public decimal DilerCNY { get; set; }

    [Display(AutoGenerateField = true, GroupName = "CHF", Name = "Результат")]
    public decimal ResultCHF => ProfitCHF - DilerCHF - LossCHF;

    [Display(AutoGenerateField = true, GroupName = "EUR", Name = "Результат")]
    public decimal ResultEUR => ProfitEUR - DilerEUR - LossEUR;

    [Display(AutoGenerateField = true, GroupName = "GBP", Name = "Результат")]
    public decimal ResultGBP => ProfitGBP - DilerGBP - LossGBP;

    [Display(AutoGenerateField = true, GroupName = "RUR", Name = "Результат")]
    public decimal ResultRUB => ProfitRUB - DilerRUB - LossRUB;

    [Display(AutoGenerateField = true, GroupName = "SEK", Name = "Результат")]
    public decimal ResultSEK => ProfitSEK - DilerSEK - LossSEK;

    [Display(AutoGenerateField = true, GroupName = "USD", Name = "Результат")]
    public decimal ResultUSD => ProfitUSD - DilerUSD - LossUSD;

    [Display(AutoGenerateField = true, GroupName = "CNY", Name = "Результат")]
    public decimal ResultCNY => ProfitCNY - DilerCNY - LossCNY;

    #endregion
}
