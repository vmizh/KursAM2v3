using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq}/{Id} {Name,nq} {ParentDC,nq}")]
public class Region : IRegion, IDocCode, IDocGuid, IName, IEquatable<Region>, IComparable, ICache
{
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : string.Compare(Name, c.Name, StringComparison.Ordinal);
    }

    [Display(AutoGenerateField = false)] public decimal DocCode { get; set; }

    [Display(AutoGenerateField = false)] public Guid Id { get; set; }

    public bool Equals(Region other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    [Display(AutoGenerateField = true, Name = "Наименование")]
    public string Name { get; set; }

    [Display(AutoGenerateField = true, Name = "Примечание")]
    public string Notes { get; set; }

    [Display(AutoGenerateField = false)] 
    [JsonIgnore]
    public string Description => $"Регион: {Name}";

    [Display(AutoGenerateField = false)] public decimal? ParentDC { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_23 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        ParentDC = entity.REG_PARENT_DC;
        Name = entity.REG_NAME;
        UpdateDate = entity.UpdateDate ?? DateTime.Now;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Region)obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }

    public void LoadFromCache()
    {
        
    }
    [Display(AutoGenerateField = false, Name = "Посл.обновление")]
    public DateTime UpdateDate { get; set; }
}
