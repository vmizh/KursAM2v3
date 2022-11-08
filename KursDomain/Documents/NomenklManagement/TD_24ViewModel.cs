using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.Documents.Invoices;
using KursDomain.ICommon;
using KursDomain.References;

// ReSharper disable CompareOfFloatsByEqualityOperator

// ReSharper disable InconsistentNaming
namespace KursDomain.Documents.NomenklManagement;

public interface ITD_24
{
    decimal DocCode { set; get; }
    Guid Id { set; get; }
    Guid DocId { set; get; }
    int Code { set; get; }
    Nomenkl Nomenkl { set; get; }
    decimal DDT_KOL_PRIHOD { set; get; }
    decimal DDT_KOL_RASHOD { set; get; }
    Unit Unit { set; get; }
    IInvoiceProvider InvoiceProvider { set; get; }
    IInvoiceProviderRow InvoiceProviderRow { set; get; }
    References.Currency Currency { set; get; }
    IInvoiceClient InvoiceClient { set; get; }
    IInvoiceClientRow InvoiceClientRow { set; get; }
    bool IsTaxExecuted { set; get; }
    bool IsFactExecuted { set; get; }
    SDRSchet SDRSchet { set; get; }
    KontragentViewModel Diler { set; get; }
}

public class TD_24ViewModel : RSViewModelBase, IEntity<TD_24>
{
    private References.Currency myCurrency;
    private KontragentViewModel myDiler;
    private TD_24 myEntity;
    private InvoiceClientViewModel _myInvoiceClient;
    private InvoiceProvider myInvoiceProvider;
    private InvoiceProviderRow myInvoiceProviderRow;
    private Nomenkl myNomenkl;
    private SDRSchet mySDRSchet;
    private Unit myUnit;

    public TD_24ViewModel()
    {
        Entity = DefaultValue();
    }

    public TD_24ViewModel(TD_24 entity)
    {
        Entity = entity ?? DefaultValue();
        LoadReference();
    }

