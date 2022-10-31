using System;
using KursDomain.ICommon;
using KursDomain.IReferences;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KursDomain.IReferences.Kontragent;
using System.Diagnostics;
using Data;

namespace KursDomain.References;

[DebuggerDisplay("{DocCode,nq} {Bank,nq} {RashAccount,nq}")]
public class BankAccount:  IBankAccount, IDocCode,  IName,  IEqualityComparer<IDocCode>
{
    [Display(AutoGenerateField = false)]
    public int RashAccCode { get; set; }
    [Display(AutoGenerateField = true, Name = "Счет №")]
    public string RashAccount { get; set; }
    [Display(AutoGenerateField = false)]
    public short BACurrency { get; set; }
    [Display(AutoGenerateField = true,Name = "Контрагент")]
    public IKontragent Kontragent { get; set; }
    [Display(AutoGenerateField = false)]
    public bool IsTransit { get; set; }
    [Display(AutoGenerateField = true,Name = "Банк")]
    public IBank Bank { get; set; }
    [Display(AutoGenerateField = true,Name = "Корр.счет")]
    public string CorrAccount => Bank?.CorrAccount;
    [Display(AutoGenerateField = true,Name = "БИК")]
    public string BIK => Bank?.BIK;
    [Display(AutoGenerateField = true,Name = "Отрицат.остатки")]
    public bool IsNegativeRests { get; set; }
    [Display(AutoGenerateField = false)]
    public short? BABankAccount { get; set; }
    [Display(AutoGenerateField = true,Name = "Короткое имя")]
    public string ShortName { get; set; }
    [Display(AutoGenerateField = true,Name = "Центр ответственности")]
    public ICentrResponsibility CentrResponsibility { get; set; }
    [Display(AutoGenerateField = true,Name = "Валюта")]
    public ICurrency Currency { get; set; }
    [Display(AutoGenerateField = true,Name = "Нач. учета")]
    public DateTime? StartDate { get; set; }
    [Display(AutoGenerateField = true,Name = "Нач. сумма")]
    [DisplayFormat(DataFormatString = "n2")]
    public decimal? StartSumma { get; set; }
    [Display(AutoGenerateField = true,Name = "Удален")]
    public bool IsDeleted { get; set; }
    [Display(AutoGenerateField = true,Name = "Дата не меньше 0")]
    public DateTime? DateNonZero { get; set; }
    [Display(AutoGenerateField = false)]
    public decimal DocCode { get; set; }
    [Display(AutoGenerateField = false)]
    public string Name
    {
        get => ShortName;
        set{} }
    [Display(AutoGenerateField = false)]
    public string Notes { get; set; }

    public string Description
    {
        get
        {
            var bik = string.IsNullOrWhiteSpace(Bank?.BIK) ? string.Empty : $"БИК: {Bank.BIK}";
            var corrAcc = string.IsNullOrWhiteSpace(Bank?.CorrAccount) ? string.Empty : $"Корр.счет: {Bank.CorrAccount}";
            return $"Расчетный счет: Банк: {Name} {bik} {corrAcc} {RashAccount}";
        }
    }

    public override string ToString()
    {
        return $"{ShortName} {RashAccount}";
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

    public void LoadFromEntity(SD_114 entity, IReferencesCache refCache)
    {
        RashAccCode = entity.BA_RASH_ACC_CODE;
        RashAccount = entity.BA_RASH_ACC;
        BACurrency = entity.BA_CURRENCY;
        Kontragent = refCache?.GetKontragent(entity.BA_BANK_AS_KONTRAGENT_DC);
        IsTransit = entity.BA_TRANSIT == 1;
        Bank = refCache?.GetBank(entity.BA_BANKDC);
        IsNegativeRests = entity.BA_NEGATIVE_RESTS == 1;
        BABankAccount = entity.BA_BANK_ACCOUNT;
        ShortName = entity.BA_ACC_SHORTNAME;
        Currency = refCache?.GetCurrency(entity.CurrencyDC);
        StartDate = entity.StartDate;
        StartSumma = entity.StartSumma;
        DateNonZero = entity.DateNonZero;
        DocCode = entity.DOC_CODE;

    }
}
