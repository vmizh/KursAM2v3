﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace KursDomain.Documents.AccruedAmount;

public class AccruedAmountTypeViewModel_FluentAPI : DataAnnotationForFluentApiBase,
    IMetadataProvider<AccruedAmountTypeViewModel>
{
    void IMetadataProvider<AccruedAmountTypeViewModel>.BuildMetadata(
        MetadataBuilder<AccruedAmountTypeViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Entity).NotAutoGenerated();
        builder.Property(_ => _.Error).NotAutoGenerated();
        builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование");
        builder.Property(_ => _.IsClient).AutoGenerated().DisplayName("Для клиентов");
        builder.Property(_ => _.IsSupplier).AutoGenerated().DisplayName("Для поставщиков");
        builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
    }
}

[MetadataType(typeof(AccruedAmountTypeViewModel_FluentAPI))]
public class AccruedAmountTypeViewModel : RSViewModelBase, IDataErrorInfo
{
    #region Fields

    private AccruedAmountType myEntity;

    #endregion

    #region Methods

    private AccruedAmountType DefaultValue()
    {
        return new AccruedAmountType
        {
            Id = Guid.NewGuid()
        };
    }

    #endregion

    #region Constructors

    public AccruedAmountTypeViewModel()
    {
        Entity = DefaultValue();
    }

    public AccruedAmountTypeViewModel(AccruedAmountType entity)
    {
        Entity = entity ?? DefaultValue();
    }

    #endregion

    #region Properties

    public AccruedAmountType Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

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

    public override string Name
    {
        get => Entity.Name;
        set
        {
            if (Entity.Name == value) return;
            Entity.Name = value;
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

    public bool IsSupplier
    {
        get => Entity.IsSupplier;
        set
        {
            if (Entity.IsSupplier == value) return;
            Entity.IsSupplier = value;
            RaisePropertyChanged();
        }
    }

    public bool IsClient
    {
        get => Entity.IsClient;
        set
        {
            if (Entity.IsClient == value) return;
            Entity.IsClient = value;
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
                case nameof(Name):
                    return Name == null ? "Наименование не может быть пустым" : null;
                default:
                    return null;
            }
        }
    }

    public string Error => null;

    #endregion
}
