using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Core.ViewModel.Base;
using Data;

namespace Core.Invoices.EntityViewModel
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public class ProjectsInfoViewModel : RSViewModelBase, IEntity<ProjectsInfo>
    {
        private ProjectsInfo myEntity;
        private decimal mySummaDiler;

        public ProjectsInfoViewModel()
        {
            Entity = new ProjectsInfo();
        }

        public ProjectsInfoViewModel(ProjectsInfo entity)
        {
            Entity = entity ?? new ProjectsInfo();
        }

        public ProjectsInfo Entity
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

        public DateTime? DATE_ORD
        {
            get => Entity.DATE_ORD;
            set
            {
                if (Entity.DATE_ORD == value) return;
                Entity.DATE_ORD = value;
                RaisePropertyChanged();
            }
        }

        public double? SUMM_ORD
        {
            get => Entity.SUMM_ORD;
            set
            {
                if (Entity.SUMM_ORD == value) return;
                Entity.SUMM_ORD = value;
                RaisePropertyChanged();
            }
        }

        public decimal? CRS_DC
        {
            get => Entity.CRS_DC;
            set
            {
                if (Entity.CRS_DC == value) return;
                Entity.CRS_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal SummaDiler
        {
            get => mySummaDiler;
            set
            {
                if (mySummaDiler == value) return;
                mySummaDiler = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }

        public List<ProjectsInfo> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual ProjectsInfo Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual ProjectsInfo Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(ProjectsInfo doc)
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

        public void UpdateFrom(ProjectsInfo ent)
        {
            Id = ent.Id;
            DATE_ORD = ent.DATE_ORD;
            SUMM_ORD = ent.SUMM_ORD;
            CRS_DC = ent.CRS_DC;
        }

        public void UpdateTo(ProjectsInfo ent)
        {
            ent.Id = Id;
            ent.DATE_ORD = DATE_ORD;
            ent.SUMM_ORD = SUMM_ORD;
            ent.CRS_DC = CRS_DC;
        }

        public ProjectsInfo DefaultValue()
        {
            throw new NotImplementedException();
        }
    }
}