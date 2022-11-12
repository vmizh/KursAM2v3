using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{Id} {Name,nq} {ParentId,nq}")]
public class Project : IProject, IDocGuid, IName, IEquatable<Project>
{
    public Guid Id { get; set; }

    public bool Equals(Project other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Проект: {Name}";
    public bool IsDeleted { get; set; }
    public bool IsClosed { get; set; }
    public DateTime DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public IEmployee Employee { get; set; }
    public Guid? ParentId { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(Projects entity, IReferencesCache refCache)
    {
        if (entity == null)
        {
            Id = Guid.Empty;
            return;
        }

        Id = entity.Id;
        IsDeleted = entity.IsDeleted;
        IsClosed = entity.IsClosed;
        DateStart = entity.DateStart;
        DateEnd = entity.DateEnd;
        Employee = refCache?.GetEmployee(entity.ManagerDC);
        ParentId = entity.ParentId;
        Name = entity.Name;
        Notes = entity.Note;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Project) obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

[MetadataType(typeof(DataAnnotationsProjectsViewModel))]
// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class ProjectViewModel : RSViewModelBase, IEntity<Projects>
{
    private Projects myEntity;
    private Employee myResponsible;

    public ProjectViewModel()
    {
        Entity = DefaultValue();
        UpdateFrom(Entity);
    }

    public ProjectViewModel(Projects entity)
    {
        Entity = entity ?? DefaultValue();
        UpdateFrom(Entity);
    }

    public override Guid Id
    {
        set
        {
            if (Entity.Id == value) return;
            Entity.Id = value;
            RaisePropertyChanged();
        }
        get => Entity.Id;
    }

    public DateTime DateStart
    {
        get => Entity.DateStart;
        set
        {
            if (Entity.DateStart == value) return;
            Entity.DateStart = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? DateEnd
    {
        get => Entity.DateEnd;
        set
        {
            if (Entity.DateEnd == value) return;
            Entity.DateEnd = value;
            RaisePropertyChanged();
        }
    }

    public decimal? ManagerDC
    {
        get => Entity.ManagerDC;
        set
        {
            if (Entity.ManagerDC == value) return;
            Entity.ManagerDC = value;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<ProjectsDocs> ProjectsDocs { get; set; }

    /// <summary>
    ///     Ответственный
    /// </summary>
    public Employee Responsible
    {
        get => myResponsible;
        set
        {
            if (myResponsible != null && myResponsible.Equals(value)) return;
            myResponsible = value;
            ManagerDC = myResponsible?.DocCode;
            RaisePropertyChanged();
        }
    }

    public bool IsClosed
    {
        get => Entity.IsClosed;
        set
        {
            if (Entity.IsClosed == value) return;
            Entity.IsClosed = value;
            RaisePropertyChanged();
        }
    }

    public bool IsDeleted
    {
        get => Entity.IsDeleted;
        set
        {
            if (Entity.IsDeleted == value) return;
            Entity.IsDeleted = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; } = new EntityLoadCodition();

    public bool IsAccessRight { get; set; }

    public Projects Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public Projects DefaultValue()
    {
        return new Projects
        {
            Id = Guid.NewGuid(),
            ProjectsDocs = new List<ProjectsDocs>()
        };
    }

    public bool Check()
    {
        return !string.IsNullOrWhiteSpace(Name) && Id != Guid.Empty && DateStart != DateTime.MinValue;
    }

    public List<Projects> LoadList()
    {
        var list = new List<Projects>();
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                list.AddRange(LoadCondition.IsShort
                    ? ctx.Projects.ToList()
                    : ctx.Projects.Include(_ => _.ProjectsDocs).ToList());
                return list;
            }
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(ex);
        }

        return null;
    }

    public virtual Projects Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual Projects Load(Guid id)
    {
        Projects prj = null;
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (LoadCondition.IsShort)
                    prj = ctx.Projects
                        .FirstOrDefault(_ => _.Id == id);
                else
                    prj = ctx.Projects
                        .Include(_ => _.ProjectsDocs)
                        .FirstOrDefault(_ => _.Id == id);
                if (prj != null) UpdateFrom(prj);
                return prj;
            }
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(ex);
        }

        return prj;
    }

    public virtual void Save(ProjectViewModel doc)
    {
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                switch (State)
                {
                    case RowStatus.NewRow:
                        ctx.Projects.Add(new Projects
                        {
                            Id = Guid.NewGuid(),
                            ParentId = doc.ParentId,
                            Name = doc.Name,
                            Note = doc.Note,
                            DateEnd = doc.DateEnd,
                            DateStart = doc.DateStart,
                            IsClosed = doc.IsClosed,
                            IsDeleted = doc.IsDeleted,
                            ManagerDC = doc.ManagerDC
                        });
                        ctx.SaveChanges();
                        break;
                    case RowStatus.Edited:
                        var entity = ctx.Projects.FirstOrDefault(_ => _.Id == Id);
                        if (entity != null)
                        {
                            entity.ParentId = doc.ParentId;
                            entity.Name = doc.Name;
                            entity.Note = doc.Note;
                            entity.DateEnd = doc.DateEnd;
                            entity.DateStart = doc.DateStart;
                            entity.IsClosed = doc.IsClosed;
                            entity.IsDeleted = doc.IsDeleted;
                            entity.ManagerDC = doc.ManagerDC;
                        }

                        ctx.SaveChanges();
                        break;
                }
            }

            State = RowStatus.NotEdited;
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(ex);
        }
    }

    public virtual void Save(Projects doc)
    {
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                switch (State)
                {
                    case RowStatus.NewRow:
                        var newPrj = new Projects
                        {
                            Id = Guid.NewGuid(),
                            ParentId = doc.ParentId,
                            Name = doc.Name,
                            Note = doc.Note,
                            DateEnd = doc.DateEnd,
                            DateStart = doc.DateStart,
                            IsClosed = doc.IsClosed,
                            IsDeleted = doc.IsDeleted,
                            ManagerDC = doc.ManagerDC
                        };
                        ctx.Projects.Add(newPrj);
                        ctx.SaveChanges();
                        break;
                    case RowStatus.Edited:
                        var entity = ctx.Projects.FirstOrDefault(_ => _.Id == Id);
                        if (entity != null)
                        {
                            entity.ParentId = doc.ParentId;
                            entity.Name = doc.Name;
                            entity.Note = doc.Note;
                            entity.DateEnd = doc.DateEnd;
                            entity.DateStart = doc.DateStart;
                            entity.IsClosed = doc.IsClosed;
                            entity.IsDeleted = doc.IsDeleted;
                            entity.ManagerDC = doc.ManagerDC;
                        }

                        ctx.SaveChanges();
                        // ReSharper disable once PossibleNullReferenceException
                        break;
                }
            }

            State = RowStatus.NotEdited;
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(ex);
        }
    }

