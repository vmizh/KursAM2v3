using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel
{
    public class NomenklTransferRowViewModel : RSViewModelBase, IEntity<NomenklTransferRow>
    {
        private NomenklTransferRow myEntity;

        public NomenklTransferRowViewModel()
        {
            Entity = new NomenklTransferRow {Id = Guid.Empty};
        }

        public NomenklTransferRowViewModel(NomenklTransferRow entity)
        {
            Entity = entity ?? new NomenklTransferRow {Id = Guid.Empty};
        }

        public new Guid Id
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

        public decimal NomenklOutDC
        {
            get => Entity.NomenklOutDC;
            set
            {
                if (Entity.NomenklOutDC == value) return;
                Entity.NomenklOutDC = value;
                RaisePropertyChanged();
            }
        }

        public decimal NomenklInDC
        {
            get => Entity.NomenklInDC;
            set
            {
                if (Entity.NomenklInDC == value) return;
                Entity.NomenklInDC = value;
                RaisePropertyChanged();
            }
        }

        public decimal Quantity
        {
            get => Entity.Quantity;
            set
            {
                if (Entity.Quantity == value) return;
                Entity.Quantity = value;
                RaisePropertyChanged();
            }
        }

        public decimal Rate
        {
            get => Entity.Rate;
            set
            {
                if (Entity.Rate == value) return;
                Entity.Rate = value;
                RaisePropertyChanged();
            }
        }

        public string LastUpdater
        {
            get => Entity.LastUpdater;
            set
            {
                if (Entity.LastUpdater == value) return;
                Entity.LastUpdater = value;
                RaisePropertyChanged();
            }
        }

        public DateTime LastUpdate
        {
            get => Entity.LastUpdate;
            set
            {
                if (Entity.LastUpdate == value) return;
                Entity.LastUpdate = value;
                RaisePropertyChanged();
            }
        }

        public bool IsAccepted
        {
            get => Entity.IsAccepted;
            set
            {
                if (Entity.IsAccepted == value) return;
                Entity.IsAccepted = value;
                RaisePropertyChanged();
            }
        }

        public decimal PriceIn
        {
            get => Entity.PriceIn;
            set
            {
                if (Entity.PriceIn == value) return;
                Entity.PriceIn = value;
                RaisePropertyChanged();
            }
        }

        public decimal PriceOut
        {
            get => Entity.PriceOut;
            set
            {
                if (Entity.PriceOut == value) return;
                Entity.PriceOut = value;
                RaisePropertyChanged();
            }
        }

        public bool? IsPriceAcepted
        {
            get => Entity.IsPriceAcepted;
            set
            {
                if (Entity.IsPriceAcepted == value) return;
                Entity.IsPriceAcepted = value;
                RaisePropertyChanged();
            }
        }

        public decimal? StoreDC
        {
            get => Entity.StoreDC;
            set
            {
                if (Entity.StoreDC == value) return;
                Entity.StoreDC = value;
                RaisePropertyChanged();
            }
        }

        public Guid? InvoiceSupplierRowId
        {
            get => Entity.InvoiceSupplierRowId;
            set
            {
                if (Entity.InvoiceSupplierRowId == value) return;
                Entity.InvoiceSupplierRowId = value;
                RaisePropertyChanged();
            }
        }

        public decimal? NakladRate
        {
            get => Entity.NakladRate;
            set
            {
                if (Entity.NakladRate == value) return;
                Entity.NakladRate = value;
                RaisePropertyChanged();
            }
        }

        public decimal? NakladEdSumma
        {
            get => Entity.NakladEdSumma;
            set
            {
                if (Entity.NakladEdSumma == value) return;
                Entity.NakladEdSumma = value;
                RaisePropertyChanged();
            }
        }

        public NomenklTransfer NomenklTransfer
        {
            get => Entity.NomenklTransfer;
            set
            {
                if (Entity.NomenklTransfer == value) return;
                Entity.NomenklTransfer = value;
                RaisePropertyChanged();
            }
        }

        public SD_83 SD_83
        {
            get => Entity.SD_83;
            set
            {
                if (Entity.SD_83 == value) return;
                Entity.SD_83 = value;
                RaisePropertyChanged();
            }
        }

        public SD_83 SD_831
        {
            get => Entity.SD_831;
            set
            {
                if (Entity.SD_831 == value) return;
                Entity.SD_831 = value;
                RaisePropertyChanged();
            }
        }

        public NomenklTransferRow Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }

        public List<NomenklTransferRow> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public NomenklTransferRow Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(NomenklTransferRow doc)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Delete(decimal dc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(NomenklTransferRow ent)
        {
            Id = ent.Id;
            DocId = ent.DocId;
            NomenklOutDC = ent.NomenklOutDC;
            NomenklInDC = ent.NomenklInDC;
            Quantity = ent.Quantity;
            Rate = ent.Rate;
            Note = ent.Note;
            LastUpdater = ent.LastUpdater;
            LastUpdate = ent.LastUpdate;
            IsAccepted = ent.IsAccepted;
            PriceIn = ent.PriceIn;
            PriceOut = ent.PriceOut;
            IsPriceAcepted = ent.IsPriceAcepted;
            StoreDC = ent.StoreDC;
            InvoiceSupplierRowId = ent.InvoiceSupplierRowId;
            NakladRate = ent.NakladRate;
            NakladEdSumma = ent.NakladEdSumma;
            NomenklTransfer = ent.NomenklTransfer;
            SD_83 = ent.SD_83;
            SD_831 = ent.SD_831;
        }

        public void UpdateTo(NomenklTransferRow ent)
        {
            ent.Id = Id;
            ent.DocId = DocId;
            ent.NomenklOutDC = NomenklOutDC;
            ent.NomenklInDC = NomenklInDC;
            ent.Quantity = Quantity;
            ent.Rate = Rate;
            ent.Note = Note;
            ent.LastUpdater = LastUpdater;
            ent.LastUpdate = LastUpdate;
            ent.IsAccepted = IsAccepted;
            ent.PriceIn = PriceIn;
            ent.PriceOut = PriceOut;
            ent.IsPriceAcepted = IsPriceAcepted;
            ent.StoreDC = StoreDC;
            ent.InvoiceSupplierRowId = InvoiceSupplierRowId;
            ent.NakladRate = NakladRate;
            ent.NakladEdSumma = NakladEdSumma;
            ent.NomenklTransfer = NomenklTransfer;
            ent.SD_83 = SD_83;
            ent.SD_831 = SD_831;
        }

        public NomenklTransferRow DefaultValue()
        {
            return new NomenklTransferRow {Id = Guid.Empty};
        }

        public virtual NomenklTransferRow Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual NomenklTransferRow Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public NomenklTransferRow Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }
}