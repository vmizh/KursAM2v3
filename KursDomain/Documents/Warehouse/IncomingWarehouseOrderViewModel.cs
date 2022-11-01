using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Data;
using KursDomain.Annotations;
using KursDomain.ICommon;
using KursDomain.IDocuments.Finance;
using KursDomain.IDocuments.WarehouseOrder;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;

namespace KursDomain.Documents.Warehouse;

/// <summary>
///     Приходный складской ордер
/// </summary>
[DebuggerDisplay(
    "{DocCode,nq} {OrderNumber,nq} Отправитель: {SenderKontragent,nq} {SenderWarehouse,nq} от {OrderDate.ToShortDateString(),nq}")]
public class IncomingWarehouseOrderViewModel : IDocCode, IDescription, IIncomingWarehouseOrder,
    IEqualityComparer<IDocCode>,
    INotifyPropertyChanged, IDocumentState
{
    private readonly SD_24 Entity;
    private readonly IReferencesCache myReferenceCache;
    private IInvoiceProvider _InvoiceProvider;
    private SenderType _SenderType;

    public IncomingWarehouseOrderViewModel(SD_24 entity, IReferencesCache refCache)
    {
        Entity = entity;
        myReferenceCache = refCache;
        if (Entity.TD_24 == null || Entity.TD_24.Count == 0) return;
        foreach (var row in Entity.TD_24)
        {
            var newRow = new IncomingWarehouseOrderRowViewModel(row, myReferenceCache);
            newRow.SetHeader(Entity);
            ((ObservableCollection<IIncomingWarehouseOrderRow>)Rows).Add(newRow);
        }
    }

    public string Notes
    {
        get => Entity.DD_NOTES;
        set
        {
            if (Entity.DD_NOTES == value) return;
            Entity.DD_NOTES = value;
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get
        {
            var num = "№:" + (string.IsNullOrWhiteSpace(OuterNumber)
                ? OrderNumber.ToString()
                : $"{OrderNumber}/{OuterNumber}");
            var sender = SenderWarehouse == null ? SenderKontragent.ToString() : SenderWarehouse.ToString();
            return
                $"Приходный складской ордер {num} от {OrderDate.ToShortDateString()} Получатель {sender} Создал {Creator}";
        }
    }

    public decimal DocCode { get; set; }
    public RowStatus RowStatus { get; set; }

    public bool Equals(IDocCode x, IDocCode y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.DocCode == y.DocCode;
    }

    public int GetHashCode(IDocCode obj)
    {
        return obj.DocCode.GetHashCode();
    }

    public int OrderNumber
    {
        get => Entity.DD_IN_NUM;
        set
        {
            if (Entity.DD_IN_NUM == value) return;
            Entity.DD_IN_NUM = value;
            OnPropertyChanged();
        }
    }

    public string OuterNumber
    {
        get => Entity.DD_EXT_NUM;
        set
        {
            if (Entity.DD_EXT_NUM == value) return;
            Entity.DD_EXT_NUM = value;
            OnPropertyChanged();
        }
    }

    public DateTime OrderDate
    {
        get => Entity.DD_DATE;
        set
        {
            if (Entity.DD_DATE == value) return;
            Entity.DD_DATE = value;
            OnPropertyChanged();
        }
    }

    public string Creator
    {
        get => Entity.CREATOR;
        set
        {
            if (Entity.CREATOR == value) return;
            Entity.CREATOR = value;
            OnPropertyChanged();
        }
    }

    public SenderType SenderType
    {
        get => _SenderType;
        set
        {
            if (_SenderType == value) return;
            _SenderType = value;
            OnPropertyChanged();
        }
    }

    public IKontragent SenderKontragent
    {
        get => myReferenceCache.GetKontragent(Entity.DD_KONTR_OTPR_DC);
        set
        {
            if (myReferenceCache.GetKontragent(Entity.DD_KONTR_OTPR_DC).Equals(value)) return;
            Entity.DD_KONTR_OTPR_DC = ((IDocCode)value)?.DocCode;
            OnPropertyChanged();
        }
    }

    public IWarehouse SenderWarehouse
    {
        get => myReferenceCache.GetWarehouse(Entity.DD_SKLAD_OTPR_DC);
        set
        {
            if (myReferenceCache.GetWarehouse(Entity.DD_SKLAD_OTPR_DC).Equals(value)) return;
            Entity.DD_SKLAD_OTPR_DC = ((IDocCode)value)?.DocCode;
            OnPropertyChanged();
        }
    }

    public IInvoiceProvider InvoiceProvider
    {
        get => _InvoiceProvider;
        set
        {
            if (_InvoiceProvider == value) return;
            _InvoiceProvider = value;
            OnPropertyChanged();
        }
    }

    public IWarehouse Warehouse
    {
        get => myReferenceCache.GetWarehouse(Entity.DD_SKLAD_POL_DC);
        set
        {
            if (myReferenceCache.GetWarehouse(Entity.DD_SKLAD_POL_DC).Equals(value)) return;
            Entity.DD_SKLAD_POL_DC = ((IDocCode)value)?.DocCode;
            OnPropertyChanged();
        }
    }

    public IEnumerable<IIncomingWarehouseOrderRow> Rows { get; set; } =
        new ObservableCollection<IIncomingWarehouseOrderRow>();

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
