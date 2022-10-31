using KursDomain.ICommon;
using KursDomain.IReferences;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Data;

namespace KursDomain.References;

/// <summary>
/// Банк основные реквизиты
/// </summary>
[DebuggerDisplay("{DocCode,nq}/{Id} {Name,nq}")]
public class Bank :  IBank, IDocCode,  IName, IEqualityComparer<IDocCode>
{
    [Display(AutoGenerateField = true,Name = "Корр.счет")]
    [MaxLength(30)]
    public string CorrAccount { get; set; }
    [MaxLength(10)]
    [Display(AutoGenerateField = true,Name = "БИК")]
    public string BIK { get; set; }
    [Display(AutoGenerateField = true,Name = "Адрес")]
    public string Address { get; set; }
    [Display(AutoGenerateField = true,Name = "Короткое имя")]
    public string NickName { get; set; }
    [Display(AutoGenerateField = false,Name = "Ид")]
    public decimal DocCode { get; set; }
    [Display(AutoGenerateField = true,Name = "Наименование")]
    public string Name { get; set; }
    [Display(AutoGenerateField = true,Name = "Примечание")]
    public string Notes { get; set; }
    [Display(AutoGenerateField = false,Name = "Описание")]
    public string Description => $"Банк: {Name}";

    public override string ToString()
    {
        return Name;
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

    public void LoadFromEntity(SD_44 entity)
    {
        if (entity == null)
        {
            DocCode = 1;
            return;
        }
        DocCode = entity.DOC_CODE;
        Name = entity.BANK_NAME;
        Address = entity.ADDRESS;
        CorrAccount = entity.CORRESP_ACC;
        BIK = entity.POST_CODE;
        NickName = string.IsNullOrWhiteSpace(entity.BANK_NICKNAME) ? entity.BANK_NAME : entity.BANK_NICKNAME;

    }
}
