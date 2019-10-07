using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using Core.Helper;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel
{
    [MetadataType(typeof(DataAnnotationsSFClientRowViewModel))]
    public class InvoiceClientRow : RSViewModelBase, IEntity<TD_84>
    {
        private decimal myCurrentRemains;
        private TD_84 myEntity;
        private decimal myNDSPercent;
        private Nomenkl myNomenkl;
        private decimal myRest;
        private SDRSchet mySDRSchet;
        private decimal myShipped;
        // ReSharper disable once RedundantDefaultMemberInitializer
        public bool IsNDSInPrice = false;

        public InvoiceClientRow(bool isNDSInPrice = false)
        {
            Entity = DefaultValue();
            IsNDSInPrice = isNDSInPrice;
        }

        public InvoiceClientRow(TD_84 entity, bool isNDSInPrice = false)
        {
            Entity = entity ?? DefaultValue();
            IsNDSInPrice = isNDSInPrice;
            LoadReference();
        }

        public string NomNomenkl => Nomenkl?.NOM_NOMENKL;
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
        public override Guid Id
        {
            set
            {
                if (Entity.Id == value) return;
                Entity.Id = value;
                RaisePropertyChanged();
            }
            get => Entity.Id;
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
        public string SFT_TEXT
        {
            get => Entity.SFT_TEXT;
            set
            {
                if (Entity.SFT_TEXT == value) return;
                Entity.SFT_TEXT = value;
                RaisePropertyChanged();
            }
        }
        public override string Note
        {
            get => SFT_TEXT;
            set
            {
                if (SFT_TEXT == value) return;
                SFT_TEXT = value;
                RaisePropertyChanged();
            }
        }
        public decimal SFT_NEMENKL_DC
        {
            get => Entity.SFT_NEMENKL_DC;
            set
            {
                if (Entity.SFT_NEMENKL_DC == value) return;
                Entity.SFT_NEMENKL_DC = value;
                if (Entity.SFT_NEMENKL_DC >= 0)
                {
                    Nomenkl = MainReferences.GetNomenkl(Entity.SFT_NEMENKL_DC);
                    RaisePropertyChanged(nameof(NomNomenkl));
                }
                RaisePropertyChanged();
            }
        }
        public Nomenkl Nomenkl
        {
            get => myNomenkl;
            set
            {
                if (myNomenkl != null && myNomenkl.Equals(value)) return;
                myNomenkl = value;
                if (myNomenkl != null)
                    SFT_NEMENKL_DC = myNomenkl.DocCode;
                RaisePropertyChanged();
            }
        }
        public double SFT_KOL
        {
            get => Entity.SFT_KOL;
            set
            {
                if (Entity.SFT_KOL == value) return;
                if (value < (double) Shipped && !Nomenkl.IsUsluga)
                {
                    WindowManager.ShowMessage($"Отгружено {Shipped}. Уменьшить кол-во в счете нельзя", "Ошибка", MessageBoxImage.Error);
                    return;
                }
                Entity.SFT_KOL = value;
                if (IsNDSInPrice)
                {
                    if (SFT_KOL != 0)
                    {
                        Entity.SFT_ED_CENA = (SFT_SUMMA_K_OPLATE ?? 0) / (decimal?) SFT_KOL;
                        Entity.SFT_SUMMA_NDS = (SFT_SUMMA_K_OPLATE ?? 0) - SFT_SUMMA_K_OPLATE ??
                                               0 * 100 / ((decimal?) SFT_KOL * 100 / (100 + NDSPercent));
                    }
                    else
                    {
                        Entity.SFT_ED_CENA = 0;
                        Entity.SFT_SUMMA_NDS = 0;
                        Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = 0;
                    }
                    RaisePropertyChanged(nameof(SFT_ED_CENA));
                    RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
                }
                else
                {
                    Entity.SFT_SUMMA_NDS = (SFT_ED_CENA ?? 0) * (decimal?) SFT_KOL * NDSPercent/ 100;
                    Entity.SFT_SUMMA_K_OPLATE = (decimal?)(SFT_KOL * (double)(SFT_ED_CENA ?? 0)) + Entity.SFT_SUMMA_NDS;
                    Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
                    RaisePropertyChanged(nameof(SFT_SUMMA_K_OPLATE));
                    RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
                }
                RaisePropertyChanged();
                if (Parent is InvoiceClient par)
                {
                    par.UpdateActualValues();
                }
            }
        }
        public decimal Quantity
        {
            get => (decimal) SFT_KOL;
            set
            {
                if (SFT_KOL == (double) value) return;
                if (value < Shipped)
                {
                    WindowManager.ShowMessage($"Отгружено {Shipped}. Уменьшить кол-во в счете нельзя","Ошибка",MessageBoxImage.Error);
                    return;
                }
                SFT_KOL = (double) value;
                RaisePropertyChanged();
            }
        }
        public decimal? SFT_ED_CENA
        {
            get => Entity.SFT_ED_CENA;
            set
            {
                if (Entity.SFT_ED_CENA == value) return;
                Entity.SFT_ED_CENA = value;
                if (IsNDSInPrice)
                {
                    if (SFT_KOL != 0)
                    {
                        Entity.SFT_SUMMA_NDS = (SFT_SUMMA_K_OPLATE ?? 0) - (SFT_SUMMA_K_OPLATE ?? 0) * 100 /
                                               ((decimal?) SFT_KOL * 100 / (100 + myNDSPercent));
                    }
                    else
                    {
                        Entity.SFT_SUMMA_NDS = 0;
                    }
                    RaisePropertyChanged(nameof(SFT_ED_CENA));
                    RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
                }
                Entity.SFT_SUMMA_K_OPLATE = (decimal?) (SFT_KOL * (double) (SFT_ED_CENA ?? 0))+Entity.SFT_SUMMA_NDS;
                Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE+Entity.SFT_SUMMA_NDS;
                RaisePropertyChanged(nameof(SFT_SUMMA_K_OPLATE));
                RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
                RaisePropertyChanged();
            }
        }
        public decimal Price
        {
            get => (decimal) (SFT_SUMMA_K_OPLATE != null && Quantity != 0
                ? SFT_SUMMA_K_OPLATE / (decimal?) SFT_KOL
                : 0);
            set
            {
                if (SFT_ED_CENA == value) return;
                Entity.SFT_ED_CENA = value;
                if (IsNDSInPrice)
                {
                    if (SFT_KOL != 0)
                    {
                        Entity.SFT_SUMMA_NDS = (SFT_SUMMA_K_OPLATE ?? 0) - (SFT_SUMMA_K_OPLATE ?? 0) * 100 /
                                               ((decimal?) SFT_KOL * 100 / (100 + myNDSPercent));
                    }
                    else
                    {
                        Entity.SFT_SUMMA_NDS = 0;
                    }
                    RaisePropertyChanged(nameof(SFT_ED_CENA));
                    RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
                }
                RaisePropertyChanged();
            }
        }
        public decimal? SFT_ACCIZ
        {
            get => Entity.SFT_ACCIZ;
            set
            {
                if (Entity.SFT_ACCIZ == value) return;
                Entity.SFT_ACCIZ = value;
                RaisePropertyChanged();
            }
        }
        public double SFT_NDS_PERCENT
        {
            get => Entity.SFT_NDS_PERCENT;
            set
            {
                if (Entity.SFT_NDS_PERCENT == value) return;
                Entity.SFT_NDS_PERCENT = value;
                RaisePropertyChanged();
            }
        }
        public decimal NDSPercent
        {
            get => (decimal) SFT_NDS_PERCENT;
            set
            {
                if (myNDSPercent == value) return;
                myNDSPercent = value;
                Entity.SFT_NDS_PERCENT = (double) myNDSPercent;
                if (IsNDSInPrice)
                {
                    if (SFT_KOL != 0)
                    {
                        Entity.SFT_ED_CENA = (SFT_SUMMA_K_OPLATE ?? 0) / (decimal?)SFT_KOL;
                        Entity.SFT_SUMMA_NDS = (SFT_SUMMA_K_OPLATE ?? 0) - (SFT_SUMMA_K_OPLATE ?? 0) * 100 /
                                               ((decimal?) SFT_KOL * 100 / (100 + myNDSPercent));
                    }
                    else
                    {
                        Entity.SFT_ED_CENA = 0;
                        Entity.SFT_SUMMA_NDS = 0;
                        Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = 0;
                    }
                    RaisePropertyChanged(nameof(SFT_ED_CENA));
                    RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
                }
                else
                {
                    Entity.SFT_SUMMA_NDS = (SFT_ED_CENA ?? 0) * (decimal?)SFT_KOL *  myNDSPercent / 100;
                    Entity.SFT_SUMMA_K_OPLATE = (decimal?)(SFT_KOL * (double)(SFT_ED_CENA ?? 0)) + Entity.SFT_SUMMA_NDS;
                    Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
                    RaisePropertyChanged(nameof(SFT_SUMMA_K_OPLATE));
                    RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
                }
                RaisePropertyChanged();
            }
        }
        public decimal? SFT_SUMMA_NDS
        {
            get => Entity.SFT_SUMMA_NDS;
            set
            {
                if (Entity.SFT_SUMMA_NDS == value) return;
                Entity.SFT_SUMMA_NDS = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SFT_SUMMA_K_OPLATE
        {
            get => Entity.SFT_SUMMA_K_OPLATE;
            set
            {
                if (Entity.SFT_SUMMA_K_OPLATE == value) return;
                Entity.SFT_SUMMA_K_OPLATE = value;
                if (IsNDSInPrice)
                {
                    if (SFT_KOL != 0)
                    {
                        Entity.SFT_ED_CENA = (SFT_SUMMA_K_OPLATE ?? 0) / (decimal?)SFT_KOL;
                        Entity.SFT_SUMMA_NDS = (SFT_SUMMA_K_OPLATE ?? 0) - (SFT_SUMMA_K_OPLATE ?? 0) * 100 /
                                               ((decimal?) SFT_KOL * 100 / (100 + myNDSPercent));
                    }
                    else
                    {
                        Entity.SFT_ED_CENA = 0;
                        Entity.SFT_SUMMA_NDS = 0;
                        Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = 0;
                    }
                    RaisePropertyChanged(nameof(SFT_ED_CENA));
                    RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
                }
                else
                {
                    Entity.SFT_SUMMA_NDS = (SFT_ED_CENA ?? 0) * (decimal?)SFT_KOL * ((100 + myNDSPercent) / 100);
                    Entity.SFT_SUMMA_K_OPLATE = (decimal?)(SFT_KOL * (double)(SFT_ED_CENA ?? 0));
                    Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
                    RaisePropertyChanged(nameof(SFT_SUMMA_K_OPLATE));
                    RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
                }
                RaisePropertyChanged();
            }
        }
        public decimal Summa
        {
            get => SFT_SUMMA_K_OPLATE ?? 0;
            set
            {
                if (SFT_SUMMA_K_OPLATE == value) return;
                SFT_SUMMA_K_OPLATE = value;
                Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
                RaisePropertyChanged();
            }
        }
        public decimal? SFT_STDP_DC
        {
            get => Entity.SFT_STDP_DC;
            set
            {
                if (Entity.SFT_STDP_DC == value) return;
                Entity.SFT_STDP_DC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SFT_UCHET_ED_IZM_DC
        {
            get => Entity.SFT_UCHET_ED_IZM_DC;
            set
            {
                if (Entity.SFT_UCHET_ED_IZM_DC == value) return;
                Entity.SFT_UCHET_ED_IZM_DC = value;
                RaisePropertyChanged();
            }
        }
        public short? SFT_KOMPLEKT
        {
            get => Entity.SFT_KOMPLEKT;
            set
            {
                if (Entity.SFT_KOMPLEKT == value) return;
                Entity.SFT_KOMPLEKT = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SFT_NALOG_NA_PROD
        {
            get => Entity.SFT_NALOG_NA_PROD;
            set
            {
                if (Entity.SFT_NALOG_NA_PROD == value) return;
                Entity.SFT_NALOG_NA_PROD = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SFT_DOG_OTGR_DC
        {
            get => Entity.SFT_DOG_OTGR_DC;
            set
            {
                if (Entity.SFT_DOG_OTGR_DC == value) return;
                Entity.SFT_DOG_OTGR_DC = value;
                RaisePropertyChanged();
            }
        }
        public int? SFT_DOG_OTGR_PLAN_CODE
        {
            get => Entity.SFT_DOG_OTGR_PLAN_CODE;
            set
            {
                if (Entity.SFT_DOG_OTGR_PLAN_CODE == value) return;
                Entity.SFT_DOG_OTGR_PLAN_CODE = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SFT_NACENKA_DILERA
        {
            get => Entity.SFT_NACENKA_DILERA;
            set
            {
                if (Entity.SFT_NACENKA_DILERA == value) return;
                Entity.SFT_NACENKA_DILERA = value;
                RaisePropertyChanged();
            }
        }
        public double? SFT_PROCENT_ZS_RASHODOV
        {
            get => Entity.SFT_PROCENT_ZS_RASHODOV;
            set
            {
                if (Entity.SFT_PROCENT_ZS_RASHODOV == value) return;
                Entity.SFT_PROCENT_ZS_RASHODOV = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SFT_SUMMA_K_OPLATE_KONTR_CRS
        {
            get => Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS;
            set
            {
                if (Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS == value) return;
                Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SFT_SHPZ_DC
        {
            get => Entity.SFT_SHPZ_DC;
            set
            {
                if (Entity.SFT_SHPZ_DC == value) return;
                Entity.SFT_SHPZ_DC = value;
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
                SFT_SHPZ_DC = mySDRSchet?.DocCode;
                RaisePropertyChanged();
            }
        }
        public string SFT_STRANA_PROIS
        {
            get => Entity.SFT_STRANA_PROIS;
            set
            {
                if (Entity.SFT_STRANA_PROIS == value) return;
                Entity.SFT_STRANA_PROIS = value;
                RaisePropertyChanged();
            }
        }
        public string SFT_N_GRUZ_DECLAR
        {
            get => Entity.SFT_N_GRUZ_DECLAR;
            set
            {
                if (Entity.SFT_N_GRUZ_DECLAR == value) return;
                Entity.SFT_N_GRUZ_DECLAR = value;
                RaisePropertyChanged();
            }
        }
        public double? SFT_TARA_MEST
        {
            get => Entity.SFT_TARA_MEST;
            set
            {
                if (Entity.SFT_TARA_MEST == value) return;
                Entity.SFT_TARA_MEST = value;
                RaisePropertyChanged();
            }
        }
        public decimal? SFT_TARA_DC
        {
            get => Entity.SFT_TARA_DC;
            set
            {
                if (Entity.SFT_TARA_DC == value) return;
                Entity.SFT_TARA_DC = value;
                RaisePropertyChanged();
            }
        }
        public short? SFT_TARA_FLAG
        {
            get => Entity.SFT_TARA_FLAG;
            set
            {
                if (Entity.SFT_TARA_FLAG == value) return;
                Entity.SFT_TARA_FLAG = value;
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
        public string SFT_COUNTRY_CODE
        {
            get => Entity.SFT_COUNTRY_CODE;
            set
            {
                if (Entity.SFT_COUNTRY_CODE == value) return;
                Entity.SFT_COUNTRY_CODE = value;
                RaisePropertyChanged();
            }
        }
        public string OLD_NOM_NOMENKL
        {
            get => Entity.OLD_NOM_NOMENKL;
            set
            {
                if (Entity.OLD_NOM_NOMENKL == value) return;
                Entity.OLD_NOM_NOMENKL = value;
                RaisePropertyChanged();
            }
        }
        public string OLD_NOM_NAME
        {
            get => Entity.OLD_NOM_NAME;
            set
            {
                if (Entity.OLD_NOM_NAME == value) return;
                Entity.OLD_NOM_NAME = value;
                RaisePropertyChanged();
            }
        }
        public string OLD_OVERHEAD_NAME
        {
            get => Entity.OLD_OVERHEAD_NAME;
            set
            {
                if (Entity.OLD_OVERHEAD_NAME == value) return;
                Entity.OLD_OVERHEAD_NAME = value;
                RaisePropertyChanged();
            }
        }
        public string OLD_OVERHEAD_CRS_NAME
        {
            get => Entity.OLD_OVERHEAD_CRS_NAME;
            set
            {
                if (Entity.OLD_OVERHEAD_CRS_NAME == value) return;
                Entity.OLD_OVERHEAD_CRS_NAME = value;
                RaisePropertyChanged();
            }
        }
        public string OLD_UNIT_NAME
        {
            get => Entity.OLD_UNIT_NAME;
            set
            {
                if (Entity.OLD_UNIT_NAME == value) return;
                Entity.OLD_UNIT_NAME = value;
                RaisePropertyChanged();
            }
        }
        public SD_165 SD_165
        {
            get => Entity.SD_165;
            set
            {
                if (Entity.SD_165 == value) return;
                Entity.SD_165 = value;
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
        public VD_9 VD_9
        {
            get => Entity.VD_9;
            set
            {
                if (Entity.VD_9 == value) return;
                Entity.VD_9 = value;
                RaisePropertyChanged();
            }
        }
        public COUNTRY COUNTRY
        {
            get => Entity.COUNTRY;
            set
            {
                if (Entity.COUNTRY == value) return;
                Entity.COUNTRY = value;
                RaisePropertyChanged();
            }
        }
        public TD_84 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }
        public string NomenklNumber => Nomenkl?.NomenklNumber;
        public bool IsUsluga => Nomenkl?.NOM_0MATER_1USLUGA == 1;
        /// <summary>
        ///     Отгружено
        /// </summary>
        public decimal Shipped
        {
            get => myShipped;
            set
            {
                if (myShipped == value) return;
                myShipped = value;
                myRest = (decimal) (SFT_KOL - (double) myShipped);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Rest));
            }
        }
        /// <summary>
        ///     остаток для отгрузки по счету
        /// </summary>
        public decimal Rest
        {
            get => myRest;
            set
            {
                if (myRest == value) return;
                myRest = value;
                RaisePropertyChanged();
            }
        }
        public decimal CurrentRemains
        {
            get => myCurrentRemains;
            set
            {
                if (myCurrentRemains == value) return;
                myCurrentRemains = value;
                RaisePropertyChanged();
            }
        }
        public EntityLoadCodition LoadCondition { get; set; }

        public List<TD_84> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        private void LoadReference()
        {
            Nomenkl = MainReferences.GetNomenkl(SFT_NEMENKL_DC);
            if (Entity.SFT_SHPZ_DC != null)
                SDRSchet = MainReferences.SDRSchets[Entity.SFT_SHPZ_DC.Value];
        }

        public virtual void Save(TD_84 doc)
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

        public void UpdateFrom(TD_84 ent)
        {
            Code = ent.CODE;
            SFT_TEXT = ent.SFT_TEXT;
            SFT_NEMENKL_DC = ent.SFT_NEMENKL_DC;
            SFT_KOL = ent.SFT_KOL;
            SFT_ED_CENA = ent.SFT_ED_CENA;
            SFT_ACCIZ = ent.SFT_ACCIZ;
            SFT_NDS_PERCENT = ent.SFT_NDS_PERCENT;
            SFT_SUMMA_NDS = ent.SFT_SUMMA_NDS;
            SFT_SUMMA_K_OPLATE = ent.SFT_SUMMA_K_OPLATE;
            SFT_STDP_DC = ent.SFT_STDP_DC;
            SFT_UCHET_ED_IZM_DC = ent.SFT_UCHET_ED_IZM_DC;
            SFT_KOMPLEKT = ent.SFT_KOMPLEKT;
            SFT_NALOG_NA_PROD = ent.SFT_NALOG_NA_PROD;
            SFT_DOG_OTGR_DC = ent.SFT_DOG_OTGR_DC;
            SFT_DOG_OTGR_PLAN_CODE = ent.SFT_DOG_OTGR_PLAN_CODE;
            SFT_NACENKA_DILERA = ent.SFT_NACENKA_DILERA;
            SFT_PROCENT_ZS_RASHODOV = ent.SFT_PROCENT_ZS_RASHODOV;
            SFT_SUMMA_K_OPLATE_KONTR_CRS = ent.SFT_SUMMA_K_OPLATE_KONTR_CRS;
            SFT_SHPZ_DC = ent.SFT_SHPZ_DC;
            SFT_STRANA_PROIS = ent.SFT_STRANA_PROIS;
            SFT_N_GRUZ_DECLAR = ent.SFT_N_GRUZ_DECLAR;
            SFT_TARA_MEST = ent.SFT_TARA_MEST;
            SFT_TARA_DC = ent.SFT_TARA_DC;
            SFT_TARA_FLAG = ent.SFT_TARA_FLAG;
            TSTAMP = ent.TSTAMP;
            SFT_COUNTRY_CODE = ent.SFT_COUNTRY_CODE;
            OLD_NOM_NOMENKL = ent.OLD_NOM_NOMENKL;
            OLD_NOM_NAME = ent.OLD_NOM_NAME;
            OLD_OVERHEAD_NAME = ent.OLD_OVERHEAD_NAME;
            OLD_OVERHEAD_CRS_NAME = ent.OLD_OVERHEAD_CRS_NAME;
            OLD_UNIT_NAME = ent.OLD_UNIT_NAME;
            SD_165 = ent.SD_165;
            SD_175 = ent.SD_175;
            SD_303 = ent.SD_303;
            SD_83 = ent.SD_83;
            SD_831 = ent.SD_831;
            SD_84 = ent.SD_84;
            VD_9 = ent.VD_9;
            COUNTRY = ent.COUNTRY;
            Id = ent.Id;
            DocId = ent.DocId;
        }

        public void UpdateTo(TD_84 ent)
        {
            ent.CODE = Code;
            ent.SFT_TEXT = SFT_TEXT;
            ent.SFT_NEMENKL_DC = SFT_NEMENKL_DC;
            ent.SFT_KOL = SFT_KOL;
            ent.SFT_ED_CENA = SFT_ED_CENA;
            ent.SFT_ACCIZ = SFT_ACCIZ;
            ent.SFT_NDS_PERCENT = SFT_NDS_PERCENT;
            ent.SFT_SUMMA_NDS = SFT_SUMMA_NDS;
            ent.SFT_SUMMA_K_OPLATE = SFT_SUMMA_K_OPLATE;
            ent.SFT_STDP_DC = SFT_STDP_DC;
            ent.SFT_UCHET_ED_IZM_DC = SFT_UCHET_ED_IZM_DC;
            ent.SFT_KOMPLEKT = SFT_KOMPLEKT;
            ent.SFT_NALOG_NA_PROD = SFT_NALOG_NA_PROD;
            ent.SFT_DOG_OTGR_DC = SFT_DOG_OTGR_DC;
            ent.SFT_DOG_OTGR_PLAN_CODE = SFT_DOG_OTGR_PLAN_CODE;
            ent.SFT_NACENKA_DILERA = SFT_NACENKA_DILERA;
            ent.SFT_PROCENT_ZS_RASHODOV = SFT_PROCENT_ZS_RASHODOV;
            ent.SFT_SUMMA_K_OPLATE_KONTR_CRS = SFT_SUMMA_K_OPLATE_KONTR_CRS;
            ent.SFT_SHPZ_DC = SFT_SHPZ_DC;
            ent.SFT_STRANA_PROIS = SFT_STRANA_PROIS;
            ent.SFT_N_GRUZ_DECLAR = SFT_N_GRUZ_DECLAR;
            ent.SFT_TARA_MEST = SFT_TARA_MEST;
            ent.SFT_TARA_DC = SFT_TARA_DC;
            ent.SFT_TARA_FLAG = SFT_TARA_FLAG;
            ent.TSTAMP = TSTAMP;
            ent.SFT_COUNTRY_CODE = SFT_COUNTRY_CODE;
            ent.OLD_NOM_NOMENKL = OLD_NOM_NOMENKL;
            ent.OLD_NOM_NAME = OLD_NOM_NAME;
            ent.OLD_OVERHEAD_NAME = OLD_OVERHEAD_NAME;
            ent.OLD_OVERHEAD_CRS_NAME = OLD_OVERHEAD_CRS_NAME;
            ent.OLD_UNIT_NAME = OLD_UNIT_NAME;
            ent.SD_165 = SD_165;
            ent.SD_175 = SD_175;
            ent.SD_303 = SD_303;
            ent.SD_83 = SD_83;
            ent.SD_831 = SD_831;
            ent.SD_84 = SD_84;
            ent.VD_9 = VD_9;
            ent.COUNTRY = COUNTRY;
            ent.Id = Id;
            ent.DocId = DocId;
        }

        public TD_84 DefaultValue()
        {
            return new TD_84
            {
                Id = Guid.NewGuid(),
                DOC_CODE = -1,
                CODE = -1
            };
        }

        public virtual TD_84 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual TD_84 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        ///TODO Доделать расчет сумм по строке счета-фактуры для клиентов
        public void CalcRowSumma(bool isBackCalc, bool isNDSInPrice, string fieldName)
        {
            switch (fieldName)
            {
                case "Quantity":
                    if (isBackCalc)
                    {
                        if (isNDSInPrice)
                        {
                            Price = Math.Round(Summa / (Quantity * (1 + NDSPercent / 100)), 2);
                            SFT_SUMMA_NDS = Math.Round(Summa * NDSPercent / 100, 2);
                        }
                        else
                        {
                            Price = Math.Round(Summa / Quantity);
                        }
                    }
                    else
                    {
                        if (isNDSInPrice)
                        {
                            Price = Math.Round(Summa / (Quantity * (1 + NDSPercent / 100)), 2);
                            SFT_SUMMA_NDS = Math.Round(Summa * NDSPercent / 100, 2);
                        }
                        else
                        {
                            Price = Math.Round(Summa / Quantity);
                        }
                    }
                    break;
            }
        }
    }

    public class DataAnnotationsSFClientRowViewModel : DataAnnotationForFluentApiBase,
        IMetadataProvider<InvoiceClientRow>
    {
        void IMetadataProvider<InvoiceClientRow>.BuildMetadata(MetadataBuilder<InvoiceClientRow> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.Nomenkl).AutoGenerated().DisplayName("Наименование");
            builder.Property(_ => _.NomNomenkl).AutoGenerated().DisplayName("Ном.№");
            builder.Property(_ => _.IsUsluga).AutoGenerated().DisplayName("Услуга");
            builder.Property(_ => _.SFT_KOL).AutoGenerated().DisplayName("Кол-во").DisplayFormatString("n4");
            builder.Property(_ => _.SFT_ED_CENA).AutoGenerated().DisplayName("Цена").DisplayFormatString("n2");
            builder.Property(_ => _.SFT_SUMMA_K_OPLATE).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2");
            builder.Property(_ => _.SFT_NACENKA_DILERA).AutoGenerated().DisplayName("Дилерские").DisplayFormatString("n2");
            builder.Property(_ => _.Shipped).AutoGenerated().DisplayName("Отгружено").DisplayFormatString("n4");
            builder.Property(_ => _.Rest).AutoGenerated().DisplayName("Остаток").DisplayFormatString("n4");
            builder.Property(_ => _.CurrentRemains).AutoGenerated().DisplayName("Текущие остатки").DisplayFormatString("n4");
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
            builder.Property(_ => _.NDSPercent).AutoGenerated().DisplayName("НДС %").DisplayFormatString("n2");
            builder.Property(_ => _.SFT_SUMMA_NDS).AutoGenerated().DisplayName("Сумма НДС").ReadOnly().DisplayFormatString("n2");
            builder.Property(_ => _.SDRSchet).AutoGenerated().DisplayName("Счет дох./расх.");
        }
    }
}