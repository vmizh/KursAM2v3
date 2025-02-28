using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Data;
using KursDomain.ICommon;
using KursDomain.IDocuments.TransferOut;
using KursDomain.IReferences.Nomenkl;
using KursDomain.References;
using KursDomain.Wrapper.Base;
using Prism.Events;

namespace KursDomain.Wrapper.TransferOut;

public class TransferOutBalansRowsWrapper : BaseWrapper<TransferOutBalansRows>,
    ITransferOutBalansRows, IEquatable<TransferOutBalansRowsWrapper>
{
    private decimal myCostPrice;
    private decimal myMaxCount;

    public TransferOutBalansRowsWrapper(TransferOutBalansRows model, IEventAggregator eventAggregator,
        IMessageDialogService messageDialogService) :
        base(model, eventAggregator, messageDialogService)
    {
    }

    [Display(AutoGenerateField = true, Name = "Ном.№", Order = 1)]
    public string NomenklNumber => Nomenkl?.NomenklNumber;

    [Display(AutoGenerateField = true, Name = "Ед.изм", Order = 5)]
    public string Unit => Nomenkl?.Unit?.OKEI;

    [Display(AutoGenerateField = true, Name = "Валюта", Order = 6)]
    public Currency Currency => Nomenkl?.Currency as Currency;

    [Display(AutoGenerateField = true, Name = "Макс. кол-во")]
    [DisplayFormat(DataFormatString = "n2")]
    [ReadOnly(true)]
    public decimal MaxCount
    {
        set
        {
            if (value == myMaxCount) return;
            myMaxCount = value;
            RaisePropertyChanged();
        }
        get => myMaxCount;
    }

    [Display(AutoGenerateField = true, Name = "Себестоимость", Order = 5)]
    [DisplayFormat(DataFormatString = "n2")]
    [ReadOnly(true)]
    public decimal CostPrice
    {
        set
        {
            if (value == myCostPrice) return;
            myCostPrice = value;
            RaisePropertyChanged();
        }
        get => myCostPrice;
    }

    public bool Equals(TransferOutBalansRowsWrapper other)
    {
        if (other == null) return false;
        return Id == other.Id;
    }

    [Display(AutoGenerateField = false, Name = "Id")]
    public override Guid Id
    {
        get => GetValue<Guid>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = false, Name = "DocId")]
    public Guid DocId
    {
        get => GetValue<Guid>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Себ-ть (сумма)", Order = 5)]
    [DisplayFormat(DataFormatString = "n2")]
    [ReadOnly(true)]
    public decimal CostSumma => Quatntity*CostPrice;

    [Display(AutoGenerateField = true, Name = "Примечание")]
    [StringLength(500)]
    public override string Note
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    private References.Nomenkl myNomenkl;

    [Display(AutoGenerateField = true, Name = "Номенклатура", Order = 2)]
    [ReadOnly(true)]
    public References.Nomenkl Nomenkl
    {
        get => myNomenkl; // GlobalOptions.ReferencesCache.GetNomenkl(Model.NomenklDC) as References.Nomenkl;
        set
        {
            if (myNomenkl  == value) return;
            myNomenkl = value;
            SetValue(value?.DocCode ?? 0, nameof(TransferOutBalansRows.NomenklDC));
        }
    }

    [Display(AutoGenerateField = true, Name = "Кол-во", Order = 3)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Quatntity
    {
        get => GetValue<decimal>();
        set
        {
            if (value > MaxCount) return;
            SetValue(value);
            RaisePropertyChanged(nameof(Summa));
        }
    }

    [Display(AutoGenerateField = true, Name = "Цена", Order = 4)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Price
    {
        get => GetValue<decimal>();
        set
        {
            SetValue(value);
            RaisePropertyChanged(nameof(Summa));
        }
    }

    [Display(AutoGenerateField = true, Name = "Сумма", Order = 5)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Summa => Quatntity * Price;

    [Display(AutoGenerateField = false, Name = "Документ")]
    public ITransferOutBalans TransferOutBalans { get; set; }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TransferOutBalansRowsWrapper)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(TransferOutBalansRowsWrapper left, TransferOutBalansRowsWrapper right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TransferOutBalansRowsWrapper left, TransferOutBalansRowsWrapper right)
    {
        return !Equals(left, right);
    }

    public override object ToJson()
    {
        var res = new
        {
            Статус = State == RowStatus.NewRow ? "Новый" : "Изменен",
            Id,
            DocId,
            Номенклатурный_номер = NomenklNumber,
            Номенклатура = Nomenkl.Name,
            Количество = Quatntity.ToString("n3"),
            Единица_измерения = Nomenkl.Unit.OKEI,
            Цена = Price.ToString("n2"),
            Сумма = Summa.ToString("n2"),
            Примечание = Note
        };

        return res;
    }
}
