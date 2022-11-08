using System;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

/// <summary>
///     Тип взаимозачета
/// </summary>
[DebuggerDisplay("{DocCode,nq} {Name,nq} Валют.конверт.:{IsCurrencyConvert,nq}")]
public class MutualSettlementType : IMutualSettlementType, IDocCode, IName, IEquatable<MutualSettlementType>
{
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
}
