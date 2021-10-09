using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Windows.Input;
using Core.EntityViewModel.Cash;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using Helper;
using Newtonsoft.Json;

namespace Core.EntityViewModel.Bank
{
    [MetadataType(typeof(DataAnnotationsTD_101ViewModel))]
    public sealed class BankOperationsViewModel : RSViewModelBase, IEntity<TD_101>
    {
        public decimal? DeltaPrihod = 0;
        public decimal? DeltaRashod = 0;
        private BankAccount myBankAccount;
        public BankAccount myBankAccountIn;
        public BankAccount myBankAccountOut;
        private int? myBankFromTransactionCode;
        private BankOperationType myBankOperationType;
        private CashOrder myCashIn;
        private CashOrder myCashOut;
        private Currency myCurrency;
        private TD_101 myEntity;
        private Kontragent myKontragent;
        private Kontragent myPayment;
        private string mySFName;
        private SDRSchet mySHPZ;

        public BankOperationsViewModel()
        {
            Entity = new TD_101
            {
                DOC_CODE = -1,
                SD_101 = new SD_101()
            };
            BankOperationType = BankOperationType.NotChoice;

            RaisePropertyChanged(nameof(SHPZList));
        }

        public BankOperationsViewModel(TD_101 entity)
        {
            Entity = entity ?? new TD_101
            {
                DOC_CODE = -1,
                SD_101 = new SD_101()
            };
            SHPZList = new List<SDRSchet>(MainReferences.SDRSchets.Values.ToList());
            updateReferences();
            RaisePropertyChanged(nameof(SHPZList));
        }

        public bool NotAllowSummaPrihodChanged => BankOperationType == BankOperationType.BankIn ||
                                                  BankOperationType == BankOperationType.CashIn;

        public bool NotAllowSummaRashodChanged => BankOperationType == BankOperationType.BankOut ||
                                                  BankOperationType == BankOperationType.CashOut;

        public bool IsChangeTypeEnable => State == RowStatus.NewRow;
        public bool IsNotCurrencyChange => Entity.IsCurrencyChange == false;

