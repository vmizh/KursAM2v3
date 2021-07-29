using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Systems
{
    public class DocHistoryViewModel : RSViewModelBase, IEntity<DocHistory>
    {
        private DocHistory myEntity;

        public DocHistoryViewModel()
        {
            Entity = new DocHistory {Id = Guid.NewGuid()};
        }

        public DocHistoryViewModel(DocHistory entity)
        {
            Entity = entity ?? new DocHistory {Id = Guid.NewGuid()};
        }

        [DisplayName("Id")]
        [Display(AutoGenerateField = false)]
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

        [DisplayName("Дата")]
        [Display(AutoGenerateField = true)]
        [DisplayFormat(DataFormatString = "G")]
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

        [DisplayName("Пользователь")]
        [Display(AutoGenerateField = true)]
        public string UserName
        {
            get => Entity.UserName;
            set
            {
                if (Entity.UserName == value) return;
                Entity.UserName = value;
                RaisePropertyChanged();
            }
        }

        [DisplayName("Type")]
        [Display(AutoGenerateField = true)]
        public string DocType
        {
            get => Entity.DocType;
            set
            {
                if (Entity.DocType == value) return;
                Entity.DocType = value;
                RaisePropertyChanged();
            }
        }

        [DisplayName("DocId")]
        [Display(AutoGenerateField = false)]
        public Guid? DocId
        {
            get => Entity.DocId;
            set
            {
                if (Entity.DocId == value) return;
                Entity.DocId = value;
                RaisePropertyChanged();
            }
        }

        [DisplayName("DocDC")]
        [Display(AutoGenerateField = false)]
        public decimal? DocDC
        {
            get => Entity.DocDC;
            set
            {
                if (Entity.DocDC == value) return;
                Entity.DocDC = value;
                RaisePropertyChanged();
            }
        }

        [DisplayName("Code")]
        [Display(AutoGenerateField = false)]
        public override int Code
        {
            get => (int) (Entity.Code ?? 0);
            set
            {
                if (Entity.Code == value) return;
                Entity.Code = value;
                RaisePropertyChanged();
            }
        }

        [DisplayName("DocData")]
        [Display(AutoGenerateField = false)]
        public string DocData
        {
            get => Entity.DocData;
            set
            {
                if (Entity.DocData == value) return;
                Entity.DocData = value;
                RaisePropertyChanged();
            }
        }

        [DisplayName("IsAccessRight")]
        [Display(AutoGenerateField = false)]
        public bool IsAccessRight { get; set; }

        [DisplayName("Entity")]
        [Display(AutoGenerateField = false)]
        public DocHistory Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public List<DocHistory> LoadList()
        {
            throw new NotImplementedException();
        }

        public virtual DocHistory Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual DocHistory Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(DocHistory doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(DocHistory ent)
        {
            Id = ent.Id;
            Date = ent.Date;
            UserName = ent.UserName;
            DocType = ent.DocType;
            DocId = ent.DocId;
            DocDC = ent.DocDC;
            Code = (int) (ent.Code ?? 0);
            DocData = ent.DocData;
        }

        public void UpdateTo(DocHistory ent)
        {
            ent.Id = Id;
            ent.Date = Date;
            ent.UserName = UserName;
            ent.DocType = DocType;
            ent.DocId = DocId;
            ent.DocDC = DocDC;
            ent.Code = Code;
            ent.DocData = DocData;
        }
    }
}