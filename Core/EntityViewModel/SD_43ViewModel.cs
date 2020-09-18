using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class SD_43ViewModel : RSViewModelBase, IEntity<SD_43>
    {
        private SD_43 myEntity;

        public SD_43ViewModel()
        {
            Entity = new SD_43 {DOC_CODE = -1};
        }

        public SD_43ViewModel(SD_43 entity)
        {
            Entity = entity ?? new SD_43 {DOC_CODE = -1};
            LoadReference();
        }

        public SD_43 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
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
                {
                    if (Entity.DOC_CODE == value) return;
                    Entity.DOC_CODE = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string INN
        {
            get => Entity.INN;
            set
            {
                if (Entity.INN == value) return;
                Entity.INN = value;
                RaisePropertyChanged();
            }
        }

        public string NAME
        {
            get => Entity.NAME;
            set
            {
                if (Entity.NAME == value) return;
                Entity.NAME = value;
                RaisePropertyChanged();
            }
        }

        public string HEADER
        {
            get => Entity.HEADER;
            set
            {
                if (Entity.HEADER == value) return;
                Entity.HEADER = value;
                RaisePropertyChanged();
            }
        }

        public string GLAVBUH
        {
            get => Entity.GLAVBUH;
            set
            {
                if (Entity.GLAVBUH == value) return;
                Entity.GLAVBUH = value;
                RaisePropertyChanged();
            }
        }

        public string NOTES
        {
            get => Entity.NOTES;
            set
            {
                if (Entity.NOTES == value) return;
                Entity.NOTES = value;
                RaisePropertyChanged();
            }
        }

        public string TYPE_PROP
        {
            get => Entity.TYPE_PROP;
            set
            {
                if (Entity.TYPE_PROP == value) return;
                Entity.TYPE_PROP = value;
                RaisePropertyChanged();
            }
        }

        public short? DELETED
        {
            get => Entity.DELETED;
            set
            {
                if (Entity.DELETED == value) return;
                Entity.DELETED = value;
                RaisePropertyChanged();
            }
        }

        public bool IsDeleted
        {
            get => DELETED == 1;
            set
            {
                if (DELETED == 1 == value) return;
                DELETED = (short?) (value ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        public string ADDRESS
        {
            get => Entity.ADDRESS;
            set
            {
                if (Entity.ADDRESS == value) return;
                Entity.ADDRESS = value;
                RaisePropertyChanged();
            }
        }

        public string TEL
        {
            get => Entity.TEL;
            set
            {
                if (Entity.TEL == value) return;
                Entity.TEL = value;
                RaisePropertyChanged();
            }
        }

        public string FAX
        {
            get => Entity.FAX;
            set
            {
                if (Entity.FAX == value) return;
                Entity.FAX = value;
                RaisePropertyChanged();
            }
        }

        public string OKPO
        {
            get => Entity.OKPO;
            set
            {
                if (Entity.OKPO == value) return;
                Entity.OKPO = value;
                RaisePropertyChanged();
            }
        }

        public string OKONH
        {
            get => Entity.OKONH;
            set
            {
                if (Entity.OKONH == value) return;
                Entity.OKONH = value;
                RaisePropertyChanged();
            }
        }

        public short? FLAG_0UR_1PHYS
        {
            get => Entity.FLAG_0UR_1PHYS;
            set
            {
                if (Entity.FLAG_0UR_1PHYS == value) return;
                Entity.FLAG_0UR_1PHYS = value;
                RaisePropertyChanged();
            }
        }

        public string PASSPORT
        {
            get => Entity.PASSPORT;
            set
            {
                if (Entity.PASSPORT == value) return;
                Entity.PASSPORT = value;
                RaisePropertyChanged();
            }
        }

        public short? SHIPPING_TRAIN_DAYS
        {
            get => Entity.SHIPPING_TRAIN_DAYS;
            set
            {
                if (Entity.SHIPPING_TRAIN_DAYS == value) return;
                Entity.SHIPPING_TRAIN_DAYS = value;
                RaisePropertyChanged();
            }
        }

        public short? SHIPPING_AUTO_DAYS
        {
            get => Entity.SHIPPING_AUTO_DAYS;
            set
            {
                if (Entity.SHIPPING_AUTO_DAYS == value) return;
                Entity.SHIPPING_AUTO_DAYS = value;
                RaisePropertyChanged();
            }
        }

        public short? PAYMENT_DAYS
        {
            get => Entity.PAYMENT_DAYS;
            set
            {
                if (Entity.PAYMENT_DAYS == value) return;
                Entity.PAYMENT_DAYS = value;
                RaisePropertyChanged();
            }
        }

        public int? EG_ID
        {
            get => Entity.EG_ID;
            set
            {
                if (Entity.EG_ID == value) return;
                Entity.EG_ID = value;
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
            }
        }

        public decimal? NAL_PAYER_DC
        {
            get => Entity.NAL_PAYER_DC;
            set
            {
                if (Entity.NAL_PAYER_DC == value) return;
                Entity.NAL_PAYER_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? REGION_DC
        {
            get => Entity.REGION_DC;
            set
            {
                if (Entity.REGION_DC == value) return;
                Entity.REGION_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? CLIENT_CATEG_DC
        {
            get => Entity.CLIENT_CATEG_DC;
            set
            {
                if (Entity.CLIENT_CATEG_DC == value) return;
                Entity.CLIENT_CATEG_DC = value;
                RaisePropertyChanged();
            }
        }

        public short? AUTO_CLIENT_CATEGORY
        {
            get => Entity.AUTO_CLIENT_CATEGORY;
            set
            {
                if (Entity.AUTO_CLIENT_CATEGORY == value) return;
                Entity.AUTO_CLIENT_CATEGORY = value;
                RaisePropertyChanged();
            }
        }

        public decimal? AB_OTRASL_DC
        {
            get => Entity.AB_OTRASL_DC;
            set
            {
                if (Entity.AB_OTRASL_DC == value) return;
                Entity.AB_OTRASL_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? AB_BUDGET_DC
        {
            get => Entity.AB_BUDGET_DC;
            set
            {
                if (Entity.AB_BUDGET_DC == value) return;
                Entity.AB_BUDGET_DC = value;
                RaisePropertyChanged();
            }
        }

        public decimal? AB_MINISTRY_DC
        {
            get => Entity.AB_MINISTRY_DC;
            set
            {
                if (Entity.AB_MINISTRY_DC == value) return;
                Entity.AB_MINISTRY_DC = value;
                RaisePropertyChanged();
            }
        }

        public short? PODRAZD_CORP_OBOSOBL
        {
            get => Entity.PODRAZD_CORP_OBOSOBL;
            set
            {
                if (Entity.PODRAZD_CORP_OBOSOBL == value) return;
                Entity.PODRAZD_CORP_OBOSOBL = value;
                RaisePropertyChanged();
            }
        }

        public short? PODRAZD_CORP_GOLOVNOE
        {
            get => Entity.PODRAZD_CORP_GOLOVNOE;
            set
            {
                if (Entity.PODRAZD_CORP_GOLOVNOE == value) return;
                Entity.PODRAZD_CORP_GOLOVNOE = value;
                RaisePropertyChanged();
            }
        }

        public short? FLAG_BALANS
        {
            get => Entity.FLAG_BALANS;
            set
            {
                if (Entity.FLAG_BALANS == value) return;
                Entity.FLAG_BALANS = value;
                RaisePropertyChanged();
            }
        }

        public decimal? VALUTA_DC
        {
            get => Entity.VALUTA_DC;
            set
            {
                if (Entity.VALUTA_DC == value) return;
                Entity.VALUTA_DC = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? START_BALANS
        {
            get => Entity.START_BALANS;
            set
            {
                if (Entity.START_BALANS == value) return;
                Entity.START_BALANS = value;
                RaisePropertyChanged();
            }
        }

        public decimal? START_SUMMA
        {
            get => Entity.START_SUMMA;
            set
            {
                if (Entity.START_SUMMA == value) return;
                Entity.START_SUMMA = value;
                RaisePropertyChanged();
            }
        }

        public int? INNER_CODE
        {
            get => Entity.INNER_CODE;
            set
            {
                if (Entity.INNER_CODE == value) return;
                Entity.INNER_CODE = value;
                RaisePropertyChanged();
            }
        }

        public string NAME_FULL
        {
            get => Entity.NAME_FULL;
            set
            {
                if (Entity.NAME_FULL == value) return;
                Entity.NAME_FULL = value;
                RaisePropertyChanged();
            }
        }

        public short? NO_NDS
        {
            get => Entity.NO_NDS;
            set
            {
                if (Entity.NO_NDS == value) return;
                Entity.NO_NDS = value;
                RaisePropertyChanged();
            }
        }

        public string PREFIX_IN_NUMBER
        {
            get => Entity.PREFIX_IN_NUMBER;
            set
            {
                if (Entity.PREFIX_IN_NUMBER == value) return;
                Entity.PREFIX_IN_NUMBER = value;
                RaisePropertyChanged();
            }
        }

        public string CONTAKT_LICO
        {
            get => Entity.CONTAKT_LICO;
            set
            {
                if (Entity.CONTAKT_LICO == value) return;
                Entity.CONTAKT_LICO = value;
                RaisePropertyChanged();
            }
        }

        public string KASSIR
        {
            get => Entity.KASSIR;
            set
            {
                if (Entity.KASSIR == value) return;
                Entity.KASSIR = value;
                RaisePropertyChanged();
            }
        }

        public decimal? SPOSOB_OTPRAV_DC
        {
            get => Entity.SPOSOB_OTPRAV_DC;
            set
            {
                if (Entity.SPOSOB_OTPRAV_DC == value) return;
                Entity.SPOSOB_OTPRAV_DC = value;
                RaisePropertyChanged();
            }
        }

        public string KPP
        {
            get => Entity.KPP;
            set
            {
                if (Entity.KPP == value) return;
                Entity.KPP = value;
                RaisePropertyChanged();
            }
        }

        public short? KONTR_DISABLE
        {
            get => Entity.KONTR_DISABLE;
            set
            {
                if (Entity.KONTR_DISABLE == value) return;
                Entity.KONTR_DISABLE = value;
                RaisePropertyChanged();
            }
        }

        public decimal? MAX_KREDIT_SUM
        {
            get => Entity.MAX_KREDIT_SUM;
            set
            {
                if (Entity.MAX_KREDIT_SUM == value) return;
                Entity.MAX_KREDIT_SUM = value;
                RaisePropertyChanged();
            }
        }

        public double? TRANSP_KOEF
        {
            get => Entity.TRANSP_KOEF;
            set
            {
                if (Entity.TRANSP_KOEF == value) return;
                Entity.TRANSP_KOEF = value;
                RaisePropertyChanged();
            }
        }

        public string TELEKS
        {
            get => Entity.TELEKS;
            set
            {
                if (Entity.TELEKS == value) return;
                Entity.TELEKS = value;
                RaisePropertyChanged();
            }
        }

        public string E_MAIL
        {
            get => Entity.E_MAIL;
            set
            {
                if (Entity.E_MAIL == value) return;
                Entity.E_MAIL = value;
                RaisePropertyChanged();
            }
        }

        public string WWW
        {
            get => Entity.WWW;
            set
            {
                if (Entity.WWW == value) return;
                Entity.WWW = value;
                RaisePropertyChanged();
            }
        }

        public int? OTVETSTV_LICO
        {
            get => Entity.OTVETSTV_LICO;
            set
            {
                if (Entity.OTVETSTV_LICO == value) return;
                Entity.OTVETSTV_LICO = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? LAST_CALC_BALANS
        {
            get => Entity.LAST_CALC_BALANS;
            set
            {
                if (Entity.LAST_CALC_BALANS == value) return;
                Entity.LAST_CALC_BALANS = value;
                RaisePropertyChanged();
            }
        }

        public byte[] LAST_MAX_VERSION
        {
            get => Entity.LAST_MAX_VERSION;
            set
            {
                if (Entity.LAST_MAX_VERSION == value) return;
                Entity.LAST_MAX_VERSION = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? UpdateDate
        {
            get => Entity.UpdateDate;
            set
            {
                if (Entity.UpdateDate == value) return;
                Entity.UpdateDate = value;
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

        public SD_23 SD_23
        {
            get => Entity.SD_23;
            set
            {
                if (Entity.SD_23 == value) return;
                Entity.SD_23 = value;
                RaisePropertyChanged();
            }
        }

        public SD_252 SD_252
        {
            get => Entity.SD_252;
            set
            {
                if (Entity.SD_252 == value) return;
                Entity.SD_252 = value;
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

        public UD_43 UD_43
        {
            get => Entity.UD_43;
            set
            {
                if (Entity.UD_43 == value) return;
                Entity.UD_43 = value;
                RaisePropertyChanged();
            }
        }

        public SD_57 SD_57
        {
            get => Entity.SD_57;
            set
            {
                if (Entity.SD_57 == value) return;
                Entity.SD_57 = value;
                RaisePropertyChanged();
            }
        }

        public SD_53 SD_53
        {
            get => Entity.SD_53;
            set
            {
                if (Entity.SD_53 == value) return;
                Entity.SD_53 = value;
                RaisePropertyChanged();
            }
        }

        public SD_92 SD_92
        {
            get => Entity.SD_92;
            set
            {
                if (Entity.SD_92 == value) return;
                Entity.SD_92 = value;
                RaisePropertyChanged();
            }
        }

        public SD_56 SD_56
        {
            get => Entity.SD_56;
            set
            {
                if (Entity.SD_56 == value) return;
                Entity.SD_56 = value;
                RaisePropertyChanged();
            }
        }

        public bool IsAccessRight { get; set; }

        public List<SD_43> LoadList()
        {
            throw new NotImplementedException();
        }

        private void LoadReference()
        {
            // ReSharper disable once PossibleNullReferenceException
            //Unit = MainReferences.Units.ContainsKey(Entity.NOM_ED_IZM_DC)
            //    ? MainReferences.Units[Entity.NOM_ED_IZM_DC]
            //    : null;
            //Group = MainReferences.NomenklGroups.ContainsKey(Entity.NOM_CATEG_DC)
            //    ? MainReferences.NomenklGroups[Entity.NOM_CATEG_DC]
            //    : null;
            //Currency = MainReferences.GetNomenkl(Entity.DOC_CODE).Currency;
            //if (Entity.NOM_TYPE_DC != null)
            //    NomenklType =
            //        MainReferences.NomenklTypes.ContainsKey(Entity.NOM_TYPE_DC ?? 0)
            //            ? MainReferences.NomenklTypes[Entity.NOM_TYPE_DC.Value]
            //            : null;
        }

        public virtual SD_43 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual SD_43 Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(SD_43 doc)
        {
            throw new NotImplementedException();
        }

        public void UpdateFrom(SD_43 ent)
        {
            INN = ent.INN;
            NAME = ent.NAME;
            HEADER = ent.HEADER;
            GLAVBUH = ent.GLAVBUH;
            NOTES = ent.NOTES;
            TYPE_PROP = ent.TYPE_PROP;
            DELETED = ent.DELETED;
            ADDRESS = ent.ADDRESS;
            TEL = ent.TEL;
            FAX = ent.FAX;
            OKPO = ent.OKPO;
            OKONH = ent.OKONH;
            FLAG_0UR_1PHYS = ent.FLAG_0UR_1PHYS;
            PASSPORT = ent.PASSPORT;
            SHIPPING_TRAIN_DAYS = ent.SHIPPING_TRAIN_DAYS;
            SHIPPING_AUTO_DAYS = ent.SHIPPING_AUTO_DAYS;
            PAYMENT_DAYS = ent.PAYMENT_DAYS;
            EG_ID = ent.EG_ID;
            TABELNUMBER = ent.TABELNUMBER;
            NAL_PAYER_DC = ent.NAL_PAYER_DC;
            REGION_DC = ent.REGION_DC;
            CLIENT_CATEG_DC = ent.CLIENT_CATEG_DC;
            AUTO_CLIENT_CATEGORY = ent.AUTO_CLIENT_CATEGORY;
            AB_OTRASL_DC = ent.AB_OTRASL_DC;
            AB_BUDGET_DC = ent.AB_BUDGET_DC;
            AB_MINISTRY_DC = ent.AB_MINISTRY_DC;
            PODRAZD_CORP_OBOSOBL = ent.PODRAZD_CORP_OBOSOBL;
            PODRAZD_CORP_GOLOVNOE = ent.PODRAZD_CORP_GOLOVNOE;
            FLAG_BALANS = ent.FLAG_BALANS;
            VALUTA_DC = ent.VALUTA_DC;
            START_BALANS = ent.START_BALANS;
            START_SUMMA = ent.START_SUMMA;
            INNER_CODE = ent.INNER_CODE;
            NAME_FULL = ent.NAME_FULL;
            NO_NDS = ent.NO_NDS;
            PREFIX_IN_NUMBER = ent.PREFIX_IN_NUMBER;
            CONTAKT_LICO = ent.CONTAKT_LICO;
            KASSIR = ent.KASSIR;
            SPOSOB_OTPRAV_DC = ent.SPOSOB_OTPRAV_DC;
            KPP = ent.KPP;
            KONTR_DISABLE = ent.KONTR_DISABLE;
            MAX_KREDIT_SUM = ent.MAX_KREDIT_SUM;
            TRANSP_KOEF = ent.TRANSP_KOEF;
            TELEKS = ent.TELEKS;
            E_MAIL = ent.E_MAIL;
            WWW = ent.WWW;
            OTVETSTV_LICO = ent.OTVETSTV_LICO;
            LAST_CALC_BALANS = ent.LAST_CALC_BALANS;
            LAST_MAX_VERSION = ent.LAST_MAX_VERSION;
            UpdateDate = ent.UpdateDate;
            Id = ent.Id;
            SD_2 = ent.SD_2;
            SD_23 = ent.SD_23;
            SD_252 = ent.SD_252;
            SD_301 = ent.SD_301;
            UD_43 = ent.UD_43;
            SD_57 = ent.SD_57;
            SD_53 = ent.SD_53;
            SD_92 = ent.SD_92;
            SD_56 = ent.SD_56;
        }

        public void UpdateTo(SD_43 ent)
        {
            ent.INN = INN;
            ent.NAME = NAME;
            ent.HEADER = HEADER;
            ent.GLAVBUH = GLAVBUH;
            ent.NOTES = NOTES;
            ent.TYPE_PROP = TYPE_PROP;
            ent.DELETED = DELETED;
            ent.ADDRESS = ADDRESS;
            ent.TEL = TEL;
            ent.FAX = FAX;
            ent.OKPO = OKPO;
            ent.OKONH = OKONH;
            ent.FLAG_0UR_1PHYS = FLAG_0UR_1PHYS;
            ent.PASSPORT = PASSPORT;
            ent.SHIPPING_TRAIN_DAYS = SHIPPING_TRAIN_DAYS;
            ent.SHIPPING_AUTO_DAYS = SHIPPING_AUTO_DAYS;
            ent.PAYMENT_DAYS = PAYMENT_DAYS;
            ent.EG_ID = EG_ID;
            ent.TABELNUMBER = TABELNUMBER;
            ent.NAL_PAYER_DC = NAL_PAYER_DC;
            ent.REGION_DC = REGION_DC;
            ent.CLIENT_CATEG_DC = CLIENT_CATEG_DC;
            ent.AUTO_CLIENT_CATEGORY = AUTO_CLIENT_CATEGORY;
            ent.AB_OTRASL_DC = AB_OTRASL_DC;
            ent.AB_BUDGET_DC = AB_BUDGET_DC;
            ent.AB_MINISTRY_DC = AB_MINISTRY_DC;
            ent.PODRAZD_CORP_OBOSOBL = PODRAZD_CORP_OBOSOBL;
            ent.PODRAZD_CORP_GOLOVNOE = PODRAZD_CORP_GOLOVNOE;
            ent.FLAG_BALANS = FLAG_BALANS;
            ent.VALUTA_DC = VALUTA_DC;
            ent.START_BALANS = START_BALANS;
            ent.START_SUMMA = START_SUMMA;
            ent.INNER_CODE = INNER_CODE;
            ent.NAME_FULL = NAME_FULL;
            ent.NO_NDS = NO_NDS;
            ent.PREFIX_IN_NUMBER = PREFIX_IN_NUMBER;
            ent.CONTAKT_LICO = CONTAKT_LICO;
            ent.KASSIR = KASSIR;
            ent.SPOSOB_OTPRAV_DC = SPOSOB_OTPRAV_DC;
            ent.KPP = KPP;
            ent.KONTR_DISABLE = KONTR_DISABLE;
            ent.MAX_KREDIT_SUM = MAX_KREDIT_SUM;
            ent.TRANSP_KOEF = TRANSP_KOEF;
            ent.TELEKS = TELEKS;
            ent.E_MAIL = E_MAIL;
            ent.WWW = WWW;
            ent.OTVETSTV_LICO = OTVETSTV_LICO;
            ent.LAST_CALC_BALANS = LAST_CALC_BALANS;
            ent.LAST_MAX_VERSION = LAST_MAX_VERSION;
            ent.UpdateDate = UpdateDate;
            ent.Id = Id;
            ent.SD_2 = SD_2;
            ent.SD_23 = SD_23;
            ent.SD_252 = SD_252;
            ent.SD_301 = SD_301;
            ent.UD_43 = UD_43;
            ent.SD_57 = SD_57;
            ent.SD_53 = SD_53;
            ent.SD_92 = SD_92;
            ent.SD_56 = SD_56;
        }
    }
}