    public decimal DOC_CODE
    {
        get => Entity.DOC_CODE;
        set
        {
            if (Entity.DOC_CODE == value) return;
            Entity.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public override decimal DocCode
    {
        get => DOC_CODE;
        set
        {
            if (DOC_CODE == value) return;
            DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public override Guid Id
    {
        get => Entity.Id;
        set
        {
            if (Entity.Id == value) return;
            Entity.Id = value;
            RaisePropertyChanged();
        }
    }

    public override int Code
    {
        get => Entity.CODE;
        set
        {
            if (Entity.CODE == value) return;
            Entity.CODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal DDT_NOMENKL_DC
    {
        get => Entity.DDT_NOMENKL_DC;
        set
        {
            if (Entity.DDT_NOMENKL_DC == value) return;
            Entity.DDT_NOMENKL_DC = value;
            RaisePropertyChanged();
        }
    }

    public Nomenkl Nomenkl
    {
        get => myNomenkl;
        set
        {
            if (myNomenkl == value) return;
            myNomenkl = value;
            if (myNomenkl != null)
            {
                DDT_NOMENKL_DC = myNomenkl.DocCode;
                DDT_ED_IZM_DC = ((IDocCode) myNomenkl.Unit).DocCode;
                DDT_POST_ED_IZM_DC = ((IDocCode) myNomenkl.Unit).DocCode;
            }

            RaisePropertyChanged();
        }
    }

    public decimal DDT_KOL_PRIHOD
    {
        get => Entity.DDT_KOL_PRIHOD;
        set
        {
            if (Entity.DDT_KOL_PRIHOD == value) return;
            Entity.DDT_KOL_PRIHOD = value;
            RaisePropertyChanged();
        }
    }

    public double? DDT_KOL_ZATREBOVANO
    {
        get => Entity.DDT_KOL_ZATREBOVANO;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.DDT_KOL_ZATREBOVANO == value) return;
            Entity.DDT_KOL_ZATREBOVANO = value;
            RaisePropertyChanged();
        }
    }

    public virtual decimal DDT_KOL_RASHOD
    {
        get => Entity.DDT_KOL_RASHOD;
        set
        {
            if (Entity.DDT_KOL_RASHOD == value) return;
            Entity.DDT_KOL_RASHOD = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_KOL_PODTVERZHDENO
    {
        get => Convert.ToDecimal(Entity.DDT_KOL_PODTVERZHDENO);
        set
        {
            if (Entity.DDT_KOL_PODTVERZHDENO == (double?) value) return;
            Entity.DDT_KOL_PODTVERZHDENO = (double?) value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_KOL_SHAB_PRIHOD
    {
        get => Convert.ToDecimal(Entity.DDT_KOL_SHAB_PRIHOD);
        set
        {
            if (Entity.DDT_KOL_SHAB_PRIHOD == (double?) value) return;
            Entity.DDT_KOL_SHAB_PRIHOD = (double?) value;
            RaisePropertyChanged();
        }
    }

    public decimal DDT_ED_IZM_DC
    {
        get => Entity.DDT_ED_IZM_DC;
        set
        {
            if (Entity.DDT_ED_IZM_DC == value) return;
            Entity.DDT_ED_IZM_DC = value;
            RaisePropertyChanged();
        }
    }

    public virtual Unit Unit
    {
        get => myUnit;
        set
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (myUnit == value) return;
            myUnit = value;
            if (myUnit != null)
                DDT_ED_IZM_DC = myUnit.DocCode;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_SPOST_DC
    {
        get => Entity.DDT_SPOST_DC;
        set
        {
            if (Entity.DDT_SPOST_DC == value) return;
            Entity.DDT_SPOST_DC = value;
            RaisePropertyChanged();
        }
    }

    public InvoiceProvider InvoiceProvider
    {
        get => myInvoiceProvider;
        set
        {
            if (myInvoiceProvider != null && myInvoiceProvider.Equals(value)) return;
            myInvoiceProvider = value;
            if (myInvoiceProvider != null)
                DDT_SPOST_DC = myInvoiceProvider.DocCode;
            RaisePropertyChanged();
        }
    }

    public int? DDT_SPOST_ROW_CODE
    {
        get => Entity.DDT_SPOST_ROW_CODE;
        set
        {
            if (Entity.DDT_SPOST_ROW_CODE == value) return;
            Entity.DDT_SPOST_ROW_CODE = value;
            RaisePropertyChanged();
        }
    }

    public InvoiceProviderRow InvoiceProviderRow
    {
        get => myInvoiceProviderRow;
        set
        {
            if (myInvoiceProviderRow != null && myInvoiceProviderRow.Equals(value)) return;
            myInvoiceProviderRow = value;
            if (myInvoiceProviderRow != null)
            {
                DDT_SPOST_DC = myInvoiceProviderRow.DocCode;
                DDT_SPOST_ROW_CODE = myInvoiceProviderRow.Code;
            }

            RaisePropertyChanged();
        }
    }

    public decimal DDT_CRS_DC
    {
        get => Entity.DDT_CRS_DC;
        set
        {
            if (Entity.DDT_CRS_DC == value) return;
            Entity.DDT_CRS_DC = value;
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
            if (myCurrency != null)
                DDT_CRS_DC = myCurrency.DocCode;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_SFACT_DC
    {
        get => Entity.DDT_SFACT_DC;
        set
        {
            if (Entity.DDT_SFACT_DC == value) return;
            Entity.DDT_SFACT_DC = value;
            RaisePropertyChanged();
        }
    }

    public InvoiceClientViewModel InvoiceClientViewModel
    {
        get => _myInvoiceClient;
        set
        {
            if (_myInvoiceClient != null && _myInvoiceClient.Equals(value)) return;
            _myInvoiceClient = value;
            if (myInvoiceProvider != null)
                DDT_SFACT_DC = _myInvoiceClient.DocCode;
            RaisePropertyChanged();
        }
    }

    public int? DDT_SFACT_ROW_CODE
    {
        get => Entity.DDT_SFACT_ROW_CODE;
        set
        {
            if (Entity.DDT_SFACT_ROW_CODE == value) return;
            Entity.DDT_SFACT_ROW_CODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_OSTAT_STAR
    {
        get => (decimal?) Entity.DDT_OSTAT_STAR;
        set
        {
            if (Entity.DDT_OSTAT_STAR == (double?) value) return;
            Entity.DDT_OSTAT_STAR = (double?) value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_OSTAT_NOV
    {
        get => (decimal?) Entity.DDT_OSTAT_NOV;
        set
        {
            if (Entity.DDT_OSTAT_NOV == (double?) value) return;
            Entity.DDT_OSTAT_NOV = (double?) value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_TAX_CRS_CENA
    {
        get => Entity.DDT_TAX_CRS_CENA;
        set
        {
            if (Entity.DDT_TAX_CRS_CENA == value) return;
            Entity.DDT_TAX_CRS_CENA = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_TAX_CENA
    {
        get => Entity.DDT_TAX_CENA;
        set
        {
            if (Entity.DDT_TAX_CENA == value) return;
            Entity.DDT_TAX_CENA = value;
            RaisePropertyChanged();
        }
    }

    public short DDT_TAX_EXECUTED
    {
        get => Entity.DDT_TAX_EXECUTED;
        set
        {
            if (Entity.DDT_TAX_EXECUTED == value) return;
            Entity.DDT_TAX_EXECUTED = value;
            RaisePropertyChanged();
        }
    }

    public bool IsTaxExecuted
    {
        get => Entity.DDT_TAX_EXECUTED == 1;
        set
        {
            if (Entity.DDT_TAX_EXECUTED == 1 == value) return;
            DDT_TAX_EXECUTED = (short) (value ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    public short? DDT_TAX_IN_SFACT
    {
        get => Entity.DDT_TAX_IN_SFACT;
        set
        {
            if (Entity.DDT_TAX_IN_SFACT == value) return;
            Entity.DDT_TAX_IN_SFACT = value;
            RaisePropertyChanged();
        }
    }

    public bool IsTaxInSFact
    {
        get => Entity.DDT_TAX_IN_SFACT == 1;
        set
        {
            if (Entity.DDT_TAX_IN_SFACT == 1 == value) return;
            DDT_TAX_IN_SFACT = (short) (Entity.DDT_TAX_IN_SFACT == 1 ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_FACT_CRS_CENA
    {
        get => Entity.DDT_FACT_CRS_CENA;
        set
        {
            if (Entity.DDT_FACT_CRS_CENA == value) return;
            Entity.DDT_FACT_CRS_CENA = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_FACT_CENA
    {
        get => Entity.DDT_FACT_CENA;
        set
        {
            if (Entity.DDT_FACT_CENA == value) return;
            Entity.DDT_FACT_CENA = value;
            RaisePropertyChanged();
        }
    }

    public short DDT_FACT_EXECUTED
    {
        get => Entity.DDT_FACT_EXECUTED;
        set
        {
            if (Entity.DDT_FACT_EXECUTED == value) return;
            Entity.DDT_FACT_EXECUTED = value;
            RaisePropertyChanged();
        }
    }

    public bool IsFactExecuted
    {
        get => Entity.DDT_FACT_EXECUTED == 1;
        set
        {
            if (Entity.DDT_FACT_EXECUTED == 1 == value) return;
            Entity.DDT_FACT_EXECUTED = (short) (Entity.DDT_TAX_IN_SFACT == 1 ? 1 : 0);
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_TREB_DC
    {
        get => Entity.DDT_TREB_DC;
        set
        {
            if (Entity.DDT_TREB_DC == value) return;
            Entity.DDT_TREB_DC = value;
            RaisePropertyChanged();
        }
    }

    public int? DDT_TREB_CODE
    {
        get => Entity.DDT_TREB_CODE;
        set
        {
            if (Entity.DDT_TREB_CODE == value) return;
            Entity.DDT_TREB_CODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_NOSZATR_DC
    {
        get => Entity.DDT_NOSZATR_DC;
        set
        {
            if (Entity.DDT_NOSZATR_DC == value) return;
            Entity.DDT_NOSZATR_DC = value;
            RaisePropertyChanged();
        }
    }

    public int? DDT_NOSZATR_ROW_CODE
    {
        get => Entity.DDT_NOSZATR_ROW_CODE;
        set
        {
            if (Entity.DDT_NOSZATR_ROW_CODE == value) return;
            Entity.DDT_NOSZATR_ROW_CODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_POST_ED_IZM_DC
    {
        get => Entity.DDT_POST_ED_IZM_DC;
        set
        {
            if (Entity.DDT_POST_ED_IZM_DC == value) return;
            Entity.DDT_POST_ED_IZM_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_KOL_POST_PRIHOD
    {
        get => (decimal?) Entity.DDT_KOL_POST_PRIHOD;
        set
        {
            if (Entity.DDT_KOL_POST_PRIHOD == (double?) value) return;
            Entity.DDT_KOL_POST_PRIHOD = (double?) value;
            RaisePropertyChanged();
        }
    }

    public string DDT_PRICHINA_SPISANIA
    {
        get => Entity.DDT_PRICHINA_SPISANIA;
        set
        {
            if (Entity.DDT_PRICHINA_SPISANIA == value) return;
            Entity.DDT_PRICHINA_SPISANIA = value;
            RaisePropertyChanged();
        }
    }

    public short? DDT_VOZVRAT_TREBOVINIA
    {
        get => Entity.DDT_VOZVRAT_TREBOVINIA;
        set
        {
            if (Entity.DDT_VOZVRAT_TREBOVINIA == value) return;
            Entity.DDT_VOZVRAT_TREBOVINIA = value;
            RaisePropertyChanged();
        }
    }

    public string DDT_VOZVRAT_PRICHINA
    {
        get => Entity.DDT_VOZVRAT_PRICHINA;
        set
        {
            if (Entity.DDT_VOZVRAT_PRICHINA == value) return;
            Entity.DDT_VOZVRAT_PRICHINA = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_TOV_CHECK_DC
    {
        get => Entity.DDT_TOV_CHECK_DC;
        set
        {
            if (Entity.DDT_TOV_CHECK_DC == value) return;
            Entity.DDT_TOV_CHECK_DC = value;
            RaisePropertyChanged();
        }
    }

    public int? DDT_TOV_CHECK_CODE
    {
        get => Entity.DDT_TOV_CHECK_CODE;
        set
        {
            if (Entity.DDT_TOV_CHECK_CODE == value) return;
            Entity.DDT_TOV_CHECK_CODE = value;
            RaisePropertyChanged();
        }
    }

    public int? DDT_ACT_GP_PROD_CODE
    {
        get => Entity.DDT_ACT_GP_PROD_CODE;
        set
        {
            if (Entity.DDT_ACT_GP_PROD_CODE == value) return;
            Entity.DDT_ACT_GP_PROD_CODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_SHPZ_DC
    {
        get => Entity.DDT_SHPZ_DC;
        set
        {
            if (Entity.DDT_SHPZ_DC == value) return;
            Entity.DDT_SHPZ_DC = value;
            RaisePropertyChanged();
        }
    }

    public SDRSchet SDRSchet
    {
        get => mySDRSchet;
        set
        {
            if (mySDRSchet != null && mySDRSchet.Equals(value)) return;
            mySDRSchet = value;
            if (mySDRSchet != null)
                DDT_SHPZ_DC = mySDRSchet.DocCode;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_KONTR_CRS_SUMMA
    {
        get => Entity.DDT_KONTR_CRS_SUMMA;
        set
        {
            if (Entity.DDT_KONTR_CRS_SUMMA == value) return;
            Entity.DDT_KONTR_CRS_SUMMA = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_SUMMA_V_UCHET_VALUTE
    {
        get => Entity.DDT_SUMMA_V_UCHET_VALUTE;
        set
        {
            if (Entity.DDT_SUMMA_V_UCHET_VALUTE == value) return;
            Entity.DDT_SUMMA_V_UCHET_VALUTE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_CENA_V_UCHET_VALUTE
    {
        get => Entity.DDT_CENA_V_UCHET_VALUTE;
        set
        {
            if (Entity.DDT_CENA_V_UCHET_VALUTE == value) return;
            Entity.DDT_CENA_V_UCHET_VALUTE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_SKLAD_OTPR_DC
    {
        get => Entity.DDT_SKLAD_OTPR_DC;
        set
        {
            if (Entity.DDT_SKLAD_OTPR_DC == value) return;
            Entity.DDT_SKLAD_OTPR_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_NOM_CRS_RATE
    {
        get => (decimal?) Entity.DDT_NOM_CRS_RATE;
        set
        {
            if (Entity.DDT_NOM_CRS_RATE == (double?) value) return;
            Entity.DDT_NOM_CRS_RATE = (double?) value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_PROIZV_PLAN_DC
    {
        get => Entity.DDT_PROIZV_PLAN_DC;
        set
        {
            if (Entity.DDT_PROIZV_PLAN_DC == value) return;
            Entity.DDT_PROIZV_PLAN_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_RASH_ORD_DC
    {
        get => Entity.DDT_RASH_ORD_DC;
        set
        {
            if (Entity.DDT_RASH_ORD_DC == value) return;
            Entity.DDT_RASH_ORD_DC = value;
            RaisePropertyChanged();
        }
    }

    public int? DDT_RASH_ORD_CODE
    {
        get => Entity.DDT_RASH_ORD_CODE;
        set
        {
            if (Entity.DDT_RASH_ORD_CODE == value) return;
            Entity.DDT_RASH_ORD_CODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_VOZVR_OTGR_CSR_DC
    {
        get => Entity.DDT_VOZVR_OTGR_CSR_DC;
        set
        {
            if (Entity.DDT_VOZVR_OTGR_CSR_DC == value) return;
            Entity.DDT_VOZVR_OTGR_CSR_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_VOZVR_UCH_CRS_RATE
    {
        get => (decimal?) Entity.DDT_VOZVR_UCH_CRS_RATE;
        set
        {
            if (Entity.DDT_VOZVR_UCH_CRS_RATE == (double?) value) return;
            Entity.DDT_VOZVR_UCH_CRS_RATE = (double?) value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_VOZVR_OTGR_CRS_TAX_CENA
    {
        get => Entity.DDT_VOZVR_OTGR_CRS_TAX_CENA;
        set
        {
            if (Entity.DDT_VOZVR_OTGR_CRS_TAX_CENA == value) return;
            Entity.DDT_VOZVR_OTGR_CRS_TAX_CENA = value;
            RaisePropertyChanged();
        }
    }

    public int? DDT_SBORSCHIK_TN
    {
        get => Entity.DDT_SBORSCHIK_TN;
        set
        {
            if (Entity.DDT_SBORSCHIK_TN == value) return;
            Entity.DDT_SBORSCHIK_TN = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_KOL_IN_ONE
    {
        get => (decimal?) Entity.DDT_KOL_IN_ONE;
        set
        {
            if (Entity.DDT_KOL_IN_ONE == (double?) value) return;
            Entity.DDT_KOL_IN_ONE = (double?) value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_OS_DC
    {
        get => Entity.DDT_OS_DC;
        set
        {
            if (Entity.DDT_OS_DC == value) return;
            Entity.DDT_OS_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_GARANT_DC
    {
        get => Entity.DDT_GARANT_DC;
        set
        {
            if (Entity.DDT_GARANT_DC == value) return;
            Entity.DDT_GARANT_DC = value;
            RaisePropertyChanged();
        }
    }

    public int? DDT_GARANT_ROW_CODE
    {
        get => Entity.DDT_GARANT_ROW_CODE;
        set
        {
            if (Entity.DDT_GARANT_ROW_CODE == value) return;
            Entity.DDT_GARANT_ROW_CODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_ACT_RAZ_PROC_STOIM
    {
        get => (decimal?) Entity.DDT_ACT_RAZ_PROC_STOIM;
        set
        {
            if (Entity.DDT_ACT_RAZ_PROC_STOIM == (double?) value) return;
            Entity.DDT_ACT_RAZ_PROC_STOIM = (double?) value;
            RaisePropertyChanged();
        }
    }

    public int? DDT_PROIZV_PLAN_ROW_CODE
    {
        get => Entity.DDT_PROIZV_PLAN_ROW_CODE;
        set
        {
            if (Entity.DDT_PROIZV_PLAN_ROW_CODE == value) return;
            Entity.DDT_PROIZV_PLAN_ROW_CODE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_APGP_TO_EXECUTE
    {
        get => (decimal?) Entity.DDT_APGP_TO_EXECUTE;
        set
        {
            if (Entity.DDT_APGP_TO_EXECUTE == (double?) value) return;
            Entity.DDT_APGP_TO_EXECUTE = (double?) value;
            RaisePropertyChanged();
        }
    }

    public short? DDT_APGP_NOT_EXECUTE
    {
        get => Entity.DDT_APGP_NOT_EXECUTE;
        set
        {
            if (Entity.DDT_APGP_NOT_EXECUTE == value) return;
            Entity.DDT_APGP_NOT_EXECUTE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_DILER_DC
    {
        get => Entity.DDT_DILER_DC;
        set
        {
            if (Entity.DDT_DILER_DC == value) return;
            Entity.DDT_DILER_DC = value;
            RaisePropertyChanged();
        }
    }

    public KontragentViewModel Diler
    {
        get => myDiler;
        set
        {
            if (myDiler != null && myDiler.Equals(value)) return;
            myDiler = value;
            if (myDiler != null)
                DDT_DILER_DC = myDiler.DocCode;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_DILER_SUM
    {
        get => Entity.DDT_DILER_SUM;
        set
        {
            if (Entity.DDT_DILER_SUM == value) return;
            Entity.DDT_DILER_SUM = value;
            RaisePropertyChanged();
        }
    }

    public short? DDT_VHOD_KONTR_EXECUTE
    {
        get => Entity.DDT_VHOD_KONTR_EXECUTE;
        set
        {
            if (Entity.DDT_VHOD_KONTR_EXECUTE == value) return;
            Entity.DDT_VHOD_KONTR_EXECUTE = value;
            RaisePropertyChanged();
        }
    }

    public string DDT_VHOD_KONTR_NOTE
    {
        get => Entity.DDT_VHOD_KONTR_NOTE;
        set
        {
            if (Entity.DDT_VHOD_KONTR_NOTE == value) return;
            Entity.DDT_VHOD_KONTR_NOTE = value;
            RaisePropertyChanged();
        }
    }

    public string DDT_VHOD_KONTR_USER
    {
        get => Entity.DDT_VHOD_KONTR_USER;
        set
        {
            if (Entity.DDT_VHOD_KONTR_USER == value) return;
            Entity.DDT_VHOD_KONTR_USER = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_ZAIAVKA_DC
    {
        get => Entity.DDT_ZAIAVKA_DC;
        set
        {
            if (Entity.DDT_ZAIAVKA_DC == value) return;
            Entity.DDT_ZAIAVKA_DC = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? DDT_RASHOD_DATE
    {
        get => Entity.DDT_RASHOD_DATE;
        set
        {
            if (Entity.DDT_RASHOD_DATE == value) return;
            Entity.DDT_RASHOD_DATE = value;
            RaisePropertyChanged();
        }
    }

    public string DDT_VOZVRAT_TREB_CREATOR
    {
        get => Entity.DDT_VOZVRAT_TREB_CREATOR;
        set
        {
            if (Entity.DDT_VOZVRAT_TREB_CREATOR == value) return;
            Entity.DDT_VOZVRAT_TREB_CREATOR = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_VOZVRAT_SFACT_OPLAT_DC
    {
        get => Entity.DDT_VOZVRAT_SFACT_OPLAT_DC;
        set
        {
            if (Entity.DDT_VOZVRAT_SFACT_OPLAT_DC == value) return;
            Entity.DDT_VOZVRAT_SFACT_OPLAT_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_VOZVRAT_SFACT_CRS_DC
    {
        get => Entity.DDT_VOZVRAT_SFACT_CRS_DC;
        set
        {
            if (Entity.DDT_VOZVRAT_SFACT_CRS_DC == value) return;
            Entity.DDT_VOZVRAT_SFACT_CRS_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_VOZVRAT_SFACT_SUM
    {
        get => Entity.DDT_VOZVRAT_SFACT_SUM;
        set
        {
            if (Entity.DDT_VOZVRAT_SFACT_SUM == value) return;
            Entity.DDT_VOZVRAT_SFACT_SUM = value;
            RaisePropertyChanged();
        }
    }

    public double? DDT_VOZVRAT_SFACT_CRS_RATE
    {
        get => Entity.DDT_VOZVRAT_SFACT_CRS_RATE;
        set
        {
            if (Entity.DDT_VOZVRAT_SFACT_CRS_RATE == value) return;
            Entity.DDT_VOZVRAT_SFACT_CRS_RATE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_PROIZV_PROC_NOMENKL_DC
    {
        get => Entity.DDT_PROIZV_PROC_NOMENKL_DC;
        set
        {
            if (Entity.DDT_PROIZV_PROC_NOMENKL_DC == value) return;
            Entity.DDT_PROIZV_PROC_NOMENKL_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_PROIZV_PARTIA_DC
    {
        get => Entity.DDT_PROIZV_PARTIA_DC;
        set
        {
            if (Entity.DDT_PROIZV_PARTIA_DC == value) return;
            Entity.DDT_PROIZV_PARTIA_DC = value;
            RaisePropertyChanged();
        }
    }

    public int? DDT_PROIZV_PARTIA_CODE
    {
        get => Entity.DDT_PROIZV_PARTIA_CODE;
        set
        {
            if (Entity.DDT_PROIZV_PARTIA_CODE == value) return;
            Entity.DDT_PROIZV_PARTIA_CODE = value;
            RaisePropertyChanged();
        }
    }

    public short? DDT_DAVAL_SIRIE
    {
        get => Entity.DDT_DAVAL_SIRIE;
        set
        {
            if (Entity.DDT_DAVAL_SIRIE == value) return;
            Entity.DDT_DAVAL_SIRIE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_MEST_TARA
    {
        get => (decimal?) Entity.DDT_MEST_TARA;
        set
        {
            if (Entity.DDT_MEST_TARA == (double?) value) return;
            Entity.DDT_MEST_TARA = (double?) value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_TARA_DC
    {
        get => Entity.DDT_TARA_DC;
        set
        {
            if (Entity.DDT_TARA_DC == value) return;
            Entity.DDT_TARA_DC = value;
            RaisePropertyChanged();
        }
    }

    public short? DDT_TARA_FLAG
    {
        get => Entity.DDT_TARA_FLAG;
        set
        {
            if (Entity.DDT_TARA_FLAG == value) return;
            Entity.DDT_TARA_FLAG = value;
            RaisePropertyChanged();
        }
    }

    public string DDT_PART_NUMBER
    {
        get => Entity.DDT_PART_NUMBER;
        set
        {
            if (Entity.DDT_PART_NUMBER == value) return;
            Entity.DDT_PART_NUMBER = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_VNPER_UD_POINT_UCHVAL_PRICE
    {
        get => Entity.DDT_VNPER_UD_POINT_UCHVAL_PRICE;
        set
        {
            if (Entity.DDT_VNPER_UD_POINT_UCHVAL_PRICE == value) return;
            Entity.DDT_VNPER_UD_POINT_UCHVAL_PRICE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_SKIDKA_CREDIT_NOTE
    {
        get => Entity.DDT_SKIDKA_CREDIT_NOTE;
        set
        {
            if (Entity.DDT_SKIDKA_CREDIT_NOTE == value) return;
            Entity.DDT_SKIDKA_CREDIT_NOTE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_CALC_NOM_TAX_PRICE
    {
        get => Entity.DDT_CALC_NOM_TAX_PRICE;
        set
        {
            if (Entity.DDT_CALC_NOM_TAX_PRICE == value) return;
            Entity.DDT_CALC_NOM_TAX_PRICE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? DDT_CALC_UCHET_TAX_PRICE
    {
        get => Entity.DDT_CALC_UCHET_TAX_PRICE;
        set
        {
            if (Entity.DDT_CALC_UCHET_TAX_PRICE == value) return;
            Entity.DDT_CALC_UCHET_TAX_PRICE = value;
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

    public Guid DocId
    {
        get => Entity.DocId;
        set
        {
            if (Entity.DocId == value) return;
            Entity.DocId = value;
            RaisePropertyChanged();
        }
    }

    public decimal? SaleTaxPrice
    {
        get => Entity.SaleTaxPrice;
        set
        {
            if (Entity.SaleTaxPrice == value) return;
            Entity.SaleTaxPrice = value;
            RaisePropertyChanged();
        }
    }

    public Guid? SaleTaxCurrency
    {
        get => Entity.SaleTaxCurrency;
        set
        {
            if (Entity.SaleTaxCurrency == value) return;
            Entity.SaleTaxCurrency = value;
            RaisePropertyChanged();
        }
    }

    public decimal? SaleTaxRate
    {
        get => Entity.SaleTaxRate;
        set
        {
            if (Entity.SaleTaxRate == value) return;
            Entity.SaleTaxRate = value;
            RaisePropertyChanged();
        }
    }

    public string SaleTaxNote
    {
        get => Entity.SaleTaxNote;
        set
        {
            if (Entity.SaleTaxNote == value) return;
            Entity.SaleTaxNote = value;
            RaisePropertyChanged();
        }
    }

    public bool IsSaleTax
    {
        get => Entity.IsSaleTax;
        set
        {
            if (Entity.IsSaleTax == value) return;
            Entity.IsSaleTax = value;
            RaisePropertyChanged();
        }
    }

    public bool? IsAutoTax
    {
        get => Entity.IsAutoTax;
        set
        {
            if (Entity.IsAutoTax == value) return;
            Entity.IsAutoTax = value;
            RaisePropertyChanged();
        }
    }

    public string TaxUpdater
    {
        get => Entity.TaxUpdater;
        set
        {
            if (Entity.TaxUpdater == value) return;
            Entity.TaxUpdater = value;
            RaisePropertyChanged();
        }
    }

    public SD_122 SD_122
    {
        get => Entity.SD_122;
        set
        {
            if (Entity.SD_122 == value) return;
            Entity.SD_122 = value;
            RaisePropertyChanged();
        }
    }

    public SD_170 SD_170
    {
        get => Entity.SD_170;
        set
        {
            if (Entity.SD_170 == value) return;
            Entity.SD_170 = value;
            RaisePropertyChanged();
        }
    }

    public SD_175 SD_175
    {
        get => Entity.SD_175;
        set
        {
            if (Entity.SD_175 == value) return;
            Entity.SD_175 = value;
            RaisePropertyChanged();
        }
    }

    public SD_175 SD_1751
    {
        get => Entity.SD_1751;
        set
        {
            if (Entity.SD_1751 == value) return;
            Entity.SD_1751 = value;
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

    public SD_24 SD_24
    {
        get => Entity.SD_24;
        set
        {
            if (Entity.SD_24 == value) return;
            Entity.SD_24 = value;
            RaisePropertyChanged();
        }
    }

    public SD_254 SD_254
    {
        get => Entity.SD_254;
        set
        {
            if (Entity.SD_254 == value) return;
            Entity.SD_254 = value;
            RaisePropertyChanged();
        }
    }

    public SD_27 SD_27
    {
        get => Entity.SD_27;
        set
        {
            if (Entity.SD_27 == value) return;
            Entity.SD_27 = value;
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

    public SD_384 SD_384
    {
        get => Entity.SD_384;
        set
        {
            if (Entity.SD_384 == value) return;
            Entity.SD_384 = value;
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

    public SD_83 SD_83
    {
        get => Entity.SD_83;
        set
        {
            if (Entity.SD_83 == value) return;
            Entity.SD_83 = value;
            RaisePropertyChanged();
        }
    }

    public SD_83 SD_831
    {
        get => Entity.SD_831;
        set
        {
            if (Entity.SD_831 == value) return;
            Entity.SD_831 = value;
            RaisePropertyChanged();
        }
    }

    public SD_83 SD_832
    {
        get => Entity.SD_832;
        set
        {
            if (Entity.SD_832 == value) return;
            Entity.SD_832 = value;
            RaisePropertyChanged();
        }
    }

    public SD_84 SD_84
    {
        get => Entity.SD_84;
        set
        {
            if (Entity.SD_84 == value) return;
            Entity.SD_84 = value;
            RaisePropertyChanged();
        }
    }

    public TD_73 TD_73
    {
        get => Entity.TD_73;
        set
        {
            if (Entity.TD_73 == value) return;
            Entity.TD_73 = value;
            RaisePropertyChanged();
        }
    }

    public TD_9 TD_9
    {
        get => Entity.TD_9;
        set
        {
            if (Entity.TD_9 == value) return;
            Entity.TD_9 = value;
            RaisePropertyChanged();
        }
    }

    public TD_84 TD_84
    {
        get => Entity.TD_84;
        set
        {
            if (Entity.TD_84 == value) return;
            Entity.TD_84 = value;
            RaisePropertyChanged();
        }
    }

    public TD_26 TD_26
    {
        get => Entity.TD_26;
        set
        {
            if (Entity.TD_26 == value) return;
            Entity.TD_26 = value;
            RaisePropertyChanged();
        }
    }

    public TD_24 TD_242
    {
        get => Entity.TD_242;
        set
        {
            if (Entity.TD_242 == value) return;
            Entity.TD_242 = value;
            RaisePropertyChanged();
        }
    }

    public TD_24 TD_243
    {
        get => Entity.TD_243;
        set
        {
            if (Entity.TD_243 == value) return;
            Entity.TD_243 = value;
            RaisePropertyChanged();
        }
    }

    public TD_24 TD_244
    {
        get => Entity.TD_244;
        set
        {
            if (Entity.TD_244 == value) return;
            Entity.TD_244 = value;
            RaisePropertyChanged();
        }
    }

    public EntityLoadCodition LoadCondition { get; set; }

    public bool IsAccessRight { get; set; }

    public TD_24 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public TD_24 DefaultValue()
    {
        return new TD_24
        {
            DOC_CODE = -1,
            CODE = -1,
            Id = Guid.NewGuid(),
            DocId = Guid.Empty
        };
    }

    public List<TD_24> LoadList()
    {
        throw new NotImplementedException();
    }

    private void LoadReference()
    {
        myNomenkl = MainReferences.GetNomenkl(DDT_NOMENKL_DC);
        Unit = (Unit) Nomenkl.Unit;
        //if (DDT_SPOST_ROW_CODE != null && TD_26 != null) InvoiceProviderRow = new InvoiceProviderRow(TD_26);
        //if (DDT_SPOST_DC != null && TD_26 != null && TD_26.SD_26 != null)
        //    InvoiceProvider = new InvoiceProvider(TD_26.SD_26);
        if (MainReferences.Currencies.ContainsKey(DDT_CRS_DC)) Currency = MainReferences.Currencies[DDT_CRS_DC];
        if (DDT_SHPZ_DC != null)
            SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(DDT_SHPZ_DC.Value) as SDRSchet;
        if (MainReferences.Currencies.ContainsKey(Entity.DDT_CRS_DC))
            Currency = MainReferences.Currencies[Entity.DDT_CRS_DC];
    }

    public virtual void Save(TD_24 doc)
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

    public void Delete(decimal dc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(TD_24 ent)
    {
        Code = ent.CODE;
        DDT_NOMENKL_DC = ent.DDT_NOMENKL_DC;
        DDT_KOL_PRIHOD = ent.DDT_KOL_PRIHOD;
        DDT_KOL_ZATREBOVANO = ent.DDT_KOL_ZATREBOVANO;
        DDT_KOL_RASHOD = ent.DDT_KOL_RASHOD;
        DDT_KOL_PODTVERZHDENO = (decimal?) ent.DDT_KOL_PODTVERZHDENO;
        DDT_KOL_SHAB_PRIHOD = (decimal?) ent.DDT_KOL_SHAB_PRIHOD;
        DDT_ED_IZM_DC = ent.DDT_ED_IZM_DC;
        DDT_SPOST_DC = ent.DDT_SPOST_DC;
        DDT_SPOST_ROW_CODE = ent.DDT_SPOST_ROW_CODE;
        DDT_CRS_DC = ent.DDT_CRS_DC;
        DDT_SFACT_DC = ent.DDT_SFACT_DC;
        DDT_SFACT_ROW_CODE = ent.DDT_SFACT_ROW_CODE;
        DDT_OSTAT_STAR = (decimal?) ent.DDT_OSTAT_STAR;
        DDT_OSTAT_NOV = (decimal?) ent.DDT_OSTAT_NOV;
        DDT_TAX_CRS_CENA = ent.DDT_TAX_CRS_CENA;
        DDT_TAX_CENA = ent.DDT_TAX_CENA;
        DDT_TAX_EXECUTED = ent.DDT_TAX_EXECUTED;
        DDT_TAX_IN_SFACT = ent.DDT_TAX_IN_SFACT;
        DDT_FACT_CRS_CENA = ent.DDT_FACT_CRS_CENA;
        DDT_FACT_CENA = ent.DDT_FACT_CENA;
        DDT_FACT_EXECUTED = ent.DDT_FACT_EXECUTED;
        DDT_TREB_DC = ent.DDT_TREB_DC;
        DDT_TREB_CODE = ent.DDT_TREB_CODE;
        DDT_NOSZATR_DC = ent.DDT_NOSZATR_DC;
        DDT_NOSZATR_ROW_CODE = ent.DDT_NOSZATR_ROW_CODE;
        DDT_POST_ED_IZM_DC = ent.DDT_POST_ED_IZM_DC;
        DDT_KOL_POST_PRIHOD = (decimal?) ent.DDT_KOL_POST_PRIHOD;
        DDT_PRICHINA_SPISANIA = ent.DDT_PRICHINA_SPISANIA;
        DDT_VOZVRAT_TREBOVINIA = ent.DDT_VOZVRAT_TREBOVINIA;
        DDT_VOZVRAT_PRICHINA = ent.DDT_VOZVRAT_PRICHINA;
        DDT_TOV_CHECK_DC = ent.DDT_TOV_CHECK_DC;
        DDT_TOV_CHECK_CODE = ent.DDT_TOV_CHECK_CODE;
        DDT_ACT_GP_PROD_CODE = ent.DDT_ACT_GP_PROD_CODE;
        DDT_SHPZ_DC = ent.DDT_SHPZ_DC;
        DDT_KONTR_CRS_SUMMA = ent.DDT_KONTR_CRS_SUMMA;
        DDT_SUMMA_V_UCHET_VALUTE = ent.DDT_SUMMA_V_UCHET_VALUTE;
        DDT_CENA_V_UCHET_VALUTE = ent.DDT_CENA_V_UCHET_VALUTE;
        DDT_SKLAD_OTPR_DC = ent.DDT_SKLAD_OTPR_DC;
        DDT_NOM_CRS_RATE = (decimal?) ent.DDT_NOM_CRS_RATE;
        DDT_PROIZV_PLAN_DC = ent.DDT_PROIZV_PLAN_DC;
        DDT_RASH_ORD_DC = ent.DDT_RASH_ORD_DC;
        DDT_RASH_ORD_CODE = ent.DDT_RASH_ORD_CODE;
        DDT_VOZVR_OTGR_CSR_DC = ent.DDT_VOZVR_OTGR_CSR_DC;
        DDT_VOZVR_UCH_CRS_RATE = (decimal?) ent.DDT_VOZVR_UCH_CRS_RATE;
        DDT_VOZVR_OTGR_CRS_TAX_CENA = ent.DDT_VOZVR_OTGR_CRS_TAX_CENA;
        DDT_SBORSCHIK_TN = ent.DDT_SBORSCHIK_TN;
        DDT_KOL_IN_ONE = (decimal?) ent.DDT_KOL_IN_ONE;
        DDT_OS_DC = ent.DDT_OS_DC;
        DDT_GARANT_DC = ent.DDT_GARANT_DC;
        DDT_GARANT_ROW_CODE = ent.DDT_GARANT_ROW_CODE;
        DDT_ACT_RAZ_PROC_STOIM = (decimal?) ent.DDT_ACT_RAZ_PROC_STOIM;
        DDT_PROIZV_PLAN_ROW_CODE = ent.DDT_PROIZV_PLAN_ROW_CODE;
        DDT_APGP_TO_EXECUTE = (decimal?) ent.DDT_APGP_TO_EXECUTE;
        DDT_APGP_NOT_EXECUTE = ent.DDT_APGP_NOT_EXECUTE;
        DDT_DILER_DC = ent.DDT_DILER_DC;
        DDT_DILER_SUM = ent.DDT_DILER_SUM;
        DDT_VHOD_KONTR_EXECUTE = ent.DDT_VHOD_KONTR_EXECUTE;
        DDT_VHOD_KONTR_NOTE = ent.DDT_VHOD_KONTR_NOTE;
        DDT_VHOD_KONTR_USER = ent.DDT_VHOD_KONTR_USER;
        DDT_ZAIAVKA_DC = ent.DDT_ZAIAVKA_DC;
        DDT_RASHOD_DATE = ent.DDT_RASHOD_DATE;
        DDT_VOZVRAT_TREB_CREATOR = ent.DDT_VOZVRAT_TREB_CREATOR;
        DDT_VOZVRAT_SFACT_OPLAT_DC = ent.DDT_VOZVRAT_SFACT_OPLAT_DC;
        DDT_VOZVRAT_SFACT_CRS_DC = ent.DDT_VOZVRAT_SFACT_CRS_DC;
        DDT_VOZVRAT_SFACT_SUM = ent.DDT_VOZVRAT_SFACT_SUM;
        DDT_VOZVRAT_SFACT_CRS_RATE = ent.DDT_VOZVRAT_SFACT_CRS_RATE;
        DDT_PROIZV_PROC_NOMENKL_DC = ent.DDT_PROIZV_PROC_NOMENKL_DC;
        DDT_PROIZV_PARTIA_DC = ent.DDT_PROIZV_PARTIA_DC;
        DDT_PROIZV_PARTIA_CODE = ent.DDT_PROIZV_PARTIA_CODE;
        DDT_DAVAL_SIRIE = ent.DDT_DAVAL_SIRIE;
        DDT_MEST_TARA = (decimal?) ent.DDT_MEST_TARA;
        DDT_TARA_DC = ent.DDT_TARA_DC;
        DDT_TARA_FLAG = ent.DDT_TARA_FLAG;
        DDT_PART_NUMBER = ent.DDT_PART_NUMBER;
        DDT_VNPER_UD_POINT_UCHVAL_PRICE = ent.DDT_VNPER_UD_POINT_UCHVAL_PRICE;
        DDT_SKIDKA_CREDIT_NOTE = ent.DDT_SKIDKA_CREDIT_NOTE;
        DDT_CALC_NOM_TAX_PRICE = ent.DDT_CALC_NOM_TAX_PRICE;
        DDT_CALC_UCHET_TAX_PRICE = ent.DDT_CALC_UCHET_TAX_PRICE;
        TSTAMP = ent.TSTAMP;
        Id = ent.Id;
        DocId = ent.DocId;
        SaleTaxPrice = ent.SaleTaxPrice;
        SaleTaxCurrency = ent.SaleTaxCurrency;
        SaleTaxRate = ent.SaleTaxRate;
        SaleTaxNote = ent.SaleTaxNote;
        IsSaleTax = ent.IsSaleTax;
        IsAutoTax = ent.IsAutoTax;
        TaxUpdater = ent.TaxUpdater;
        SD_122 = ent.SD_122;
        SD_170 = ent.SD_170;
        SD_175 = ent.SD_175;
        SD_1751 = ent.SD_1751;
        SD_2 = ent.SD_2;
        SD_24 = ent.SD_24;
        SD_254 = ent.SD_254;
        SD_27 = ent.SD_27;
        SD_301 = ent.SD_301;
        SD_3011 = ent.SD_3011;
        SD_3012 = ent.SD_3012;
        SD_303 = ent.SD_303;
        SD_384 = ent.SD_384;
        SD_43 = ent.SD_43;
        SD_83 = ent.SD_83;
        SD_831 = ent.SD_831;
        SD_832 = ent.SD_832;
        SD_84 = ent.SD_84;
        TD_73 = ent.TD_73;
        TD_9 = ent.TD_9;
        TD_84 = ent.TD_84;
        TD_26 = ent.TD_26;
        TD_242 = ent.TD_242;
        TD_243 = ent.TD_243;
        TD_244 = ent.TD_244;
    }

    public void UpdateTo(TD_24 ent)
    {
        ent.CODE = Code;
        ent.DDT_NOMENKL_DC = DDT_NOMENKL_DC;
        ent.DDT_KOL_PRIHOD = DDT_KOL_PRIHOD;
        ent.DDT_KOL_ZATREBOVANO = DDT_KOL_ZATREBOVANO;
        ent.DDT_KOL_RASHOD = DDT_KOL_RASHOD;
        ent.DDT_KOL_PODTVERZHDENO = (double?) DDT_KOL_PODTVERZHDENO;
        ent.DDT_KOL_SHAB_PRIHOD = (double?) DDT_KOL_SHAB_PRIHOD;
        ent.DDT_ED_IZM_DC = DDT_ED_IZM_DC;
        ent.DDT_SPOST_DC = DDT_SPOST_DC;
        ent.DDT_SPOST_ROW_CODE = DDT_SPOST_ROW_CODE;
        ent.DDT_CRS_DC = DDT_CRS_DC;
        ent.DDT_SFACT_DC = DDT_SFACT_DC;
        ent.DDT_SFACT_ROW_CODE = DDT_SFACT_ROW_CODE;
        ent.DDT_OSTAT_STAR = (double?) DDT_OSTAT_STAR;
        ent.DDT_OSTAT_NOV = (double?) DDT_OSTAT_NOV;
        ent.DDT_TAX_CRS_CENA = DDT_TAX_CRS_CENA;
        ent.DDT_TAX_CENA = DDT_TAX_CENA;
        ent.DDT_TAX_EXECUTED = DDT_TAX_EXECUTED;
        ent.DDT_TAX_IN_SFACT = DDT_TAX_IN_SFACT;
        ent.DDT_FACT_CRS_CENA = DDT_FACT_CRS_CENA;
        ent.DDT_FACT_CENA = DDT_FACT_CENA;
        ent.DDT_FACT_EXECUTED = DDT_FACT_EXECUTED;
        ent.DDT_TREB_DC = DDT_TREB_DC;
        ent.DDT_TREB_CODE = DDT_TREB_CODE;
        ent.DDT_NOSZATR_DC = DDT_NOSZATR_DC;
        ent.DDT_NOSZATR_ROW_CODE = DDT_NOSZATR_ROW_CODE;
        ent.DDT_POST_ED_IZM_DC = DDT_POST_ED_IZM_DC;
        ent.DDT_KOL_POST_PRIHOD = (double?) DDT_KOL_POST_PRIHOD;
        ent.DDT_PRICHINA_SPISANIA = DDT_PRICHINA_SPISANIA;
        ent.DDT_VOZVRAT_TREBOVINIA = DDT_VOZVRAT_TREBOVINIA;
        ent.DDT_VOZVRAT_PRICHINA = DDT_VOZVRAT_PRICHINA;
        ent.DDT_TOV_CHECK_DC = DDT_TOV_CHECK_DC;
        ent.DDT_TOV_CHECK_CODE = DDT_TOV_CHECK_CODE;
        ent.DDT_ACT_GP_PROD_CODE = DDT_ACT_GP_PROD_CODE;
        ent.DDT_SHPZ_DC = DDT_SHPZ_DC;
        ent.DDT_KONTR_CRS_SUMMA = DDT_KONTR_CRS_SUMMA;
        ent.DDT_SUMMA_V_UCHET_VALUTE = DDT_SUMMA_V_UCHET_VALUTE;
        ent.DDT_CENA_V_UCHET_VALUTE = DDT_CENA_V_UCHET_VALUTE;
        ent.DDT_SKLAD_OTPR_DC = DDT_SKLAD_OTPR_DC;
        ent.DDT_NOM_CRS_RATE = (double?) DDT_NOM_CRS_RATE;
        ent.DDT_PROIZV_PLAN_DC = DDT_PROIZV_PLAN_DC;
        ent.DDT_RASH_ORD_DC = DDT_RASH_ORD_DC;
        ent.DDT_RASH_ORD_CODE = DDT_RASH_ORD_CODE;
        ent.DDT_VOZVR_OTGR_CSR_DC = DDT_VOZVR_OTGR_CSR_DC;
        ent.DDT_VOZVR_UCH_CRS_RATE = (double?) DDT_VOZVR_UCH_CRS_RATE;
        ent.DDT_VOZVR_OTGR_CRS_TAX_CENA = DDT_VOZVR_OTGR_CRS_TAX_CENA;
        ent.DDT_SBORSCHIK_TN = DDT_SBORSCHIK_TN;
        ent.DDT_KOL_IN_ONE = (double?) DDT_KOL_IN_ONE;
        ent.DDT_OS_DC = DDT_OS_DC;
        ent.DDT_GARANT_DC = DDT_GARANT_DC;
        ent.DDT_GARANT_ROW_CODE = DDT_GARANT_ROW_CODE;
        ent.DDT_ACT_RAZ_PROC_STOIM = (double?) DDT_ACT_RAZ_PROC_STOIM;
        ent.DDT_PROIZV_PLAN_ROW_CODE = DDT_PROIZV_PLAN_ROW_CODE;
        ent.DDT_APGP_TO_EXECUTE = (double?) DDT_APGP_TO_EXECUTE;
        ent.DDT_APGP_NOT_EXECUTE = DDT_APGP_NOT_EXECUTE;
        ent.DDT_DILER_DC = DDT_DILER_DC;
        ent.DDT_DILER_SUM = DDT_DILER_SUM;
        ent.DDT_VHOD_KONTR_EXECUTE = DDT_VHOD_KONTR_EXECUTE;
        ent.DDT_VHOD_KONTR_NOTE = DDT_VHOD_KONTR_NOTE;
        ent.DDT_VHOD_KONTR_USER = DDT_VHOD_KONTR_USER;
        ent.DDT_ZAIAVKA_DC = DDT_ZAIAVKA_DC;
        ent.DDT_RASHOD_DATE = DDT_RASHOD_DATE;
        ent.DDT_VOZVRAT_TREB_CREATOR = DDT_VOZVRAT_TREB_CREATOR;
        ent.DDT_VOZVRAT_SFACT_OPLAT_DC = DDT_VOZVRAT_SFACT_OPLAT_DC;
        ent.DDT_VOZVRAT_SFACT_CRS_DC = DDT_VOZVRAT_SFACT_CRS_DC;
        ent.DDT_VOZVRAT_SFACT_SUM = DDT_VOZVRAT_SFACT_SUM;
        ent.DDT_VOZVRAT_SFACT_CRS_RATE = DDT_VOZVRAT_SFACT_CRS_RATE;
        ent.DDT_PROIZV_PROC_NOMENKL_DC = DDT_PROIZV_PROC_NOMENKL_DC;
        ent.DDT_PROIZV_PARTIA_DC = DDT_PROIZV_PARTIA_DC;
        ent.DDT_PROIZV_PARTIA_CODE = DDT_PROIZV_PARTIA_CODE;
        ent.DDT_DAVAL_SIRIE = DDT_DAVAL_SIRIE;
        ent.DDT_MEST_TARA = (double?) DDT_MEST_TARA;
        ent.DDT_TARA_DC = DDT_TARA_DC;
        ent.DDT_TARA_FLAG = DDT_TARA_FLAG;
        ent.DDT_PART_NUMBER = DDT_PART_NUMBER;
        ent.DDT_VNPER_UD_POINT_UCHVAL_PRICE = DDT_VNPER_UD_POINT_UCHVAL_PRICE;
        ent.DDT_SKIDKA_CREDIT_NOTE = DDT_SKIDKA_CREDIT_NOTE;
        ent.DDT_CALC_NOM_TAX_PRICE = DDT_CALC_NOM_TAX_PRICE;
        ent.DDT_CALC_UCHET_TAX_PRICE = DDT_CALC_UCHET_TAX_PRICE;
        ent.TSTAMP = TSTAMP;
        ent.Id = Id;
        ent.DocId = DocId;
        ent.SaleTaxPrice = SaleTaxPrice;
        ent.SaleTaxCurrency = SaleTaxCurrency;
        ent.SaleTaxRate = SaleTaxRate;
        ent.SaleTaxNote = SaleTaxNote;
        ent.IsSaleTax = IsSaleTax;
        ent.IsAutoTax = IsAutoTax;
        ent.TaxUpdater = TaxUpdater;
        ent.SD_122 = SD_122;
        ent.SD_170 = SD_170;
        ent.SD_175 = SD_175;
        ent.SD_1751 = SD_1751;
        ent.SD_2 = SD_2;
        ent.SD_24 = SD_24;
        ent.SD_254 = SD_254;
        ent.SD_27 = SD_27;
        ent.SD_301 = SD_301;
        ent.SD_3011 = SD_3011;
        ent.SD_3012 = SD_3012;
        ent.SD_303 = SD_303;
        ent.SD_384 = SD_384;
        ent.SD_43 = SD_43;
        ent.SD_83 = SD_83;
        ent.SD_831 = SD_831;
        ent.SD_832 = SD_832;
        ent.SD_84 = SD_84;
        ent.TD_73 = TD_73;
        ent.TD_9 = TD_9;
        ent.TD_84 = TD_84;
        ent.TD_26 = TD_26;
        ent.TD_242 = TD_242;
        ent.TD_243 = TD_243;
        ent.TD_244 = TD_244;
    }

    public TD_24 Load(decimal dc, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public TD_24 Load(Guid id, bool isShort = true)
    {
        throw new NotImplementedException();
    }

    public virtual TD_24 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual TD_24 Load(Guid id)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///     Строка TD_24 для счета-фактуры поставщика
/// </summary>
[MetadataType(typeof(InvoiceProviderWarehouseReceipt_LayoutData_FluentAPI))]
public class InvoiceProviderWarehouseReceipt : TD_24ViewModel
{
    public InvoiceProviderWarehouseReceipt(TD_24 entity) : base(entity)
    {
    }
}

public class InvoiceProviderWarehouseReceipt_LayoutData_FluentAPI : DataAnnotationForFluentApiBase,
    IMetadataProvider<InvoiceProviderWarehouseReceipt>
{
    void IMetadataProvider<InvoiceProviderWarehouseReceipt>.BuildMetadata(
        MetadataBuilder<InvoiceProviderWarehouseReceipt> builder)
    {
        SetNotAutoGenerated(builder);
        builder.Property(_ => _.Currency).NotAutoGenerated();
        builder.Property(_ => _.Unit).NotAutoGenerated();
        builder.Property(_ => _.SDRSchet).NotAutoGenerated();
        builder.Property(_ => _.Nomenkl).NotAutoGenerated();
    }
}
