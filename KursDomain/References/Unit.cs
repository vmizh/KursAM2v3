using System.Collections.Generic;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class Unit : IUnit, IDocCode, IName, IEqualityComparer<IDocCode>
{
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
    public string Description => $"Ед. изм.: {Name}";
    public string OKEI { get; set; }
    public string OKEI_Code { get; set; }

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_175 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        Name = entity.ED_IZM_NAME;
        OKEI = entity.ED_IZM_OKEI;
        OKEI_Code = entity.ED_IZM_OKEI_CODE;
    }
}
