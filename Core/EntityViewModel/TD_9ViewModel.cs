using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel
{
    public class TD_9ViewModel : RSViewModelBase, IEntity<TD_9>
    {
        private TD_9 myEntity;

        public TD_9ViewModel()
        {
            Entity = DefaultValue();
        }

        public TD_9ViewModel(TD_9 entity)
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
        public decimal? ZAKT_PARENT_DC
        {
            get => Entity.ZAKT_PARENT_DC;
            set
            {
                if (Entity.ZAKT_PARENT_DC == value) return;
                Entity.ZAKT_PARENT_DC = value;
                RaisePropertyChanged();
            }
        }
        public int? ZAKT_PARENT_CODE
        {
            get => Entity.ZAKT_PARENT_CODE;
            set
            {
                if (Entity.ZAKT_PARENT_CODE == value) return;
                Entity.ZAKT_PARENT_CODE = value;
                RaisePropertyChanged();
            }
        }
        public decimal ZAKT_NOMENKL_DC
        {
            get => Entity.ZAKT_NOMENKL_DC;
            set
            {
                if (Entity.ZAKT_NOMENKL_DC == value) return;
                Entity.ZAKT_NOMENKL_DC = value;
                RaisePropertyChanged();
            }
        }
        public double ZAKT_KOL_IN_ONE
        {
            get => Entity.ZAKT_KOL_IN_ONE;
            set
            {
                if (Entity.ZAKT_KOL_IN_ONE == value) return;
                Entity.ZAKT_KOL_IN_ONE = value;
                RaisePropertyChanged();
            }
        }
        public double ZAKT_KOL_ALL
        {
            get => Entity.ZAKT_KOL_ALL;
            set
            {
                if (Entity.ZAKT_KOL_ALL == value) return;
                Entity.ZAKT_KOL_ALL = value;
                RaisePropertyChanged();
            }
        }
        public double ZAKT_KOL_ALL_FACT
        {
            get => Entity.ZAKT_KOL_ALL_FACT;
            set
            {
                if (Entity.ZAKT_KOL_ALL_FACT == value) return;
                Entity.ZAKT_KOL_ALL_FACT = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAKT_CRS_PRICE
        {
            get => Entity.ZAKT_CRS_PRICE;
            set
            {
                if (Entity.ZAKT_CRS_PRICE == value) return;
                Entity.ZAKT_CRS_PRICE = value;
                RaisePropertyChanged();
            }
        }
        public short ZAKT_CAT_REMOVE
        {
            get => Entity.ZAKT_CAT_REMOVE;
            set
            {
                if (Entity.ZAKT_CAT_REMOVE == value) return;
                Entity.ZAKT_CAT_REMOVE = value;
                RaisePropertyChanged();
            }
        }
        public short? ZAKT_MANUAL_PRICE
        {
            get => Entity.ZAKT_MANUAL_PRICE;
            set
            {
                if (Entity.ZAKT_MANUAL_PRICE == value) return;
                Entity.ZAKT_MANUAL_PRICE = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAKT_ZAK_ED_IZM_DC
        {
            get => Entity.ZAKT_ZAK_ED_IZM_DC;
            set
            {
                if (Entity.ZAKT_ZAK_ED_IZM_DC == value) return;
                Entity.ZAKT_ZAK_ED_IZM_DC = value;
                RaisePropertyChanged();
            }
        }
        public double? ZAKT_ZAK_KOL_ALL
        {
            get => Entity.ZAKT_ZAK_KOL_ALL;
            set
            {
                if (Entity.ZAKT_ZAK_KOL_ALL == value) return;
                Entity.ZAKT_ZAK_KOL_ALL = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAKT_CRS_DC
        {
            get => Entity.ZAKT_CRS_DC;
            set
            {
                if (Entity.ZAKT_CRS_DC == value) return;
                Entity.ZAKT_CRS_DC = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAKT_CRS_RATE
        {
            get => Entity.ZAKT_CRS_RATE;
            set
            {
                if (Entity.ZAKT_CRS_RATE == value) return;
                Entity.ZAKT_CRS_RATE = value;
                RaisePropertyChanged();
            }
        }
        public float? ZAKT_NDS_PERCENT
        {
            get => Entity.ZAKT_NDS_PERCENT;
            set
            {
                if (Entity.ZAKT_NDS_PERCENT == value) return;
                Entity.ZAKT_NDS_PERCENT = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAKT_BASE_PRICE
        {
            get => Entity.ZAKT_BASE_PRICE;
            set
            {
                if (Entity.ZAKT_BASE_PRICE == value) return;
                Entity.ZAKT_BASE_PRICE = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAKT_NACENKA_DILERA
        {
            get => Entity.ZAKT_NACENKA_DILERA;
            set
            {
                if (Entity.ZAKT_NACENKA_DILERA == value) return;
                Entity.ZAKT_NACENKA_DILERA = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAKT_DOG_CRS_ITOG_CENA
        {
            get => Entity.ZAKT_DOG_CRS_ITOG_CENA;
            set
            {
                if (Entity.ZAKT_DOG_CRS_ITOG_CENA == value) return;
                Entity.ZAKT_DOG_CRS_ITOG_CENA = value;
                RaisePropertyChanged();
            }
        }
        public decimal? ZAKT_RUB_SUMMA
        {
            get => Entity.ZAKT_RUB_SUMMA;
            set
            {
                if (Entity.ZAKT_RUB_SUMMA == value) return;
                Entity.ZAKT_RUB_SUMMA = value;
                RaisePropertyChanged();
            }
        }
        public short? ZAKT_PERERAB_PRODUKT
        {
            get => Entity.ZAKT_PERERAB_PRODUKT;
            set
            {
                if (Entity.ZAKT_PERERAB_PRODUKT == value) return;
                Entity.ZAKT_PERERAB_PRODUKT = value;
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
        public TD_9 TD_92
        {
            get => Entity.TD_92;
            set
            {
                if (Entity.TD_92 == value) return;
                Entity.TD_92 = value;
                RaisePropertyChanged();
            }
        }
        public TD_9 Entity
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

        public List<TD_9> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual void Save(TD_9 doc)
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

        public void UpdateFrom(TD_9 ent)
        {
            Code = ent.CODE;
            ZAKT_PARENT_DC = ent.ZAKT_PARENT_DC;
            ZAKT_PARENT_CODE = ent.ZAKT_PARENT_CODE;
            ZAKT_NOMENKL_DC = ent.ZAKT_NOMENKL_DC;
            ZAKT_KOL_IN_ONE = ent.ZAKT_KOL_IN_ONE;
            ZAKT_KOL_ALL = ent.ZAKT_KOL_ALL;
            ZAKT_KOL_ALL_FACT = ent.ZAKT_KOL_ALL_FACT;
            ZAKT_CRS_PRICE = ent.ZAKT_CRS_PRICE;
            ZAKT_CAT_REMOVE = ent.ZAKT_CAT_REMOVE;
            ZAKT_MANUAL_PRICE = ent.ZAKT_MANUAL_PRICE;
            ZAKT_ZAK_ED_IZM_DC = ent.ZAKT_ZAK_ED_IZM_DC;
            ZAKT_ZAK_KOL_ALL = ent.ZAKT_ZAK_KOL_ALL;
            ZAKT_CRS_DC = ent.ZAKT_CRS_DC;
            ZAKT_CRS_RATE = ent.ZAKT_CRS_RATE;
            ZAKT_NDS_PERCENT = ent.ZAKT_NDS_PERCENT;
            ZAKT_BASE_PRICE = ent.ZAKT_BASE_PRICE;
            ZAKT_NACENKA_DILERA = ent.ZAKT_NACENKA_DILERA;
            ZAKT_DOG_CRS_ITOG_CENA = ent.ZAKT_DOG_CRS_ITOG_CENA;
            ZAKT_RUB_SUMMA = ent.ZAKT_RUB_SUMMA;
            ZAKT_PERERAB_PRODUKT = ent.ZAKT_PERERAB_PRODUKT;
            SD_301 = ent.SD_301;
            SD_83 = ent.SD_83;
            SD_9 = ent.SD_9;
            TD_92 = ent.TD_92;
        }

        public void UpdateTo(TD_9 ent)
        {
            ent.CODE = Code;
            ent.ZAKT_PARENT_DC = ZAKT_PARENT_DC;
            ent.ZAKT_PARENT_CODE = ZAKT_PARENT_CODE;
            ent.ZAKT_NOMENKL_DC = ZAKT_NOMENKL_DC;
            ent.ZAKT_KOL_IN_ONE = ZAKT_KOL_IN_ONE;
            ent.ZAKT_KOL_ALL = ZAKT_KOL_ALL;
            ent.ZAKT_KOL_ALL_FACT = ZAKT_KOL_ALL_FACT;
            ent.ZAKT_CRS_PRICE = ZAKT_CRS_PRICE;
            ent.ZAKT_CAT_REMOVE = ZAKT_CAT_REMOVE;
            ent.ZAKT_MANUAL_PRICE = ZAKT_MANUAL_PRICE;
            ent.ZAKT_ZAK_ED_IZM_DC = ZAKT_ZAK_ED_IZM_DC;
            ent.ZAKT_ZAK_KOL_ALL = ZAKT_ZAK_KOL_ALL;
            ent.ZAKT_CRS_DC = ZAKT_CRS_DC;
            ent.ZAKT_CRS_RATE = ZAKT_CRS_RATE;
            ent.ZAKT_NDS_PERCENT = ZAKT_NDS_PERCENT;
            ent.ZAKT_BASE_PRICE = ZAKT_BASE_PRICE;
            ent.ZAKT_NACENKA_DILERA = ZAKT_NACENKA_DILERA;
            ent.ZAKT_DOG_CRS_ITOG_CENA = ZAKT_DOG_CRS_ITOG_CENA;
            ent.ZAKT_RUB_SUMMA = ZAKT_RUB_SUMMA;
            ent.ZAKT_PERERAB_PRODUKT = ZAKT_PERERAB_PRODUKT;
            ent.SD_301 = SD_301;
            ent.SD_83 = SD_83;
            ent.SD_9 = SD_9;
            ent.TD_92 = TD_92;
        }

        public TD_9 DefaultValue()
        {
            return new TD_9
            {
                DOC_CODE = -1,
                CODE = -1
            };
        }

        public TD_9 Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public TD_9 Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual TD_9 Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public virtual TD_9 Load(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}