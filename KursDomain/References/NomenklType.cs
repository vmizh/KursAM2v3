using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences.Nomenkl;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class NomenklType : IDocCode, IName, INomenklType, IEquatable<NomenklType>, IComparable, ICache
{
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    public decimal DocCode { get; set; }

    public bool Equals(NomenklType other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    [JsonIgnore]
    public string Description => $"Тип продукции: {Name}";
    public bool IsDeleted { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_119 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        Name = entity.MC_NAME;
        IsDeleted = entity.MC_DELETED == 1;
        UpdateDate = entity.UpdateDate ?? DateTime.Now;
        
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((NomenklType) obj);
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
