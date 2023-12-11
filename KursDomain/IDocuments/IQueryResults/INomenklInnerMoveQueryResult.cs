using System;
using System.ComponentModel.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.References;

namespace KursDomain.IDocuments.IQueryResults;

public interface INomenklInnerMoveQueryResult
{
    [Display(AutoGenerateField = false)]
    public decimal DocCode { get;  }
    [Display(AutoGenerateField = false)]
    public int Code { get;  }
    [Display(AutoGenerateField = true, Name = "Номенклатура")]
    public Nomenkl Nomenkl { get; }
    [Display(AutoGenerateField = true, Name = "Ном.№")]
    public string NomenklNumber { get; }
    [Display(AutoGenerateField = true, Name = "Ед.изм")]
    public string Unit { get; }
    [Display(AutoGenerateField = true, Name = "Кол-во")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Quantity { get; }
    [Display(AutoGenerateField = true, Name = "Склад отправитель")]
    public Warehouse Warehouse { get; }
    [Display(AutoGenerateField = true, Name = "Дата")]
    public DateTime DocData { get; }
    [Display(AutoGenerateField = true, Name = "№ ордера")]
    public string DocNumber { get; }
    [Display(AutoGenerateField = true, Name = "Примечание")]
    public string Note { get; }
    [Display(AutoGenerateField = true, Name = "Выбрать")]
    public bool IsSelected { set; get; }
}


public class NomenklInnerMoveQueryResult : INomenklInnerMoveQueryResult
{
    public NomenklInnerMoveQueryResult(decimal docCode, int code, Nomenkl nomenkl, decimal quantity, Warehouse warehouse, DateTime docData,
        string docNumber, string note)
    {
        DocCode = docCode;
        Code = code;
        Nomenkl = nomenkl;
        Quantity = quantity;
        Warehouse = warehouse;
        DocData = docData;
        DocNumber = docNumber;
        Note = note;
    }

    public decimal DocCode { get; }
    public int Code { get; }
    public Nomenkl Nomenkl { get; }
    public string NomenklNumber => Nomenkl?.NomenklNumber;
    public string Unit => ((IName)Nomenkl?.Unit)?.Name;
    public decimal Quantity { get; }
    public Warehouse Warehouse { get; }
    public DateTime DocData { get; }
    public string DocNumber { get; }
    public string Note { get; }
    public bool IsSelected { get; set; }
}
