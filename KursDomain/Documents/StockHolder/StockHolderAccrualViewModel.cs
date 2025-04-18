﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace KursDomain.Documents.StockHolder;

public class StockHolderAccrualViewModel_FluentAPI : DataAnnotationForFluentApiBase,
    IMetadataProvider<StockHolderAccrualViewModel>
{
    void IMetadataProvider<StockHolderAccrualViewModel>.BuildMetadata(
        MetadataBuilder<StockHolderAccrualViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Entity).NotAutoGenerated();
        builder.Property(_ => _.Error).NotAutoGenerated();
        builder.Property(_ => _.Num).AutoGenerated()
            .DisplayName("№").ReadOnly();
        builder.Property(_ => _.Date).AutoGenerated()
            .DisplayName("Дата");
        builder.Property(_ => _.Note).AutoGenerated()
            .DisplayName("Примечания");
        builder.Property(_ => _.Creator).AutoGenerated()
            .DisplayName("Создатель").ReadOnly();
        builder.Property(_ => _.State).AutoGenerated()
            .DisplayName("Статус").ReadOnly();
        builder.Property(_ => _.SummaRUB).AutoGenerated().DisplayName("RUB").DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.SummaUSD).AutoGenerated().DisplayName("USD").DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.SummaEUR).AutoGenerated().DisplayName("EUR").DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.SummaCHF).AutoGenerated().DisplayName("CHF").DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.SummaGBP).AutoGenerated().DisplayName("GBP").DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.SummaCNY).AutoGenerated().DisplayName("CNY").DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.SummaSEK).AutoGenerated().DisplayName("SEK").DisplayFormatString("n2").ReadOnly();
    }
}

[MetadataType(typeof(StockHolderAccrualViewModel_FluentAPI))]
public class StockHolderAccrualViewModel : RSViewModelBase, IDataErrorInfo, IEntity<StockHolderAccrual>,
    ISummaMultiCurrency
{
    #region Constructors

    public StockHolderAccrualViewModel(StockHolderAccrual entity)
    {
        Entity = entity ?? DefaultValue();
        LoadReference();
    }

    #endregion

    #region Fields

    #endregion

    #region Properties

    public StockHolderAccrual Entity { get; set; }

    public ObservableCollection<StockHolderAccrualRowViewModel> Rows { set; get; } =
        new ObservableCollection<StockHolderAccrualRowViewModel>();

    public override Guid Id
    {
        get => Entity.Id;
        set
        {
            if (Entity.Id == value) return;
            Entity.Id = value;
            RaisePropertyChanged();
        }
    }

    public override string Note
    {
        get => Entity.Note;
        set
        {
            if (Entity.Note == value) return;
            Entity.Note = value;
            RaisePropertyChanged();
        }
    }

    public int Num
    {
        get => Entity.Num;
        set
        {
            if (Entity.Num == value) return;
            Entity.Num = value;
            RaisePropertyChanged();
        }
    }


    public DateTime Date
    {
        get => Entity.Date;
        set
        {
            if (Entity.Date == value) return;
            Entity.Date = value;
            RaisePropertyChanged();
        }
    }

    public string Creator
    {
        get => Entity.Creator;
        set
        {
            if (Entity.Creator == value) return;
            Entity.Creator = value;
            RaisePropertyChanged();
        }
    }

    public decimal SummaCHF
    {
        get => Rows.Where(_ => _.Currency.DocCode == CurrencyCode.CHF).Sum(_ => _.Summa);
        set { }
    }

    public decimal SummaEUR
    {
        get => Rows.Where(_ => _.Currency.DocCode == CurrencyCode.EUR).Sum(_ => _.Summa);
        set { }
    }

    public decimal SummaGBP
    {
        get => Rows.Where(_ => _.Currency.DocCode == CurrencyCode.GBP).Sum(_ => _.Summa);
        set { }
    }

    public decimal SummaRUB
    {
        get => Rows.Where(_ => _.Currency.DocCode == CurrencyCode.RUB).Sum(_ => _.Summa);
        set { }
    }

    public decimal SummaSEK
    {
        get => Rows.Where(_ => _.Currency.DocCode == CurrencyCode.SEK).Sum(_ => _.Summa);
        set { }
    }

    public decimal SummaUSD
    {
        get => Rows.Where(_ => _.Currency.DocCode == CurrencyCode.USD).Sum(_ => _.Summa);
        set { }
    }

    public decimal SummaCNY
    {
        get => Rows.Where(_ => _.Currency.DocCode == CurrencyCode.CNY).Sum(_ => _.Summa);
        set { }
    }

    #endregion

    #region Methods

    private void LoadReference()
    {
        if (Entity.StockHolderAccrualRows != null && Entity.StockHolderAccrualRows.Count > 0)
            foreach (var r in Entity.StockHolderAccrualRows)
                Rows.Add(new StockHolderAccrualRowViewModel(r));
    }

    public StockHolderAccrual DefaultValue()
    {
        return new StockHolderAccrual
        {
            Id = Guid.NewGuid(),
            Creator = GlobalOptions.UserInfo.NickName,
            Date = DateTime.Today,
            Note = ""
        };
    }

    #endregion


    #region IDataErrorInfo

    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case nameof(Note):
                    return Name == null ? "Примечание не может быть пустым" : null;
                default:
                    return null;
            }
        }
    }

    public string Error => null;

    #endregion
}
