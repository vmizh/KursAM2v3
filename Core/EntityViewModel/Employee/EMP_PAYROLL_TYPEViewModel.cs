using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Employee
{
    // ReSharper disable once InconsistentNaming
    public class EMP_PAYROLL_TYPEViewModel : RSViewModelBase, IEntity<EMP_PAYROLL_TYPE>
    {
        private EMP_PAYROLL_TYPE myEntity;

        public EMP_PAYROLL_TYPEViewModel()
        {
            myEntity = DefaultValue();
        }

        public EMP_PAYROLL_TYPEViewModel(EMP_PAYROLL_TYPE entity)
        {
            Entity = entity ?? DefaultValue();
        }

        public string ID
        {
            get => Entity.ID;
            set
            {
                if (Entity.ID == value) return;
                Entity.ID = value;
                RaisePropertyChanged();
            }
        }

        public override decimal DocCode
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

        public override string Name
        {
            get => Entity.Name;
            set
            {
                if (Entity.Name == value) return;
                Entity.Name = value;
                RaisePropertyChanged();
            }
        }

        public int Type
        {
            get => Entity.Type;
            set
            {
                if (Entity.Type == value) return;
                Entity.Type = value;
                RaisePropertyChanged();
            }
        }

        public EMP_PAYROLL_TYPE Entity
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

        public List<EMP_PAYROLL_TYPE> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public EMP_PAYROLL_TYPE Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(EMP_PAYROLL_TYPE doc)
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

        public void UpdateFrom(EMP_PAYROLL_TYPE ent)
        {
            ID = ent.ID;
            Name = ent.Name;
            Type = ent.Type;
        }

        public void UpdateTo(EMP_PAYROLL_TYPE ent)
        {
            ent.ID = ID;
            ent.Name = Name;
            ent.Type = Type;
        }

        public EMP_PAYROLL_TYPE DefaultValue()
        {
            return new EMP_PAYROLL_TYPE
            {
                ID = Guid.NewGuid().ToString(),
                DOC_CODE = -1,
                EMP_PR_ROWS = new List<EMP_PR_ROWS>(),
                Type = 0
            };
        }

        public virtual EMP_PAYROLL_TYPE Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual EMP_PAYROLL_TYPE Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public EMP_PAYROLL_TYPE Load(decimal dc)
        {
            throw new NotImplementedException();
        }
    }
}