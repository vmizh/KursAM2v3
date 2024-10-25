using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

/// <summary>
///     Тип взаимозачета
/// </summary>
[DebuggerDisplay("{DocCode,nq} {Name,nq} Валют.конверт.:{IsCurrencyConvert,nq}")]
public class MutualSettlementType : IMutualSettlementType, IDocCode, IName, IEquatable<MutualSettlementType>, IComparable, ICache
{
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    public decimal DocCode { get; set; }

    public bool Equals(MutualSettlementType other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    public bool IsCurrencyConvert { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    [JsonIgnore]
    public string Description => $"Тип взаимозачета: {Name}";

    public void LoadFromEntity(SD_111 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        IsCurrencyConvert = entity.IsCurrencyConvert;
        Name = entity.ZACH_NAME;
        UpdateDate = entity.UpdateDate ?? DateTime.Now;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((MutualSettlementType) obj);
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
