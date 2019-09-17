using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class SD_9ViewModel : RSViewModelBase, IEntity<SD_9>
    {
        private SD_9 myEntity;

        public SD_9ViewModel()
        {
            Entity = DefaultValue();
        }

        public SD_9ViewModel(SD_9 entity)
        {
            Entity = entity ?? DefaultValue();
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
        public decimal? ZAK_CLIENT_DC
        {
            get => Entity.ZAK_CLIENT_DC;
            set
            {
                if (Entity.ZAK_CLIENT_DC == value) return;
                Entity.ZAK_CLIENT_DC = value;
                RaisePropertyChanged();
            }
        }
        public int? ZAK_CLIENT_TABELNUMBER
        {
            get => Entity.ZAK_CLIENT_TABELNUMBER;
            set
            {
                if (Entity.ZAK_CLIENT_TABELNUMBER == value) return;
                Entity.ZAK_CLIENT_TABELNUMBER = value;
                RaisePropertyChanged();
            }
        }
        public string ZAK_CLIENT_NAME
        {
            get => Entity.ZAK_CLIENT_NAME;
            set
            {
                if (Entity.ZAK_CLIENT_NAME == value) return;
                Entity.ZAK_CLIENT_NAME = value;
                RaisePropertyChanged();
            }
        }
        public decimal ZAK_CRS_DC
        {
            get => Entity.ZAK_CRS_DC;
            set
            {
                if (Entity.ZAK_CRS_DC == value) return;
                Entity.ZAK_CRS_DC = value;
                RaisePropertyChanged();
            }
        }
        public double ZAK_CRS_RATE
        {
            get => Entity.ZAK_CRS_RATE;
            set
            {
                if (Entity.ZAK_CRS_RATE == value) return;
                Entity.ZAK_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAK_CRS_SUMMA
        {
            get => Entity.ZAK_CRS_SUMMA;
            set
            {
                if (Entity.ZAK_CRS_SUMMA == value) return;
                Entity.ZAK_CRS_SUMMA = value;
                RaisePropertyChanged();
            }
        }
        public double ZAK_SKID_PERC_QUAN
        {
            get => Entity.ZAK_SKID_PERC_QUAN;
            set
            {
                if (Entity.ZAK_SKID_PERC_QUAN == value) return;
                Entity.ZAK_SKID_PERC_QUAN = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAK_SKID_CRS_SUMMA
        {
            get => Entity.ZAK_SKID_CRS_SUMMA;
            set
            {
                if (Entity.ZAK_SKID_CRS_SUMMA == value) return;
                Entity.ZAK_SKID_CRS_SUMMA = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAK_PAY_CRS_SUMMA
        {
            get => Entity.ZAK_PAY_CRS_SUMMA;
            set
            {
                if (Entity.ZAK_PAY_CRS_SUMMA == value) return;
                Entity.ZAK_PAY_CRS_SUMMA = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAK_PAY_RUB_SUMMA
        {
            get => Entity.ZAK_PAY_RUB_SUMMA;
            set
            {
                if (Entity.ZAK_PAY_RUB_SUMMA == value) return;
                Entity.ZAK_PAY_RUB_SUMMA = value;
                RaisePropertyChanged();
            }
        }
        public short ZAK_ACCEPTED
        {
            get => Entity.ZAK_ACCEPTED;
            set
            {
                if (Entity.ZAK_ACCEPTED == value) return;
                Entity.ZAK_ACCEPTED = value;
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
        public string ZAK_BASE
        {
            get => Entity.ZAK_BASE;
            set
            {
                if (Entity.ZAK_BASE == value) return;
                Entity.ZAK_BASE = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAK_FORM_RASCH_DC
        {
            get => Entity.ZAK_FORM_RASCH_DC;
            set
            {
                if (Entity.ZAK_FORM_RASCH_DC == value) return;
                Entity.ZAK_FORM_RASCH_DC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAK_TYPE_DC
        {
            get => Entity.ZAK_TYPE_DC;
            set
            {
                if (Entity.ZAK_TYPE_DC == value) return;
                Entity.ZAK_TYPE_DC = value;
                RaisePropertyChanged();
            }
        }
        public short? ZAK_EXECUTED
        {
            get => Entity.ZAK_EXECUTED;
            set
            {
                if (Entity.ZAK_EXECUTED == value) return;
                Entity.ZAK_EXECUTED = value;
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
        public int? ZAK_OTV_TABELNUMBER
        {
            get => Entity.ZAK_OTV_TABELNUMBER;
            set
            {
                if (Entity.ZAK_OTV_TABELNUMBER == value) return;
                Entity.ZAK_OTV_TABELNUMBER = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAK_BASE_KOTRACT_DC
        {
            get => Entity.ZAK_BASE_KOTRACT_DC;
            set
            {
                if (Entity.ZAK_BASE_KOTRACT_DC == value) return;
                Entity.ZAK_BASE_KOTRACT_DC = value;
                RaisePropertyChanged();
            }
        }
        public short? ZAK_NDS_1INCLUD_0NO
        {
            get => Entity.ZAK_NDS_1INCLUD_0NO;
            set
            {
                if (Entity.ZAK_NDS_1INCLUD_0NO == value) return;
                Entity.ZAK_NDS_1INCLUD_0NO = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAK_KATEGORY_DC
        {
            get => Entity.ZAK_KATEGORY_DC;
            set
            {
                if (Entity.ZAK_KATEGORY_DC == value) return;
                Entity.ZAK_KATEGORY_DC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAK_DILER_DC
        {
            get => Entity.ZAK_DILER_DC;
            set
            {
                if (Entity.ZAK_DILER_DC == value) return;
                Entity.ZAK_DILER_DC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAK_DOHOD_DILERA
        {
            get => Entity.ZAK_DOHOD_DILERA;
            set
            {
                if (Entity.ZAK_DOHOD_DILERA == value) return;
                Entity.ZAK_DOHOD_DILERA = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAK_RUB_CRS_SUMMA
        {
            get => Entity.ZAK_RUB_CRS_SUMMA;
            set
            {
                if (Entity.ZAK_RUB_CRS_SUMMA == value) return;
                Entity.ZAK_RUB_CRS_SUMMA = value;
                RaisePropertyChanged();
            }
        }
        public double? ZAK_RUB_CRS_RATE
        {
            get => Entity.ZAK_RUB_CRS_RATE;
            set
            {
                if (Entity.ZAK_RUB_CRS_RATE == value) return;
                Entity.ZAK_RUB_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }
        public short? ZAK_OBRAT_RASCHET
        {
            get => Entity.ZAK_OBRAT_RASCHET;
            set
            {
                if (Entity.ZAK_OBRAT_RASCHET == value) return;
                Entity.ZAK_OBRAT_RASCHET = value;
                RaisePropertyChanged();
            }
        }
        public DateTime? ZAK_CONTROL_DATE
        {
            get => Entity.ZAK_CONTROL_DATE;
            set
            {
                if (Entity.ZAK_CONTROL_DATE == value) return;
                Entity.ZAK_CONTROL_DATE = value;
                RaisePropertyChanged();
            }
        }
        public string ZAK_CONTROL_NOTE
        {
            get => Entity.ZAK_CONTROL_NOTE;
            set
            {
                if (Entity.ZAK_CONTROL_NOTE == value) return;
                Entity.ZAK_CONTROL_NOTE = value;
                RaisePropertyChanged();
            }
        }
        public string ZAK_CONTROL_FLAGS
        {
            get => Entity.ZAK_CONTROL_FLAGS;
            set
            {
                if (Entity.ZAK_CONTROL_FLAGS == value) return;
                Entity.ZAK_CONTROL_FLAGS = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAK_POSTAV_DC
        {
            get => Entity.ZAK_POSTAV_DC;
            set
            {
                if (Entity.ZAK_POSTAV_DC == value) return;
                Entity.ZAK_POSTAV_DC = value;
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
            ZAK_DATE = ent.ZAK_DATE;
            ZAK_NUM = ent.ZAK_NUM;
            ZAK_PLAN_OTRGRUZ_DATE = ent.ZAK_PLAN_OTRGRUZ_DATE;
            ZAK_PAY_LAST_DATE = ent.ZAK_PAY_LAST_DATE;
            ZAK_CLIENT_DC = ent.ZAK_CLIENT_DC;
            ZAK_CLIENT_TABELNUMBER = ent.ZAK_CLIENT_TABELNUMBER;
            ZAK_CLIENT_NAME = ent.ZAK_CLIENT_NAME;
            ZAK_CRS_DC = ent.ZAK_CRS_DC;
            ZAK_CRS_RATE = ent.ZAK_CRS_RATE;
            ZAK_CRS_SUMMA = ent.ZAK_CRS_SUMMA;
            ZAK_SKID_PERC_QUAN = ent.ZAK_SKID_PERC_QUAN;
            ZAK_SKID_CRS_SUMMA = ent.ZAK_SKID_CRS_SUMMA;
            ZAK_PAY_CRS_SUMMA = ent.ZAK_PAY_CRS_SUMMA;
            ZAK_PAY_RUB_SUMMA = ent.ZAK_PAY_RUB_SUMMA;
            ZAK_ACCEPTED = ent.ZAK_ACCEPTED;
            CREATOR = ent.CREATOR;
            ZAK_BASE = ent.ZAK_BASE;
            ZAK_FORM_RASCH_DC = ent.ZAK_FORM_RASCH_DC;
            ZAK_TYPE_DC = ent.ZAK_TYPE_DC;
            ZAK_EXECUTED = ent.ZAK_EXECUTED;
            ZAK_START_DATE = ent.ZAK_START_DATE;
            ZAK_STOP_DATE = ent.ZAK_STOP_DATE;
            ZAK_USL_POSTAV_DC = ent.ZAK_USL_POSTAV_DC;
            ZAK_FILE = ent.ZAK_FILE;
            ZAK_OTV_TABELNUMBER = ent.ZAK_OTV_TABELNUMBER;
            ZAK_BASE_KOTRACT_DC = ent.ZAK_BASE_KOTRACT_DC;
            ZAK_NDS_1INCLUD_0NO = ent.ZAK_NDS_1INCLUD_0NO;
            ZAK_KATEGORY_DC = ent.ZAK_KATEGORY_DC;
            ZAK_DILER_DC = ent.ZAK_DILER_DC;
            ZAK_DOHOD_DILERA = ent.ZAK_DOHOD_DILERA;
            ZAK_RUB_CRS_SUMMA = ent.ZAK_RUB_CRS_SUMMA;
            ZAK_RUB_CRS_RATE = ent.ZAK_RUB_CRS_RATE;
            ZAK_OBRAT_RASCHET = ent.ZAK_OBRAT_RASCHET;
            ZAK_CONTROL_DATE = ent.ZAK_CONTROL_DATE;
            ZAK_CONTROL_NOTE = ent.ZAK_CONTROL_NOTE;
            ZAK_CONTROL_FLAGS = ent.ZAK_CONTROL_FLAGS;
            ZAK_POSTAV_DC = ent.ZAK_POSTAV_DC;
            ZAK_SUDNO_DC = ent.ZAK_SUDNO_DC;
            ZAK_OUT_NUM = ent.ZAK_OUT_NUM;
            SD_102 = ent.SD_102;
            SD_103 = ent.SD_103;
            SD_148 = ent.SD_148;
            SD_189 = ent.SD_189;
            SD_2 = ent.SD_2;
            SD_21 = ent.SD_21;
            SD_301 = ent.SD_301;
            SD_43 = ent.SD_43;
            SD_431 = ent.SD_431;
            SD_432 = ent.SD_432;
            SD_437 = ent.SD_437;
            SD_92 = ent.SD_92;
        }

        public void UpdateTo(SD_9 ent)
        {
            ent.ZAK_DATE = ZAK_DATE;
            ent.ZAK_NUM = ZAK_NUM;
            ent.ZAK_PLAN_OTRGRUZ_DATE = ZAK_PLAN_OTRGRUZ_DATE;
            ent.ZAK_PAY_LAST_DATE = ZAK_PAY_LAST_DATE;
            ent.ZAK_CLIENT_DC = ZAK_CLIENT_DC;
            ent.ZAK_CLIENT_TABELNUMBER = ZAK_CLIENT_TABELNUMBER;
            ent.ZAK_CLIENT_NAME = ZAK_CLIENT_NAME;
            ent.ZAK_CRS_DC = ZAK_CRS_DC;
            ent.ZAK_CRS_RATE = ZAK_CRS_RATE;
            ent.ZAK_CRS_SUMMA = ZAK_CRS_SUMMA;
            ent.ZAK_SKID_PERC_QUAN = ZAK_SKID_PERC_QUAN;
            ent.ZAK_SKID_CRS_SUMMA = ZAK_SKID_CRS_SUMMA;
            ent.ZAK_PAY_CRS_SUMMA = ZAK_PAY_CRS_SUMMA;
            ent.ZAK_PAY_RUB_SUMMA = ZAK_PAY_RUB_SUMMA;
            ent.ZAK_ACCEPTED = ZAK_ACCEPTED;
            ent.CREATOR = CREATOR;
            ent.ZAK_BASE = ZAK_BASE;
            ent.ZAK_FORM_RASCH_DC = ZAK_FORM_RASCH_DC;
            ent.ZAK_TYPE_DC = ZAK_TYPE_DC;
            ent.ZAK_EXECUTED = ZAK_EXECUTED;
            ent.ZAK_START_DATE = ZAK_START_DATE;
            ent.ZAK_STOP_DATE = ZAK_STOP_DATE;
            ent.ZAK_USL_POSTAV_DC = ZAK_USL_POSTAV_DC;
            ent.ZAK_FILE = ZAK_FILE;
            ent.ZAK_OTV_TABELNUMBER = ZAK_OTV_TABELNUMBER;
            ent.ZAK_BASE_KOTRACT_DC = ZAK_BASE_KOTRACT_DC;
            ent.ZAK_NDS_1INCLUD_0NO = ZAK_NDS_1INCLUD_0NO;
            ent.ZAK_KATEGORY_DC = ZAK_KATEGORY_DC;
            ent.ZAK_DILER_DC = ZAK_DILER_DC;
            ent.ZAK_DOHOD_DILERA = ZAK_DOHOD_DILERA;
            ent.ZAK_RUB_CRS_SUMMA = ZAK_RUB_CRS_SUMMA;
            ent.ZAK_RUB_CRS_RATE = ZAK_RUB_CRS_RATE;
            ent.ZAK_OBRAT_RASCHET = ZAK_OBRAT_RASCHET;
            ent.ZAK_CONTROL_DATE = ZAK_CONTROL_DATE;
            ent.ZAK_CONTROL_NOTE = ZAK_CONTROL_NOTE;
            ent.ZAK_CONTROL_FLAGS = ZAK_CONTROL_FLAGS;
            ent.ZAK_POSTAV_DC = ZAK_POSTAV_DC;
            ent.ZAK_SUDNO_DC = ZAK_SUDNO_DC;
            ent.ZAK_OUT_NUM = ZAK_OUT_NUM;
            ent.SD_102 = SD_102;
            ent.SD_103 = SD_103;
            ent.SD_148 = SD_148;
            ent.SD_189 = SD_189;
            ent.SD_2 = SD_2;
            ent.SD_21 = SD_21;
            ent.SD_301 = SD_301;
            ent.SD_43 = SD_43;
            ent.SD_431 = SD_431;
            ent.SD_432 = SD_432;
            ent.SD_437 = SD_437;
            ent.SD_92 = SD_92;
        }

        public SD_9 DefaultValue()
        {
            return new SD_9
            {
                DOC_CODE = -1
            };
        }

        public SD_9 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

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
}