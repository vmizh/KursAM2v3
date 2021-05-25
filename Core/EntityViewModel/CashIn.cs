using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows.Controls;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Data;
using DevExpress.Mvvm.DataAnnotations;

namespace Core.EntityViewModel
{
    [MetadataType(typeof(SD_33LayoutData_FluentAPI))]
    public class CashIn : RSViewModelBase, IEntity<SD_33>
    {
        public decimal MaxSumma = decimal.MaxValue;
        private BankAccount myBankAccount;
        private Cash myCash;
        private Currency myCurrency;
        private SD_33 myEntity;
        private bool myIsKontrSelectEnable;
        private CashKontragentType myKontragentType;
        private string myRashodOrderFromName;
        private SDRSchet mySDRSchet;
        private string mySFactName;

        public CashIn()
        {
            Entity = DefaultValue();
            LoadReferences();
        }

        public CashIn(SD_33 entity)
        {
            Entity = entity ?? DefaultValue();
            LoadReferences();
        }

        public override string Description => $"Приходный кассовый ордер №{NUM_ORD} от {DATE_ORD?.ToShortDateString()} Касса: {Cash} " +
                                              $"от {Kontragent} ({KontragentType.GetDisplayAttributesFrom(typeof(CashKontragentType)).Name}) сумма {CRS_SUMMA} {Currency} {NOTES_ORD}";

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

