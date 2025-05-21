using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

[DebuggerDisplay("{Id} {Name,nq}")]
public class Project : IProject, IDocGuid, IName, IEquatable<Project>, IComparable, ICache
{
    [Display(AutoGenerateField = false)]
    public decimal? ManagerDC { set; get; }

    public void LoadFromCache()
    {
        if (GlobalOptions.ReferencesCache is not RedisCacheReferences cache) return;
        if (ManagerDC is not null)
            Employee = cache.GetEmployee(ManagerDC.Value);
    }

    [Display(AutoGenerateField = false, Name = "Посл.обновление")]
    public DateTime UpdateDate { get; set; }

    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : string.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    [Display(AutoGenerateField = false)]
    public Guid Id { get; set; }

    public bool Equals(Project other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    [Display(AutoGenerateField = true,Name = "Наименование")]
    public string Name { get; set; }
    [Display(AutoGenerateField = true,Name = "Примечание")]
    public string Notes { get; set; }

    [Display(AutoGenerateField = false,Name = "Описание")]
    [JsonIgnore] public string Description => $"Проект: {Name}";

    [Display(AutoGenerateField = true,Name = "Удален")]
    public bool IsDeleted { get; set; }
    [Display(AutoGenerateField = true,Name = "Закрыт")]
    public bool IsClosed { get; set; }
    [Display(AutoGenerateField = true,Name = "Начало")]
    public DateTime DateStart { get; set; }
    [Display(AutoGenerateField = true,Name = "Окончание")]
    public DateTime? DateEnd { get; set; }

    [Display(AutoGenerateField = true,Name = "Ответственный")]
    [JsonIgnore] public IEmployee Employee { get; set; }

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
        UpdateDate = entity.UpdateDate ?? DateTime.Now;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Project)obj);
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

    public override Guid? ParentId
    {
        set
        {
            if (Entity.ParentId == value) return;
            Entity.ParentId = value;
            RaisePropertyChanged();
        }
        get => Entity.ParentId;
    }

    public override string Name
    {
        set
        {
            if (Entity.Name == value) return;
            Entity.Name = value;
            RaisePropertyChanged();
        }
        get => Entity.Name;
    }

    public override string Note
    {
        set
        {
            if (Entity.Note == value) return;
            Entity.Note = value;
            RaisePropertyChanged();
        }
        get => Entity.Note;
    }

    public DateTime DateStart
    {
        get => Entity.DateStart;
        set
        {
            if (Entity.DateStart == value) return;
            if (DateEnd is not null && value > DateEnd)
                Entity.DateStart = DateEnd.Value;
            else
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
            if (value < DateStart)
                Entity.DateEnd = DateStart;
            else
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

    //public ObservableCollection<ProjectsDocs> ProjectsDocs { get; set; }

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
            Id = Guid.NewGuid()
        };
    }

    public bool Check()
    {
        //TODO Убрать перед коммитом 
        if ((!string.IsNullOrWhiteSpace(Name) && Id != Guid.Empty && DateStart != DateTime.MinValue &&
             DateStart <= (DateEnd ?? DateTime.MaxValue)) == false)
        {
            var i = 1;
        }

        return !string.IsNullOrWhiteSpace(Name) && Id != Guid.Empty && DateStart != DateTime.MinValue &&
               DateStart <= (DateEnd ?? DateTime.MaxValue);
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
        builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
        builder.Group("Проект")
            .ContainsProperty(_ => _.Name)
            .ContainsProperty(_ => _.DateEnd)
            .ContainsProperty(_ => _.DateStart)
            .ContainsProperty(_ => _.IsClosed)
            .ContainsProperty(_ => _.IsDeleted)
            .ContainsProperty(_ => _.Responsible).EndGroup();
    }
}
