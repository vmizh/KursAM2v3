using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel
{
    public class KONTRAGENT_REF_OUT_REQUISITEViewModel : RSViewModelBase, IEntity<KONTRAGENT_REF_OUT_REQUISITE>
    {
        private KONTRAGENT_REF_OUT_REQUISITE myEntity;

        public KONTRAGENT_REF_OUT_REQUISITEViewModel()
        {
        }

        public KONTRAGENT_REF_OUT_REQUISITEViewModel(KONTRAGENT_REF_OUT_REQUISITE entity)
        {
            Entity = entity;
        }

        public Guid KontrId
        {
            get => Entity.KontrId;
            set
            {
                if (Entity.KontrId == value) return;
                Entity.KontrId = value;
                RaisePropertyChanged();
            }
        }

        public string OKPO
        {
            get => Entity.OKPO;
            set
            {
                if (Entity.OKPO == value) return;
                Entity.OKPO = value;
                RaisePropertyChanged();
            }
        }

        public string SFText
        {
            get => Entity.SFText;
            set
            {
                if (Entity.SFText == value) return;
                Entity.SFText = value;
                RaisePropertyChanged();
            }
        }

        public string NaklText
        {
            get => Entity.NaklText;
            set
            {
                if (Entity.NaklText == value) return;
                Entity.NaklText = value;
                RaisePropertyChanged();
            }
        }

        public new string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Entity.Note = value;
                RaisePropertyChanged();
            }
        }

        public KONTRAGENT_REF_OUT KontragentRefOut
        {
            get => Entity.KONTRAGENT_REF_OUT;
            set
            {
                if (Entity.KONTRAGENT_REF_OUT == value) return;
                Entity.KONTRAGENT_REF_OUT = value;
                RaisePropertyChanged();
            }
        }

        public KONTRAGENT_REF_OUT_REQUISITE Entity
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

        public List<KONTRAGENT_REF_OUT_REQUISITE> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public KONTRAGENT_REF_OUT_REQUISITE Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(KONTRAGENT_REF_OUT_REQUISITE doc)
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

        public void UpdateFrom(KONTRAGENT_REF_OUT_REQUISITE ent)
        {
            Id = ent.Id;
            KontrId = ent.KontrId;
            Name = ent.Name;
            OKPO = ent.OKPO;
            SFText = ent.SFText;
            NaklText = ent.NaklText;
            Note = ent.Note;
            KontragentRefOut = ent.KONTRAGENT_REF_OUT;
        }

        public void UpdateTo(KONTRAGENT_REF_OUT_REQUISITE ent)
        {
            ent.Id = Id;
            ent.KontrId = KontrId;
            ent.Name = Name;
            ent.OKPO = OKPO;
            ent.SFText = SFText;
            ent.NaklText = NaklText;
            ent.Note = Note;
            ent.KONTRAGENT_REF_OUT = KontragentRefOut;
        }

        public KONTRAGENT_REF_OUT_REQUISITE DefaultValue()
        {
            return new KONTRAGENT_REF_OUT_REQUISITE
            {
                Id = Guid.NewGuid(),
                KontrId = Guid.Empty
            };
        }

        public virtual KONTRAGENT_REF_OUT_REQUISITE Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual KONTRAGENT_REF_OUT_REQUISITE Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public KONTRAGENT_REF_OUT_REQUISITE Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }
}