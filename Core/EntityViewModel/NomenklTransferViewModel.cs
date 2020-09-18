using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.ViewModel.Base;
using Data;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel
{
    public class NomenklTransferViewModel : RSViewModelBase, IEntity<NomenklTransfer>
    {
        private NomenklTransfer myEntity;

        public NomenklTransferViewModel()
        {
            Entity = new NomenklTransfer {Id = Guid.Empty};
        }

        public NomenklTransferViewModel(NomenklTransfer entity)
        {
            Entity = entity ?? new NomenklTransfer {Id = Guid.Empty};
            if (entity != null && entity.NomenklTransferRow != null && entity.NomenklTransferRow.Count > 0)
                foreach (var r in entity.NomenklTransferRow)
                    NomenklTransferRow.Add(r);
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

        public DateTime Date
        {
            get => Entity.Date;
            set
            {
                if (Entity.Date == value) return;
                Entity.Date = value;
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

        public decimal SkladDC
        {
            get
            {
                if (Entity.SkladDC != null) return Entity.SkladDC.Value;
                return -1;
            }
            set
            {
                if (Entity.SkladDC == value) return;
                Entity.SkladDC = value;
                RaisePropertyChanged();
            }
        }

        public string Creator
        {
            get => Entity.Creator;
            set
            {
                if (Entity.Creator == value) return;
                Entity.Creator = value;
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

        public DateTime? LastUpdate
        {
            get => Entity.LastUpdate;
            set
            {
                if (Entity.LastUpdate == value) return;
                Entity.LastUpdate = value;
                RaisePropertyChanged();
            }
        }

        public int DucNum
        {
            get => Entity.DucNum;
            set
            {
                if (Entity.DucNum == value) return;
                Entity.DucNum = value;
                RaisePropertyChanged();
            }
        }

        public SD_27 SD_27
        {
            get => Entity.SD_27;
            set
            {
                if (Entity.SD_27 == value) return;
                Entity.SD_27 = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<NomenklTransferRow> NomenklTransferRow { get; set; } =
            new ObservableCollection<NomenklTransferRow>();

        public NomenklTransfer Entity
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

        public List<NomenklTransfer> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public NomenklTransfer Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(NomenklTransfer doc)
        {
            throw new NotImplementedException();
        }

        public virtual void Save()
        {
            throw new NotImplementedException();
        }

        public virtual void Delete()
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

        public void UpdateFrom(NomenklTransfer ent)
        {
            Id = ent.Id;
            Date = ent.Date;
            Note = ent.Note;
            SkladDC = ent.SkladDC ?? -1;
            Creator = ent.Creator;
            LastUpdater = ent.LastUpdater;
            LastUpdate = ent.LastUpdate;
            DucNum = ent.DucNum;
            NomenklTransferRow = (ObservableCollection<NomenklTransferRow>) ent.NomenklTransferRow;
            SD_27 = ent.SD_27;
        }

        public void UpdateTo(NomenklTransfer ent)
        {
            ent.Id = Id;
            ent.Date = Date;
            ent.Note = Note;
            ent.SkladDC = SkladDC;
            ent.Creator = Creator;
            ent.LastUpdater = LastUpdater;
            ent.LastUpdate = LastUpdate;
            ent.DucNum = DucNum;
            ent.NomenklTransferRow = NomenklTransferRow;
            ent.SD_27 = SD_27;
        }

        public NomenklTransfer DefaultValue()
        {
            return new NomenklTransfer {Id = Guid.Empty};
        }

        public virtual NomenklTransfer Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual NomenklTransfer Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public NomenklTransfer Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }
}