using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.ViewModel.Common
{
    public class ProjectsDocsViewModel : RSViewModelBase, IEntity<ProjectsDocs>
    {
        private ProjectsDocs myEntity;

        public ProjectsDocsViewModel()
        {
            Entity = new ProjectsDocs {Id = Guid.NewGuid()};
        }

        public ProjectsDocsViewModel(ProjectsDocs entity)
        {
            Entity = entity ?? new ProjectsDocs {Id = Guid.NewGuid()};
        }

        public Guid ProjectId
        {
            get => Entity.ProjectId;
            set
            {
                if (Entity.ProjectId == value) return;
                Entity.ProjectId = value;
                RaisePropertyChanged();
            }
        }

        public string DocTypeName
        {
            get => Entity.DocTypeName;
            set
            {
                if (Entity.DocTypeName == value) return;
                Entity.DocTypeName = value;
                RaisePropertyChanged();
            }
        }

        public string DocInfo
        {
            get => Entity.DocInfo;
            set
            {
                if (Entity.DocInfo == value) return;
                Entity.DocInfo = value;
                RaisePropertyChanged();
            }
        }

        public decimal DocDC
        {
            get => Entity.DocDC;
            set
            {
                if (Entity.DocDC == value) return;
                Entity.DocDC = value;
                RaisePropertyChanged();
            }
        }

        public Projects Projects
        {
            get => Entity.Projects;
            set
            {
                if (Entity.Projects == value) return;
                Entity.Projects = value;
                RaisePropertyChanged();
            }
        }

        public ProjectsDocs Entity
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

        public List<ProjectsDocs> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual ProjectsDocs Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual ProjectsDocs Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public ProjectsDocs Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public ProjectsDocs Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(ProjectsDocs doc)
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

        public void UpdateFrom(ProjectsDocs ent)
        {
            Id = ent.Id;
            ProjectId = ent.ProjectId;
            DocTypeName = ent.DocTypeName;
            DocInfo = ent.DocInfo;
            Note = ent.Note;
            DocDC = ent.DocDC;
            Projects = ent.Projects;
        }

        public void UpdateTo(ProjectsDocs ent)
        {
            ent.Id = Id;
            ent.ProjectId = ProjectId;
            ent.DocTypeName = DocTypeName;
            ent.DocInfo = DocInfo;
            ent.Note = Note;
            ent.DocDC = DocDC;
            ent.Projects = Projects;
        }

        public ProjectsDocs DefaultValue()
        {
            return new ProjectsDocs {Id = Guid.NewGuid()};
        }
    }
}