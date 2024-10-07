using System;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;
using KursDomain.References.RedisCache;
using Newtonsoft.Json;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class ContractType : IContractType, IDocCode, IName, IEquatable<ContractType>, IComparable, ICache
{
    public ContractType()
    {
        LoadFromCache();
    }
    public int CompareTo(object obj)
    {
        var c = obj as Unit;
        return c == null ? 0 : String.Compare(Name, c.Name, StringComparison.Ordinal);
    }
    public bool IsBuy { get; set; }
    public bool IsAadditionalAgreement { get; set; }
    public bool IsDiler { get; set; }
    public decimal DocCode { get; set; }

    public bool Equals(ContractType other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return DocCode == other.DocCode;
    }

    public string Name { get; set; }
    public string Notes { get; set; }
    [JsonIgnore]
    public string Description
    {
        get
        {
            var typeDog = IsBuy ? "закупка" : "продажа";
            return $"Тип договора: {Name} ({typeDog})";
        }
    }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_102 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        Name = entity.TD_NAME;
        IsAadditionalAgreement = entity.TD_DOP_SOGL == 1;
        IsBuy = entity.TD_0BUY_1SALE == 0;
        IsDiler = entity.TD_DILER == 1;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ContractType) obj);
    }

    public override int GetHashCode()
    {
        return DocCode.GetHashCode();
    }

    public void LoadFromCache()
    {
        
    }
}
