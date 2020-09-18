using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class OffBalanceSheetOutRowViewModel : RSViewModelBase, IEntity<OffBalanceSheetOutRow>
    {
        private OffBalanceSheetOutRow myEntity;

        public OffBalanceSheetOutRowViewModel()
        {
            Entity = new OffBalanceSheetOutRow {Id = Guid.NewGuid()};
        }

        public OffBalanceSheetOutRowViewModel(OffBalanceSheetOutRow entity)
        {
            Entity = entity ?? new OffBalanceSheetOutRow {Id = Guid.NewGuid()};
        }

        public OffBalanceSheetOutRow Entity
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

        public decimal NomenklDC
        {
            get => Entity.NomenklDC;
            set
            {
                if (Entity.NomenklDC == value) return;
                Entity.NomenklDC = value;
                RaisePropertyChanged();
            }
        }

        public decimal Price
        {
            get => Entity.Price;
            set
            {
                if (Entity.Price == value) return;
                Entity.Price = value;
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

        public decimal Summa
        {
            get => Entity.Summa;
            set
            {
                if (Entity.Summa == value) return;
                Entity.Summa = value;
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

        public decimal SDR_DC
        {
            get => Entity.SDR_DC;
            set
            {
                if (Entity.SDR_DC == value) return;
                Entity.SDR_DC = value;
                RaisePropertyChanged();
            }
        }

        public OffBalanceSheetOutDoc OffBalanceSheetOutDoc
        {
            get => Entity.OffBalanceSheetOutDoc;
            set
            {
                if (Entity.OffBalanceSheetOutDoc == value) return;
                Entity.OffBalanceSheetOutDoc = value;
                RaisePropertyChanged();
            }
        }

        public SD_303 SD_303
        {
            get => Entity.SD_303;
            set
            {
                if (Entity.SD_303 == value) return;
                Entity.SD_303 = value;
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

        public bool IsAccessRight { get; set; }

        public List<OffBalanceSheetOutRow> LoadList()
        {
            throw new NotImplementedException();
        }

        public virtual OffBalanceSheetOutRow Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual OffBalanceSheetOutRow Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(OffBalanceSheetOutRow doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(OffBalanceSheetOutRow ent)
        {
            Id = ent.Id;
            DocId = ent.DocId;
            NomenklDC = ent.NomenklDC;
            Price = ent.Price;
            Quantity = ent.Quantity;
            Summa = ent.Summa;
            Note = ent.Note;
            SDR_DC = ent.SDR_DC;
            OffBalanceSheetOutDoc = ent.OffBalanceSheetOutDoc;
            SD_303 = ent.SD_303;
            SD_83 = ent.SD_83;
        }

        public void UpdateTo(OffBalanceSheetOutRow ent)
        {
            ent.Id = Id;
            ent.DocId = DocId;
            ent.NomenklDC = NomenklDC;
            ent.Price = Price;
            ent.Quantity = Quantity;
            ent.Summa = Summa;
            ent.Note = Note;
            ent.SDR_DC = SDR_DC;
            ent.OffBalanceSheetOutDoc = OffBalanceSheetOutDoc;
            ent.SD_303 = SD_303;
            ent.SD_83 = SD_83;
        }
    }
}