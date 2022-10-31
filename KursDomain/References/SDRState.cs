using KursDomain.ICommon;
using KursDomain.IReferences;
using System.Collections.Generic;
using System.Diagnostics;
using Data;

namespace KursDomain.References;
[DebuggerDisplay("{DocCode,nq} {Name,nq}")]
public class SDRState : ISDRState, IDocCode,  IName, IEqualityComparer<IDocCode>
{
    public string Shifr { get; set; }
    public decimal? ParentDC { get; set; }
    public bool IsDohod { get; set; }
    public decimal DocCode { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Статья дох/расх: {Name}";

    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_99 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        Shifr = entity.SZ_SHIFR;
        ParentDC = entity.SZ_PARENT_DC;
        IsDohod = entity.SZ_1DOHOD_0_RASHOD == 1;
        Name = entity.SZ_NAME;

    }

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
}
