﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using Helper;
using KursDomain.ICommon;
using Newtonsoft.Json;

namespace KursDomain.Documents.AktSpisaniya;

// ReSharper disable once InconsistentNaming
public class AktSpisaniyaNomenklTitleViewModel_FluentAPI : DataAnnotationForFluentApiBase,
    IMetadataProvider<AktSpisaniyaNomenklTitleViewModel>
{
    void IMetadataProvider<AktSpisaniyaNomenklTitleViewModel>.BuildMetadata(
        MetadataBuilder<AktSpisaniyaNomenklTitleViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Entity).NotAutoGenerated();
        builder.Property(_ => _.Warehouse).AutoGenerated().DisplayName("Склад");
        builder.Property(_ => _.DocNumber).AutoGenerated().DisplayName("Номер документа");
        builder.Property(_ => _.DocDate).AutoGenerated().DisplayName("Дата документа");
        builder.Property(_ => _.NomenklCount).AutoGenerated().DisplayName("Кол-во").DisplayFormatString("n4")
            .ReadOnly();
        builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2");
        builder.Property(_ => _.DocCreator).AutoGenerated().DisplayName("Создатель документа");
        builder.Property(_ => _.ReasonCreation).AutoGenerated().DisplayName("Причина списания");
        builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
    }
}

[MetadataType(typeof(AktSpisaniyaNomenklTitleViewModel_FluentAPI))]
public class AktSpisaniyaNomenklTitleViewModel : RSWindowViewModelBase, IEntity<AktSpisaniyaNomenkl_Title>,
    IDataErrorInfo
{
    #region Fields

    // ReSharper disable once InconsistentNaming
    private AktSpisaniyaNomenkl_Title myEntity;

    #endregion

    #region Methods

    public AktSpisaniyaNomenkl_Title DefaultValue()
    {
        return new AktSpisaniyaNomenkl_Title
        {
            Id = Guid.NewGuid()
        };
    }

    public override bool IsCorrect()
    {
        if (Warehouse != null && DocNumber >= 0 && DocCreator != null && Rows.All(_ => _.IsCorrect()))
            return true;
        return false;
    }

    public override object ToJson()
    {
        var res = new
        {
            Статус = CustomFormat.GetEnumName(State),
            Id,
            Номер = DocNumber.ToString(),
            Дата = DocDate.ToShortDateString(),
            Склад = Warehouse.Name,
            Cоздатель = DocCreator,
            Причина = ReasonCreation,
            Примечание = Note,
            Подписан = IsSign ? "Да" : "Нет",
            Позиции = Rows.Select(_ => _.ToJson())
        };
        return JsonConvert.SerializeObject(res);
    }

    #endregion

    #region Constructor

    public AktSpisaniyaNomenklTitleViewModel()
    {
        Entity = DefaultValue();
    }

    public AktSpisaniyaNomenklTitleViewModel(AktSpisaniyaNomenkl_Title entity, RowStatus state = RowStatus.NotEdited)
    {
        Entity = entity ?? DefaultValue();

        foreach (var row in Entity.AktSpisaniya_row)
        {
            Rows.Add(new AktSpisaniyaRowViewModel(row)
            {
                Parent = this,
                myState = state
            });
            myState = state;
        }

        LoadReference();
    }

    private void LoadReference()
    {
    }

    #endregion

    #region Properties

    public List<AktSpisaniyaNomenkl_Title> LoadList()
    {
        throw new NotImplementedException();
    }


    public bool IsAccessRight { get; set; }

    public ObservableCollection<AktSpisaniyaRowViewModel> Rows { set; get; } =
        new ObservableCollection<AktSpisaniyaRowViewModel>();

    public AktSpisaniyaNomenkl_Title Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value)
                return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public decimal NomenklCount => Rows?.Sum(_ => _.Quantity) ?? 0;
    public decimal Summa => Rows?.Sum(_ => _.Summa) ?? 0;

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


    private bool myIsSign;

    public bool IsSign
    {
        get => myIsSign;
        set
        {
            if (myIsSign == value) return;
            myIsSign = value;
            RaisePropertyChanged();
        }
    }

    public References.Warehouse Warehouse
    {
        get => GlobalOptions.ReferencesCache.GetWarehouse(Entity.Warehouse_DC) as References.Warehouse;
        set
        {
            if (GlobalOptions.ReferencesCache.GetWarehouse(Entity.Warehouse_DC) == value)
                return;
            Entity.Warehouse_DC = value.DocCode;
            RaisePropertyChanged();
        }
    }

    public int DocNumber
    {
        get => Entity.Num_Doc;
        set
        {
            if (Entity.Num_Doc == value)
                return;
            Entity.Num_Doc = value;
            RaisePropertyChanged();
        }
    }

    public DateTime DocDate
    {
        get => Entity.Date_Doc;
        set
        {
            if (Entity.Date_Doc == value)
                return;
            Entity.Date_Doc = value;
            RaisePropertyChanged();
        }
    }

    public string DocCreator
    {
        get => Entity.Creator;
        set
        {
            if (Entity.Creator == value)
                return;
            Entity.Creator = value;
            RaisePropertyChanged();
        }
    }

    public string ReasonCreation
    {
        get => Entity.Reason_Creation;
        set
        {
            if (Entity.Reason_Creation == value)
                return;
            Entity.Reason_Creation = value;
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

    #endregion

    #region IDataErrorInfo

    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case "Warehouse":
                    return Warehouse == null ? "Склад должен быть обязательно выбран" : null;
                case "DocNumber":
                    return DocNumber == 0 ? "Необходимо указать номер документа" : null;
                case "DocDate":
                    return DocDate == default ? "Необходимо указать дату документа" : null;
                case "DocCreator":
                    return DocCreator == null ? "Необходимо указать создателя документа" : null;
                default:
                    return null;
            }
        }
    }

    [Display(AutoGenerateField = false)] public string Error => "";

    #endregion
}