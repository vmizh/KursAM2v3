﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.NomenklManagement;
using Core.Helper;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.DataAnnotations;

// ReSharper disable InconsistentNaming
namespace Core.EntityViewModel.Invoices
{
    [MetadataType(typeof(DataAnnotationsSFClientRowViewModel))]
    public class InvoiceClientRow : RSViewModelBase, IEntity<TD_84>
    {
        private decimal myCurrentRemains;
        private TD_84 myEntity;

        // ReSharper disable once RedundantDefaultMemberInitializer
        public bool myIsNDSInPrice = false;
        private Nomenkl myNomenkl;
        private decimal myRest;
        private SDRSchet mySDRSchet;
        private decimal myShipped;

        public InvoiceClientRow(bool isNDSInPrice = false)
        {
            Entity = DefaultValue();
            IsNDSInPrice = isNDSInPrice;
        }

        public InvoiceClientRow(TD_84 entity, bool isNDSInPrice = false)
        {
            if (entity == null)
            {
                Entity = DefaultValue();
            }
            else
            {
                Entity = entity;
                LoadReference();
            }

            IsNDSInPrice = isNDSInPrice;
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

        public bool IsNDSInPrice
        {
            get => myIsNDSInPrice;
            set
            {
                if (myIsNDSInPrice == value) return;
                myIsNDSInPrice = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Entity.SFT_ED_CENA));
            }
        }

        public override string Note
        {
            get => Entity.SFT_TEXT;
            set
            {
                if (Entity.SFT_TEXT == value) return;
                Entity.SFT_TEXT = value;
                RaisePropertyChanged();
            }
        }

        public Nomenkl Nomenkl
        {
            get => MainReferences.GetNomenkl(Entity.SFT_NEMENKL_DC);
            set
            {
                if (MainReferences.GetNomenkl(Entity.SFT_NEMENKL_DC) == value) return;
                myNomenkl = value;
                if (myNomenkl != null)
                    Entity.SFT_NEMENKL_DC = myNomenkl.DocCode;
                RaisePropertyChanged();
            }
        }

        public decimal Quantity
        {
            get => (decimal) Entity.SFT_KOL;
            set
            {
                if (value <= 0)
                {
                    WindowManager.ShowMessage("Кол-во должно быть больше нуля", "Ошибка",
                        MessageBoxImage.Error);
                    return;
                }

                if (Math.Abs(Entity.SFT_KOL - (double) value) < 0.00001) return;
                if (value < Shipped)
                {
                    WindowManager.ShowMessage($"Отгружено {Shipped}. Уменьшить кол-во в счете нельзя", "Ошибка",
                        MessageBoxImage.Error);
                    return;
                }

                Entity.SFT_KOL = (double) value;
                CalcRow();
                RaisePropertyChanged();
            }
        }

