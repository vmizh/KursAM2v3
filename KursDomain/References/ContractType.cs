using System.Collections.Generic;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class ContractType : IContractType, IDocCode, IName, IEqualityComparer<IDocCode>
{
    public bool IsBuy { get; set; }
    public bool IsAadditionalAgreement { get; set; }
    public bool IsDiler { get; set; }
    public decimal DocCode { get; set; }

    public bool Equals(IDocCode x, IDocCode y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (ReferenceEquals(x, null)) return false;
        if (ReferenceEquals(y, null)) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.DocCode == y.DocCode;
    }

    public int GetHashCode(IDocCode obj)
    {
        return obj.DocCode.GetHashCode();
    }

    public string Name { get; set; }
    public string Notes { get; set; }

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
}
