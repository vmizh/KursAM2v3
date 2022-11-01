using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Data;
using KursDomain.Annotations;
using KursDomain.ICommon;
using KursDomain.IDocuments.Finance;
using KursDomain.IDocuments.WarehouseOrder;
using KursDomain.IReferences;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.Documents.Warehouse;

[DebuggerDisplay("{DocCode,nq} {Code,nq} Номенклатура: {Nomenkl,nq} {Quantity,nq} ")]
public class IncomingWarehouseOrderRowViewModel : IDescription, IRowDC, IIncomingWarehouseOrderRow,
    IEqualityComparer<IRowDC>,
    INotifyPropertyChanged,
    IDocumentState
{
    private readonly TD_24 Entity;
    private readonly IReferencesCache myReferenceCache;
    private IInvoiceProviderRow _InvoiceProviderRow;
    private IIssueWarehouseOrderRow _IssueOrderRow;
    private IIncomingWarehouseOrder Header;

    public IncomingWarehouseOrderRowViewModel(TD_24 entity, IReferencesCache refCache)
    {
        myReferenceCache = refCache;
        Entity = entity;
    }

    [Display(Name = "Ном.№", AutoGenerateField = true)]
    [ReadOnly(true)]
    public string NomenklNumber => Nomenkl?.NomenklNumber;

    [Display(Name = "Связанный док-т", AutoGenerateField = true)]
    public string LinkDocument => (InvoiceProviderRow as IDescription)?.Description ??
                                  (IssueOrderRow as IDescription)?.Description;

    [Display(Name = "Описание", AutoGenerateField = false)]
    public string Description => $"{Header} приход {Nomenkl} {Quantity}";

    [Display(Name = "Наименование", AutoGenerateField = false)]
    public string Notes { get; set; }

    [Display(Name = "Статус", AutoGenerateField = false)]
    public RowStatus RowStatus { get; set; }


    public bool Equals(IRowDC x, IRowDC y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.DocCode == y.DocCode && x.Code == y.Code;
    }

    public int GetHashCode(IRowDC obj)
    {
        unchecked
        {
            return (obj.DocCode.GetHashCode() * 397) ^ obj.Code;
        }
    }

    [Display(Name = "Номенклатура", AutoGenerateField = true)]
    public INomenkl Nomenkl
    {
        get => myReferenceCache.GetNomenkl(Entity.DDT_NOMENKL_DC);
        set
        {
            if (myReferenceCache.GetNomenkl(Entity.DDT_NOMENKL_DC).Equals(value)) return;
            Entity.DDT_NOMENKL_DC = ((IDocCode)value).DocCode;
            OnPropertyChanged();
        }
    }

    [Display(Name = "Кол-во", AutoGenerateField = true)]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Quantity
    {
        get => Entity.DDT_KOL_PRIHOD;
        set
        {
            if (Entity.DDT_KOL_PRIHOD == value) return;
            Entity.DDT_KOL_PRIHOD = value;
            OnPropertyChanged();
        }
    }

    [Display(Name = "Таксирован", AutoGenerateField = true)]
    [ReadOnly(true)]
    public bool IsTaxExecuted
    {
        get => Entity.DDT_SPOST_DC != null;
        set { }
    }

    [Display(Name = "Счет дох/расх", AutoGenerateField = true)]
    [ReadOnly(true)]
    public ISDRSchet SDRSchet
    {
        get => myReferenceCache.GetSDRSchet(Entity.DDT_SHPZ_DC);
        set
        {
            if (myReferenceCache.GetSDRSchet(Entity.DDT_SHPZ_DC).Equals(value)) return;
            Entity.DDT_NOMENKL_DC = ((IDocCode)value).DocCode;
            OnPropertyChanged();
        }
    }

    [Display(Name = "Счет-фактура поставщика", AutoGenerateField = false)]
    public IInvoiceProviderRow InvoiceProviderRow
    {
        get => _InvoiceProviderRow;
        set
        {
            if (_InvoiceProviderRow == value) return;
            _InvoiceProviderRow = value;
            OnPropertyChanged();
        }
    }

    [Display(Name = "Расходный ордер", AutoGenerateField = false)]
    public IIssueWarehouseOrderRow IssueOrderRow
    {
        get => _IssueOrderRow;
        set
        {
            if (_IssueOrderRow == value) return;
            _IssueOrderRow = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public decimal DocCode
    {
        get => Entity.DOC_CODE;
        set
        {
            if (Entity.DOC_CODE == value) return;
            Entity.DOC_CODE = value;
            OnPropertyChanged();
        }
    }

    public int Code
    {
        get => Entity.CODE;
        set
        {
            if (Entity.CODE == value) return;
            Entity.CODE = value;
            OnPropertyChanged();
        }
    }

    public void SetHeader(SD_24 entity)
    {
        Header = new IncomingWarehouseOrder
        {
            DocCode = entity.DOC_CODE,
            OrderNumber = entity.DD_IN_NUM,
            OuterNumber = entity.DD_EXT_NUM,
            OrderDate = entity.DD_DATE,
            SenderWarehouse = myReferenceCache.GetWarehouse(entity.DD_SKLAD_OTPR_DC),
            SenderKontragent = myReferenceCache.GetKontragent(entity.DD_KONTR_OTPR_DC),
            Warehouse = myReferenceCache.GetWarehouse(entity.DD_SKLAD_POL_DC)
        };
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
