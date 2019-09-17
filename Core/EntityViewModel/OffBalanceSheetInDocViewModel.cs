using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class OffBalanceSheetInDocViewModel : RSViewModelBase, IEntity<OffBalanceSheetInDoc>
    {
        private OffBalanceSheetInDoc myEntity;

        public OffBalanceSheetInDocViewModel()
        {
            Entity = new OffBalanceSheetInDoc {Id = Guid.NewGuid()};
        }

        public OffBalanceSheetInDocViewModel(OffBalanceSheetInDoc entity)
        {
            Entity = entity ?? new OffBalanceSheetInDoc {Id = Guid.NewGuid()};
        }

        public ObservableCollection<OffBalanceSheetInRowViewModel> Rows { set; get; }
            = new ObservableCollection<OffBalanceSheetInRowViewModel>();
        public ObservableCollection<OffBalanceSheetInRowViewModel> DeletedRows { set; get; }
            = new ObservableCollection<OffBalanceSheetInRowViewModel>();
        public OffBalanceSheetInDoc Entity
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
        public string Num
        {
            get => Entity.Num;
            set
            {
                if (Entity.Num == value) return;
                Entity.Num = value;
                RaisePropertyChanged();
            }
        }
        public decimal KontrDC
        {
            get => Entity.KontrDC;
            set
            {
                if (Entity.KontrDC == value) return;
                Entity.KontrDC = value;
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
        public Guid TypeDocId
        {
            get => Entity.TypeDocId;
            set
            {
                if (Entity.TypeDocId == value) return;
                Entity.TypeDocId = value;
                RaisePropertyChanged();
            }
        }
        public OffBalanceSheetChargesType OffBalanceSheetChargesType
        {
            get => Entity.OffBalanceSheetChargesType;
            set
            {
                if (Entity.OffBalanceSheetChargesType == value) return;
                Entity.OffBalanceSheetChargesType = value;
                RaisePropertyChanged();
            }
        }
        public SD_43 SD_43
        {
            get => Entity.SD_43;
            set
            {
                if (Entity.SD_43 == value) return;
                Entity.SD_43 = value;
                RaisePropertyChanged();
            }
        }
        public bool IsAccessRight { get; set; }

        public List<OffBalanceSheetInDoc> LoadList()
        {
            throw new NotImplementedException();
        }

        public virtual OffBalanceSheetInDoc Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual OffBalanceSheetInDoc Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(OffBalanceSheetInDoc doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(OffBalanceSheetInDoc ent)
        {
            Id = ent.Id;
            Num = ent.Num;
            KontrDC = ent.KontrDC;
            Note = ent.Note;
            Date = ent.Date;
            TypeDocId = ent.TypeDocId;
            OffBalanceSheetChargesType = ent.OffBalanceSheetChargesType;
            SD_43 = ent.SD_43;
        }

        public void UpdateTo(OffBalanceSheetInDoc ent)
        {
            ent.Id = Id;
            ent.Num = Num;
            ent.KontrDC = KontrDC;
            ent.Note = Note;
            ent.Date = Date;
            ent.TypeDocId = TypeDocId;
            ent.OffBalanceSheetChargesType = OffBalanceSheetChargesType;
            ent.SD_43 = SD_43;
        }
    }
}