        public decimal Price
        {
            get => Entity.SFT_ED_CENA ?? 0;
            set
            {
                if (value < 0)
                {
                    WindowManager.ShowMessage("Цена должна быть больше нуля", "Ошибка",
                        MessageBoxImage.Error);
                    return;
                }

                if (Entity.SFT_ED_CENA == value) return;
                Entity.SFT_ED_CENA = value;
                CalcRow();
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

        public decimal NDSPercent
        {
            get => (decimal) Entity.SFT_NDS_PERCENT;
            set
            {
                if (value < 0)
                {
                    WindowManager.ShowMessage("НДС должен быть больше нуля", "Ошибка",
                        MessageBoxImage.Error);
                    return;
                }

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Entity.SFT_NDS_PERCENT == (double) value) return;
                Entity.SFT_NDS_PERCENT = (double) value;
                CalcRow();
                RaisePropertyChanged();
            }
        }

        public decimal? SFT_SUMMA_NDS => Entity.SFT_SUMMA_NDS;

        public decimal Summa
        {
            get => Entity.SFT_SUMMA_K_OPLATE ?? 0;
            set
            {
                if (value < 0)
                {
                    WindowManager.ShowMessage("Сумма должна быть больше нуля", "Ошибка",
                        MessageBoxImage.Error);
                    return;
                }

                if (Entity.SFT_SUMMA_K_OPLATE == value) return;
                Entity.SFT_SUMMA_K_OPLATE = value;
                Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = value;
                CalcRow();
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
                if (Parent is InvoiceClient doc)
                    // ReSharper disable once PossibleInvalidOperationException
                    doc.SF_DILER_SUMMA = (decimal?) doc.Rows.Sum(_ => _.Entity.SFT_KOL * (double) _.SFT_NACENKA_DILERA);
                RaisePropertyChanged();
            }
        }

        public double? SFT_PROCENT_ZS_RASHODOV
        {
            get => Entity.SFT_PROCENT_ZS_RASHODOV;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Entity.SFT_PROCENT_ZS_RASHODOV == value) return;
                Entity.SFT_PROCENT_ZS_RASHODOV = value;
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
                Entity.SFT_SHPZ_DC = mySDRSchet?.DocCode;
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
                // ReSharper disable once CompareOfFloatsByEqualityOperator
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
                myRest = (decimal) (Entity.SFT_KOL - (double) myShipped);
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

        public List<TD_84> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        private void CalcRow()
        {
            if (IsNDSInPrice)
            {
                Entity.SFT_SUMMA_NDS = Math.Round((Entity.SFT_SUMMA_K_OPLATE ?? 0) -
                                                  (Entity.SFT_SUMMA_K_OPLATE ?? 0) * 100 /
                                                  (decimal) (100 + Entity.SFT_NDS_PERCENT), 2);
                Entity.SFT_ED_CENA =
                    (Entity.SFT_SUMMA_K_OPLATE ?? 0) /
                    (decimal) Entity.SFT_KOL;
                RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
                RaisePropertyChanged(nameof(Price));
            }
            else
            {
                Entity.SFT_SUMMA_NDS =
                    Math.Round((Entity.SFT_SUMMA_K_OPLATE ?? 0) * NDSPercent / (100 + NDSPercent), 2);
                Entity.SFT_SUMMA_K_OPLATE =
                    Math.Round((decimal) (Entity.SFT_KOL * (double) (Entity.SFT_ED_CENA ?? 0)), 2)
                    + Entity.SFT_SUMMA_NDS;
                Entity.SFT_SUMMA_K_OPLATE_KONTR_CRS = Entity.SFT_SUMMA_K_OPLATE;
                RaisePropertyChanged(nameof(SFT_SUMMA_NDS));
                RaisePropertyChanged(nameof(Summa));
            }
        }

        private void LoadReference()
        {
            Nomenkl = MainReferences.GetNomenkl(Entity.SFT_NEMENKL_DC);
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
        }

        public void UpdateTo(TD_84 ent)
        {
        }

        public TD_84 DefaultValue()
        {
            return new()
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
            builder.Property(_ => _.Quantity).AutoGenerated().DisplayName("Кол-во").DisplayFormatString("n4");
            builder.Property(_ => _.Price).AutoGenerated().DisplayName("Цена").DisplayFormatString("n2");
            builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2");
            builder.Property(_ => _.SFT_NACENKA_DILERA).AutoGenerated().DisplayName("Наценка дилера на единицу")
                .DisplayFormatString("n2");
            builder.Property(_ => _.Shipped).AutoGenerated().DisplayName("Отгружено").DisplayFormatString("n4");
            builder.Property(_ => _.Rest).AutoGenerated().DisplayName("Остаток").DisplayFormatString("n4");
            builder.Property(_ => _.CurrentRemains).AutoGenerated().DisplayName("Текущие остатки")
                .DisplayFormatString("n4");
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечание");
            builder.Property(_ => _.NDSPercent).AutoGenerated().DisplayName("НДС %").DisplayFormatString("n2");
            builder.Property(_ => _.SFT_SUMMA_NDS).AutoGenerated().DisplayName("Сумма НДС").ReadOnly()
                .DisplayFormatString("n2");
            builder.Property(_ => _.SDRSchet).AutoGenerated().DisplayName("Счет дох./расх.");
        }
    }
}