        public BankOperationType BankOperationType
        {
            get => myBankOperationType;
            set
            {
                if (myBankOperationType == value) return;
                myBankOperationType = value;
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

        private string myAccuredInfo;
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

        public BankAccount BankAccount
        {
            get => myBankAccount;
            set
            {
                if (myBankAccount != null && myBankAccount.Equals(value)) return;
                myBankAccount = value;
                RaisePropertyChanged();
            }
        }

        public BankAccount BankAccountOut
        {
            get => myBankAccountOut;
            set
            {
                if (myBankAccountOut != null && myBankAccountOut.Equals(value)) return;
                myBankAccountOut = value;
                RaisePropertyChanged();
            }
        }

        public BankAccount BankAccountIn
        {
            get => myBankAccountIn;
            set
            {
                if (myBankAccountIn != null && myBankAccountIn.Equals(value)) return;
                myBankAccountIn = value;
                if (myBankAccountIn != null)
                    Entity.BankAccountDC = myBankAccountIn.DocCode;
                RaisePropertyChanged();
            }
        }

        public bool IsCurrencyChange => Entity.IsCurrencyChange ?? false;

        public int? BankFromTransactionCode
        {
            get => myBankFromTransactionCode;
            set
            {
                if (myBankFromTransactionCode == value) return;
                myBankFromTransactionCode = value;
                Entity.BankFromTransactionCode = myBankFromTransactionCode;
                RaisePropertyChanged();
            }
        }

        public SDRSchet SHPZ
        {
            get => mySHPZ;
            set
            {
                if (mySHPZ != null && mySHPZ.Equals(value)) return;
                mySHPZ = value;
                if (mySHPZ != null)
                    Entity.VVT_SHPZ_DC = mySHPZ.DocCode;
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
                if (myCurrency != null)
                    Entity.VVT_CRS_DC = myCurrency.DOC_CODE;
                RaisePropertyChanged();
            }
        }

        public Kontragent Kontragent
        {
            get => myKontragent;
            set
            {
                if (myKontragent != null && myKontragent.Equals(value)) return;
                myKontragent = value;
                if (myKontragent != null)
                    Entity.VVT_KONTRAGENT = myKontragent.DocCode;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(KontragentName));
            }
        }

        public Kontragent Payment
        {
            get => myPayment;
            set
            {
                if (myPayment != null && myPayment.Equals(value)) return;
                myPayment = value;
                if (myPayment != null)
                    Entity.VVT_PLATEL_POLUCH_DC = myPayment.DOC_CODE;
                RaisePropertyChanged();
            }
        }

        public ICommand SFNameRemoveCommand
        {
            get { return new Command(SFNameRemove, _ => !string.IsNullOrEmpty(SFName)); }
        }

        public string KontragentName => name();

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

        public DateTime Date
        {
            get => Entity.SD_101.VV_START_DATE;
            set
            {
                if (Entity.SD_101.VV_START_DATE == value) return;
                Entity.SD_101.VV_START_DATE = value;
                Entity.SD_101.VV_STOP_DATE = value;
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

        public string VVT_DOC_NUM
        {
            get => Entity.VVT_DOC_NUM;
            set
            {
                if (Entity.VVT_DOC_NUM == value) return;
                Entity.VVT_DOC_NUM = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_VAL_PRIHOD
        {
            get => Entity.VVT_VAL_PRIHOD;
            set
            {
                if (Entity.VVT_VAL_PRIHOD == value) return;
                DeltaPrihod = value - Entity.VVT_VAL_PRIHOD;
                Entity.VVT_VAL_PRIHOD = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_VAL_RASHOD
        {
            get => Entity.VVT_VAL_RASHOD;
            set
            {
                if (Entity.VVT_VAL_RASHOD == value) return;
                DeltaRashod = value - Entity.VVT_VAL_RASHOD;
                Entity.VVT_VAL_RASHOD = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_KONTRAGENT
        {
            get => Entity.VVT_KONTRAGENT;
            set
            {
                if (Entity.VVT_KONTRAGENT == value) return;
                Entity.VVT_KONTRAGENT = value;
                RaisePropertyChanged();
            }
        }

        public decimal VVT_CRS_DC
        {
            get => Entity.VVT_CRS_DC;
            set
            {
                if (Entity.VVT_CRS_DC == value) return;
                Entity.VVT_CRS_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_PLAT_PORUCH_DC
        {
            get => Entity.VVT_PLAT_PORUCH_DC;
            set
            {
                if (Entity.VVT_PLAT_PORUCH_DC == value) return;
                Entity.VVT_PLAT_PORUCH_DC = value;
                RaisePropertyChanged();
            }
        }

        public short? VVT_KURS_RAZN
        {
            get => Entity.VVT_KURS_RAZN;
            set
            {
                if (Entity.VVT_KURS_RAZN == value) return;
                Entity.VVT_KURS_RAZN = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_RUB_PRIHOD
        {
            get => Entity.VVT_RUB_PRIHOD;
            set
            {
                if (Entity.VVT_RUB_PRIHOD == value) return;
                Entity.VVT_RUB_PRIHOD = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_RUB_RASHOD
        {
            get => Entity.VVT_RUB_RASHOD;
            set
            {
                if (Entity.VVT_RUB_RASHOD == value) return;
                Entity.VVT_RUB_RASHOD = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_SHPZ_DC
        {
            get => Entity.VVT_SHPZ_DC;
            set
            {
                if (Entity.VVT_SHPZ_DC == value) return;
                Entity.VVT_SHPZ_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_PLATEL_POLUCH_DC
        {
            get => Entity.VVT_PLATEL_POLUCH_DC;
            set
            {
                if (Entity.VVT_PLATEL_POLUCH_DC == value) return;
                Entity.VVT_PLATEL_POLUCH_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_KONTR_CRS_DC
        {
            get => Entity.VVT_KONTR_CRS_DC;
            set
            {
                if (Entity.VVT_KONTR_CRS_DC == value) return;
                Entity.VVT_KONTR_CRS_DC = value;
                RaisePropertyChanged();
            }
        }

        public double? VVT_KONTR_CRS_RATE
        {
            get => Entity.VVT_KONTR_CRS_RATE;
            set
            {
                if (Entity.VVT_KONTR_CRS_RATE - value <= 0.001) return;
                Entity.VVT_KONTR_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_KONTR_CRS_SUMMA
        {
            get => Entity.VVT_KONTR_CRS_SUMMA;
            set
            {
                if (Entity.VVT_KONTR_CRS_SUMMA == value) return;
                Entity.VVT_KONTR_CRS_SUMMA = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_UCHET_VALUTA_DC
        {
            get => Entity.VVT_UCHET_VALUTA_DC;
            set
            {
                if (Entity.VVT_UCHET_VALUTA_DC == value) return;
                Entity.VVT_UCHET_VALUTA_DC = value;
                RaisePropertyChanged();
            }
        }

        public double? VVT_UCHET_VALUTA_RATE
        {
            get => Entity.VVT_UCHET_VALUTA_RATE;
            set
            {
                if (Entity.VVT_UCHET_VALUTA_RATE - value <= 0.001) return;
                Entity.VVT_UCHET_VALUTA_RATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_SUMMA_V_UCHET_VALUTE
        {
            get => Entity.VVT_SUMMA_V_UCHET_VALUTE;
            set
            {
                if (Entity.VVT_SUMMA_V_UCHET_VALUTE == value) return;
                Entity.VVT_SUMMA_V_UCHET_VALUTE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_SFACT_POSTAV_DC
        {
            get => Entity.VVT_SFACT_POSTAV_DC;
            set
            {
                if (Entity.VVT_SFACT_POSTAV_DC == value) return;
                Entity.VVT_SFACT_POSTAV_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_SFACT_CLIENT_DC
        {
            get => Entity.VVT_SFACT_CLIENT_DC;
            set
            {
                if (Entity.VVT_SFACT_CLIENT_DC == value) return;
                Entity.VVT_SFACT_CLIENT_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_SF_OPLACHENO
        {
            get => Entity.VVT_SF_OPLACHENO;
            set
            {
                if (Entity.VVT_SF_OPLACHENO == value) return;
                Entity.VVT_SF_OPLACHENO = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_SF_CRS_DC
        {
            get => Entity.VVT_SF_CRS_DC;
            set
            {
                if (Entity.VVT_SF_CRS_DC == value) return;
                Entity.VVT_SF_CRS_DC = value;
                RaisePropertyChanged();
            }
        }

        public double? VVT_SF_CRS_RATE
        {
            get => Entity.VVT_SF_CRS_RATE;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Entity.VVT_SF_CRS_RATE == value) return;
                Entity.VVT_SF_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VVT_RASH_KASS_ORDER_DC
        {
            get => Entity.VVT_RASH_KASS_ORDER_DC;
            set
            {
                if (Entity.VVT_RASH_KASS_ORDER_DC == value) return;
                Entity.VVT_RASH_KASS_ORDER_DC = value;
                RaisePropertyChanged();
            }
        }

        public CashOrder CashOut
        {
            get => myCashOut;
            set
            {
                if (myCashOut != null && myCashOut.Equals(value)) return;
                myCashOut = value;
                if (myCashOut != null)
                    VVT_RASH_KASS_ORDER_DC = myCashOut.DocCode;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(KontragentName));
            }
        }

        public decimal? VVT_KASS_PRIH_ORDER_DC
        {
            get => Entity.VVT_KASS_PRIH_ORDER_DC;
            set
            {
                if (Entity.VVT_KASS_PRIH_ORDER_DC == value) return;
                Entity.VVT_KASS_PRIH_ORDER_DC = value;
                RaisePropertyChanged();
            }
        }

        public CashOrder CashIn
        {
            get => myCashIn;
            set
            {
                if (myCashIn != null && myCashIn.Equals(value)) return;
                myCashIn = value;
                if (myCashIn != null)
                    VVT_KASS_PRIH_ORDER_DC = myCashIn.DocCode;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(KontragentName));
            }
        }

        public decimal? VVT_DOG_OTGR_DC
        {
            get => Entity.VVT_DOG_OTGR_DC;
            set
            {
                if (Entity.VVT_DOG_OTGR_DC == value) return;
                Entity.VVT_DOG_OTGR_DC = value;
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

        public SD_101 SD_101
        {
            get => Entity.SD_101;
            set
            {
                if (Entity.SD_101 == value) return;
                Entity.SD_101 = value;
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
            get
            {
                if (Entity.SD_3011 == null) return Entity.SD_301;
                return Entity.SD_3011;
            }
            set
            {
                if (Entity.SD_3011 == value) return;
                Entity.SD_3011 = value;
                RaisePropertyChanged();
            }
        }

        public SD_301 SD_3012
        {
            get => Entity.SD_3012 != null ? Entity.SD_3012 : Entity.SD_301;
            set
            {
                if (Entity.SD_3012 == value) return;
                Entity.SD_3012 = value;
                RaisePropertyChanged();
            }
        }

        public SD_301 SD_3013
        {
            get => Entity.SD_3013 ?? Entity.SD_301;
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

        public SD_33 SD_33
        {
            get => Entity.SD_33;
            set
            {
                if (Entity.SD_33 == value) return;
                Entity.SD_33 = value;
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

        public SD_42 SD_42
        {
            get => Entity.SD_42;
            set
            {
                if (Entity.SD_42 == value) return;
                Entity.SD_42 = value;
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

        public SD_43 SD_431
        {
            get => Entity.SD_431;
            set
            {
                if (Entity.SD_431 == value) return;
                Entity.SD_431 = value;
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

        public SD_9 SD_9
        {
            get => Entity.SD_9;
            set
            {
                if (Entity.SD_9 == value) return;
                Entity.SD_9 = value;
                RaisePropertyChanged();
            }
        }

        public string SFName
        {
            get => mySFName;
            set
            {
                if (mySFName == value) return;
                mySFName = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }

        public bool IsAccessRight { get; set; }

        public TD_101 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public override string ToString()
        {
            if (VVT_VAL_PRIHOD > 0)
                return myBankAccount +
                       $" {BankOperationType.GetDisplayAttributesFrom(typeof(BankOperationType)).Name} на сумму {VVT_VAL_PRIHOD ?? 0} {Currency}";
            return myBankAccount + $" {BankOperationType.GetDisplayAttributesFrom(typeof(BankOperationType)).Name} " +
                   $"{KontragentName}" +
                   $"на сумму {VVT_VAL_RASHOD ?? 0} {Currency}";
        }

        public List<TD_101> LoadList()
        {
            throw new NotImplementedException();
        }

        private void SFNameRemove(object obj)
        {
            Entity.VVT_SFACT_CLIENT_DC = null;
            Entity.VVT_SFACT_POSTAV_DC = null;
            SFName = null;
        }

        private string name()
        {
            if (Kontragent != null) return Kontragent.Name;
            if (CashIn != null) return CashIn.Cash.Name;
            if (CashOut != null) return CashOut.ToString();
            if (BankAccountIn != null) return BankAccountIn.BankName + " " + BankAccountIn.Account;
            if (BankAccountOut != null) return BankAccountOut.BankName + " " + BankAccountOut.Account;
            return null;
        }

        private void updateReferences()
        {
            if (Entity.SD_34 != null)
                myCashOut = new CashOrder(Entity.SD_34);
            if (Entity.SD_33 != null)
                myCashIn = new CashOrder(Entity.SD_33);
            if (Entity.SD_101 != null)
                myBankAccount = MainReferences.BankAccounts[Entity.SD_101.VV_ACC_DC];
            if (Entity.VVT_KONTRAGENT != null)
                myKontragent = MainReferences.GetKontragent(Entity.VVT_KONTRAGENT);
            if (Entity.VVT_SHPZ_DC != null)
                mySHPZ = MainReferences.SDRSchets[Entity.VVT_SHPZ_DC.Value];
            if (Entity.VVT_PLATEL_POLUCH_DC != null)
                myPayment = MainReferences.GetKontragent(Entity.VVT_PLATEL_POLUCH_DC);
            if (MainReferences.Currencies.ContainsKey(Entity.VVT_CRS_DC))
                myCurrency = MainReferences.Currencies[Entity.VVT_CRS_DC];
            if (Entity.VVT_SFACT_CLIENT_DC != null)
                if (Entity.SD_84 != null)
                    SFName =
                        $"С/ф №{Entity.SD_84.SF_IN_NUM}/{Entity.SD_84.SF_OUT_NUM} от " +
                        $"{Entity.SD_84.SF_DATE} на {Entity.SD_84.SF_CRS_SUMMA_K_OPLATE} " +
                        $"{MainReferences.Currencies[Entity.SD_84.SF_CRS_DC]}";
            if (Entity.VVT_SFACT_POSTAV_DC != null)
                SFName =
                    $"С/ф №{Entity.SD_26.SF_IN_NUM}/{Entity.SD_26.SF_POSTAV_NUM} от " +
                    $"{Entity.SD_26.SF_POSTAV_DATE} на {Entity.SD_26.SF_CRS_SUMMA} " +
                    // ReSharper disable once PossibleInvalidOperationException
                    $"{MainReferences.Currencies[(decimal)Entity.SD_26.SF_CRS_DC]}";
            if (Entity.AccuredAmountOfSupplierRow != null)
            {
                var acc = Entity.AccuredAmountOfSupplierRow;
                var snum = string.IsNullOrWhiteSpace(acc.AccruedAmountOfSupplier.DocExtNum)
                    ? acc.AccruedAmountOfSupplier.DocInNum.ToString()
                    : $"{acc.AccruedAmountOfSupplier.DocInNum}/{acc.AccruedAmountOfSupplier.DocExtNum}";
                var nom = MainReferences.GetNomenkl(acc.NomenklDC);
                var snom = $"{nom}({nom.NomenklNumber})";
                AccuredInfo =
                    $"Прямой расход №{snum} от {acc.AccruedAmountOfSupplier.DocDate.ToShortDateString()} " +
                    $"{snom}";
            }
            updateBankInfo();
            BankOperationType = Kontragent != null ? BankOperationType.Kontragent :
                CashIn != null ? BankOperationType.CashIn :
                CashOut != null ? BankOperationType.CashOut :
                BankAccountIn != null ? BankOperationType.BankIn :
                BankAccountOut != null ? BankOperationType.BankOut :
                IsCurrencyChange ? BankOperationType.CurrencyChange :
                BankOperationType.NotChoice;
        }

        private void updateBankInfo()
        {
            if (Entity.BankAccountDC != null)
                BankAccountIn = MainReferences.BankAccounts[Entity.BankAccountDC.Value];
            if (Entity.BankFromTransactionCode != null)
                using (var dtx = GlobalOptions.GetEntities())
                {
                    var row = dtx.TD_101.Include(_ => _.SD_101)
                        .FirstOrDefault(_ => _.CODE == Entity.BankFromTransactionCode);
                    if (row == null) return;
                    BankAccountOut = MainReferences.BankAccounts[row.SD_101.VV_ACC_DC];
                }
        }

        public TD_101 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public TD_101 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Save(TD_101 doc)
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

        public void UpdateFrom(TD_101 ent)
        {
            Code = ent.CODE;
            VVT_DOC_NUM = ent.VVT_DOC_NUM;
            VVT_VAL_PRIHOD = ent.VVT_VAL_PRIHOD;
            VVT_VAL_RASHOD = ent.VVT_VAL_RASHOD;
            VVT_KONTRAGENT = ent.VVT_KONTRAGENT;
            VVT_CRS_DC = ent.VVT_CRS_DC;
            VVT_PLAT_PORUCH_DC = ent.VVT_PLAT_PORUCH_DC;
            VVT_KURS_RAZN = ent.VVT_KURS_RAZN;
            VVT_RUB_PRIHOD = ent.VVT_RUB_PRIHOD;
            VVT_RUB_RASHOD = ent.VVT_RUB_RASHOD;
            VVT_SHPZ_DC = ent.VVT_SHPZ_DC;
            VVT_PLATEL_POLUCH_DC = ent.VVT_PLATEL_POLUCH_DC;
            VVT_KONTR_CRS_DC = ent.VVT_KONTR_CRS_DC;
            VVT_KONTR_CRS_RATE = ent.VVT_KONTR_CRS_RATE;
            VVT_KONTR_CRS_SUMMA = ent.VVT_KONTR_CRS_SUMMA;
            VVT_UCHET_VALUTA_DC = ent.VVT_UCHET_VALUTA_DC;
            VVT_UCHET_VALUTA_RATE = ent.VVT_UCHET_VALUTA_RATE;
            VVT_SUMMA_V_UCHET_VALUTE = ent.VVT_SUMMA_V_UCHET_VALUTE;
            VVT_SFACT_POSTAV_DC = ent.VVT_SFACT_POSTAV_DC;
            VVT_SFACT_CLIENT_DC = ent.VVT_SFACT_CLIENT_DC;
            VVT_SF_OPLACHENO = ent.VVT_SF_OPLACHENO;
            VVT_SF_CRS_DC = ent.VVT_SF_CRS_DC;
            VVT_SF_CRS_RATE = ent.VVT_SF_CRS_RATE;
            VVT_RASH_KASS_ORDER_DC = ent.VVT_RASH_KASS_ORDER_DC;
            VVT_KASS_PRIH_ORDER_DC = ent.VVT_KASS_PRIH_ORDER_DC;
            VVT_DOG_OTGR_DC = ent.VVT_DOG_OTGR_DC;
            TSTAMP = ent.TSTAMP;
            SD_101 = ent.SD_101;
            SD_26 = ent.SD_26;
            SD_301 = ent.SD_301;
            SD_33 = ent.SD_33;
            SD_34 = ent.SD_34;
            SD_42 = ent.SD_42;
            SD_43 = ent.SD_43;
            SD_431 = ent.SD_431;
            SD_84 = ent.SD_84;
            SD_9 = ent.SD_9;
        }

        public void UpdateTo(TD_101 ent)
        {
            ent.CODE = Code;
            ent.VVT_DOC_NUM = VVT_DOC_NUM;
            ent.VVT_VAL_PRIHOD = VVT_VAL_PRIHOD;
            ent.VVT_VAL_RASHOD = VVT_VAL_RASHOD;
            ent.VVT_KONTRAGENT = VVT_KONTRAGENT;
            ent.VVT_CRS_DC = VVT_CRS_DC;
            ent.VVT_PLAT_PORUCH_DC = VVT_PLAT_PORUCH_DC;
            ent.VVT_KURS_RAZN = VVT_KURS_RAZN;
            ent.VVT_RUB_PRIHOD = VVT_RUB_PRIHOD;
            ent.VVT_RUB_RASHOD = VVT_RUB_RASHOD;
            ent.VVT_SHPZ_DC = VVT_SHPZ_DC;
            ent.VVT_PLATEL_POLUCH_DC = VVT_PLATEL_POLUCH_DC;
            ent.VVT_KONTR_CRS_DC = VVT_KONTR_CRS_DC;
            ent.VVT_KONTR_CRS_RATE = VVT_KONTR_CRS_RATE;
            ent.VVT_KONTR_CRS_SUMMA = VVT_KONTR_CRS_SUMMA;
            ent.VVT_UCHET_VALUTA_DC = VVT_UCHET_VALUTA_DC;
            ent.VVT_UCHET_VALUTA_RATE = VVT_UCHET_VALUTA_RATE;
            ent.VVT_SUMMA_V_UCHET_VALUTE = VVT_SUMMA_V_UCHET_VALUTE;
            ent.VVT_SFACT_POSTAV_DC = VVT_SFACT_POSTAV_DC;
            ent.VVT_SFACT_CLIENT_DC = VVT_SFACT_CLIENT_DC;
            ent.VVT_SF_OPLACHENO = VVT_SF_OPLACHENO;
            ent.VVT_SF_CRS_DC = VVT_SF_CRS_DC;
            ent.VVT_SF_CRS_RATE = VVT_SF_CRS_RATE;
            ent.VVT_RASH_KASS_ORDER_DC = VVT_RASH_KASS_ORDER_DC;
            ent.VVT_KASS_PRIH_ORDER_DC = VVT_KASS_PRIH_ORDER_DC;
            ent.VVT_DOG_OTGR_DC = VVT_DOG_OTGR_DC;
            ent.TSTAMP = TSTAMP;
            ent.SD_101 = SD_101;
            ent.SD_26 = SD_26;
            ent.SD_301 = SD_301;
            ent.SD_303 = SD_303;
            ent.SD_33 = SD_33;
            ent.SD_34 = SD_34;
            ent.SD_42 = SD_42;
            ent.SD_43 = SD_43;
            ent.SD_431 = SD_431;
            ent.SD_84 = SD_84;
            ent.SD_9 = SD_9;
        }


        public TD_101 DefaultValue()
        {
            throw new NotImplementedException();
        }

        public override object ToJson()
        {
            var res = new
            {
                DocCode,
                Code,
                Дата = SD_101.VV_START_DATE.ToShortDateString(),
                Банк = BankAccount.Name,
                Тип_контрагента = CustomFormat.GetEnumName(BankOperationType),
                Контрагент = KontragentName,
                Плательщик_Получатель = Payment?.Name,
                Приход = VVT_VAL_PRIHOD?.ToString("n2"),
                Расход = VVT_VAL_RASHOD?.ToString("n2"),
                Валюта = Currency.Name,
                Описание = VVT_DOC_NUM,
                Счет_фактура = SFName,
                Счет_доходов_расходов = SHPZ?.Name
            };
            return JsonConvert.SerializeObject(res);
        }

        #region compendiums

        public List<Currency> CurrencysCompendium => MainReferences.Currencies.Values.ToList();
        public List<SDRSchet> SHPZList { set; get; } = new(MainReferences.SDRSchets.Values.ToList());

        #endregion
    }

    public class DataAnnotationsTD_101ViewModel : DataAnnotationForFluentApiBase,
        IMetadataProvider<BankOperationsViewModel>
    {
        void IMetadataProvider<BankOperationsViewModel>.BuildMetadata(MetadataBuilder<BankOperationsViewModel> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.SFNameRemoveCommand).NotAutoGenerated();
            builder.Property(_ => _.CashIn).NotAutoGenerated();
            builder.Property(_ => _.CashOut).NotAutoGenerated();
            builder.Property(_ => _.BankAccount).NotAutoGenerated();
            builder.Property(_ => _.BankAccountIn).NotAutoGenerated();
            builder.Property(_ => _.BankAccountOut).NotAutoGenerated();
            builder.Property(_ => _.BankFromTransactionCode).NotAutoGenerated();
            builder.Property(_ => _.Kontragent).NotAutoGenerated();
            builder.Property(_ => _.VVT_DOC_NUM).AutoGenerated().ReadOnly().DisplayName("Описание");
            builder.Property(_ => _.BankOperationType).AutoGenerated().DisplayName("Тип контрагента");
            builder.Property(_ => _.KontragentName).AutoGenerated().ReadOnly().DisplayName("Контрагент");
            builder.Property(_ => _.Payment).AutoGenerated().ReadOnly().DisplayName("Плательщик-получатель");
            builder.Property(_ => _.Currency).AutoGenerated().ReadOnly().DisplayName("Валюта");
            builder.Property(_ => _.Date).AutoGenerated().ReadOnly().DisplayName("Дата");
            builder.Property(_ => _.VVT_VAL_PRIHOD).AutoGenerated().ReadOnly().DisplayName("Приход")
                .DisplayFormatString("n2");
            builder.Property(_ => _.VVT_VAL_RASHOD).AutoGenerated().ReadOnly().DisplayName("Расход")
                .DisplayFormatString("n2");
            builder.Property(_ => _.SHPZ).AutoGenerated().ReadOnly().DisplayName("Счет дох/расх");
            builder.Property(_ => _.SFName).AutoGenerated().ReadOnly().DisplayName("Счет-фактура");
            builder.Property(_ => _.AccuredId).NotAutoGenerated();
            builder.Property(_ => _.AccuredInfo).AutoGenerated().DisplayName("Прямые затраты");
        }
    }
}