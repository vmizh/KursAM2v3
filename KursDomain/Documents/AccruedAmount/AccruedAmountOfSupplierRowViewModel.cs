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
using KursDomain.Documents.NomenklManagement;
using KursDomain.References;

namespace KursDomain.Documents.AccruedAmount;

public class AccruedAmountOfSupplierRowViewModel_FluentAPI : DataAnnotationForFluentApiBase,
    IMetadataProvider<AccruedAmountOfSupplierRowViewModel>
{
    void IMetadataProvider<AccruedAmountOfSupplierRowViewModel>.BuildMetadata(
        MetadataBuilder<AccruedAmountOfSupplierRowViewModel> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Entity).NotAutoGenerated();
        builder.Property(_ => _.Parent).NotAutoGenerated();
        builder.Property(_ => _.Error).NotAutoGenerated();
        builder.Property(_ => _.Name).NotAutoGenerated();
        builder.Property(_ => _.Nomenkl).AutoGenerated().DisplayName("Начисление").ReadOnly();
        builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2");
        builder.Property(_ => _.PaySumma).AutoGenerated().DisplayName("Сумма оплачено")
            .DisplayFormatString("n2").ReadOnly();
        builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
        builder.Property(_ => _.SDRSchet).AutoGenerated().DisplayName("Счет дох/расх");
    }
}

[MetadataType(typeof(AccruedAmountOfSupplierRowViewModel_FluentAPI))]
public class AccruedAmountOfSupplierRowViewModel : RSViewModelBase, IDataErrorInfo
{
    #region Methods

    public override object ToJson()
    {
        var res = new
        {
            НоменклатурныйНомер = Nomenkl.NomenklNumber,
            НоменклатураЗатрат = Nomenkl.ToString(),
            Сумма = Summa,
            Оплачено = PaySumma,
            СчетДоходовРасходов = SDRSchet?.ToString(),
            Примечание = Note
        };
        return res;
    }

    #endregion

    #region Fields

    private AccuredAmountOfSupplierRow myEntity;
    private new AccruedAmountOfSupplierViewModel myParent;

    private SDRSchet mySDRSchet;
    //private CashOut myCashDoc;
    //private BankOperationsViewModel myBankDoc;

    #endregion

    #region Properties

    public new AccruedAmountOfSupplierViewModel Parent
    {
        get => myParent;
        set
        {
            if (myParent == value) return;
            myParent = value;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<AccruedAmountOfSupplierCashDocViewModel> CashDocs { set; get; } =
        new ObservableCollection<AccruedAmountOfSupplierCashDocViewModel>();

    public AccuredAmountOfSupplierRow Entity
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
            if (GlobalOptions.ReferencesCache.GetNomenkl(Entity.NomenklDC) == value) return;
            if (value != null)
            {
                Entity.NomenklDC = value.DocCode;
                RaisePropertyChanged();
            }
        }
    }

    public decimal PaySumma => CashDocs?.Sum(_ => _.Summa) ?? 0;

    public decimal Summa
    {
        get => Entity.Summa;
        set
        {
            if (Entity.Summa == value) return;

            if (value < PaySumma)
                //WindowManager.ShowMessage($"Сумма начисления {value} " +
                //                          $"не можен быть меньше оплаченной суммы {PaySumma}",
                //    "Предупреждение", MessageBoxImage.Stop);
                return;

            Entity.Summa = value;
            RaisePropertyChanged();
            if (Parent is { } p) p.RaisePropertyChanged(nameof(p.Summa));
        }
    }

    public SDRSchet SDRSchet
    {
        get => mySDRSchet;
        set
        {
            if (mySDRSchet == value) return;
            mySDRSchet = value;
            Entity.SHPZ_DC = value?.DocCode;
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

    #region Constructors

    public AccruedAmountOfSupplierRowViewModel(AccuredAmountOfSupplierRow entity,
        AccruedAmountOfSupplierViewModel parent = null)
    {
        Entity = entity ?? DefaultValue();
        if (parent != null)
        {
            Parent = parent;
            Entity.DocId = parent.Id;
        }

        //if (Entity.CashDC != null)
        //    CashDoc = new CashOut(Entity.SD_34);
        if (Entity.SHPZ_DC != null)
            SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(Entity.SHPZ_DC.Value) as SDRSchet;
        //if (Entity.BankCode != null)
        //{
        //    var td101 = GlobalOptions.GetEntities().TD_101
        //        .Include(_ => _.SD_101).FirstOrDefault(_ => _.CODE == Entity.BankCode.Value);
        //    if (td101 != null)
        //        BankDoc = new BankOperationsViewModel(td101)
        //        {
        //            Parent = new SD_101ViewModel(td101.SD_101)
        //        };
        //}
    }

    private AccuredAmountOfSupplierRow DefaultValue()
    {
        return new AccuredAmountOfSupplierRow
        {
            Id = Guid.NewGuid(),
            Summa = 0
        };
    }

    #endregion

    #region IDataErrorInfo

    public string this[string columnName]
    {
        get
        {
            return columnName switch
            {
                nameof(Nomenkl) => Nomenkl == null ? "Тип начисления должен быть обязательно выбран" : null,
                nameof(Summa) => Summa <= 0 ? "Сумма должна быть больше 0" : null,
                nameof(SDRSchet) => SDRSchet == null
                    ? "Счет доходов/расходов обязателен для связи с платежами"
                    : null,
                _ => null
            };
        }
    }

    public string Error => null;

    #endregion
}