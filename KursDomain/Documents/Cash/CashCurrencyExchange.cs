using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Controls;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using Helper;
using KursDomain.ICommon;
using KursDomain.References;
using Newtonsoft.Json;

namespace KursDomain.Documents.Cash;

[MetadataType(typeof(SD_251LayoutData_FluentAPI))]
public class CashCurrencyExchange : RSViewModelBase, IEntity<SD_251>
{
    #region Fields

    private decimal myCrsInCBRate;
    private decimal myCrsOutCBRate;
    private SD_251 myEntity;
    private bool myIsKontrSelectEnable;
    private Kontragent myKontragent;
    private Employee.Employee myEmployee;
    private CashCurrencyExchangeKontragentType myKontragentType;
    private References.Currency myCurrencyIn;
    private References.Currency myCurrencyOut;
    private Cash myCash;
    private SDRSchet mySDRSchet;

    #endregion

    #region Constructors

    public CashCurrencyExchange()
    {
        Entity = DefaultValue();
        KontragentType = CashCurrencyExchangeKontragentType.NotChoice;
    }

    public CashCurrencyExchange(SD_251 entity)
    {
        Entity = entity ?? new SD_251 { DOC_CODE = -1 };
        if (Entity.DOC_CODE < 0) myKontragentType = CashCurrencyExchangeKontragentType.NotChoice;
        if (CH_CASH_DC != 0) Cash = MainReferences.Cashs[CH_CASH_DC];
        if (CH_SHPZ_DC != null)
            SDRSchet = GlobalOptions.ReferencesCache.GetSDRSchet(CH_SHPZ_DC) as SDRSchet;
        if (TABELNUMBER != null)
            Employee = MainReferences.Employees.Values.FirstOrDefault(_ => _.TabelNumber == TABELNUMBER.Value);
        if (CH_KONTRAGENT_DC != null)
            Kontragent = GlobalOptions.ReferencesCache.GetKontragent(CH_KONTRAGENT_DC) as Kontragent;
        myKontragentType = Kontragent != null ? CashCurrencyExchangeKontragentType.Kontragent :
            Employee != null ? CashCurrencyExchangeKontragentType.Employee :
            CashCurrencyExchangeKontragentType.NotChoice;
        IsKontrSelectEnable = KontragentType != CashCurrencyExchangeKontragentType.NotChoice;
    }

    #endregion

    #region Properties

    public override string Description =>
        $"Обмен валюты №{CH_NUM_ORD} от {CH_DATE.ToShortDateString()} Касса: {Cash} " +
        $"от {Kontragent} ({KontragentType.GetDisplayAttributesFrom(typeof(CashKontragentType)).Name}) " +
        $"приход: {CH_CRS_IN_SUM} {CurrencyIn} расход: {CH_CRS_OUT_SUM} {CurrencyOut} {CH_NOTE}";