        public decimal? SUMM_ORD
        {
            get => Entity.SUMM_ORD;
            set
            {
                if (Entity.SUMM_ORD == value) return;
                Entity.SUMM_ORD = value;
                if ((decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0) >= 0 &&
                    (decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0) < 100)
                {
                    if (IsBackCalc)
                    {
                        Entity.SUMM_ORD = Math.Round((Entity.SUMM_ORD ?? 0) * 100 /
                                                     (100 + (decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0)), 2);
                        RaisePropertyChanged(nameof(SUMM_ORD));
                    }
                    else
                    {
                        Entity.CRS_SUMMA = Math.Round((Entity.SUMM_ORD ?? 0) * 100 /
                                                      (100 - (decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0)), 2);
                        RaisePropertyChanged(nameof(CRS_SUMMA));
                    }
                }

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CRS_SUMMA));
            }
        }

        public decimal? CRS_SUMMA
        {
            get => Entity.CRS_SUMMA;
            set
            {
                if (Entity.CRS_SUMMA == value) return;
                Entity.CRS_SUMMA = value;
                if ((decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0) >= 0 &&
                    (decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0) < 100)
                {
                    if (IsBackCalc)
                    {
                        Entity.SUMM_ORD = Math.Round((Entity.SUMM_ORD ?? 0) * 100 /
                                                     (100 + (decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0)), 2);
                        RaisePropertyChanged(nameof(SUMM_ORD));
                    }
                    else
                    {
                        Entity.CRS_SUMMA = Math.Round((Entity.SUMM_ORD ?? 0) * 100 /
                                                      (100 - (decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0)), 2);
                        RaisePropertyChanged(nameof(CRS_SUMMA));
                    }
                }

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SUMM_ORD));
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

        public CashKontragentType KontragentType
        {
            get => myKontragentType;
            set
            {
                if (myKontragentType == value) return;
                myKontragentType = value;
                KONTRAGENT_DC = null;
                TABELNUMBER = null;
                RASH_ORDER_FROM_DC = null;
                if (myKontragentType != CashKontragentType.Kontragent)
                {
                    SFACT_DC = null;
                    SFactName = null;
                }

                BankAccount = null;
                IsKontrSelectEnable = myKontragentType != CashKontragentType.NotChoice;
                if (State == RowStatus.NewRow)
                {
                    NAME_ORD = "";
                    Note = "";
                }

                RaisePropertyChanged();
                RaisePropertiesChanged();
            }
        }

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

        public Employee Employee => TABELNUMBER != null
            ? MainReferences.Employees.Values.FirstOrDefault(_ => _.TabelNumber == TABELNUMBER)
            : null;

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

        public Cash Cash
        {
            get => myCash;
            set
            {
                if (myCash != null && myCash.Equals(value)) return;
                myCash = value;
                if (myCash != null)
                    CA_DC = myCash.DocCode;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Kontragent));
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

        public string Kontragent
        {
            get
            {
                switch (KontragentType)
                {
                    case CashKontragentType.Bank:
                        return BANK_RASCH_SCHET_DC != null
                            ? MainReferences.BankAccounts[BANK_RASCH_SCHET_DC.Value].Name
                            : null;
                    case CashKontragentType.Kontragent:
                        return KONTRAGENT_DC != null
                            ? MainReferences.GetKontragent(KONTRAGENT_DC).Name
                            : null;
                    case CashKontragentType.Cash:
                        return RashodOrderFromName;
                    case CashKontragentType.Employee:
                        return Employee?.Name;
                }

                return null;
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

        public SDRSchet SDRSchet
        {
            get => mySDRSchet;
            set
            {
                if (Equals(mySDRSchet, value)) return;
                mySDRSchet = value;
                SHPZ_DC = mySDRSchet.DocCode;
                RaisePropertyChanged();
            }
        }

        public decimal? RASH_ORDER_FROM_DC
        {
            get => Entity.RASH_ORDER_FROM_DC;
            set
            {
                if (Entity.RASH_ORDER_FROM_DC == value) return;
                Entity.RASH_ORDER_FROM_DC = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Kontragent));
            }
        }

        public string RashodOrderFromName
        {
            get => myRashodOrderFromName;
            set
            {
                if (myRashodOrderFromName == value) return;
                myRashodOrderFromName = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Kontragent));
            }
        }

        public decimal? SFACT_DC
        {
            get => Entity.SFACT_DC;
            set
            {
                if (Entity.SFACT_DC == value) return;
                Entity.SFACT_DC = value;
                if (Entity.SFACT_DC == null) SFactName = null;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SFactName));
            }
        }

        public string SFactName
        {
            get => mySFactName;
            set
            {
                if (mySFactName == value) return;
                mySFactName = value;
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

        public decimal? POS_DC
        {
            get => Entity.POS_DC;
            set
            {
                if (Entity.POS_DC == value) return;
                Entity.POS_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? POS_PREV_OST
        {
            get => Entity.POS_PREV_OST;
            set
            {
                if (Entity.POS_PREV_OST == value) return;
                Entity.POS_PREV_OST = value;
                RaisePropertyChanged();
            }
        }

        public decimal? POS_PRIHOD
        {
            get => Entity.POS_PRIHOD;
            set
            {
                if (Entity.POS_PRIHOD == value) return;
                Entity.POS_PRIHOD = value;
                RaisePropertyChanged();
            }
        }

        public decimal? POS_NOW_OST
        {
            get => Entity.POS_NOW_OST;
            set
            {
                if (Entity.POS_NOW_OST == value) return;
                Entity.POS_NOW_OST = value;
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

        public Currency Currency
        {
            get => myCurrency;
            set
            {
                if (Equals(myCurrency, value)) return;
                myCurrency = value;
                CRS_DC = myCurrency?.DocCode;
                if (myCurrency != null)
                    UCH_VALUTA_RATE = (double?) CurrencyRate.GetCBRate(myCurrency, (DateTime) DATE_ORD);
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
                if (Entity.UCH_VALUTA_RATE == value) return;
                Entity.UCH_VALUTA_RATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SUM_NDS
        {
            get => Entity.SUM_NDS;
            set
            {
                if (Entity.SUM_NDS == value) return;
                Entity.SUM_NDS = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFACT_OPLACHENO
        {
            get => Entity.SFACT_OPLACHENO;
            set
            {
                if (Entity.SFACT_OPLACHENO == value) return;
                Entity.SFACT_OPLACHENO = value;
                RaisePropertyChanged();
            }
        }

        public double? SFACT_CRS_RATE
        {
            get => Entity.SFACT_CRS_RATE;
            set
            {
                if (Entity.SFACT_CRS_RATE == value) return;
                Entity.SFACT_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SFACT_CRS_DC
        {
            get => Entity.SFACT_CRS_DC;
            set
            {
                if (Entity.SFACT_CRS_DC == value) return;
                Entity.SFACT_CRS_DC = value;
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
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(Kontragent));
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
                if (Entity.RUB_RATE == value) return;
                Entity.RUB_RATE = value;
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
                RaisePropertyChanged(nameof(IsKontrSummaEnabled));
                RaisePropertyChanged(nameof(IsSummaEnabled));
            }
        }

        public bool IsBackCalc
        {
            get => Entity.OBRATNY_RASCHET == 1;
            set
            {
                if (OBRATNY_RASCHET == (value ? 1 : 0)) return;
                OBRATNY_RASCHET = value ? (short?) 1 : (short?) 0;
                if ((decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0) >= 0 &&
                    (decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0) < 100)
                {
                    if (IsBackCalc)
                    {
                        Entity.SUMM_ORD = Math.Round((Entity.SUMM_ORD ?? 0) * 100 /
                                                     (100 + (decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0)), 2);
                        RaisePropertyChanged(nameof(SUMM_ORD));
                    }
                    else
                    {
                        Entity.CRS_SUMMA = Math.Round((Entity.SUMM_ORD ?? 0) * 100 /
                                                      (100 - (decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0)), 2);
                        RaisePropertyChanged(nameof(CRS_SUMMA));
                    }
                }

                RaisePropertyChanged();
            }
        }

        public double? KONTR_CRS_SUM_CORRECT_PERCENT
        {
            get => Entity.KONTR_CRS_SUM_CORRECT_PERCENT;
            set
            {
                if (Entity.KONTR_CRS_SUM_CORRECT_PERCENT == value) return;
                Entity.KONTR_CRS_SUM_CORRECT_PERCENT = value;
                RaisePropertyChanged();
            }
        }

        public decimal? PercentForKontragent
        {
            get => Convert.ToDecimal(Entity.KONTR_CRS_SUM_CORRECT_PERCENT);
            set
            {
                if (Entity.KONTR_CRS_SUM_CORRECT_PERCENT == Convert.ToDouble(value)) return;
                Entity.KONTR_CRS_SUM_CORRECT_PERCENT = Convert.ToDouble(value);
                if ((decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0) >= 0 &&
                    (decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0) < 100)
                {
                    if (IsBackCalc)
                    {
                        Entity.SUMM_ORD = Math.Round((Entity.SUMM_ORD ?? 0) * 100 /
                                                     (100 + (decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0)), 2);
                        RaisePropertyChanged(nameof(SUMM_ORD));
                    }
                    else
                    {
                        Entity.CRS_SUMMA = Math.Round((Entity.SUMM_ORD ?? 0) * 100 /
                                                      (100 - (decimal) (KONTR_CRS_SUM_CORRECT_PERCENT ?? 0)), 2);
                        RaisePropertyChanged(nameof(CRS_SUMMA));
                    }
                }

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

        public bool IsSummaEnabled => (OBRATNY_RASCHET ?? 0) == 0;
        public bool IsKontrSummaEnabled => (OBRATNY_RASCHET ?? 0) == 1;

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

        public SD_114 SD_114
        {
            get => Entity.SD_114;
            set
            {
                if (Entity.SD_114 == value) return;
                Entity.SD_114 = value;
                BANK_RASCH_SCHET_DC = SD_114.DOC_CODE;
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
                TABELNUMBER = Entity.SD_2?.TABELNUMBER;
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
                CA_DC = Entity.SD_22?.DOC_CODE;
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

        public SD_34 SD_34
        {
            get => Entity.SD_34;
            set
            {
                if (Entity.SD_34 == value) return;
                Entity.SD_34 = value;
                RaisePropertyChanged();
            }
        }

        public VD_46 VD_46
        {
            get => Entity.VD_46;
            set
            {
                if (Entity.VD_46 == value) return;
                Entity.VD_46 = value;
                RaisePropertyChanged();
            }
        }

        public SD_90 SD_90
        {
            get => Entity.SD_90;
            set
            {
                if (Entity.SD_90 == value) return;
                Entity.SD_90 = value;
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

        public SD_33 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public EntityLoadCodition LoadCondition { get; set; }

        public List<SD_33> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        private void LoadReferences()
        {
            if (Entity?.CA_DC != null) Cash = MainReferences.Cashs[(decimal) Entity.CA_DC];
            myKontragentType = CashKontragentType.NotChoice;
            if (KONTRAGENT_DC != null)
                myKontragentType = CashKontragentType.Kontragent;
            if (TABELNUMBER != null)
                myKontragentType = CashKontragentType.Employee;
            if (BANK_RASCH_SCHET_DC != null)
                myKontragentType = CashKontragentType.Bank;
            if (RASH_ORDER_FROM_DC != null)
                myKontragentType = CashKontragentType.Cash;
            IsKontrSelectEnable = myKontragentType != CashKontragentType.NotChoice;
            if (Entity?.CRS_DC != null) Currency = MainReferences.Currencies[(decimal) Entity?.CRS_DC];
            if (SFACT_DC != null) SFactName = SFact(SFACT_DC.Value);
            if (SHPZ_DC != null) SDRSchet = MainReferences.SDRSchets[SHPZ_DC.Value];
            if (BANK_RASCH_SCHET_DC != null) BankAccount = MainReferences.BankAccounts[BANK_RASCH_SCHET_DC.Value];
            if (RASH_ORDER_FROM_DC != null) RashodOrderFromName = RashodOrderFrom(RASH_ORDER_FROM_DC.Value);
            RaisePropertyChanged(nameof(IsKontrSelectEnable));
            RaisePropertyChanged(nameof(KontragentType));
            RaisePropertyChanged(nameof(Kontragent));
        }

        private string RashodOrderFrom(decimal dc)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var doc = ctx.SD_34.Include(_ => _.SD_22).FirstOrDefault(_ => _.DOC_CODE == dc);
                return doc == null ? null : MainReferences.Cashs[(decimal) doc.CA_DC].Name;
            }
        }

        private string SFact(decimal dc)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var doc = ctx.SD_84.FirstOrDefault(_ => _.DOC_CODE == dc);
                return doc == null
                    ? null
                    : $"№ {doc.SF_IN_NUM}/{doc.SF_OUT_NUM} от {doc.SF_DATE.ToShortDateString()} {doc.SF_CRS_SUMMA_K_OPLATE} {MainReferences.Currencies[doc.SF_CRS_DC]} {doc.SF_NOTE}";
            }
        }

        public virtual void Save(SD_33 doc)
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

        public void UpdateFrom(SD_33 ent)
        {
            NUM_ORD = ent.NUM_ORD;
            SUMM_ORD = ent.SUMM_ORD;
            NAME_ORD = ent.NAME_ORD;
            OSN_ORD = ent.OSN_ORD;
            NOTES_ORD = ent.NOTES_ORD;
            DATE_ORD = ent.DATE_ORD;
            CODE_CASS = ent.CODE_CASS;
            TABELNUMBER = ent.TABELNUMBER;
            OP_CODE = ent.OP_CODE;
            CA_DC = ent.CA_DC;
            KONTRAGENT_DC = ent.KONTRAGENT_DC;
            SHPZ_ORD = ent.SHPZ_ORD;
            SHPZ_DC = ent.SHPZ_DC;
            RASH_ORDER_FROM_DC = ent.RASH_ORDER_FROM_DC;
            SFACT_DC = ent.SFACT_DC;
            NCODE = ent.NCODE;
            POS_DC = ent.POS_DC;
            POS_PREV_OST = ent.POS_PREV_OST;
            POS_PRIHOD = ent.POS_PRIHOD;
            POS_NOW_OST = ent.POS_NOW_OST;
            KONTR_CRS_DC = ent.KONTR_CRS_DC;
            CRS_KOEF = ent.CRS_KOEF;
            CRS_SUMMA = ent.CRS_SUMMA;
            CHANGE_ORD = ent.CHANGE_ORD;
            CRS_DC = ent.CRS_DC;
            UCH_VALUTA_DC = ent.UCH_VALUTA_DC;
            SUMMA_V_UCH_VALUTE = ent.SUMMA_V_UCH_VALUTE;
            UCH_VALUTA_RATE = ent.UCH_VALUTA_RATE;
            SUM_NDS = ent.SUM_NDS;
            SFACT_OPLACHENO = ent.SFACT_OPLACHENO;
            SFACT_CRS_RATE = ent.SFACT_CRS_RATE;
            SFACT_CRS_DC = ent.SFACT_CRS_DC;
            BANK_RASCH_SCHET_DC = ent.BANK_RASCH_SCHET_DC;
            CREATOR = ent.CREATOR;
            RUB_SUMMA = ent.RUB_SUMMA;
            RUB_RATE = ent.RUB_RATE;
            OBRATNY_RASCHET = ent.OBRATNY_RASCHET;
            KONTR_CRS_SUM_CORRECT_PERCENT = ent.KONTR_CRS_SUM_CORRECT_PERCENT;
            V_TOM_CHISLE = ent.V_TOM_CHISLE;
            KONTR_FROM_DC = ent.KONTR_FROM_DC;
            SFACT_FLAG = ent.SFACT_FLAG;
            TSTAMP = ent.TSTAMP;
            SD_114 = ent.SD_114;
            SD_2 = ent.SD_2;
            SD_22 = ent.SD_22;
            SD_301 = ent.SD_301;
            SD_3011 = ent.SD_3011;
            SD_3012 = ent.SD_3012;
            SD_3013 = ent.SD_3013;
            SD_303 = ent.SD_303;
            SD_34 = ent.SD_34;
            VD_46 = ent.VD_46;
            SD_90 = ent.SD_90;
            SD_84 = ent.SD_84;
            SD_43 = ent.SD_43;
        }

        public void UpdateTo(SD_33 ent)
        {
            ent.NUM_ORD = NUM_ORD;
            ent.SUMM_ORD = SUMM_ORD;
            ent.NAME_ORD = NAME_ORD;
            ent.OSN_ORD = OSN_ORD;
            ent.NOTES_ORD = NOTES_ORD;
            ent.DATE_ORD = DATE_ORD;
            ent.CODE_CASS = CODE_CASS;
            ent.TABELNUMBER = TABELNUMBER;
            ent.OP_CODE = OP_CODE;
            ent.CA_DC = CA_DC;
            ent.KONTRAGENT_DC = KONTRAGENT_DC;
            ent.SHPZ_ORD = SHPZ_ORD;
            ent.SHPZ_DC = SHPZ_DC;
            ent.RASH_ORDER_FROM_DC = RASH_ORDER_FROM_DC;
            ent.SFACT_DC = SFACT_DC;
            ent.NCODE = NCODE;
            ent.POS_DC = POS_DC;
            ent.POS_PREV_OST = POS_PREV_OST;
            ent.POS_PRIHOD = POS_PRIHOD;
            ent.POS_NOW_OST = POS_NOW_OST;
            ent.KONTR_CRS_DC = KONTR_CRS_DC;
            ent.CRS_KOEF = CRS_KOEF;
            ent.CRS_SUMMA = CRS_SUMMA;
            ent.CHANGE_ORD = CHANGE_ORD;
            ent.CRS_DC = CRS_DC;
            ent.UCH_VALUTA_DC = UCH_VALUTA_DC;
            ent.SUMMA_V_UCH_VALUTE = SUMMA_V_UCH_VALUTE;
            ent.UCH_VALUTA_RATE = UCH_VALUTA_RATE;
            ent.SUM_NDS = SUM_NDS;
            ent.SFACT_OPLACHENO = SFACT_OPLACHENO;
            ent.SFACT_CRS_RATE = SFACT_CRS_RATE;
            ent.SFACT_CRS_DC = SFACT_CRS_DC;
            ent.BANK_RASCH_SCHET_DC = BANK_RASCH_SCHET_DC;
            ent.CREATOR = CREATOR;
            ent.RUB_SUMMA = RUB_SUMMA;
            ent.RUB_RATE = RUB_RATE;
            ent.OBRATNY_RASCHET = OBRATNY_RASCHET;
            ent.KONTR_CRS_SUM_CORRECT_PERCENT = KONTR_CRS_SUM_CORRECT_PERCENT;
            ent.V_TOM_CHISLE = V_TOM_CHISLE;
            ent.KONTR_FROM_DC = KONTR_FROM_DC;
            ent.SFACT_FLAG = SFACT_FLAG;
            ent.TSTAMP = TSTAMP;
            ent.SD_114 = SD_114;
            ent.SD_2 = SD_2;
            ent.SD_22 = SD_22;
            ent.SD_301 = SD_301;
            ent.SD_3011 = SD_3011;
            ent.SD_3012 = SD_3012;
            ent.SD_3013 = SD_3013;
            ent.SD_303 = SD_303;
            ent.SD_34 = SD_34;
            ent.VD_46 = VD_46;
            ent.SD_90 = SD_90;
            ent.SD_84 = SD_84;
            ent.SD_43 = SD_43;
        }

        public SD_33 DefaultValue()
        {
            return new SD_33
            {
                DOC_CODE = -1,
                DATE_ORD = DateTime.Today
            };
        }

        public SD_33 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public SD_33 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_33 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_33 Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }

    public static class SD_33LayoutData_FluentAPI
    {
        public static void BuildMetadata(MetadataBuilder<CashIn> builder)
        {
            builder.Property(_ => _.DocCode).NotAutoGenerated();
            builder.Property(_ => _.Id).NotAutoGenerated();
            builder.Property(_ => _.Code).NotAutoGenerated();
            builder.Property(_ => _.Note).NotAutoGenerated();
            builder.Property(_ => _.DOC_CODE).NotAutoGenerated();
            builder.Property(_ => _.Entity).NotAutoGenerated();
            builder.Property(_ => _.LoadCondition).NotAutoGenerated();
            builder.Property(_ => _.ParentDC).NotAutoGenerated();
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
            builder.Property(_ => _.SD_34).NotAutoGenerated();
            builder.Property(_ => _.VD_46).NotAutoGenerated();
            builder.Property(_ => _.SD_90).NotAutoGenerated();
            builder.Property(_ => _.SD_84).NotAutoGenerated();
            builder.Property(_ => _.SD_43).NotAutoGenerated();
            builder.Property(_ => _.CODE_CASS).NotAutoGenerated();
            builder.Property(_ => _.TABELNUMBER).NotAutoGenerated();
            builder.Property(x => x.Employee).NotAutoGenerated();
            builder.Property(x => x.CA_DC).NotAutoGenerated();
            builder.Property(x => x.KONTRAGENT_DC).NotAutoGenerated();
            builder.Property(x => x.SHPZ_ORD).NotAutoGenerated();
            builder.Property(x => x.SHPZ_DC).NotAutoGenerated();
            builder.Property(x => x.RASH_ORDER_FROM_DC).NotAutoGenerated();
            builder.Property(x => x.RashodOrderFromName).NotAutoGenerated();
            builder.Property(x => x.SFACT_DC).NotAutoGenerated();
            builder.Property(x => x.POS_DC).NotAutoGenerated();
            builder.Property(x => x.POS_NOW_OST).NotAutoGenerated();
            builder.Property(x => x.POS_PRIHOD).NotAutoGenerated();
            builder.Property(x => x.POS_PREV_OST).NotAutoGenerated();
            builder.Property(x => x.KONTR_CRS_DC).NotAutoGenerated();
            builder.Property(x => x.CRS_KOEF).NotAutoGenerated();
            builder.Property(x => x.CHANGE_ORD).NotAutoGenerated();
            builder.Property(x => x.CRS_DC).NotAutoGenerated();
            builder.Property(x => x.UCH_VALUTA_DC).NotAutoGenerated();
            builder.Property(x => x.SUMMA_V_UCH_VALUTE).NotAutoGenerated();
            builder.Property(x => x.UCH_VALUTA_RATE).NotAutoGenerated();
            builder.Property(x => x.SUM_NDS).NotAutoGenerated();
            builder.Property(x => x.SFACT_OPLACHENO).NotAutoGenerated();
            builder.Property(x => x.SFACT_CRS_RATE).NotAutoGenerated();
            builder.Property(x => x.SFACT_CRS_DC).NotAutoGenerated();
            builder.Property(x => x.BANK_RASCH_SCHET_DC).NotAutoGenerated();
            builder.Property(x => x.BankAccount).NotAutoGenerated();
            builder.Property(x => x.RUB_RATE).NotAutoGenerated();
            builder.Property(x => x.RUB_SUMMA).NotAutoGenerated();
            builder.Property(x => x.OBRATNY_RASCHET).NotAutoGenerated();
            builder.Property(x => x.V_TOM_CHISLE).NotAutoGenerated();
            builder.Property(x => x.KONTR_FROM_DC).NotAutoGenerated();
            builder.Property(x => x.SFACT_FLAG).NotAutoGenerated();
            builder.Property(x => x.OP_CODE).NotAutoGenerated();
            builder.Property(x => x.IsAccessRight).NotAutoGenerated();
            builder.Property(_ => _.IsKontrSelectEnable).NotAutoGenerated();
            builder.Property(_ => _.IsKontrSummaEnabled).NotAutoGenerated();
            builder.Property(_ => _.IsSummaEnabled).NotAutoGenerated();
            builder.Property(_ => _.KONTR_CRS_SUM_CORRECT_PERCENT).NotAutoGenerated();
            builder.Property(x => x.Currency)
                .DisplayName("Валюта");
            builder.Property(x => x.Cash)
                .DisplayName("Касса");
            builder.Property(x => x.KontragentType)
                .DisplayName("Тип контрагента");
            builder.Property(x => x.DATE_ORD)
                .DisplayName("Дата");
            builder.Property(x => x.SUMM_ORD)
                .DisplayName("Сумма").DisplayFormatString("n2");
            builder.Property(x => x.SDRSchet)
                .DisplayName("Счет доходов/расходов");
            builder.Property(x => x.State)
                .DisplayName("Статус");
            builder.Property(x => x.NUM_ORD)
                .DisplayName("№");
            builder.Property(x => x.NAME_ORD)
                .DisplayName("Наименование");
            builder.Property(x => x.OSN_ORD)
                .DisplayName("Основание");
            builder.Property(x => x.NOTES_ORD)
                .DisplayName("Примечание");
            builder.Property(x => x.Kontragent)
                .DisplayName("Контрагент");
            builder.Property(x => x.SFactName)
                .DisplayName("С/фактура");
            builder.Property(x => x.CRS_SUMMA).DisplayFormatString("n2")
                .DisplayName("Сумма контрагенту");
            builder.Property(x => x.NCODE)
                .DisplayName("в/о зарплаты").DisplayFormatString("n0");
            builder.Property(x => x.CREATOR)
                .DisplayName("Создатель");
            builder.Property(x => x.IsBackCalc)
                .DisplayName("Обратный расчет");
            builder.Property(x => x.PercentForKontragent)
                .DisplayName("%").DisplayFormatString("n2");
            // @formatter:off
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
                    .ContainsProperty(_ => _.SFactName)
                    .GroupBox("Контрагент", Orientation.Horizontal)
                        .ContainsProperty(_ => _.KontragentType)
                        .ContainsProperty(_ => _.Kontragent)
                        .ContainsProperty(_ => _.CRS_SUMMA)
                        .ContainsProperty(_ => _.PercentForKontragent)
                        .ContainsProperty(_ => _.IsBackCalc)
                    .EndGroup()
                    .GroupBox("Дополнения", Orientation.Vertical)
                        .ContainsProperty(_ => _.SDRSchet)
                        .ContainsProperty(_ => _.NAME_ORD)
                        .ContainsProperty(_ => _.OSN_ORD)
                        .ContainsProperty(_ => _.NOTES_ORD)
                    .EndGroup()
                .EndGroup();
            // @formatter:on
        }
    }
}