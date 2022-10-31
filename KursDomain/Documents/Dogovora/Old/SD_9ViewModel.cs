using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Core.Helper;
using Core.ViewModel.Base;
using Data;
using DevExpress.Mvvm.DataAnnotations;
using KursDomain.Documents.CommonReferences.Kontragent;

namespace Core.EntityViewModel.Dogovora.Old
{
    /// <summary>
    /// Шапка договора для коиентов
    /// </summary>
    [MetadataType(typeof(SD9_FluentAPI))]
    public class SD_9ViewModel : RSViewModelBase, IEntity<SD_9>
    {
        private SD_9 myEntity;
        private decimal myShipped;

        public SD_9ViewModel()
        {
            Entity = DefaultValue();
        }

        public SD_9ViewModel(SD_9 entity)
        {
            Entity = entity ?? DefaultValue();
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

        public DateTime ZAK_DATE
        {
            get => Entity.ZAK_DATE;
            set
            {
                if (Entity.ZAK_DATE == value) return;
                Entity.ZAK_DATE = value;
                RaisePropertyChanged();
            }
        }

        public int ZAK_NUM
        {
            get => Entity.ZAK_NUM;
            set
            {
                if (Entity.ZAK_NUM == value) return;
                Entity.ZAK_NUM = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? ZAK_PLAN_OTRGRUZ_DATE
        {
            get => Entity.ZAK_PLAN_OTRGRUZ_DATE;
            set
            {
                if (Entity.ZAK_PLAN_OTRGRUZ_DATE == value) return;
                Entity.ZAK_PLAN_OTRGRUZ_DATE = value;
                RaisePropertyChanged();
            }
        }

        public DateTime ZAK_PAY_LAST_DATE
        {
            get => Entity.ZAK_PAY_LAST_DATE;
            set
            {
                if (Entity.ZAK_PAY_LAST_DATE == value) return;
                Entity.ZAK_PAY_LAST_DATE = value;
                RaisePropertyChanged();
            }
        }

        public Kontragent Client
        {
            get => MainReferences.GetKontragent(Entity.ZAK_CLIENT_DC);
            set
            {
                if (MainReferences.GetKontragent(Entity.ZAK_CLIENT_DC) == value) return;
                if (value != null)
                {
                    Entity.ZAK_CLIENT_DC = value.DocCode;
                }
                else Entity.ZAK_CLIENT_DC = null;
                RaisePropertyChanged();
            }
        }

        public Currency Currency => Client?.BalansCurrency;

        //public int? ZAK_CLIENT_TABELNUMBER
        //{
        //    get => Entity.ZAK_CLIENT_TABELNUMBER;
        //    set
        //    {
        //        if (Entity.ZAK_CLIENT_TABELNUMBER == value) return;
        //        Entity.ZAK_CLIENT_TABELNUMBER = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public string ZAK_CLIENT_NAME
        //{
        //    get => Entity.ZAK_CLIENT_NAME;
        //    set
        //    {
        //        if (Entity.ZAK_CLIENT_NAME == value) return;
        //        Entity.ZAK_CLIENT_NAME = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public decimal ZAK_CRS_DC
        //{
        //    get => Entity.ZAK_CRS_DC;
        //    set
        //    {
        //        if (Entity.ZAK_CRS_DC == value) return;
        //        Entity.ZAK_CRS_DC = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public double ZAK_CRS_RATE
        //{
        //    get => Entity.ZAK_CRS_RATE;
        //    set
        //    {
        //        if (Entity.ZAK_CRS_RATE == value) return;
        //        Entity.ZAK_CRS_RATE = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public decimal? ZAK_CRS_SUMMA
        //{
        //    get => Entity.ZAK_CRS_SUMMA;
        //    set
        //    {
        //        if (Entity.ZAK_CRS_SUMMA == value) return;
        //        Entity.ZAK_CRS_SUMMA = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public decimal Summa
        {
            get => Entity.ZAK_CRS_SUMMA ?? 0;
            set
            {
                if (Entity.ZAK_CRS_SUMMA == value) return;
                Entity.ZAK_CRS_SUMMA = value;
                RaisePropertyChanged();
            }
        }

        //public double ZAK_SKID_PERC_QUAN
        //{
        //    get => Entity.ZAK_SKID_PERC_QUAN;
        //    set
        //    {
        //        if (Entity.ZAK_SKID_PERC_QUAN == value) return;
        //        Entity.ZAK_SKID_PERC_QUAN = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public decimal? ZAK_SKID_CRS_SUMMA
        //{
        //    get => Entity.ZAK_SKID_CRS_SUMMA;
        //    set
        //    {
        //        if (Entity.ZAK_SKID_CRS_SUMMA == value) return;
        //        Entity.ZAK_SKID_CRS_SUMMA = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public decimal? ZAK_PAY_CRS_SUMMA
        //{
        //    get => Entity.ZAK_PAY_CRS_SUMMA;
        //    set
        //    {
        //        if (Entity.ZAK_PAY_CRS_SUMMA == value) return;
        //        Entity.ZAK_PAY_CRS_SUMMA = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public decimal PaySumma
        {
            get => Entity.ZAK_PAY_CRS_SUMMA ?? 0;
            set
            {
                if (Entity.ZAK_PAY_CRS_SUMMA == value) return;
                Entity.ZAK_PAY_CRS_SUMMA = value;
                RaisePropertyChanged();
            }
        }

        //public decimal? ZAK_PAY_RUB_SUMMA
        //{
        //    get => Entity.ZAK_PAY_RUB_SUMMA;
        //    set
        //    {
        //        if (Entity.ZAK_PAY_RUB_SUMMA == value) return;
        //        Entity.ZAK_PAY_RUB_SUMMA = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public short ZAK_ACCEPTED
        //{
        //    get => Entity.ZAK_ACCEPTED;
        //    set
        //    {
        //        if (Entity.ZAK_ACCEPTED == value) return;
        //        Entity.ZAK_ACCEPTED = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public bool IsAccepted
        {
            get => Entity.ZAK_ACCEPTED == 1;
            set
            {
                if ((Entity.ZAK_ACCEPTED == 1) == value) return;
                Entity.ZAK_ACCEPTED = (short) (value ? 1 : 0);
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

        public override string Note
        {
            get => Entity.ZAK_BASE;
            set
            {
                if (Entity.ZAK_BASE == value) return;
                Entity.ZAK_BASE = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Форма расчетов
        /// </summary>
        public FormPay FormPay
        {
            get => Entity.ZAK_FORM_RASCH_DC != null
                ? MainReferences.FormRaschets[Entity.ZAK_FORM_RASCH_DC.Value]
                : null;
            set
            {
                if (value != null)
                {
                    if (Entity.ZAK_FORM_RASCH_DC == value.DocCode) return;
                    Entity.ZAK_FORM_RASCH_DC = value.DocCode;
                    RaisePropertyChanged();
                }
                else
                {
                    if (Entity.ZAK_FORM_RASCH_DC != null)
                    {
                        Entity.ZAK_FORM_RASCH_DC = null;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        //public decimal? ZAK_TYPE_DC
        //{
        //    get => Entity.ZAK_TYPE_DC;
        //    set
        //    {
        //        if (Entity.ZAK_TYPE_DC == value) return;
        //        Entity.ZAK_TYPE_DC = value;
        //        RaisePropertyChanged();
        //    }
        //}

        /// <summary>
        /// Тип договора
        /// </summary>
        public ContractType DogovorType
        {
            get => Entity.ZAK_TYPE_DC != null ? MainReferences.GetContractType(Entity.ZAK_TYPE_DC.Value) : null;
            set
            {
                if (Entity.ZAK_TYPE_DC == value?.DocCode) return;
                Entity.ZAK_TYPE_DC = value?.DocCode;
                RaisePropertyChanged();
            }
        }

        //public short? ZAK_EXECUTED
        //{
        //    get => Entity.ZAK_EXECUTED;
        //    set
        //    {
        //        if (Entity.ZAK_EXECUTED == value) return;
        //        Entity.ZAK_EXECUTED = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public bool IsExecuted
        {
            get => Entity.ZAK_EXECUTED == 1;
            set
            {
                if (Entity.ZAK_EXECUTED == 1 == value) return;
                Entity.ZAK_EXECUTED = (short?) (value ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        public DateTime? ZAK_START_DATE
        {
            get => Entity.ZAK_START_DATE;
            set
            {
                if (Entity.ZAK_START_DATE == value) return;
                Entity.ZAK_START_DATE = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? ZAK_STOP_DATE
        {
            get => Entity.ZAK_STOP_DATE;
            set
            {
                if (Entity.ZAK_STOP_DATE == value) return;
                Entity.ZAK_STOP_DATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? ZAK_USL_POSTAV_DC
        {
            get => Entity.ZAK_USL_POSTAV_DC;
            set
            {
                if (Entity.ZAK_USL_POSTAV_DC == value) return;
                Entity.ZAK_USL_POSTAV_DC = value;
                RaisePropertyChanged();
            }
        }

        public byte[] ZAK_FILE
        {
            get => Entity.ZAK_FILE;
            set
            {
                if (Entity.ZAK_FILE == value) return;
                Entity.ZAK_FILE = value;
                RaisePropertyChanged();
            }
        }

        //public int? ZAK_OTV_TABELNUMBER
        //{
        //    get => Entity.ZAK_OTV_TABELNUMBER;
        //    set
        //    {
        //        if (Entity.ZAK_OTV_TABELNUMBER == value) return;
        //        Entity.ZAK_OTV_TABELNUMBER = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public decimal? ZAK_BASE_KOTRACT_DC
        //{
        //    get => Entity.ZAK_BASE_KOTRACT_DC;
        //    set
        //    {
        //        if (Entity.ZAK_BASE_KOTRACT_DC == value) return;
        //        Entity.ZAK_BASE_KOTRACT_DC = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public bool IsNDSInPrice
        {
            get => Entity.ZAK_NDS_1INCLUD_0NO == 1;
            set
            {
                if (Entity.ZAK_NDS_1INCLUD_0NO == 1 == value) return;
                Entity.ZAK_NDS_1INCLUD_0NO = (short?) (value ? 1 : 0);
                RaisePropertyChanged();
            }
        }
        
        //public decimal? ZAK_KATEGORY_DC
        //{
        //    get => Entity.ZAK_KATEGORY_DC;
        //    set
        //    {
        //        if (Entity.ZAK_KATEGORY_DC == value) return;
        //        Entity.ZAK_KATEGORY_DC = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public CategoryClientTypeViewModel ClientCategory
        {
            get => Entity.ZAK_KATEGORY_DC != null ? MainReferences.ClientKategory[Entity.ZAK_KATEGORY_DC.Value] : null;
            set
            {
                if (Entity.ZAK_KATEGORY_DC != null)
                {
                    if (MainReferences.ClientKategory[Entity.ZAK_KATEGORY_DC.Value] == value) return;
                    Entity.ZAK_KATEGORY_DC = value.DocCode;
                }
                else
                {
                    Entity.ZAK_KATEGORY_DC = value?.DocCode;
                }
                RaisePropertyChanged();
            }
        }

        public Kontragent Diler
        {
            get => MainReferences.GetKontragent(Entity.ZAK_DILER_DC);
            set
            {
                if (MainReferences.GetKontragent(Entity.ZAK_DILER_DC) == value) return;
                if (value != null)
                {
                    Entity.ZAK_DILER_DC = value.DocCode;
                }
                else Entity.ZAK_DILER_DC = null;
                RaisePropertyChanged();
            }
        }

        public decimal SummaDilera
        {
            get => Entity.ZAK_DOHOD_DILERA ?? 0;
            set
            {
                if (Entity.ZAK_DOHOD_DILERA == value) return;
                Entity.ZAK_DOHOD_DILERA = value;
                RaisePropertyChanged();
            }
        }

        //public decimal? ZAK_RUB_CRS_SUMMA
        //{
        //    get => Entity.ZAK_RUB_CRS_SUMMA;
        //    set
        //    {
        //        if (Entity.ZAK_RUB_CRS_SUMMA == value) return;
        //        Entity.ZAK_RUB_CRS_SUMMA = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public double? ZAK_RUB_CRS_RATE
        //{
        //    get => Entity.ZAK_RUB_CRS_RATE;
        //    set
        //    {
        //        if (Entity.ZAK_RUB_CRS_RATE == value) return;
        //        Entity.ZAK_RUB_CRS_RATE = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public short? ZAK_OBRAT_RASCHET
        //{
        //    get => Entity.ZAK_OBRAT_RASCHET;
        //    set
        //    {
        //        if (Entity.ZAK_OBRAT_RASCHET == value) return;
        //        Entity.ZAK_OBRAT_RASCHET = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public bool IsBackCalc
        {
            get => Entity.ZAK_OBRAT_RASCHET == 1;
            set
            {
                if (Entity.ZAK_OBRAT_RASCHET == 1 == value) return;
                Entity.ZAK_OBRAT_RASCHET = (short?) (value ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        //public DateTime? ZAK_CONTROL_DATE
        //{
        //    get => Entity.ZAK_CONTROL_DATE;
        //    set
        //    {
        //        if (Entity.ZAK_CONTROL_DATE == value) return;
        //        Entity.ZAK_CONTROL_DATE = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public string ZAK_CONTROL_NOTE
        //{
        //    get => Entity.ZAK_CONTROL_NOTE;
        //    set
        //    {
        //        if (Entity.ZAK_CONTROL_NOTE == value) return;
        //        Entity.ZAK_CONTROL_NOTE = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public string ZAK_CONTROL_FLAGS
        //{
        //    get => Entity.ZAK_CONTROL_FLAGS;
        //    set
        //    {
        //        if (Entity.ZAK_CONTROL_FLAGS == value) return;
        //        Entity.ZAK_CONTROL_FLAGS = value;
        //        RaisePropertyChanged();
        //    }
        //}

        //public decimal? ZAK_POSTAV_DC
        //{
        //    get => Entity.ZAK_POSTAV_DC;
        //    set
        //    {
        //        if (Entity.ZAK_POSTAV_DC == value) return;
        //        Entity.ZAK_POSTAV_DC = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public Kontragent Provider
        {
            get => Entity.ZAK_POSTAV_DC != null ? MainReferences.GetKontragent(Entity.ZAK_POSTAV_DC) : null;
            set
            {
                if (MainReferences.GetKontragent(Entity.ZAK_POSTAV_DC) == value) return;
                Entity.ZAK_POSTAV_DC = value?.DocCode;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Отгружено
        /// </summary>
        public decimal Shipped
        {
            get => myShipped;
            set
            {
                if (myShipped == value) return;
                myShipped = value;
                RaisePropertyChanged();
            }
        }

        public decimal? ZAK_SUDNO_DC
        {
            get => Entity.ZAK_SUDNO_DC;
            set
            {
                if (Entity.ZAK_SUDNO_DC == value) return;
                Entity.ZAK_SUDNO_DC = value;
                RaisePropertyChanged();
            }
        }

        public string ZAK_OUT_NUM
        {
            get => Entity.ZAK_OUT_NUM;
            set
            {
                if (Entity.ZAK_OUT_NUM == value) return;
                Entity.ZAK_OUT_NUM = value;
                RaisePropertyChanged();
            }
        }

        public SD_102 SD_102
        {
            get => Entity.SD_102;
            set
            {
                if (Entity.SD_102 == value) return;
                Entity.SD_102 = value;
                RaisePropertyChanged();
            }
        }

        public SD_103 SD_103
        {
            get => Entity.SD_103;
            set
            {
                if (Entity.SD_103 == value) return;
                Entity.SD_103 = value;
                RaisePropertyChanged();
            }
        }

        public SD_148 SD_148
        {
            get => Entity.SD_148;
            set
            {
                if (Entity.SD_148 == value) return;
                Entity.SD_148 = value;
                RaisePropertyChanged();
            }
        }

        public SD_189 SD_189
        {
            get => Entity.SD_189;
            set
            {
                if (Entity.SD_189 == value) return;
                Entity.SD_189 = value;
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

        public SD_2 SD_21
        {
            get => Entity.SD_21;
            set
            {
                if (Entity.SD_21 == value) return;
                Entity.SD_21 = value;
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

        public SD_43 SD_432
        {
            get => Entity.SD_432;
            set
            {
                if (Entity.SD_432 == value) return;
                Entity.SD_432 = value;
                RaisePropertyChanged();
            }
        }

        public SD_437 SD_437
        {
            get => Entity.SD_437;
            set
            {
                if (Entity.SD_437 == value) return;
                Entity.SD_437 = value;
                RaisePropertyChanged();
            }
        }

        public SD_9 SD_92
        {
            get => Entity.SD_92;
            set
            {
                if (Entity.SD_92 == value) return;
                Entity.SD_92 = value;
                RaisePropertyChanged();
            }
        }

        public SD_9 Entity
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

        public List<SD_9> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual void Save(SD_9 doc)
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

        public void UpdateFrom(SD_9 ent)
        {
            
        }

        public void UpdateTo(SD_9 ent)
        {

        }

        public SD_9 DefaultValue()
        {
            return new SD_9
            {
                DOC_CODE = -1
            };
        }

        // ReSharper disable once UnusedParameter.Global
        public SD_9 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        // ReSharper disable once UnusedParameter.Global
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public SD_9 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual SD_9 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_9 Load(Guid id)
        {
            throw new NotImplementedException();
        }


    }

    public class SD9_FluentAPI : DataAnnotationForFluentApiBase, IMetadataProvider<SD_9ViewModel>
    {
        void IMetadataProvider<SD_9ViewModel>.BuildMetadata(
            MetadataBuilder<SD_9ViewModel> builder)
        {
            SetNotAutoGenerated(builder);
            builder.Property(_ => _.ZAK_NUM).AutoGenerated().DisplayName("Вн.№").ReadOnly();
            builder.Property(_ => _.ZAK_OUT_NUM).AutoGenerated().DisplayName("Внешн.№");
            builder.Property(_ => _.CREATOR).AutoGenerated().DisplayName("Создатель");
            builder.Property(_ => _.Client).AutoGenerated().DisplayName("Клиент");
            builder.Property(_ => _.ClientCategory).AutoGenerated().DisplayName("Категория");
            builder.Property(_ => _.Currency).AutoGenerated().DisplayName("Валюта");
            builder.Property(_ => _.Diler).AutoGenerated().DisplayName("Дилер");
            builder.Property(_ => _.DogovorType).AutoGenerated().DisplayName("Тип договора");
            builder.Property(_ => _.FormPay).AutoGenerated().DisplayName("Форма расчетов");
            builder.Property(_ => _.IsAccepted).AutoGenerated().DisplayName("Акцептован");
            builder.Property(_ => _.IsBackCalc).AutoGenerated().DisplayName("Обратный расчет");
            builder.Property(_ => _.IsNDSInPrice).AutoGenerated().DisplayName("НДС в цене");
            builder.Property(_ => _.IsExecuted).AutoGenerated().DisplayName("Закрыт");
            builder.Property(_ => _.Note).AutoGenerated().DisplayName("Примечания");
            builder.Property(_ => _.PaySumma).AutoGenerated().DisplayName("Оплачено").DisplayFormatString("n2");
            builder.Property(_ => _.Summa).AutoGenerated().DisplayName("Сумма").DisplayFormatString("n2");
            builder.Property(_ => _.SummaDilera).AutoGenerated().DisplayName("Сумма дилера").DisplayFormatString("n2");
            builder.Property(_ => _.Shipped).AutoGenerated().DisplayName("Отгружено").DisplayFormatString("n2");
        }
    }
}
