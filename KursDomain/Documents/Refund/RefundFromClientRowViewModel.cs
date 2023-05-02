using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Core.ViewModel.Base;
using Data;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.IDocuments.Refund;
using KursDomain.IReferences.Nomenkl;
using KursDomain.References;

namespace KursDomain.Documents.Refund;

[SuppressMessage("ReSharper", "ArrangeAccessorOwnerBody")]
public class RefundFromClientRowViewModel : IRefundFromClientRow, IEntity<RefundFromClientRow>, INotifyPropertyChanged
{
    #region Constructors

    public RefundFromClientRowViewModel(RefundFromClientRow entity, ALFAMEDIAEntities dbContext)
    {
        Entity = entity ?? DefaultValue();
        context = dbContext;
        LoadReferences();
    }

    #endregion


    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Fields

    private WaybillRow myWaybillRow;
    private ALFAMEDIAEntities context;

    #endregion

    #region Properties

    [Display(AutoGenerateField = false, Name = "Id")]
    public Guid Id
    {
        get => Entity.Id;
        set { Entity.Id = value; }
    }

    [Display(AutoGenerateField = false, Name = "ParentId")]
    public Guid DocId
    {
        get => Entity.DocId;
        set { Entity.DocId = value; }
    }

    [Display(AutoGenerateField = true, Name = "Номенклатура")]
    [ReadOnly(true)]
    public INomenkl Nomenkl
    {
        get => GlobalOptions.ReferencesCache.GetNomenkl(Entity.NimenklDC);

        set
        {
            var dc = ((IDocCode)(value ?? new Nomenkl { DocCode = 0 })).DocCode;
            if (Entity.NimenklDC == dc) return;
            Entity.NimenklDC = dc;
            OnPropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Ном.№")]
    [ReadOnly(true)]
    public string NomenklNumber => Nomenkl?.NomenklNumber;

    [Display(AutoGenerateField = true, Name = "Кол-во")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal Quantity
    {
        get => Entity.Quantity;

        set
        {
            if (Entity.Quantity == value) return;
            Entity.Quantity = value;
            OnPropertyChanged();
        }
    }

    [Display(AutoGenerateField = false, Name = "Стр.расх.накл")]
    [ReadOnly(true)]
    public WaybillRow WaybillRow
    {
        get => myWaybillRow;

        set
        {
            if (myWaybillRow == value) return;
            myWaybillRow = value;
            Entity.WayBillId = value?.Id;
            OnPropertyChanged(nameof(WaybillInfo));
            OnPropertyChanged(nameof(WaybillPrice));
        }
    }

    [Display(AutoGenerateField = true, Name = "Расх.накл")]
    [ReadOnly(true)]
    public string WaybillInfo => WaybillRow?.Description;


    [Display(AutoGenerateField = true, Name = "Цена отгрузки")]
    [ReadOnly(true)]
    public decimal WaybillPrice
    {
        get => myWaybillRow?.TD_84.SFT_ED_CENA ?? 0;
    }

    [Display(AutoGenerateField = false, Name = "Факт. цена")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal FactPrice
    {
        get => Entity.FactPrice;

        set
        {
            if (Entity.FactPrice == value) return;
            Entity.FactPrice = value;
            OnPropertyChanged();
        }
    }

    [Display(AutoGenerateField = false, Name = "Примечание")]
    public string Note
    {
        get => Entity.Note;

        set
        {
            if (Entity.Note == value) return;
            Entity.Note = value;
            OnPropertyChanged();
        }
    }

    [Display(AutoGenerateField = false, Name = "Документ")]
    public IRefundFromClient Parent { get; set; }

    [Display(AutoGenerateField = false, Name = "Entity")]
    public RefundFromClientRow Entity { get; set; }

    #endregion

    #region method

    public RefundFromClientRow DefaultValue()
    {
        return new RefundFromClientRow
        {
            Id = Guid.NewGuid()
        };
    }

    private void LoadReferences()
    {
        if (Entity.WayBillId != null)
        {
            var wb = context.TD_24
                .Include(_ => _.SD_24)
                .Include(_ => _.TD_84)
                .FirstOrDefault(_ => _.Id == Entity.WayBillId);
            WaybillRow = new WaybillRow(wb);
        }
        
    }

    #endregion

    //protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    //{
    //    if (EqualityComparer<T>.Default.Equals(field, value)) return false;
    //    field = value;
    //    OnPropertyChanged(propertyName);
    //    return true;
    //}
}