    public Kontragent Kontragent
    {
        get => myKontragent;
        set
        {
            if (myKontragent.Equals(value)) return;
            myKontragent = value;
            CH_KONTRAGENT_DC = myKontragent?.DocCode;
            CH_NAME_ORD = myKontragent?.Name;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(KontragentName));
        }
    }

    public Employee.Employee Employee
    {
        get => myEmployee;
        set
        {
            if (myEmployee != null && myEmployee.Equals(value)) return;
            myEmployee = value;
            TABELNUMBER = myEmployee?.TabelNumber;
            CH_NAME_ORD = myEmployee?.Name;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(KontragentName));
        }
    }

    public CashCurrencyExchangeKontragentType KontragentType
    {
        get => myKontragentType;
        set
        {
            if (myKontragentType == value) return;
            myKontragentType = value;
            Kontragent = null;
            Employee = null;
            CH_NAME_ORD = null;
            IsKontrSelectEnable = myKontragentType != CashCurrencyExchangeKontragentType.NotChoice;
            RaisePropertyChanged();
        }
    }

    public string CH_NAME_ORD
    {
        get => Entity.CH_NAME_ORD;
        set
        {
            if (Entity.CH_NAME_ORD == value) return;
            Entity.CH_NAME_ORD = value;
            RaisePropertyChanged();
        }
    }

    public decimal CH_CASH_DC
    {
        get => Entity.CH_CASH_DC;
        set
        {
            if (Entity.CH_CASH_DC == value) return;
            Entity.CH_CASH_DC = value;
            RaisePropertyChanged();
        }
    }

    public Cash Cash
    {
        get => myCash;
        set
        {
            if (myCash != null && myCash.Equals(value)) return;
            myCash = value;
            if (myCash != null)
                CH_CASH_DC = myCash.DocCode;
            RaisePropertyChanged();
        }
    }

    public decimal? CH_CASH_DATE_OUT_DC
    {
        get => Entity.CH_CASH_DATE_OUT_DC;
        set
        {
            if (Entity.CH_CASH_DATE_OUT_DC == value) return;
            Entity.CH_CASH_DATE_OUT_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CH_CASH_DATE_IN_DC
    {
        get => Entity.CH_CASH_DATE_IN_DC;
        set
        {
            if (Entity.CH_CASH_DATE_IN_DC == value) return;
            Entity.CH_CASH_DATE_IN_DC = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CH_CRS_OUT_DC
    {
        get => Entity.CH_CRS_OUT_DC;
        set
        {
            if (Entity.CH_CRS_OUT_DC == value) return;
            Entity.CH_CRS_OUT_DC = value;
            RaisePropertyChanged();
        }
    }

    public References.Currency CurrencyOut
    {
        get => myCurrencyOut ?? (CH_CRS_OUT_DC != null ? MainReferences.Currencies[CH_CRS_OUT_DC.Value] : null);
        set
        {
            if (Equals(myCurrencyOut, value)) return;
            myCurrencyOut = value;
            if (myCurrencyOut != null)
            {
                var rates = CurrencyRate.GetRate(CH_DATE);
                CH_CRS_OUT_DC = myCurrencyOut?.DocCode;
                CrsOutCBRate = rates[myCurrencyOut];
                if (myCurrencyOut != null && myCurrencyIn != null)
                    CrossRate = CurrencyRate.GetCBSummaRate(myCurrencyIn, myCurrencyOut, rates);
                else
                    CrossRate = 1;
            }

            RaisePropertyChanged();
        }
    }

    public decimal CH_CRS_OUT_SUM
    {
        get => Entity.CH_CRS_OUT_SUM;
        set
        {
            if (Entity.CH_CRS_OUT_SUM == value) return;
            Entity.CH_CRS_OUT_SUM = value;
            if (value == 0 || CurrencyIn == null)
                Entity.CH_CRS_IN_SUM = 0;
            else if (CH_CRS_IN_DC == GlobalOptions.SystemProfile.NationalCurrency.DocCode)
                Entity.CH_CRS_IN_SUM =
                    // ReSharper disable once PossibleInvalidOperationException
                    CrossRate == 0 ? 0 : decimal.Round((decimal)(Entity.CH_CRS_OUT_SUM * CrossRate), 2);
            else
                Entity.CH_CRS_IN_SUM =
                    // ReSharper disable once PossibleInvalidOperationException
                    CrossRate == 0 ? 0 : decimal.Round((decimal)(Entity.CH_CRS_OUT_SUM / CrossRate), 2);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(CH_CRS_IN_SUM));
            RaisePropertyChanged();
        }
    }

    public decimal CrsOutCBRate
    {
        get => myCrsOutCBRate;
        set
        {
            if (myCrsOutCBRate == value) return;
            myCrsOutCBRate = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CH_CRS_IN_DC
    {
        get => Entity.CH_CRS_IN_DC;
        set
        {
            if (Entity.CH_CRS_IN_DC == value) return;
            Entity.CH_CRS_IN_DC = value;
            RaisePropertyChanged();
        }
    }

    public References.Currency CurrencyIn
    {
        get => myCurrencyIn ?? (CH_CRS_IN_DC != null ? MainReferences.Currencies[CH_CRS_IN_DC.Value] : null);
        set
        {
            if (Equals(myCurrencyIn, value)) return;
            myCurrencyIn = value;
            CH_CRS_IN_DC = myCurrencyIn?.DocCode;
            if (myCurrencyIn != null)
            {
                var rates = CurrencyRate.GetRate(CH_DATE);
                CH_CRS_OUT_DC = myCurrencyOut?.DocCode;
                CrsInCBRate = rates[myCurrencyIn];
                if (myCurrencyOut != null && myCurrencyIn != null)
                    CrossRate = CurrencyRate.GetCBSummaRate(myCurrencyIn, myCurrencyOut, rates);
                else
                    CrossRate = 1;
            }

            RaisePropertyChanged();
        }
    }

    public decimal? CH_CRS_IN_SUM
    {
        get => Entity.CH_CRS_IN_SUM;
        set
        {
            if (Entity.CH_CRS_IN_SUM == value) return;
            Entity.CH_CRS_IN_SUM = value;
            if (value == 0 || CurrencyOut == null)
                Entity.CH_CRS_OUT_SUM = 0;
            else
                Entity.CH_CRS_OUT_SUM =
                    decimal.Round(
                        // ReSharper disable once PossibleInvalidOperationException
                        (decimal)(Entity.CH_CRS_IN_SUM *
                                  (CurrencyIn.DocCode == CurrencyCode.RUB ? 1m / CrossRate : CrossRate)), 2);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(CH_CRS_OUT_SUM));
        }
    }

    public decimal CrsInCBRate
    {
        get => myCrsInCBRate;
        set
        {
            if (myCrsInCBRate == value) return;
            myCrsInCBRate = value;
            RaisePropertyChanged();
        }
    }

    public string CH_NOTE
    {
        get => Entity.CH_NOTE;
        set
        {
            if (Entity.CH_NOTE == value) return;
            Entity.CH_NOTE = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? CH_DATE_IN
    {
        get => Entity.CH_DATE_IN;
        set
        {
            if (Entity.CH_DATE_IN == value) return;
            Entity.CH_DATE_IN = value;
            RaisePropertyChanged();
        }
    }

    public DateTime? CH_DATE_OUT
    {
        get => Entity.CH_DATE_OUT;
        set
        {
            if (Entity.CH_DATE_OUT == value) return;
            Entity.CH_DATE_OUT = value;
            RaisePropertyChanged();
        }
    }

    public double? CH_CROSS_RATE
    {
        get => Entity.CH_CROSS_RATE;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.CH_CROSS_RATE == value) return;
            Entity.CH_CROSS_RATE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CrossRate
    {
        get => (decimal?)Entity.CH_CROSS_RATE;
        set
        {
            if (Convert.ToDecimal(CH_CROSS_RATE) == value) return;
            Entity.CH_CROSS_RATE = (double?)value;
            if (IsBackCalc)
            {
                if ((Entity.CH_CROSS_RATE ?? 0) == 0)
                    Entity.CH_CRS_IN_SUM = 0;
                else if (Entity.CH_CRS_OUT_SUM == 0 || CurrencyIn == null) Entity.CH_CRS_IN_SUM = 0;
                {
                    Entity.CH_CRS_IN_SUM =
                        // ReSharper disable once PossibleInvalidOperationException
                        CrossRate == 0 ? 0 : decimal.Round((decimal)(Entity.CH_CRS_OUT_SUM / CrossRate), 2);
                }
            }
            else
            {
                if ((Entity.CH_CROSS_RATE ?? 0) == 0)
                    Entity.CH_CRS_OUT_SUM = 0;
                else if (Entity.CH_CRS_IN_SUM == 0 || CurrencyOut == null) Entity.CH_CRS_OUT_SUM = 0;
                {
                    Entity.CH_CRS_OUT_SUM =
                        CrossRate == 0
                            ? 0
                            : decimal.Round(
                                // ReSharper disable once PossibleInvalidOperationException
                                (decimal)(Entity.CH_CRS_IN_SUM * (CurrencyIn.DocCode == CurrencyCode.RUB
                                    ? 1m / CrossRate
                                    : CrossRate)), 2);
                }
            }

            RaisePropertyChanged();
            RaisePropertyChanged(nameof(CH_CRS_OUT_SUM));
            RaisePropertyChanged(nameof(CH_CRS_IN_SUM));
        }
    }

    public short? CH_DIRECTION
    {
        get => Entity.CH_DIRECTION;
        set
        {
            if (Entity.CH_DIRECTION == value) return;
            Entity.CH_DIRECTION = value;
            RaisePropertyChanged();
        }
    }

    public bool IsBackCalc
    {
        get => CH_DIRECTION == 1;
        set
        {
            if (CH_DIRECTION == 1 == value) return;
            CH_DIRECTION = (short?)(value ? 1 : 0);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(IsSummaInEnabled));
            RaisePropertyChanged(nameof(IsSummaOutEnabled));
        }
    }

    public decimal? CH_UCHET_VALUTA_DC
    {
        get => Entity.CH_UCHET_VALUTA_DC;
        set
        {
            if (Entity.CH_UCHET_VALUTA_DC == value) return;
            Entity.CH_UCHET_VALUTA_DC = value;
            RaisePropertyChanged();
        }
    }

    public References.Currency CurrencyUchet => GlobalOptions.SystemProfile.MainCurrency;

    public decimal? CH_IN_V_UCHET_VALUTE
    {
        get => Entity.CH_IN_V_UCHET_VALUTE;
        set
        {
            if (Entity.CH_IN_V_UCHET_VALUTE == value) return;
            Entity.CH_IN_V_UCHET_VALUTE = value;
            RaisePropertyChanged();
        }
    }

    public decimal? CH_OUT_V_UCHET_VALUTE
    {
        get => Entity.CH_OUT_V_UCHET_VALUTE;
        set
        {
            if (Entity.CH_OUT_V_UCHET_VALUTE == value) return;
            Entity.CH_OUT_V_UCHET_VALUTE = value;
            RaisePropertyChanged();
        }
    }

    public double? CH_IN_UCHET_VALUTA_RATE
    {
        get => Entity.CH_IN_UCHET_VALUTA_RATE;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.CH_IN_UCHET_VALUTA_RATE == value) return;
            Entity.CH_IN_UCHET_VALUTA_RATE = value;
            RaisePropertyChanged();
        }
    }

    public double? CH_OUT_UCHET_VALUTA
    {
        get => Entity.CH_OUT_UCHET_VALUTA;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (Entity.CH_OUT_UCHET_VALUTA == value) return;
            Entity.CH_OUT_UCHET_VALUTA = value;
            RaisePropertyChanged();
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

    public decimal? CH_SHPZ_DC
    {
        get => Entity.CH_SHPZ_DC;
        set
        {
            if (Entity.CH_SHPZ_DC == value) return;
            Entity.CH_SHPZ_DC = value;
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
            CH_SHPZ_DC = mySDRSchet?.DocCode;
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

    public List<SD_251> LoadList()
    {
        throw new NotImplementedException();
    }

    public bool IsAccessRight { get; set; }

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

    public SD_251 Entity
    {
        get => myEntity;
        set
        {
            if (myEntity == value) return;
            myEntity = value;
            RaisePropertyChanged();
        }
    }

    public SD_251 DefaultValue()
    {
        return new SD_251 { DOC_CODE = -1 };
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
        get => Entity.DOC_CODE;
        set
        {
            if (Entity.DOC_CODE == value) return;
            Entity.DOC_CODE = value;
            RaisePropertyChanged();
        }
    }

    public int CH_NUM_ORD
    {
        get => Entity.CH_NUM_ORD;
        set
        {
            if (Entity.CH_NUM_ORD == value) return;
            Entity.CH_NUM_ORD = value;
            RaisePropertyChanged();
        }
    }

    public bool IsSummaInEnabled => !IsBackCalc;
    public bool IsSummaOutEnabled => IsBackCalc;

    public DateTime CH_DATE
    {
        get => Entity.CH_DATE;
        set
        {
            if (Entity.CH_DATE == value) return;
            Entity.CH_DATE = value;
            if (State == RowStatus.NewRow)
            {
                var rates = CurrencyRate.GetRate(CH_DATE);
                if (CH_CRS_IN_DC != null)
                    CrsInCBRate = rates[MainReferences.Currencies[CH_CRS_IN_DC.Value]];
                if (CH_CRS_OUT_DC != null)
                    CrsOutCBRate = rates[MainReferences.Currencies[CH_CRS_OUT_DC.Value]];
                if (CH_CRS_IN_DC != null && CH_CRS_OUT_DC != null)
                    CrossRate = CurrencyRate.GetCBSummaRate(MainReferences.Currencies[CH_CRS_IN_DC.Value],
                        MainReferences.Currencies[CH_CRS_OUT_DC.Value], rates);
                else
                    CrossRate = 0;
            }

            RaisePropertyChanged();
        }
    }

    public decimal? CH_KONTRAGENT_DC
    {
        get => Entity.CH_KONTRAGENT_DC;
        set
        {
            if (Entity.CH_KONTRAGENT_DC == value) return;
            Entity.CH_KONTRAGENT_DC = value;
            RaisePropertyChanged();
        }
    }

    public string KontragentName
    {
        get
        {
            switch (KontragentType)
            {
                case CashCurrencyExchangeKontragentType.Kontragent:
                    return CH_KONTRAGENT_DC != null
                        ? MainReferences.GetKontragent(CH_KONTRAGENT_DC).Name
                        : null;
                case CashCurrencyExchangeKontragentType.Employee:
                    return TABELNUMBER != null
                        ? Employee.Name
                        : null;
            }

            return null;
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
        }
    }

    #endregion

    #region Methods

    public virtual SD_251 Load(decimal dc)
    {
        throw new NotImplementedException();
    }

    public virtual SD_251 Load(Guid id)
    {
        throw new NotImplementedException();
    }

    public virtual void Save(SD_251 doc)
    {
        throw new NotImplementedException();
    }

    public void UpdateFrom(SD_251 ent)
    {
        CH_NUM_ORD = ent.CH_NUM_ORD;
        CH_DATE = ent.CH_DATE;
        CH_KONTRAGENT_DC = ent.CH_KONTRAGENT_DC;
        TABELNUMBER = ent.TABELNUMBER;
        CH_NAME_ORD = ent.CH_NAME_ORD;
        CH_CASH_DC = ent.CH_CASH_DC;
        CH_CASH_DATE_OUT_DC = ent.CH_CASH_DATE_OUT_DC;
        CH_CASH_DATE_IN_DC = ent.CH_CASH_DATE_IN_DC;
        CH_CRS_OUT_DC = ent.CH_CRS_OUT_DC;
        CH_CRS_OUT_SUM = ent.CH_CRS_OUT_SUM;
        CH_CRS_IN_DC = ent.CH_CRS_IN_DC;
        CH_CRS_IN_SUM = ent.CH_CRS_IN_SUM;
        CH_NOTE = ent.CH_NOTE;
        CH_DATE_IN = ent.CH_DATE_IN;
        CH_DATE_OUT = ent.CH_DATE_OUT;
        CH_CROSS_RATE = ent.CH_CROSS_RATE;
        CH_DIRECTION = ent.CH_DIRECTION;
        CH_UCHET_VALUTA_DC = ent.CH_UCHET_VALUTA_DC;
        CH_IN_V_UCHET_VALUTE = ent.CH_IN_V_UCHET_VALUTE;
        CH_OUT_V_UCHET_VALUTE = ent.CH_OUT_V_UCHET_VALUTE;
        CH_IN_UCHET_VALUTA_RATE = ent.CH_IN_UCHET_VALUTA_RATE;
        CH_OUT_UCHET_VALUTA = ent.CH_OUT_UCHET_VALUTA;
        CREATOR = ent.CREATOR;
        CH_SHPZ_DC = ent.CH_SHPZ_DC;
        SD_22 = ent.SD_22;
        SD_301 = ent.SD_301;
        SD_3011 = ent.SD_3011;
        SD_3012 = ent.SD_3012;
        SD_43 = ent.SD_43;
        SD_303 = ent.SD_303;
    }

    public void UpdateTo(SD_251 ent)
    {
        ent.CH_NUM_ORD = CH_NUM_ORD;
        ent.CH_DATE = CH_DATE;
        ent.CH_KONTRAGENT_DC = CH_KONTRAGENT_DC;
        ent.TABELNUMBER = TABELNUMBER;
        ent.CH_NAME_ORD = CH_NAME_ORD;
        ent.CH_CASH_DC = CH_CASH_DC;
        ent.CH_CASH_DATE_OUT_DC = CH_CASH_DATE_OUT_DC;
        ent.CH_CASH_DATE_IN_DC = CH_CASH_DATE_IN_DC;
        ent.CH_CRS_OUT_DC = CH_CRS_OUT_DC;
        ent.CH_CRS_OUT_SUM = CH_CRS_OUT_SUM;
        ent.CH_CRS_IN_DC = CH_CRS_IN_DC;
        ent.CH_CRS_IN_SUM = CH_CRS_IN_SUM;
        ent.CH_NOTE = CH_NOTE;
        ent.CH_DATE_IN = CH_DATE_IN;
        ent.CH_DATE_OUT = CH_DATE_OUT;
        ent.CH_CROSS_RATE = CH_CROSS_RATE;
        ent.CH_DIRECTION = CH_DIRECTION;
        ent.CH_UCHET_VALUTA_DC = CH_UCHET_VALUTA_DC;
        ent.CH_IN_V_UCHET_VALUTE = CH_IN_V_UCHET_VALUTE;
        ent.CH_OUT_V_UCHET_VALUTE = CH_OUT_V_UCHET_VALUTE;
        ent.CH_IN_UCHET_VALUTA_RATE = CH_IN_UCHET_VALUTA_RATE;
        ent.CH_OUT_UCHET_VALUTA = CH_OUT_UCHET_VALUTA;
        ent.CREATOR = CREATOR;
        ent.CH_SHPZ_DC = CH_SHPZ_DC;
        ent.SD_22 = SD_22;
        ent.SD_301 = SD_301;
        ent.SD_3011 = SD_3011;
        ent.SD_3012 = SD_3012;
        ent.SD_43 = SD_43;
        ent.SD_303 = SD_303;
    }

    public override object ToJson()
    {
        var res = new
        {
            DocCode,
            Статус = CustomFormat.GetEnumName(State),
            Касса = Cash.Name,
            Номер = CH_NUM_ORD,
            Дата = CH_DATE.ToShortDateString(),
            ТипКонтрагента = CustomFormat.GetEnumName(KontragentType),
            Контрагент = KontragentName,
            СуммаПриход = CH_CRS_IN_SUM?.ToString("n2") + " " + CurrencyIn,
            СуммаРасход = CH_CRS_OUT_SUM.ToString("n2") + " " + CurrencyOut,
            Создатель = CREATOR,
            ОбратныйРасчет = IsBackCalc ? "Да" : "Нет",
            Примечание = CH_NOTE,
            СчетДоходовРасходов = SDRSchet?.Name
        };
        return JsonConvert.SerializeObject(res);
    }

    #endregion
}

public static class SD_251LayoutData_FluentAPI
{
    public static void BuildMetadata(MetadataBuilder<CashCurrencyExchange> builder)
    {
        builder.Property(_ => _.DocCode).NotAutoGenerated();
        builder.Property(_ => _.Id).NotAutoGenerated();
        //builder.Property(_ => _.State).NotAutoGenerated();
        builder.Property(_ => _.Note).NotAutoGenerated();
        builder.Property(_ => _.DOC_CODE).NotAutoGenerated();
        builder.Property(_ => _.Entity).NotAutoGenerated();
        builder.Property(_ => _.ParentDC).NotAutoGenerated();
        builder.Property(_ => _.ParentDC).NotAutoGenerated();
        builder.Property(_ => _.Parent).NotAutoGenerated();
        builder.Property(_ => _.StringId).NotAutoGenerated();
        builder.Property(_ => _.ParentId).NotAutoGenerated();
        builder.Property(_ => _.Name).NotAutoGenerated();
        builder.Property(_ => _.CH_CASH_DATE_IN_DC).NotAutoGenerated();
        builder.Property(_ => _.CH_CASH_DATE_OUT_DC).NotAutoGenerated();
        builder.Property(_ => _.CH_CASH_DC).NotAutoGenerated();
        builder.Property(_ => _.CH_CRS_IN_DC).NotAutoGenerated();
        builder.Property(_ => _.CH_CRS_OUT_DC).NotAutoGenerated();
        builder.Property(_ => _.CH_KONTRAGENT_DC).NotAutoGenerated();
        builder.Property(_ => _.CH_SHPZ_DC).NotAutoGenerated();
        builder.Property(_ => _.CH_UCHET_VALUTA_DC).NotAutoGenerated();
        builder.Property(_ => _.CH_DIRECTION).NotAutoGenerated();
        builder.Property(_ => _.CH_OUT_V_UCHET_VALUTE).NotAutoGenerated();
        builder.Property(_ => _.CH_IN_V_UCHET_VALUTE).NotAutoGenerated();
        builder.Property(_ => _.CH_OUT_UCHET_VALUTA).NotAutoGenerated();
        builder.Property(_ => _.CH_IN_UCHET_VALUTA_RATE).NotAutoGenerated();
        builder.Property(_ => _.CurrencyUchet).NotAutoGenerated();
        builder.Property(_ => _.SD_22).NotAutoGenerated();
        builder.Property(_ => _.SD_301).NotAutoGenerated();
        builder.Property(_ => _.SD_3011).NotAutoGenerated();
        builder.Property(_ => _.SD_3012).NotAutoGenerated();
        builder.Property(_ => _.SD_303).NotAutoGenerated();
        builder.Property(_ => _.SD_43).NotAutoGenerated();
        builder.Property(x => x.IsAccessRight).NotAutoGenerated();
        builder.Property(x => x.Employee).NotAutoGenerated();
        builder.Property(x => x.Kontragent).NotAutoGenerated();
        builder.Property(_ => _.TABELNUMBER).NotAutoGenerated();
        builder.Property(_ => _.IsKontrSelectEnable).NotAutoGenerated();
        builder.Property(_ => _.IsSummaInEnabled).NotAutoGenerated();
        builder.Property(_ => _.IsSummaOutEnabled).NotAutoGenerated();
        builder.Property(_ => _.CH_CROSS_RATE).NotAutoGenerated();
        builder.Property(x => x.CH_NUM_ORD)
            .DisplayName("№").ReadOnly();
        builder.Property(x => x.CH_DATE)
            .DisplayName("Дата");
        builder.Property(x => x.KontragentType)
            .DisplayName("Тип контрагента");
        builder.Property(x => x.CH_DATE_IN)
            .DisplayName("Дата получения");
        builder.Property(x => x.CH_DATE_OUT)
            .DisplayName("Дата выдачи");
        builder.Property(x => x.KontragentName)
            .DisplayName("Контрагент");
        builder.Property(x => x.Cash)
            .DisplayName("Касса");
        builder.Property(x => x.CurrencyIn)
            .DisplayName("Валюта прихода");
        builder.Property(x => x.CH_CRS_IN_SUM)
            .DisplayName("Сумма прихода");
        builder.Property(x => x.CrsInCBRate)
            .DisplayName("Курс прихода");
        builder.Property(x => x.CurrencyOut)
            .DisplayName("Валюта выдачи");
        builder.Property(x => x.CH_CRS_OUT_SUM)
            .DisplayName("Сумма расхода");
        builder.Property(x => x.CrsOutCBRate)
            .DisplayName("Курс расхода");
        builder.Property(x => x.IsBackCalc)
            .DisplayName("Обратный расчет");
        builder.Property(x => x.SDRSchet)
            .DisplayName("Счет доходов/расходов");
        builder.Property(x => x.CrossRate)
            .DisplayName("Курс");
        builder.Property(x => x.CH_NOTE)
            .DisplayName("Примечание");
        builder.Property(x => x.CREATOR)
            .DisplayName("Создал");
        builder.Property(x => x.State)
            .DisplayName("Статус");
        builder.Property(x => x.CH_NAME_ORD)
            .DisplayName("Получатель");
        builder.DataFormLayout()
            .Group("Касса", Orientation.Horizontal)
            .ContainsProperty(_ => _.Cash)
            .ContainsProperty(_ => _.CREATOR)
            .ContainsProperty(_ => _.State)
            .EndGroup()
            .GroupBox("Свойства документа", Orientation.Vertical)
            .Group("Номер и суммы", Orientation.Horizontal)
            .ContainsProperty(_ => _.CH_NUM_ORD)
            .ContainsProperty(_ => _.CH_DATE)
            .EndGroup()
            .GroupBox("Контрагент", Orientation.Vertical)
            .Group("Контрагент")
            .ContainsProperty(_ => _.KontragentType)
            .ContainsProperty(_ => _.KontragentName)
            .EndGroup()
            .Group("Получатель")
            .ContainsProperty(_ => _.TABELNUMBER)
            .ContainsProperty(_ => _.CH_NAME_ORD)
            .EndGroup()
            .EndGroup()
            .GroupBox("Операция")
            .Group("Приход")
            .ContainsProperty(_ => _.CH_CRS_IN_SUM)
            .ContainsProperty(_ => _.CurrencyIn)
            .ContainsProperty(_ => _.CH_DATE_IN)
            .ContainsProperty(_ => _.CrsInCBRate)
            .EndGroup()
            .Group("Курс")
            .ContainsProperty(_ => _.CrossRate)
            .ContainsProperty(_ => _.IsBackCalc)
            .EndGroup()
            .Group("Расход")
            .ContainsProperty(_ => _.CH_CRS_OUT_SUM)
            .ContainsProperty(_ => _.CurrencyOut)
            .ContainsProperty(_ => _.CH_DATE_OUT)
            .ContainsProperty(_ => _.CrsOutCBRate)
            .EndGroup()
            .GroupBox("Дополнения", Orientation.Vertical)
            .ContainsProperty(_ => _.SDRSchet)
            .ContainsProperty(_ => _.CH_NOTE)
            .EndGroup()
            .EndGroup();
    }
}