    public void Save()
    {
        Save(Entity);
    }

    public void Delete()
    {
        Delete(Id);
    }

    public void Delete(Guid id)
    {
        try
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var prj = ctx.Projects
                    .FirstOrDefault(_ => _.Id == id);
                if (prj == null) return;
                ctx.Projects.Remove(prj);
                ctx.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            //WindowManager.ShowError(ex);
        }
    }

    public void Delete(decimal dc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(Projects ent)
    {
        Id = ent.Id;
        ParentId = ent.ParentId;
        Name = ent.Name;
        Note = ent.Note;
        IsDeleted = ent.IsDeleted;
        IsClosed = ent.IsClosed;
        DateStart = ent.DateStart;
        DateEnd = ent.DateEnd;
        ManagerDC = ent.ManagerDC;
        Responsible = GlobalOptions.ReferencesCache.GetEmployee(ManagerDC) as Employee;
        if (ent.ProjectsDocs != null)
            ProjectsDocs = new ObservableCollection<ProjectsDocs>(ent.ProjectsDocs);
    }

    public void UpdateTo(Projects ent)
    {
        ent.Id = Id;
        ent.ParentId = ParentId;
        ent.Name = Name;
        ent.Note = Note;
        ent.IsDeleted = IsDeleted;
        ent.IsClosed = IsClosed;
        ent.DateStart = DateStart;
        ent.DateEnd = DateEnd;
        ent.ManagerDC = ManagerDC;
        ent.ProjectsDocs = ProjectsDocs;
    }
}

public class DataAnnotationsProjectsViewModel : DataAnnotationForFluentApiBase, IMetadataProvider<ProjectViewModel>
{
    void IMetadataProvider<ProjectViewModel>.BuildMetadata(MetadataBuilder<ProjectViewModel> builder)
    {
        SetNotAutoGenerated(builder);

        //builder.Property(_ => _.Entity).NotAutoGenerated();
        builder.Property(_ => _.Name).AutoGenerated().DisplayName("Наименование").Required();
        builder.Property(_ => _.DateEnd).AutoGenerated().DisplayName("Дата окончания");
        builder.Property(_ => _.DateStart).AutoGenerated().DisplayName("Дата начала").Required();
        builder.Property(_ => _.IsClosed).AutoGenerated().DisplayName("Закрыт");
        builder.Property(_ => _.IsDeleted).AutoGenerated().DisplayName("Удален");
        builder.Property(_ => _.Responsible).AutoGenerated().DisplayName("Ответственный");
        builder.Group("Проект")
            .ContainsProperty(_ => _.Name)
            .ContainsProperty(_ => _.DateEnd)
            .ContainsProperty(_ => _.DateStart)
            .ContainsProperty(_ => _.IsClosed)
            .ContainsProperty(_ => _.IsDeleted)
            .ContainsProperty(_ => _.Responsible).EndGroup();
    }
}
