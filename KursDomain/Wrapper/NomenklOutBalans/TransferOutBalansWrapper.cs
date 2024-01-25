using Data;
using KursDomain.Wrapper.Base;
using System.ComponentModel.DataAnnotations;
using System;
using KursDomain.References;

namespace KursDomain.Wrapper.NomenklOutBalans;

public class TransferOutBalansWrapper : BaseWrapper<TransferOutBalans>
{
    private decimal mySumma;

    public TransferOutBalansWrapper(TransferOutBalans model) : base(model)
    {
    }

    [Display(AutoGenerateField = false)] public new Guid Id => Model.Id;

    [Display(AutoGenerateField = true, Name = "Дата")]
    public DateTime DocDate
    {
        get => GetValue<DateTime>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "№")]
    public int DocNum
    {
        get => GetValue<int>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Склад")]
    public Warehouse Warehouse
    {
        get => GlobalOptions.ReferencesCache.GetWarehouse(Model.WarehouseDC) as Warehouse;
        set
        {
            if (Model.WarehouseDC == (value?.DocCode ?? 0)) return;
            Model.WarehouseDC = value?.DocCode ?? 0;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    public new string Note
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Создатель")]
    public string Creator
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [Display(AutoGenerateField = true, Name = "Сумма")]
    [DisplayFormat(ApplyFormatInEditMode = true,DataFormatString = "n2")]
    public decimal Summa
    {
        get => mySumma;
        set
        {
            if(mySumma == value) return;
            mySumma = value;
            RaisePropertyChanged();
        }
    }

}

