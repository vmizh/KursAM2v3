using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Controls;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Xpf.Editors.Settings;
using KursDomain.Documents.StockHolder;
using KursDomain.ICommon;
using KursDomain.References;
using Newtonsoft.Json;

namespace KursDomain.Documents.Cash;

[MetadataType(typeof(SD_34LayoutData_FluentAPI))]

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class CashOut : RSViewModelBase, IEntity<SD_34>
{
    private string myAccuredInfo;
    private BankAccount myBankAccount;
    private CashBox myCash;
    private CashBox myCashTo;
    private References.Currency myCurrency;
    private References.Employee myEmployee;
    private SD_34 myEntity;
    private bool myIsKontrSelectEnable;
    private CashKontragentType myKontragentType;
    private SDRSchet mySDRSchet;
    private string mySPostName;
    private StockHolderViewModel myStockHolder;

    public CashOut()
    {
        Entity = DefaultValue();
        LoadReferences();
    }

    public CashOut(SD_34 entity)
    {
        Entity = entity ?? DefaultValue();
        LoadReferences();
    }

    public override string Description =>
        $"Расходный кассовый ордер №{NUM_ORD} от {DATE_ORD?.ToShortDateString()} Касса: {Cash} " +
        $" {Kontragent} ({KontragentType.GetDisplayAttributesFrom(typeof(CashKontragentType)).Name}) сумма {CRS_SUMMA} {Currency} {NOTES_ORD}";

    public AccuredAmountOfSupplierRow AccuredAmountOfSupplier
    {
        get => Entity.AccuredAmountOfSupplierRow;
        set
        {
            if (Entity.AccuredAmountOfSupplierRow == value) return;
            Entity.AccuredAmountOfSupplierRow = value;
            RaisePropertyChanged();
        }
    }

    public override string Note
    {
        get => Entity.NOTES_ORD;
        set
        {
            if (Entity.NOTES_ORD == value) return;
            Entity.NOTES_ORD = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = true)]
    public string AccuredInfo
    {
        get => myAccuredInfo;
        set
        {
            if (myAccuredInfo == value) return;
            myAccuredInfo = value;
            RaisePropertyChanged();
        }
    }

    public override decimal DocCode
    {
        get => Entity.DOC_CODE;
        set
        {
            if (Entity.DOC_CODE == value) return;
            Entity.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public int? NUM_ORD
    {
        get => Entity.NUM_ORD;
        set
        {
            if (Entity.NUM_ORD == value) return;
            Entity.NUM_ORD = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? DATE_ORD
    {
        get => Entity.DATE_ORD;
        set
        {
            if (Entity.DATE_ORD == value) return;
            Entity.DATE_ORD = value;
            RaisePropertyChanged();
        }
    }

    public decimal? SUMM_ORD
    {
        get => Entity.SUMM_ORD;
        set
        {
            if (Entity.SUMM_ORD == value) return;
            Entity.SUMM_ORD = value;
            if (!IsBackCalc)
                CRS_SUMMA = Math.Round(
                    (SUMM_ORD ?? 0) * 100 / (100 - (decimal)(KONTR_CRS_SUM_CORRECT_PERCENT ?? 0)), 2);
            else
                SUMM_ORD = Math.Round(
                    (CRS_SUMMA ?? 0) + (CRS_SUMMA ?? 0) * (decimal)(KONTR_CRS_SUM_CORRECT_PERCENT ?? 0) / 100, 2);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(CRS_SUMMA));
        }
    }

    public string DOCUM_ORD
    {
        get => Entity.DOCUM_ORD;
        set
        {
            if (Entity.DOCUM_ORD == value) return;
            Entity.DOCUM_ORD = value;
            RaisePropertyChanged();
        }
    }

    public string OSN_ORD
    {
        get => Entity.OSN_ORD;
        set
        {
            if (Entity.OSN_ORD == value) return;
            Entity.OSN_ORD = value;
            RaisePropertyChanged();
        }
    }

    public string NOTES_ORD
    {
        get => Entity.NOTES_ORD;
        set
        {
            if (Entity.NOTES_ORD == value) return;
            Entity.NOTES_ORD = value;
            RaisePropertyChanged();
        }
    }

    public string NAME_ORD
    {
        get => Entity.NAME_ORD;
        set
        {
            if (Entity.NAME_ORD == value) return;
            Entity.NAME_ORD = value;
            RaisePropertyChanged();
        }
    }

    public Guid? AccuredId
    {
        get => Entity.AccuredId;
        set
        {
            if (Entity.AccuredId == value) return;
            Entity.AccuredId = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CODE_CASS
    {
        get => Entity.CODE_CASS;
        set
        {
            if (Entity.CODE_CASS == value) return;
            Entity.CODE_CASS = value;
            RaisePropertyChanged();
        }
    }

    public int? TABELNUMBER
    {
        get => Entity.TABELNUMBER;
        set
        {
            if (Entity.TABELNUMBER == value) return;
            Entity.TABELNUMBER = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Kontragent));
        }
    }

    public References.Employee Employee
    {
        get => myEmployee;
        set
        {
            if (myEmployee != null && myEmployee.Equals(value)) return;
            myEmployee = value;
            TABELNUMBER = myEmployee?.TabelNumber;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Kontragent));
        }
    }

    public StockHolderViewModel StockHolder
    {
        get => myStockHolder;
        set
        {
            if (myStockHolder == value) return;
            myStockHolder = value;
            Entity.StockHolderId = myStockHolder?.Id;
            RaisePropertyChanged();
        }
    }

    public short? OP_CODE
    {
        get => Entity.OP_CODE;
        set
        {
            if (Entity.OP_CODE == value) return;
            Entity.OP_CODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CA_DC
    {
        get => Entity.CA_DC;
        set
        {
            if (Entity.CA_DC == value) return;
            Entity.CA_DC = value;
            RaisePropertyChanged();
        }
    }

    public CashBox Cash
    {
        get => myCash;
        set
        {
            if (myCash != null && myCash.Equals(value)) return;
            myCash = value;
            CA_DC = myCash?.DocCode;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Kontragent));
        }
    }

    public CashKontragentType KontragentType
    {
        get => myKontragentType;
        set
        {
            if (myKontragentType == value) return;
            myKontragentType = value;
            KONTRAGENT_DC = null;
            CashTo = null;
            BankAccount = null;
            Employee = null;
            IsKontrSelectEnable = myKontragentType != CashKontragentType.NotChoice;
            RaisePropertyChanged();
        }
    }

    public bool IsSummaEnabled => (OBRATNY_RASCHET ?? 0) == 0;
    public bool IsKontrSummaEnabled => (OBRATNY_RASCHET ?? 0) == 1;

    public bool IsKontrSelectEnable
    {
        get => myIsKontrSelectEnable;
        set
        {
            if (myIsKontrSelectEnable == value) return;
            myIsKontrSelectEnable = value;
            RaisePropertyChanged();
        }
    }

    public string Kontragent
    {
        get
        {
            return KontragentType switch
            {
                CashKontragentType.Bank => ((IName)GlobalOptions.ReferencesCache.GetBankAccount(BANK_RASCH_SCHET_DC))
                    ?.Name,
                CashKontragentType.Kontragent => KONTRAGENT_DC != null ? ((IName)GlobalOptions.ReferencesCache.GetKontragent(KONTRAGENT_DC))
                    .Name : null,
                CashKontragentType.Cash => CASH_TO_DC != null ? CashTo.Name : null,
                CashKontragentType.Employee => TABELNUMBER != null ? Employee?.Name : null,
                CashKontragentType.StockHolder => StockHolder?.Name,
                _ => null
            };
        }
    }

    public decimal? KONTRAGENT_DC
    {
        get => Entity.KONTRAGENT_DC;
        set
        {
            if (Entity.KONTRAGENT_DC == value) return;
            Entity.KONTRAGENT_DC = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Kontragent));
        }
    }

    public string SHPZ_ORD
    {
        get => Entity.SHPZ_ORD;
        set
        {
            if (Entity.SHPZ_ORD == value) return;
            Entity.SHPZ_ORD = value;
            RaisePropertyChanged();
        }
    }

    public decimal? SHPZ_DC
    {
        get => Entity.SHPZ_DC;
        set
        {
            if (Entity.SHPZ_DC == value) return;
            Entity.SHPZ_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CASH_TO_DC
    {
        get => Entity.CASH_TO_DC;
        set
        {
            if (Entity.CASH_TO_DC == value) return;
            Entity.CASH_TO_DC = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Kontragent));
        }
    }

    public SDRSchet SDRSchet
    {
        get => mySDRSchet;
        set
        {
            if (Equals(mySDRSchet, value)) return;
            mySDRSchet = value;
            SHPZ_DC = mySDRSchet?.DocCode;
            RaisePropertyChanged();
        }
    }

    public CashBox CashTo
    {
        get => myCashTo;
        set
        {
            if (myCashTo == value) return;
            myCashTo = value;
            CASH_TO_DC = myCashTo?.DocCode;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Kontragent));
        }
    }

    public decimal? SPOST_DC
    {
        get => Entity.SPOST_DC;
        set
        {
            if (Entity.SPOST_DC == value) return;
            Entity.SPOST_DC = value;
            if (Entity.SPOST_DC == null) SPostName = null;
            RaisePropertyChanged();
        }
    }

    public short? NCODE
    {
        get => Entity.NCODE;
        set
        {
            if (Entity.NCODE == value) return;
            Entity.NCODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? KONTR_CRS_DC
    {
        get => Entity.KONTR_CRS_DC;
        set
        {
            if (Entity.KONTR_CRS_DC == value) return;
            Entity.KONTR_CRS_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CRS_KOEF
    {
        get => Entity.CRS_KOEF;
        set
        {
            if (Entity.CRS_KOEF == value) return;
            Entity.CRS_KOEF = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CRS_SUMMA
    {
        get => Entity.CRS_SUMMA;
        set
        {
            if (Entity.CRS_SUMMA == value) return;
            Entity.CRS_SUMMA = value;
            if (!IsBackCalc)
                CRS_SUMMA = Math.Round(
                    (SUMM_ORD ?? 0) * 100 / (100 - (decimal)(KONTR_CRS_SUM_CORRECT_PERCENT ?? 0)), 2);
            else
                SUMM_ORD = Math.Round(
                    (CRS_SUMMA ?? 0) + (CRS_SUMMA ?? 0) * (decimal)(KONTR_CRS_SUM_CORRECT_PERCENT ?? 0) / 100, 2);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(SUMM_ORD));
            RaisePropertyChanged();
        }
    }

    public byte? CHANGE_ORD
    {
        get => Entity.CHANGE_ORD;
        set
        {
            if (Entity.CHANGE_ORD == value) return;
            Entity.CHANGE_ORD = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CRS_DC
    {
        get => Entity.CRS_DC;
        set
        {
            if (Entity.CRS_DC == value) return;
            Entity.CRS_DC = value;
            RaisePropertyChanged();
        }
    }

    public References.Currency Currency
    {
        get => myCurrency;
        set
        {
            if (Equals(myCurrency, value)) return;
            myCurrency = value;
            CRS_DC = myCurrency?.DocCode;
            if (myCurrency != null)
                // ReSharper disable once PossibleInvalidOperationException
                UCH_VALUTA_RATE = (double?)CurrencyRate.GetCBRate(myCurrency, (DateTime)DATE_ORD);
            RaisePropertyChanged();
        }
    }

    public decimal? UCH_VALUTA_DC
    {
        get => Entity.UCH_VALUTA_DC;
        set
        {
            if (Entity.UCH_VALUTA_DC == value) return;
            Entity.UCH_VALUTA_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? SUMMA_V_UCH_VALUTE
    {
        get => Entity.SUMMA_V_UCH_VALUTE;
        set
        {
            if (Entity.SUMMA_V_UCH_VALUTE == value) return;
            Entity.SUMMA_V_UCH_VALUTE = value;
            RaisePropertyChanged();
        }
    }

    public double? UCH_VALUTA_RATE
    {
        get => Entity.UCH_VALUTA_RATE;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.UCH_VALUTA_RATE == value) return;
            Entity.UCH_VALUTA_RATE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? SPOST_OPLACHENO
    {
        get => Entity.SPOST_OPLACHENO;
        set
        {
            if (Entity.SPOST_OPLACHENO == value) return;
            Entity.SPOST_OPLACHENO = value;
            RaisePropertyChanged();
        }
    }

    public decimal? SPOST_CRS_DC
    {
        get => Entity.SPOST_CRS_DC;
        set
        {
            if (Entity.SPOST_CRS_DC == value) return;
            Entity.SPOST_CRS_DC = value;
            RaisePropertyChanged();
        }
    }

    public double? SPOST_CRS_RATE
    {
        get => Entity.SPOST_CRS_RATE;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.SPOST_CRS_RATE == value) return;
            Entity.SPOST_CRS_RATE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? BANK_RASCH_SCHET_DC
    {
        get => Entity.BANK_RASCH_SCHET_DC;
        set
        {
            if (Entity.BANK_RASCH_SCHET_DC == value) return;
            Entity.BANK_RASCH_SCHET_DC = value;
            myBankAccount = GlobalOptions.ReferencesCache.GetBankAccount(Entity.BANK_RASCH_SCHET_DC) as BankAccount;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(BankAccount));
            RaisePropertyChanged(nameof(Kontragent));
        }
    }

    public BankAccount BankAccount
    {
        get => myBankAccount;
        set
        {
            if (myBankAccount != null && myBankAccount.Equals(value)) return;
            myBankAccount = value;
            BANK_RASCH_SCHET_DC = myBankAccount?.DocCode;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Kontragent));
        }
    }

    public string CREATOR
    {
        get => Entity.CREATOR;
        set
        {
            if (Entity.CREATOR == value) return;
            Entity.CREATOR = value;
            RaisePropertyChanged();
        }
    }

    public decimal? RUB_SUMMA
    {
        get => Entity.RUB_SUMMA;
        set
        {
            if (Entity.RUB_SUMMA == value) return;
            Entity.RUB_SUMMA = value;
            RaisePropertyChanged();
        }
    }

    public double? RUB_RATE
    {
        get => Entity.RUB_RATE;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.RUB_RATE == value) return;
            Entity.RUB_RATE = value;
            RaisePropertyChanged();
        }
    }

    public double? KONTR_CRS_SUM_CORRECT_PERCENT
    {
        get => Entity.KONTR_CRS_SUM_CORRECT_PERCENT;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.KONTR_CRS_SUM_CORRECT_PERCENT == value) return;
            Entity.KONTR_CRS_SUM_CORRECT_PERCENT = value;
            RaisePropertyChanged();
        }
    }

    public bool IsBackCalc
    {
        get => Entity.OBRATNY_RASCHET == 1;
        set
        {
            if (OBRATNY_RASCHET == (value ? 1 : 0)) return;
            OBRATNY_RASCHET = value ? (short?)1 : (short?)0;
            if (!IsBackCalc)
                CRS_SUMMA = Math.Round(
                    (SUMM_ORD ?? 0) * 100 / (100 - (decimal)(KONTR_CRS_SUM_CORRECT_PERCENT ?? 0)), 2);
            else
                SUMM_ORD = Math.Round(
                    (CRS_SUMMA ?? 0) + (CRS_SUMMA ?? 0) * (decimal)(KONTR_CRS_SUM_CORRECT_PERCENT ?? 0) / 100, 2);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(IsKontrSummaEnabled));
            RaisePropertyChanged(nameof(IsSummaEnabled));
        }
    }

    public decimal? PercentForKontragent
    {
        get => Convert.ToDecimal(Entity.KONTR_CRS_SUM_CORRECT_PERCENT);
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.KONTR_CRS_SUM_CORRECT_PERCENT == Convert.ToDouble(value)) return;
            Entity.KONTR_CRS_SUM_CORRECT_PERCENT = Convert.ToDouble(value);
            if (!IsBackCalc)
                CRS_SUMMA = Math.Round(
                    (SUMM_ORD ?? 0) * 100 / (100 - (decimal)(KONTR_CRS_SUM_CORRECT_PERCENT ?? 0)), 2);
            else
                SUMM_ORD = Math.Round(
                    (CRS_SUMMA ?? 0) + (CRS_SUMMA ?? 0) * (decimal)(KONTR_CRS_SUM_CORRECT_PERCENT ?? 0) / 100, 2);
            RaisePropertyChanged();
        }
    }

    public short? OBRATNY_RASCHET
    {
        get => Entity.OBRATNY_RASCHET;
        set
        {
            if (Entity.OBRATNY_RASCHET == value) return;
            Entity.OBRATNY_RASCHET = value;
            RaisePropertyChanged();
        }
    }

    public string V_TOM_CHISLE
    {
        get => Entity.V_TOM_CHISLE;
        set
        {
            if (Entity.V_TOM_CHISLE == value) return;
            Entity.V_TOM_CHISLE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? PAY_ROLL_DC
    {
        get => Entity.PAY_ROLL_DC;
        set
        {
            if (Entity.PAY_ROLL_DC == value) return;
            Entity.PAY_ROLL_DC = value;
            RaisePropertyChanged();
        }
    }

    public int? PAY_ROLL_ROW_CODE
    {
        get => Entity.PAY_ROLL_ROW_CODE;
        set
        {
            if (Entity.PAY_ROLL_ROW_CODE == value) return;
            Entity.PAY_ROLL_ROW_CODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? KONTR_FROM_DC
    {
        get => Entity.KONTR_FROM_DC;
        set
        {
            if (Entity.KONTR_FROM_DC == value) return;
            Entity.KONTR_FROM_DC = value;
            RaisePropertyChanged();
        }
    }

    public int? SFACT_FLAG
    {
        get => Entity.SFACT_FLAG;
        set
        {
            if (Entity.SFACT_FLAG == value) return;
            Entity.SFACT_FLAG = value;
            RaisePropertyChanged();
        }
    }

    public byte[] TSTAMP
    {
        get => Entity.TSTAMP;
        set
        {
            if (Entity.TSTAMP == value) return;
            Entity.TSTAMP = value;
            RaisePropertyChanged();
        }
    }

    public decimal? PLAT_VED_DC
    {
        get => Entity.PLAT_VED_DC;
        set
        {
            if (Entity.PLAT_VED_DC == value) return;
            Entity.PLAT_VED_DC = value;
            RaisePropertyChanged();
        }
    }

    public int? PLAT_VED_ROW_CODE
    {
        get => Entity.PLAT_VED_ROW_CODE;
        set
        {
            if (Entity.PLAT_VED_ROW_CODE == value) return;
            Entity.PLAT_VED_ROW_CODE = value;
            RaisePropertyChanged();
        }
    }

    public SD_114 SD_114
    {
        get => Entity.SD_114;
        set
        {
            if (Entity.SD_114 == value) return;
            Entity.SD_114 = value;
            RaisePropertyChanged();
        }
    }

    public SD_2 SD_2
    {
        get => Entity.SD_2;
        set
        {
            if (Entity.SD_2 == value) return;
            Entity.SD_2 = value;
            RaisePropertyChanged();
        }
    }

    public SD_22 SD_22
    {
        get => Entity.SD_22;
        set
        {
            if (Entity.SD_22 == value) return;
            Entity.SD_22 = value;
            RaisePropertyChanged();
        }
    }

    public SD_22 SD_221
    {
        get => Entity.SD_221;
        set
        {
            if (Entity.SD_221 == value) return;
            Entity.SD_221 = value;
            RaisePropertyChanged();
        }
    }

    public SD_26 SD_26
    {
        get => Entity.SD_26;
        set
        {
            if (Entity.SD_26 == value) return;
            Entity.SD_26 = value;
            RaisePropertyChanged();
        }
    }

    public SD_301 SD_301
    {
        get => Entity.SD_301;
        set
        {
            if (Entity.SD_301 == value) return;
            Entity.SD_301 = value;
            RaisePropertyChanged();
        }
    }

    public SD_301 SD_3011
    {
        get => Entity.SD_3011;
        set
        {
            if (Entity.SD_3011 == value) return;
            Entity.SD_3011 = value;
            RaisePropertyChanged();
        }
    }

    public SD_301 SD_3012
    {
        get => Entity.SD_3012;
        set
        {
            if (Entity.SD_3012 == value) return;
            Entity.SD_3012 = value;
            RaisePropertyChanged();
        }
    }

    public SD_301 SD_3013
    {
        get => Entity.SD_3013;
        set
        {
            if (Entity.SD_3013 == value) return;
            Entity.SD_3013 = value;
            RaisePropertyChanged();
        }
    }

    public SD_303 SD_303
    {
        get => Entity.SD_303;
        set
        {
            if (Entity.SD_303 == value) return;
            Entity.SD_303 = value;
            RaisePropertyChanged();
        }
    }

    public SD_43 SD_43
    {
        get => Entity.SD_43;
        set
        {
            if (Entity.SD_43 == value) return;
            Entity.SD_43 = value;
            RaisePropertyChanged();
        }
    }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public EntityLoadCodition LoadCondition { get; set; }

    public string SPostName
    {
        get => mySPostName;
        set
        {
            if (mySPostName == value) return;
            mySPostName = value;
            RaisePropertyChanged();
        }
    }

    [Display(AutoGenerateField = false)] public decimal MaxSumma { get; set; }

    public bool IsAccessRight { get; set; }

    public SD_34 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_34 DefaultValue()
    {
        return new SD_34
        {
            DOC_CODE = -1,
            DATE_ORD = DateTime.Today
        };
    }

    public List<SD_34> LoadList()
    {
        throw new NotImplementedException();
    }

    private void LoadReferences()
    {
        if (Entity?.CA_DC != null) Cash = GlobalOptions.ReferencesCache.GetCashBox(Entity.CA_DC) as CashBox;
        myKontragentType = CashKontragentType.NotChoice;
        if (KONTRAGENT_DC != null)
            myKontragentType = CashKontragentType.Kontragent;
        Currency = GlobalOptions.ReferencesCache.GetCurrency(Entity?.CRS_DC) as References.Currency;
        if (SPOST_DC != null) SPostName = SPost(SPOST_DC.Value);
        if (SHPZ_DC != null) SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(SHPZ_DC) as SDRSchet;
        if (TABELNUMBER != null)
        {
            myKontragentType = CashKontragentType.Employee;
            Employee = GlobalOptions.ReferencesCache.GetEmployee(TABELNUMBER) as References.Employee;
        }

        if (BANK_RASCH_SCHET_DC != null)
        {
            myKontragentType = CashKontragentType.Bank;
            BankAccount = GlobalOptions.ReferencesCache.GetBankAccount(BANK_RASCH_SCHET_DC) as BankAccount;
        }

        if (CASH_TO_DC != null)
        {
            myKontragentType = CashKontragentType.Cash;
            CashTo = (CashBox)GlobalOptions.ReferencesCache.GetCashBox(CASH_TO_DC);
        }

        // ReSharper disable once PossibleNullReferenceException
        if (Entity.StockHolders != null)
        {
            StockHolder = new StockHolderViewModel(Entity.StockHolders);
            myKontragentType = CashKontragentType.StockHolder;
            RaisePropertyChanged("KontragentType");
        }

        IsKontrSelectEnable = myKontragentType != CashKontragentType.NotChoice;
    }

    private string SPost(decimal dc)
    {
        using (var ctx = GlobalOptions.GetEntities())
        {
            var doc = ctx.SD_26.AsNoTracking().FirstOrDefault(_ => _.DOC_CODE == dc);
            return doc == null
                ? null
                : $"Счет-фактура поставщика №{doc.SF_POSTAV_NUM}/{doc.SF_IN_NUM} от {doc.SF_POSTAV_DATE} " +
                  $"Пост:{GlobalOptions.ReferencesCache.GetKontragent(doc.SF_POST_DC)} " +
                  // ReSharper disable once PossibleInvalidOperationException
                  $"на сумму:{doc.SF_CRS_SUMMA} {GlobalOptions.ReferencesCache.GetCurrency(doc.SF_CRS_DC)}";
        }
    }


    public void Delete(decimal dc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(SD_34 ent)
    {
        NUM_ORD = ent.NUM_ORD;
        DATE_ORD = ent.DATE_ORD;
        SUMM_ORD = ent.SUMM_ORD;
        DOCUM_ORD = ent.DOCUM_ORD;
        OSN_ORD = ent.OSN_ORD;
        NOTES_ORD = ent.NOTES_ORD;
        NAME_ORD = ent.NAME_ORD;
        CODE_CASS = ent.CODE_CASS;
        TABELNUMBER = ent.TABELNUMBER;
        OP_CODE = ent.OP_CODE;
        CA_DC = ent.CA_DC;
        KONTRAGENT_DC = ent.KONTRAGENT_DC;
        SHPZ_ORD = ent.SHPZ_ORD;
        SHPZ_DC = ent.SHPZ_DC;
        CASH_TO_DC = ent.CASH_TO_DC;
        SPOST_DC = ent.SPOST_DC;
        NCODE = ent.NCODE;
        KONTR_CRS_DC = ent.KONTR_CRS_DC;
        CRS_KOEF = ent.CRS_KOEF;
        CRS_SUMMA = ent.CRS_SUMMA;
        CHANGE_ORD = ent.CHANGE_ORD;
        CRS_DC = ent.CRS_DC;
        UCH_VALUTA_DC = ent.UCH_VALUTA_DC;
        SUMMA_V_UCH_VALUTE = ent.SUMMA_V_UCH_VALUTE;
        UCH_VALUTA_RATE = ent.UCH_VALUTA_RATE;
        SPOST_OPLACHENO = ent.SPOST_OPLACHENO;
        SPOST_CRS_DC = ent.SPOST_CRS_DC;
        SPOST_CRS_RATE = ent.SPOST_CRS_RATE;
        BANK_RASCH_SCHET_DC = ent.BANK_RASCH_SCHET_DC;
        CREATOR = ent.CREATOR;
        RUB_SUMMA = ent.RUB_SUMMA;
        RUB_RATE = ent.RUB_RATE;
        KONTR_CRS_SUM_CORRECT_PERCENT = ent.KONTR_CRS_SUM_CORRECT_PERCENT;
        OBRATNY_RASCHET = ent.OBRATNY_RASCHET;
        V_TOM_CHISLE = ent.V_TOM_CHISLE;
        PAY_ROLL_DC = ent.PAY_ROLL_DC;
        PAY_ROLL_ROW_CODE = ent.PAY_ROLL_ROW_CODE;
        KONTR_FROM_DC = ent.KONTR_FROM_DC;
        SFACT_FLAG = ent.SFACT_FLAG;
        TSTAMP = ent.TSTAMP;
        PLAT_VED_DC = ent.PLAT_VED_DC;
        PLAT_VED_ROW_CODE = ent.PLAT_VED_ROW_CODE;
        SD_114 = ent.SD_114;
        SD_2 = ent.SD_2;
        SD_22 = ent.SD_22;
        SD_221 = ent.SD_221;
        SD_26 = ent.SD_26;
        SD_301 = ent.SD_301;
        SD_3011 = ent.SD_3011;
        SD_3012 = ent.SD_3012;
        SD_3013 = ent.SD_3013;
        SD_303 = ent.SD_303;
        SD_43 = ent.SD_43;
    }

    public void UpdateTo(SD_34 ent)
    {
        ent.NUM_ORD = NUM_ORD;
        ent.DATE_ORD = DATE_ORD;
        ent.SUMM_ORD = SUMM_ORD;
        ent.DOCUM_ORD = DOCUM_ORD;
        ent.OSN_ORD = OSN_ORD;
        ent.NOTES_ORD = NOTES_ORD;
        ent.NAME_ORD = NAME_ORD;
        ent.CODE_CASS = CODE_CASS;
        ent.TABELNUMBER = TABELNUMBER;
        ent.OP_CODE = OP_CODE;
        ent.CA_DC = CA_DC;
        ent.KONTRAGENT_DC = KONTRAGENT_DC;
        ent.SHPZ_ORD = SHPZ_ORD;
        ent.SHPZ_DC = SHPZ_DC;
        ent.CASH_TO_DC = CASH_TO_DC;
        ent.SPOST_DC = SPOST_DC;
        ent.NCODE = NCODE;
        ent.KONTR_CRS_DC = KONTR_CRS_DC;
        ent.CRS_KOEF = CRS_KOEF;
        ent.CRS_SUMMA = CRS_SUMMA;
        ent.CHANGE_ORD = CHANGE_ORD;
        ent.CRS_DC = CRS_DC;
        ent.UCH_VALUTA_DC = UCH_VALUTA_DC;
        ent.SUMMA_V_UCH_VALUTE = SUMMA_V_UCH_VALUTE;
        ent.UCH_VALUTA_RATE = UCH_VALUTA_RATE;
        ent.SPOST_OPLACHENO = SPOST_OPLACHENO;
        ent.SPOST_CRS_DC = SPOST_CRS_DC;
        ent.SPOST_CRS_RATE = SPOST_CRS_RATE;
        ent.BANK_RASCH_SCHET_DC = BANK_RASCH_SCHET_DC;
        ent.CREATOR = CREATOR;
        ent.RUB_SUMMA = RUB_SUMMA;
        ent.RUB_RATE = RUB_RATE;
        ent.KONTR_CRS_SUM_CORRECT_PERCENT = KONTR_CRS_SUM_CORRECT_PERCENT;
        ent.OBRATNY_RASCHET = OBRATNY_RASCHET;
        ent.V_TOM_CHISLE = V_TOM_CHISLE;
        ent.PAY_ROLL_DC = PAY_ROLL_DC;
        ent.PAY_ROLL_ROW_CODE = PAY_ROLL_ROW_CODE;
        ent.KONTR_FROM_DC = KONTR_FROM_DC;
        ent.SFACT_FLAG = SFACT_FLAG;
        ent.TSTAMP = TSTAMP;
        ent.PLAT_VED_DC = PLAT_VED_DC;
        ent.PLAT_VED_ROW_CODE = PLAT_VED_ROW_CODE;
        ent.SD_114 = SD_114;
        ent.SD_2 = SD_2;
        ent.SD_22 = SD_22;
        ent.SD_221 = SD_221;
        ent.SD_26 = SD_26;
        ent.SD_301 = SD_301;
        ent.SD_3011 = SD_3011;
        ent.SD_3012 = SD_3012;
        ent.SD_3013 = SD_3013;
        ent.SD_303 = SD_303;
        ent.SD_43 = SD_43;
    }

    public override string ToString()
    {
        return $"Расходный кассовый ордер {NUM_ORD} от {DATE_ORD} из {Cash}";
    }

    public virtual void Save(SD_34 doc)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public void Delete()
    {
        throw new NotImplementedException();
    }

    public void Delete(Guid id)
    {
        throw new NotImplementedException();
    }

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public SD_34 Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    // ReSharper disable once MethodOverloadWithOptionalParameter
    public SD_34 Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual SD_34 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual SD_34 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public override object ToJson()
    {
        var res = new
        {
            DocCode,
            Касса = Cash.Name,
            Номер = NUM_ORD,
            Дата = DATE_ORD?.ToShortDateString(),
            ТипКонтрагента = KontragentType.ToString(),
            Контрагент = Kontragent,
            Сумма = SUMM_ORD?.ToString("n2"),
            Валюта = Currency.ToString(),
            ОрдерИмя = NAME_ORD,
            Основание = OSN_ORD,
            КодЗарплаты = NCODE,
            Счет = SPostName,
            Создатель = CREATOR,
            ОбратныйРасчет = IsBackCalc ? "Да" : "Нет",
            ПроцентОбрРасчет = PercentForKontragent?.ToString("n2"),
            Примечание = NOTES_ORD
        };
        return JsonConvert.SerializeObject(res);
    }
}

// ReSharper disable once InconsistentNaming
public static class SD_34LayoutData_FluentAPI
{
    public static MemoEditSettings Memo = new MemoEditSettings
    {
        ShowIcon = false
    };

    public static void BuildMetadata(MetadataBuilder<CashOut> builder)
    {
        builder.Property(_ => _.DocCode).NotAutoGenerated();
        builder.Property(_ => _.Id).NotAutoGenerated();
        builder.Property(_ => _.Code).NotAutoGenerated();
        builder.Property(_ => _.Note).NotAutoGenerated();
        builder.Property(_ => _.DocCode).NotAutoGenerated();
        builder.Property(_ => _.Entity).NotAutoGenerated();
        builder.Property(_ => _.LoadCondition).NotAutoGenerated();
        builder.Property(_ => _.ParentDC).NotAutoGenerated();
        builder.Property(_ => _.Parent).NotAutoGenerated();
        builder.Property(_ => _.StringId).NotAutoGenerated();
        builder.Property(_ => _.ParentId).NotAutoGenerated();
        builder.Property(_ => _.Name).NotAutoGenerated();
        builder.Property(_ => _.SD_114).NotAutoGenerated();
        builder.Property(_ => _.SD_2).NotAutoGenerated();
        builder.Property(_ => _.SD_22).NotAutoGenerated();
        builder.Property(_ => _.SD_301).NotAutoGenerated();
        builder.Property(_ => _.SD_3011).NotAutoGenerated();
        builder.Property(_ => _.SD_3012).NotAutoGenerated();
        builder.Property(_ => _.SD_3013).NotAutoGenerated();
        builder.Property(_ => _.SD_303).NotAutoGenerated();
        builder.Property(_ => _.SD_43).NotAutoGenerated();
        builder.Property(_ => _.CODE_CASS).NotAutoGenerated();
        builder.Property(_ => _.TABELNUMBER).NotAutoGenerated();
        builder.Property(_ => _.AccuredId).NotAutoGenerated();
        builder.Property(_ => _.AccuredAmountOfSupplier).NotAutoGenerated();
        builder.Property(x => x.Employee).NotAutoGenerated();
        builder.Property(x => x.CA_DC).NotAutoGenerated();
        builder.Property(x => x.KONTRAGENT_DC).NotAutoGenerated();
        builder.Property(x => x.SHPZ_ORD).NotAutoGenerated();
        builder.Property(x => x.SHPZ_DC).NotAutoGenerated();
        builder.Property(x => x.KONTR_CRS_DC).NotAutoGenerated();
        builder.Property(x => x.CRS_KOEF).NotAutoGenerated();
        builder.Property(x => x.CHANGE_ORD).NotAutoGenerated();
        builder.Property(x => x.CRS_DC).NotAutoGenerated();
        builder.Property(x => x.UCH_VALUTA_DC).NotAutoGenerated();
        builder.Property(x => x.SUMMA_V_UCH_VALUTE).NotAutoGenerated();
        builder.Property(x => x.UCH_VALUTA_RATE).NotAutoGenerated();
        builder.Property(x => x.BANK_RASCH_SCHET_DC).NotAutoGenerated();
        builder.Property(x => x.BankAccount).NotAutoGenerated();
        builder.Property(x => x.RUB_RATE).NotAutoGenerated();
        builder.Property(x => x.RUB_SUMMA).NotAutoGenerated();
        builder.Property(x => x.DOCUM_ORD).NotAutoGenerated();
        builder.Property(x => x.CASH_TO_DC).NotAutoGenerated();
        builder.Property(x => x.CashTo).NotAutoGenerated();
        builder.Property(x => x.SPOST_DC).NotAutoGenerated();
        builder.Property(x => x.SPOST_OPLACHENO).NotAutoGenerated();
        builder.Property(x => x.SPOST_CRS_DC).NotAutoGenerated();
        builder.Property(x => x.SPOST_CRS_RATE).NotAutoGenerated();
        builder.Property(x => x.PAY_ROLL_DC).NotAutoGenerated();
        builder.Property(x => x.PAY_ROLL_ROW_CODE).NotAutoGenerated();
        builder.Property(x => x.PLAT_VED_DC).NotAutoGenerated();
        builder.Property(x => x.PLAT_VED_ROW_CODE).NotAutoGenerated();
        builder.Property(x => x.SD_221).NotAutoGenerated();
        builder.Property(x => x.SD_26).NotAutoGenerated();
        builder.Property(x => x.V_TOM_CHISLE).NotAutoGenerated();
        builder.Property(x => x.KONTR_FROM_DC).NotAutoGenerated();
        builder.Property(x => x.SFACT_FLAG).NotAutoGenerated();
        builder.Property(x => x.OP_CODE).NotAutoGenerated();
        builder.Property(x => x.IsAccessRight).NotAutoGenerated();
        builder.Property(_ => _.IsKontrSelectEnable).NotAutoGenerated();
        builder.Property(_ => _.IsKontrSummaEnabled).NotAutoGenerated();
        builder.Property(_ => _.IsSummaEnabled).NotAutoGenerated();
        builder.Property(_ => _.KONTR_CRS_SUM_CORRECT_PERCENT).NotAutoGenerated();
        builder.Property(_ => _.OBRATNY_RASCHET).NotAutoGenerated();
        builder.Property(_ => _.StockHolder).NotAutoGenerated();
        builder.Property(_ => _.AccuredInfo).DisplayName("Прямой расход");
        builder.Property(x => x.Currency)
            .DisplayName("Валюта");
        builder.Property(x => x.SDRSchet).DisplayName("Счет доходов/расходов");
        builder.Property(x => x.Cash)
            .DisplayName("Касса");
        builder.Property(x => x.KontragentType)
            .DisplayName("Тип контрагента");
        builder.Property(x => x.DATE_ORD)
            .DisplayName("Дата");
        builder.Property(x => x.SUMM_ORD)
            .DisplayName("Сумма").DisplayFormatString("n2");
        builder.Property(x => x.CRS_SUMMA)
            .DisplayName("Счет доходов/расходов");
        builder.Property(x => x.State).AutoGenerated()
            .DisplayName("Статус");
        builder.Property(x => x.NUM_ORD)
            .DisplayName("№");
        builder.Property(x => x.SPostName).AutoGenerated().DisplayName("С/фактура");
        builder.Property(x => x.NAME_ORD)
            .DisplayName("Наименование");
        builder.Property(x => x.OSN_ORD)
            .DisplayName("Основание");
        builder.Property(x => x.NOTES_ORD)
            .DisplayName("Примечание");
        builder.Property(x => x.Kontragent)
            .DisplayName("Контрагент");
        //builder.Property(x => x.)
        //    .DisplayName("С/фактура");
        builder.Property(x => x.CRS_SUMMA).DisplayFormatString("n2")
            .DisplayName("Сумма контрагенту");
        builder.Property(x => x.NCODE)
            .DisplayName("в/о зарплаты").DisplayFormatString("n0");
        builder.Property(x => x.CREATOR)
            .DisplayName("Создатель");
        builder.Property(x => x.IsBackCalc)
            .DisplayName("Обратный расчет");
        builder.Property(x => x.PercentForKontragent)
            .DisplayName("%");
        builder.DataFormLayout()
            .Group("Касса", Orientation.Horizontal)
            .ContainsProperty(_ => _.Cash)
            .ContainsProperty(_ => _.CREATOR)
            .ContainsProperty(_ => _.State)
            .EndGroup()
            .GroupBox("Свойства документа", Orientation.Vertical)
            .Group("Номер и суммы", Orientation.Horizontal)
            .ContainsProperty(_ => _.NUM_ORD)
            .ContainsProperty(_ => _.DATE_ORD)
            .ContainsProperty(_ => _.SUMM_ORD)
            .ContainsProperty(_ => _.Currency)
            .EndGroup()
            .ContainsProperty(_ => _.SPostName)
            .GroupBox("Контрагент", Orientation.Horizontal)
            .ContainsProperty(_ => _.KontragentType)
            .ContainsProperty(_ => _.Kontragent)
            .ContainsProperty(_ => _.CRS_SUMMA)
            .ContainsProperty(_ => _.PercentForKontragent)
            .ContainsProperty(_ => _.IsBackCalc)
            .EndGroup()
            .GroupBox("Дополнения", Orientation.Vertical)
            .ContainsProperty(_ => _.NAME_ORD)
            .ContainsProperty(_ => _.OSN_ORD)
            .ContainsProperty(_ => _.NOTES_ORD)
            .EndGroup()
            .ContainsProperty(_ => _.SDRSchet)
            .ContainsProperty(_ => _.AccuredInfo)
            .ContainsProperty(_ => _.NCODE)
            .EndGroup();
    }
}
