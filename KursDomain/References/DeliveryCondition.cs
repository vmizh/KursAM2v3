using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class DeliveryCondition : IDeliveryCondition, IDocCode, IName, IEquatable<DeliveryCondition>, IComparable, ICache
{
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    public decimal DocCode { get; set; }

    public bool Equals(DeliveryCondition other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    [JsonIgnore]
    public string Description => $"Условие доставки: {Name}";

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_103 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        Name = entity.BUP_NAME;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DeliveryCondition) obj);
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
