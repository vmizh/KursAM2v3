using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Data;
using KursDomain.IDocuments.NomenklReturn;
using KursDomain.References;

namespace KursDomain.Documents.NomenklReturn;

public class NomenklReturnToProviderSearch : INomenklReturnToProvider
{
    #region Methods

    public void LoadReferences(NomenklReturnOfClient entity)
    {
        if (entity.KontragentDC != 0)
            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(entity.KontragentDC) as Kontragent;
        if(entity.WarehouseDC != 0)
            Warehouse = GlobalOptions.ReferencesCache.GetWarehouse(entity.WarehouseDC) as References.Warehouse;
        Creator = entity.Creator;
    }

    #endregion
   
    /// <summary>
    ///     Пользователь, последний изменивший документ
    /// </summary>
    [Display(AutoGenerateField = true, Name = "Посл.изменил", Order = 21)]
    public string LastChanger { set; get; }

    /// <summary>
    ///     Дата последнего изменения
    /// </summary>
    [Display(AutoGenerateField = true, Name = "Дата посл.изм.", Order = 21)]
    public DateTime LastChangerDate { set; get; }

    [Display(AutoGenerateField = false)] public Guid Id { get; set; }

    [Display(AutoGenerateField = true, Name = "№")]
    [ReadOnly(true)]
    public int DocNum { get; set; }

    [Display(AutoGenerateField = true, Name = "Внешн.№")]
    [ReadOnly(true)]
    public string DocExtNum { get; set; }

    [Display(AutoGenerateField = true, Name = "Дата")]
    [ReadOnly(true)]
    public DateTime DocDate { get; set; }

    [Display(AutoGenerateField = false)]
    public Kontragent Kontragent { get; set; }

    [Display(AutoGenerateField = true, Name = "Контрагент")]
    public Kontragent Kontregent { get; set; }

    [Display(AutoGenerateField = true, Name = "Склад")]
    public References.Warehouse Warehouse { get; set; }


    [Display(AutoGenerateField = false)] public decimal? InvoiceClientDC { get; set; }

    [Display(AutoGenerateField = true, Name = "Валюта")]
    public References.Currency Currency => Kontragent?.Currency as References.Currency;
    [Display(AutoGenerateField = false)]
    public decimal? PrihOrderDC { get; set; }
    [Display(AutoGenerateField = false)]
    public decimal? InvoiceProviderDC { get; set; }

    [Display(AutoGenerateField = true, Name = "Сумма клиента")]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaClient { get; set; }

    [Display(AutoGenerateField = true, Name = "Сумма склад")]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal SummaWarehouse { get; set; }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    [ReadOnly(true)]
    [DisplayFormat(DataFormatString = "n2")]
    public string Note { get; set; }

    [Display(AutoGenerateField = true, Name = "Создатель")]
    [ReadOnly(true)]
    public string Creator { get; set; }

    [Display(AutoGenerateField = false)] public ObservableCollection<NomenklReturnToProviderRowViewModel> Rows { get; set; }

    [Display(AutoGenerateField = false)] public NomenklReturnToProvider Entity { get; set; }
}
