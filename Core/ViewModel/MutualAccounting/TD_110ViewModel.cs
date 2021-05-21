using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;

// ReSharper disable InconsistentNaming
namespace Core.ViewModel.MutualAccounting
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class TD_110ViewModel : RSViewModelBase, IEntity<TD_110>, IEquatable<TD_110ViewModel>
    {
        public decimal MaxSumma = decimal.MaxValue;
        private TD_110 myEntity;
        private Kontragent myKontragent;
        private InvoiceClient mySFClient;
        private InvoiceProvider mySFProvider;
        private SDRSchet mySHPZ;
        private VzaimoraschetType myVzaimoraschType;

        public TD_110ViewModel()
        {
            Entity = new TD_110 {DOC_CODE = -1};
        }

        public TD_110ViewModel(TD_110 entity)
        {
            Entity = entity ?? DefaultValue();
            UpdateFrom(entity);
        }

        public override decimal DocCode
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value) return;
                Entity.DOC_CODE = value;
                base.DocCode = value;
                RaisePropertyChanged();
            }
        }

        public override string Note
        {
            get => Entity.VZT_DOC_NOTES;
            set
            {
                if (Entity.VZT_DOC_NOTES == value) return;
                Entity.VZT_DOC_NOTES = value;
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

        public decimal? VZT_CRS_SUMMA
        {
            get => Entity.VZT_CRS_SUMMA;
            set
            {
                if (Entity.VZT_CRS_SUMMA == value) return;
                if (value < 0)
                {
                    WindowManager.ShowMessage("Сумма не может быть меньше 0!", "Ошибка",
                        MessageBoxImage.Stop);
                    return;
                }

                if (value > MaxSumma && VZT_SFACT_DC != null)
                {
                    WindowManager.ShowMessage("Сумма не может быть больше сумму оплаты по счету!", "Ошибка",
                        MessageBoxImage.Stop);
                    return;
                }

                Entity.VZT_CRS_SUMMA = value;
                VZT_KONTR_CRS_SUMMA = VZT_1MYDOLZH_0NAMDOLZH == 0 ? -Entity.VZT_CRS_SUMMA : Entity.VZT_CRS_SUMMA;
                //VZT_CRS_POGASHENO = Entity.VZT_CRS_POGASHENO;
                if (Entity.VZT_CRS_DC == CurrencyCode.USD && VZT_KONTR_CRS_DC == CurrencyCode.RUB)
                    VZT_UCH_CRS_POGASHENO = Entity.VZT_CRS_SUMMA *
                                            (Entity.VZT_UCH_CRS_RATE == 0 ? 1 : Entity.VZT_UCH_CRS_RATE);
                else
                    VZT_UCH_CRS_POGASHENO = Entity.VZT_CRS_SUMMA /
                                            (Entity.VZT_UCH_CRS_RATE == 0 ? 1 : Entity.VZT_UCH_CRS_RATE);
                VZT_CRS_POGASHENO = Entity.VZT_CRS_SUMMA;
                RaisePropertyChanged();
            }
        }

        public decimal VZT_CRS_DC
        {
            get => Entity.VZT_CRS_DC;
            set
            {
                if (Entity.VZT_CRS_DC == value) return;
                Entity.VZT_CRS_DC = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Currency));
            }
        }

        public Currency Currency => KontragentCurrency;

        public decimal? VZT_SPOST_DC
        {
            get => Entity.VZT_SPOST_DC;
            set
            {
                if (Entity.VZT_SPOST_DC == value) return;
                Entity.VZT_SPOST_DC = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceProvider SFProvider
        {
            get => mySFProvider;
            set
            {
                if (mySFProvider == value) return;
                mySFProvider = value;
                VZT_SPOST_DC = mySFProvider?.DocCode;
                RaisePropertyChanged();
            }
        }

        public decimal? VZT_SFACT_DC
        {
            get => Entity.VZT_SFACT_DC;
            set
            {
                if (Entity.VZT_SFACT_DC == value) return;
                Entity.VZT_SFACT_DC = value;
                RaisePropertyChanged();
            }
        }

        public InvoiceClient SFClient
        {
            get => mySFClient;
            set
            {
                if (mySFClient == value) return;
                mySFClient = value;
                VZT_SFACT_DC = mySFClient?.DocCode;
                RaisePropertyChanged();
            }
        }

        public DateTime VZT_DOC_DATE
        {
            get => Entity.VZT_DOC_DATE;
            set
            {
                if (Entity.VZT_DOC_DATE == value) return;
                Entity.VZT_DOC_DATE = value;
                RaisePropertyChanged();
            }
        }

        public string VZT_DOC_NUM
        {
            get => Entity.VZT_DOC_NUM;
            set
            {
                if (Entity.VZT_DOC_NUM == value) return;
                Entity.VZT_DOC_NUM = value;
                RaisePropertyChanged();
            }
        }

        public string VZT_DOC_NOTES
        {
            get => Entity.VZT_DOC_NOTES;
            set
            {
                if (Entity.VZT_DOC_NOTES == value) return;
                Entity.VZT_DOC_NOTES = value;
                RaisePropertyChanged();
            }
        }

        public decimal VZT_KONTR_DC
        {
            get => Entity.VZT_KONTR_DC;
            set
            {
                if (Entity.VZT_KONTR_DC == value) return;
                Entity.VZT_KONTR_DC = value;
                //myKontragent = MainReferences.GetKontragent(Entity.VZT_KONTR_DC);
                RaisePropertyChanged();
                //RaisePropertyChanged(nameof(Kontragent));
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
                {
                    Entity.VZT_CRS_DC = myKontragent.BalansCurrency.DocCode;
                    Entity.VZT_KONTR_DC = myKontragent.DocCode;
                    Entity.VZT_KONTR_CRS_DC = myKontragent.BalansCurrency.DocCode;
                    Entity.VZT_KONTR_CRS_RATE = 1;
                }

                RaisePropertyChanged();
                RaisePropertyChanged(nameof(VZT_KONTR_DC));
                RaisePropertyChanged(nameof(VZT_KONTR_CRS_DC));
            }
        }

        public bool IsBalans => Kontragent != null && !Kontragent.IsBalans;

        public decimal? VZT_CRS_POGASHENO
        {
            get => Entity.VZT_CRS_POGASHENO;
            set
            {
                if (Entity.VZT_CRS_POGASHENO == value) return;
                Entity.VZT_CRS_POGASHENO = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VZT_UCH_CRS_POGASHENO
        {
            get => Entity.VZT_UCH_CRS_POGASHENO;
            set
            {
                if (Entity.VZT_UCH_CRS_POGASHENO == value) return;
                Entity.VZT_UCH_CRS_POGASHENO = value;
                RaisePropertyChanged();
            }
        }

        public decimal VZT_UCH_CRS_RATE
        {
            get => Entity.VZT_UCH_CRS_RATE == 0 ? 1 : Entity.VZT_UCH_CRS_RATE;
            set
            {
                if (Entity.VZT_UCH_CRS_RATE == value) return;
                Entity.VZT_UCH_CRS_RATE = value;
                if (Entity.VZT_CRS_DC == CurrencyCode.USD && VZT_KONTR_CRS_DC == CurrencyCode.RUB)
                    VZT_UCH_CRS_POGASHENO = Entity.VZT_CRS_SUMMA *
                                            (Entity.VZT_UCH_CRS_RATE == 0 ? 1 : Entity.VZT_UCH_CRS_RATE);
                else
                    VZT_UCH_CRS_POGASHENO = Entity.VZT_CRS_SUMMA /
                                            (Entity.VZT_UCH_CRS_RATE == 0 ? 1 : Entity.VZT_UCH_CRS_RATE);
                RaisePropertyChanged();
            }
        }

        public decimal VZT_VZAIMOR_TYPE_DC
        {
            get => Entity.VZT_VZAIMOR_TYPE_DC;
            set
            {
                if (Entity.VZT_VZAIMOR_TYPE_DC == value) return;
                Entity.VZT_VZAIMOR_TYPE_DC = value;
                RaisePropertyChanged();
            }
        }

        public VzaimoraschetType VzaimoraschType
        {
            get => myVzaimoraschType;
            set
            {
                if (myVzaimoraschType != null && myVzaimoraschType.Equals(value)) return;
                myVzaimoraschType = value;
                if (myVzaimoraschType != null)
                    Entity.VZT_VZAIMOR_TYPE_DC = myVzaimoraschType.DocCode;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(VZT_VZAIMOR_TYPE_DC));
            }
        }

        public short VZT_1MYDOLZH_0NAMDOLZH
        {
            get => Entity.VZT_1MYDOLZH_0NAMDOLZH;
            set
            {
                if (Entity.VZT_1MYDOLZH_0NAMDOLZH == value) return;
                Entity.VZT_1MYDOLZH_0NAMDOLZH = value;
                RaisePropertyChanged();
            }
        }

        public decimal VZT_KONTR_CRS_DC
        {
            get => Entity.VZT_KONTR_CRS_DC;
            set
            {
                if (Entity.VZT_KONTR_CRS_DC == value) return;
                Entity.VZT_KONTR_CRS_DC = value;
                RaisePropertyChanged();
            }
        }

        public Currency KontragentCurrency => Kontragent?.BalansCurrency;

        public decimal VZT_KONTR_CRS_RATE
        {
            get => Entity.VZT_KONTR_CRS_RATE;
            set
            {
                if (Entity.VZT_KONTR_CRS_RATE == value) return;
                Entity.VZT_KONTR_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VZT_KONTR_CRS_SUMMA
        {
            get => Entity.VZT_KONTR_CRS_SUMMA;
            set
            {
                if (Entity.VZT_KONTR_CRS_SUMMA == value) return;
                Entity.VZT_KONTR_CRS_SUMMA = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VZT_SHPZ_DC
        {
            get => Entity.VZT_SHPZ_DC;
            set
            {
                if (Entity.VZT_SHPZ_DC == value) return;
                Entity.VZT_SHPZ_DC = value;
                if (Entity.VZT_SHPZ_DC != null)
                    mySHPZ = MainReferences.SDRSchets[(decimal) Entity.VZT_SHPZ_DC];
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(SHPZ));
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
                    Entity.VZT_SHPZ_DC = mySHPZ.DocCode;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(VZT_SHPZ_DC));
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

        public SD_110 SD_110
        {
            get => Entity.SD_110;
            set
            {
                if (Entity.SD_110 == value) return;
                Entity.SD_110 = value;
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

        public SD_77 SD_77
        {
            get => Entity.SD_77;
            set
            {
                if (Entity.SD_77 == value) return;
                Entity.SD_77 = value;
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

        public TD_110 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        public EntityLoadCodition LoadCondition { get; set; }

        public List<TD_110> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public bool Equals(TD_110ViewModel other)
        {
            if (other == null) return false;
            return DocCode == other.DocCode && Code == other.Code;
        }

        public virtual TD_110 Load(decimal dc)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var row = ctx.TD_110
                    .Include(_ => _.SD_26)
                    .Include(_ => _.SD_301)
                    .Include(_ => _.SD_3011)
                    .Include(_ => _.SD_303)
                    .Include(_ => _.SD_43)
                    .Include(_ => _.SD_77)
                    .Include(_ => _.SD_84)
                    .FirstOrDefault(_ => _.DOC_CODE == dc);
                return row;
            }
        }

        public virtual TD_110 Load(Guid id)
        {
            return DefaultValue();
        }

        public virtual void Save(TD_110 doc)
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

        public void UpdateFrom(TD_110 ent)
        {
            Code = ent.CODE;
            VZT_CRS_SUMMA = ent.VZT_CRS_SUMMA;
            VZT_CRS_DC = ent.VZT_CRS_DC;
            VZT_SPOST_DC = ent.VZT_SPOST_DC;
            VZT_SFACT_DC = ent.VZT_SFACT_DC;
            VZT_DOC_DATE = ent.VZT_DOC_DATE;
            VZT_DOC_NUM = ent.VZT_DOC_NUM;
            VZT_DOC_NOTES = ent.VZT_DOC_NOTES;
            VZT_KONTR_DC = ent.VZT_KONTR_DC;
            Kontragent = SD_43 == null ? null : MainReferences.GetKontragent(SD_43.DOC_CODE);
            VZT_CRS_POGASHENO = ent.VZT_CRS_POGASHENO;
            VZT_UCH_CRS_POGASHENO = ent.VZT_UCH_CRS_POGASHENO;
            VZT_UCH_CRS_RATE = ent.VZT_UCH_CRS_RATE;
            VZT_VZAIMOR_TYPE_DC = ent.VZT_VZAIMOR_TYPE_DC;
            VzaimoraschType = ent.SD_77 != null
                ?
                //new VzaimoraschetType(ent.SD_77) 
                MainReferences.VzaimoraschetTypes[ent.SD_77.DOC_CODE]
                : null;
            VZT_1MYDOLZH_0NAMDOLZH = ent.VZT_1MYDOLZH_0NAMDOLZH;
            VZT_KONTR_CRS_DC = ent.VZT_KONTR_CRS_DC;
            VZT_KONTR_CRS_RATE = ent.VZT_KONTR_CRS_RATE;
            VZT_KONTR_CRS_SUMMA = ent.VZT_KONTR_CRS_SUMMA;
            VZT_SHPZ_DC = ent.VZT_SHPZ_DC;
            SHPZ = ent.SD_303 == null ? null : MainReferences.SDRSchets[ent.SD_303.DOC_CODE];
            TSTAMP = ent.TSTAMP;
            SD_110 = ent.SD_110;
            SD_26 = ent.SD_26;
            SD_301 = ent.SD_301;
            SD_3011 = ent.SD_3011;
            SD_303 = ent.SD_303;
            SD_43 = ent.SD_43;
            SD_77 = ent.SD_77;
            SD_84 = ent.SD_84;

            //TD_397 = ent.TD_397;
        }

        public void UpdateTo(TD_110 ent)
        {
            ent.CODE = Code;
            ent.VZT_CRS_SUMMA = VZT_CRS_SUMMA;
            ent.VZT_CRS_DC = VZT_CRS_DC;
            ent.VZT_SPOST_DC = VZT_SPOST_DC;
            ent.VZT_SFACT_DC = VZT_SFACT_DC;
            ent.VZT_DOC_DATE = VZT_DOC_DATE;
            ent.VZT_DOC_NUM = VZT_DOC_NUM;
            ent.VZT_DOC_NOTES = VZT_DOC_NOTES;
            ent.VZT_KONTR_DC = VZT_KONTR_DC;
            ent.VZT_CRS_POGASHENO = VZT_CRS_POGASHENO;
            ent.VZT_UCH_CRS_POGASHENO = VZT_UCH_CRS_POGASHENO;
            ent.VZT_UCH_CRS_RATE = VZT_UCH_CRS_RATE;
            ent.VZT_VZAIMOR_TYPE_DC = VZT_VZAIMOR_TYPE_DC;
            ent.VZT_1MYDOLZH_0NAMDOLZH = VZT_1MYDOLZH_0NAMDOLZH;
            ent.VZT_KONTR_CRS_DC = VZT_KONTR_CRS_DC;
            ent.VZT_KONTR_CRS_RATE = VZT_KONTR_CRS_RATE;
            ent.VZT_KONTR_CRS_SUMMA = VZT_KONTR_CRS_SUMMA;
            ent.VZT_SHPZ_DC = VZT_SHPZ_DC;
            ent.TSTAMP = TSTAMP;
            ent.SD_110 = SD_110;
            ent.SD_26 = SD_26;
            ent.SD_301 = SD_301;
            ent.SD_3011 = SD_3011;
            ent.SD_303 = SD_303;
            ent.SD_43 = SD_43;
            ent.SD_77 = SD_77;
            ent.SD_84 = SD_84;
            //ent.TD_397 = TD_397;
        }

        public TD_110 DefaultValue()
        {
            return new TD_110
            {
                DOC_CODE = -1,
                CODE = -1
            };
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TD_110ViewModel) obj);
        }

        public override int GetHashCode()
        {
            return (DocCode.GetHashCode() * Code.GetHashCode()) ^ 397;
        }
    }
}