using KursDomain.ICommon;
using KursDomain.IReferences;
using System.Collections.Generic;
using System.Diagnostics;
using Data;

namespace KursDomain.References;
[DebuggerDisplay("{DocCode,nq}/{Id} {Name,nq}")]
public class SDRSchet :  ISDRSchet, IDocCode,  IName, IEqualityComparer<IDocCode>
{
    public bool IsDeleted { get; set; }
    public ISDRState SDRState { get; set; }
    public bool IsPodOtchet { get; set; }
    public bool IsEmployeePayment { get; set; }
    public decimal DocCode { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public string Description => $"Счет дох/расх: {Name}";
    public override string ToString()
    {
        return Name;
    }

    public void LoadFromEntity(SD_303 entity, IReferencesCache refCache)
    {
        if (entity == null)
        {
            DocCode = -1;
            return;
        }

        DocCode = entity.DOC_CODE;
        IsDeleted = entity.SHPZ_DELETED == 1;
        SDRState = refCache?.GetSDRState(entity.SHPZ_STATIA_DC);
        IsPodOtchet = entity.SHPZ_PODOTCHET == 1;
        IsEmployeePayment = entity.SHPZ_1ZARPLATA_0NO == 1;
        Name = entity.SHPZ_NAME;
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
