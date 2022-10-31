using System;
using System.Collections.Generic;
using System.Diagnostics;
using Data;
using KursDomain.ICommon;
using KursDomain.IReferences.Nomenkl;

namespace KursDomain.References;
[DebuggerDisplay("{DocCode,nq} {Name,nq} {ParentDC,nq}")]
public class NomenklCategory : IDocCode, IDocGuid, IName, INomenklCategory,  IEqualityComparer<IDocCode>
{
    public decimal DocCode { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Ном.категория: {Name}";
    public decimal? ParentDC { get; set; }
    public string PathName { get; set; }

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

    public void LoadFromEntity(SD_82 entity)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        Name = entity.CAT_NAME;
        PathName = entity.CAT_PATH_NAME;
        ParentDC = entity.CAT_PARENT_DC;
    }

    public override string ToString()
    {
        return Name;
    }
}
