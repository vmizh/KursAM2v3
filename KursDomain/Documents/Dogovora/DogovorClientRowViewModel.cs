﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.Documents.NomenklManagement;
using KursDomain.References;
using static System.Math;

namespace KursDomain.Documents.Dogovora;

public class DogovorClientRowViewModel_FluentAPI : DataAnnotationForFluentApiBase,
    IMetadataProvider<DogovorClientRowViewModel>
{
    void IMetadataProvider<DogovorClientRowViewModel>.BuildMetadata(
        MetadataBuilder<DogovorClientRowViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Entity).NotAutoGenerated();
        builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2");
        builder.Property(_ => _.IsCalcBack).AutoGenerated().DisplayName("Обр.Расчет");
        builder.Property(_ => _.NDSSumma).AutoGenerated().DisplayName("Сумма НДС").DisplayFormatString("n2")
            .ReadOnly();
        builder.Property(_ => _.NDSPercent).AutoGenerated().DisplayName("НДС %").DisplayFormatString("n2");
        builder.Property(_ => _.Quantity).AutoGenerated().DisplayName("Кол-во").DisplayFormatString("n4");
        builder.Property(_ => _.Nomenkl).AutoGenerated().DisplayName("Номенклатура");
        builder.Property(_ => _.NomenklNumber).AutoGenerated().DisplayName("Ном.№").ReadOnly();
        builder.Property(_ => _.Unit).AutoGenerated().DisplayName("Ев.изм.").ReadOnly();
        builder.Property(_ => _.Price).AutoGenerated().DisplayName("Цена").DisplayFormatString("n2");
        builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
        builder.Property(_ => _.IsUsluga).AutoGenerated().DisplayName("Услуга").ReadOnly();
    }
}

[MetadataType(typeof(DogovorClientRowViewModel_FluentAPI))]
public sealed class DogovorClientRowViewModel : RSViewModelBase, IDataErrorInfo
{
    #region Fields

    private DogovorClientRow myEntity;

    #endregion

    #region Methods

    public override bool IsCorrect()
    {
        if (Nomenkl != null && NDSPercent >= 0 && NDSSumma >= 0 && Price > 0 && Summa > 0 && Quantity > 0)
            return true;
        return false;
    }

    public override object ToJson()
    {
        return new
        {
            Id = Id.ToString(),
            Номенклатурный_номер = NomenklNumber,
            Номенклатура = Nomenkl.Name,
            Количество = Quantity.ToString("n3"),
            Единица_измерения = Unit.Name,
            Price = Price.ToString("n2"),
            Услуга = IsUsluga ? "Да" : "Нет",
            НДС_Процент = NDSPercent.ToString("n2"),
            НДС_сумма = NDSSumma.ToString("n2"),
            Сумма = Summa.ToString("n2"),
            Примечание = Note
        };
    }

    private DogovorClientRow DefaultValue()
    {
        return new DogovorClientRow
        {
            Id = Guid.NewGuid()
        };
    }

    private void CalcSum()
    {
        if (IsCalcBack)
        {
            if (Summa == 0 || Quantity == 0) return;
            Entity.Price = Round(100 * Summa / (Quantity * (100 + NDSPercent)), 2);
            Entity.NDSSumma = Round(Summa - Entity.Price * Quantity, 2);
            RaisePropertyChanged(nameof(Summa));
            RaisePropertyChanged(nameof(NDSSumma));
        }
        else
        {
            NDSSumma = Round(Price * Quantity * NDSPercent / 100, 2);
            Summa = Round(Price * Quantity + NDSSumma, 2);
        }
    }

    #endregion

    #region Constructors

    public DogovorClientRowViewModel()
    {
        Entity = DefaultValue();
    }

    public DogovorClientRowViewModel(DogovorClientRow entity, DogovorClientViewModel parent = null)
    {
        Entity = entity ?? DefaultValue();
        Parent = parent;
    }

    #endregion

    #region Properties

    public ObservableCollection<DogovorClientFactViewModel> Facts =
        new ObservableCollection<DogovorClientFactViewModel>();

    public DogovorClientRow Entity
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

    public Guid DocId
    {
        get => Entity.DocId;
        set
        {
            if (Entity.DocId == value) return;
            Entity.DocId = value;
            RaisePropertyChanged();
        }
    }

    public Nomenkl Nomenkl
    {
        get => GlobalOptions.ReferencesCache.GetNomenkl(Entity.NomenklDC) as Nomenkl;
        set
        {
            if (Entity.NomenklDC > 0 && GlobalOptions.ReferencesCache.GetNomenkl(Entity.NomenklDC) == value) return;
            Entity.NomenklDC = value?.DocCode ?? 0;
            RaisePropertyChanged();
        }
    }

    public Unit Unit => Nomenkl?.Unit as Unit;
    public string NomenklNumber => Nomenkl?.NomenklNumber;

    public decimal Quantity
    {
        get => Entity.Quantity;
        set
        {
            if (Entity.Quantity == value) return;
            Entity.Quantity = value;
            CalcSum();
            RaisePropertyChanged();
        }
    }

    public decimal Price
    {
        get => Entity.Price;
        set
        {
            if (Entity.Price == value) return;
            Entity.Price = value;
            CalcSum();
            RaisePropertyChanged();
        }
    }

    public decimal NDSPercent
    {
        get => Entity.NDSPercent;
        set
        {
            if (Entity.NDSPercent == value) return;
            Entity.NDSPercent = value;
            CalcSum();
            RaisePropertyChanged();
        }
    }

    public decimal NDSSumma
    {
        get => Entity.NDSSumma;
        set
        {
            if (Entity.NDSSumma == value) return;
            Entity.NDSSumma = value;
            RaisePropertyChanged();
        }
    }

    public decimal Summa
    {
        get => Entity.Summa;
        set
        {
            if (Entity.Summa == value) return;
            Entity.Summa = value;
            CalcSum();
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

    public bool IsUsluga => Nomenkl?.IsUsluga ?? false;

    public bool IsCalcBack
    {
        get => Entity.IsCalckBack;
        set
        {
            if (Entity.IsCalckBack == value) return;
            Entity.IsCalckBack = value;
            CalcSum();
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Commands

    #endregion

    #region IDataErrorInfo

    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case nameof(Quantity):
                    return Quantity <= 0 ? "Кол-во должно быть > 0" : null;
                case nameof(Price):
                    return Price <= 0 ? "Цена должна быть > 0" : null;
                case nameof(Summa):
                    return Summa <= 0 ? "Сумма должна быть > 0" : null;
                default:
                    return null;
            }
        }
    }

    public string Error { get; } = null;

    #endregion
}