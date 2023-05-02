using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Core.ViewModel.Base;
using Data;
using KursDomain.ICommon;
using KursDomain.IDocuments.Refund;
using KursDomain.IReferences;
using KursDomain.IReferences.Kontragent;
using KursDomain.References;

namespace KursDomain.Documents.Refund;

[SuppressMessage("ReSharper", "ArrangeAccessorOwnerBody")]
public class RefundFromClientViewModel : IRefundFromClient, IEntity<RefundFromClient>, INotifyPropertyChanged
{
    #region Fields

    private readonly ALFAMEDIAEntities context;

    #endregion

    #region Constructors

    public RefundFromClientViewModel(RefundFromClient entity, ALFAMEDIAEntities dbContext)
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


    #region Properties

    [Display(AutoGenerateField = false, Name = "Id")]
    public Guid Id
    {
        get => Entity.Id;
        set { Entity.Id = value; }
    }

    [Display(AutoGenerateField = true, Name = "№")]
    [ReadOnly(true)]
    public int Num { get; set; }

    [Display(AutoGenerateField = true, Name = "Дата")]
    public DateTime Date
    {
        get => Entity.Date;
        set
        {
            if (Entity.Date == value) return;
            Entity.Date = value;
            OnPropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Склад")]
    [ReadOnly(true)]
    public IWarehouse Warehouse
    {
        get => GlobalOptions.ReferencesCache.GetWarehouse(Entity.StoreDC);

        set
        {
            var dc = ((IDocCode)(value ?? new References.Warehouse { DocCode = 0 })).DocCode;
            if (Entity.StoreDC == dc) return;
            Entity.StoreDC = dc;
            OnPropertyChanged();
        }
    }

    [Display(AutoGenerateField = true, Name = "Контрагент")]
    [ReadOnly(true)]
    public IKontragent Client
    {
        get => GlobalOptions.ReferencesCache.GetKontragent(Entity.ClientDC);

        set
        {
            var dc = ((IDocCode)(value ?? new Kontragent { DocCode = 0 })).DocCode;
            if (Entity.ClientDC == dc) return;
            Entity.ClientDC = dc;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Currency));
        }
    }

    [Display(AutoGenerateField = true, Name = "Валюта")]
    [ReadOnly(true)]
    public ICurrency Currency => Client.Currency;

    [Display(AutoGenerateField = true, Name = "Сумма")]
    [ReadOnly(true)]
    public decimal Summa => Rows.Sum(_ => _.FactPrice * _.Quantity);


    [Display(AutoGenerateField = true, Name = "Примечание")]
    [ReadOnly(true)]
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

    [Display(AutoGenerateField = true, Name = "Создатель")]
    [ReadOnly(true)]
    public string Creator { get; set; }

    [Display(AutoGenerateField = false, Name = "Строки")]
    [ReadOnly(true)]
    public IEnumerable<IRefundFromClientRow> Rows { get; } = new ObservableCollection<IRefundFromClientRow>();

    [Display(AutoGenerateField = false, Name = "Entity")]
    [ReadOnly(true)]
    public RefundFromClient Entity { get; set; }

    #endregion

    #region Methods

    public RefundFromClient DefaultValue()
    {
        return new RefundFromClient
        {
            Id = Guid.NewGuid()
        };
    }

    private void LoadReferences()
    {
        foreach (var r in Entity.RefundFromClientRow)
            ((ObservableCollection<IRefundFromClientRow>)Rows).Add(new RefundFromClientRowViewModel(r, context)
            {
                Parent = this
            });
    }

    #endregion
}
