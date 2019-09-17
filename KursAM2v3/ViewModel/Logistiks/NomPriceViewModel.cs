using System;
using System.Collections.Generic;
using Core.ViewModel.Base;
using Data;

namespace KursAM2.ViewModel.Logistiks
{
    public class NomPriceViewModel : RSViewModelBase, IEntity<NOM_PRICE>
    {
        private NOM_PRICE myEntity;

        public NomPriceViewModel()
        {
            Entity = new NOM_PRICE();
        }

        public NomPriceViewModel(NOM_PRICE entity)
        {
            Entity = entity ?? new NOM_PRICE();
        }

        public string ID
        {
            get => Entity.ID;
            set
            {
                if (Entity.ID == value) return;
                Entity.ID = value;
                RaisePropertyChanged();
            }
        }

        public decimal NOM_DC
        {
            get => Entity.NOM_DC;
            set
            {
                if (Entity.NOM_DC == value) return;
                Entity.NOM_DC = value;
                RaisePropertyChanged();
            }
        }

        public DateTime DATE
        {
            get => Entity.DATE;
            set
            {
                if (Entity.DATE == value) return;
                Entity.DATE = value;
                RaisePropertyChanged();
            }
        }

        public decimal PRICE
        {
            get => Entity.PRICE;
            set
            {
                if (Entity.PRICE == value) return;
                Entity.PRICE = value;
                RaisePropertyChanged();
            }
        }

        public decimal FIFO
        {
            get => Entity.FIFO;
            set
            {
                if (Entity.FIFO == value) return;
                Entity.FIFO = value;
                RaisePropertyChanged();
            }
        }

        public decimal LIFO
        {
            get => Entity.LIFO;
            set
            {
                if (Entity.LIFO == value) return;
                Entity.LIFO = value;
                RaisePropertyChanged();
            }
        }

        public decimal AVG
        {
            get => Entity.AVG;
            set
            {
                if (Entity.AVG == value) return;
                Entity.AVG = value;
                RaisePropertyChanged();
            }
        }

        public string CALC_INFO
        {
            get => Entity.CALC_INFO;
            set
            {
                if (Entity.CALC_INFO == value) return;
                Entity.CALC_INFO = value;
                RaisePropertyChanged();
            }
        }

        public decimal PRICE_WO_REVAL
        {
            get => Entity.PRICE_WO_REVAL;
            set
            {
                if (Entity.PRICE_WO_REVAL == value) return;
                Entity.PRICE_WO_REVAL = value;
                RaisePropertyChanged();
            }
        }

        public decimal KOL_IN
        {
            get => Entity.KOL_IN;
            set
            {
                if (Entity.KOL_IN == value) return;
                Entity.KOL_IN = value;
                RaisePropertyChanged();
            }
        }

        public decimal KOL_OUT
        {
            get => Entity.KOL_OUT;
            set
            {
                if (Entity.KOL_OUT == value) return;
                Entity.KOL_OUT = value;
                RaisePropertyChanged();
            }
        }

        public decimal NAKOPIT
        {
            get => Entity.NAKOPIT;
            set
            {
                if (Entity.NAKOPIT == value) return;
                Entity.NAKOPIT = value;
                RaisePropertyChanged();
            }
        }

        public decimal SUM_IN
        {
            get => Entity.SUM_IN;
            set
            {
                if (Entity.SUM_IN == value) return;
                Entity.SUM_IN = value;
                RaisePropertyChanged();
            }
        }

        public decimal SUM_OUT
        {
            get => Entity.SUM_OUT;
            set
            {
                if (Entity.SUM_OUT == value) return;
                Entity.SUM_OUT = value;
                RaisePropertyChanged();
            }
        }

        public decimal PRICE_WO_NAKLAD
        {
            get => Entity.PRICE_WO_NAKLAD;
            set
            {
                if (Entity.PRICE_WO_NAKLAD == value) return;
                Entity.PRICE_WO_NAKLAD = value;
                RaisePropertyChanged();
            }
        }

        public decimal SUM_IN_WO_NAKLAD
        {
            get => Entity.SUM_IN_WO_NAKLAD;
            set
            {
                if (Entity.SUM_IN_WO_NAKLAD == value) return;
                Entity.SUM_IN_WO_NAKLAD = value;
                RaisePropertyChanged();
            }
        }

        public decimal SUM_OUT_WO_NAKLAD
        {
            get => Entity.SUM_OUT_WO_NAKLAD;
            set
            {
                if (Entity.SUM_OUT_WO_NAKLAD == value) return;
                Entity.SUM_OUT_WO_NAKLAD = value;
                RaisePropertyChanged();
            }
        }

        public decimal? PriceRentabelnost
        {
            get => Entity.PriceRentabelnost;
            set
            {
                if (Entity.PriceRentabelnost == value) return;
                Entity.PriceRentabelnost = value;
                RaisePropertyChanged();
            }
        }

        public decimal? PriceRentabelnostWithNaklad
        {
            get => Entity.PriceRentabelnostWithNaklad;
            set
            {
                if (Entity.PriceRentabelnostWithNaklad == value) return;
                Entity.PriceRentabelnostWithNaklad = value;
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

        public NOM_PRICE Entity
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

        public List<NOM_PRICE> LoadList()
        {
            throw new NotImplementedException();
        }

        public bool IsAccessRight { get; set; }

        public virtual NOM_PRICE Load(decimal dc, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public virtual NOM_PRICE Load(Guid id, bool isShort = true)
        {
            throw new NotImplementedException();
        }

        public NOM_PRICE Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public NOM_PRICE Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(NOM_PRICE doc)
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

        public void UpdateFrom(NOM_PRICE ent)
        {
            ID = ent.ID;
            NOM_DC = ent.NOM_DC;
            DATE = ent.DATE;
            PRICE = ent.PRICE;
            FIFO = ent.FIFO;
            LIFO = ent.LIFO;
            AVG = ent.AVG;
            CALC_INFO = ent.CALC_INFO;
            PRICE_WO_REVAL = ent.PRICE_WO_REVAL;
            KOL_IN = ent.KOL_IN;
            KOL_OUT = ent.KOL_OUT;
            NAKOPIT = ent.NAKOPIT;
            SUM_IN = ent.SUM_IN;
            SUM_OUT = ent.SUM_OUT;
            PRICE_WO_NAKLAD = ent.PRICE_WO_NAKLAD;
            SUM_IN_WO_NAKLAD = ent.SUM_IN_WO_NAKLAD;
            SUM_OUT_WO_NAKLAD = ent.SUM_OUT_WO_NAKLAD;
            PriceRentabelnost = ent.PriceRentabelnost;
            PriceRentabelnostWithNaklad = ent.PriceRentabelnostWithNaklad;
            SD_83 = ent.SD_83;
        }

        public void UpdateTo(NOM_PRICE ent)
        {
            ent.ID = ID;
            ent.NOM_DC = NOM_DC;
            ent.DATE = DATE;
            ent.PRICE = PRICE;
            ent.FIFO = FIFO;
            ent.LIFO = LIFO;
            ent.AVG = AVG;
            ent.CALC_INFO = CALC_INFO;
            ent.PRICE_WO_REVAL = PRICE_WO_REVAL;
            ent.KOL_IN = KOL_IN;
            ent.KOL_OUT = KOL_OUT;
            ent.NAKOPIT = NAKOPIT;
            ent.SUM_IN = SUM_IN;
            ent.SUM_OUT = SUM_OUT;
            ent.PRICE_WO_NAKLAD = PRICE_WO_NAKLAD;
            ent.SUM_IN_WO_NAKLAD = SUM_IN_WO_NAKLAD;
            ent.SUM_OUT_WO_NAKLAD = SUM_OUT_WO_NAKLAD;
            ent.PriceRentabelnost = PriceRentabelnost;
            ent.PriceRentabelnostWithNaklad = PriceRentabelnostWithNaklad;
            ent.SD_83 = SD_83;
        }

        public NOM_PRICE DefaultValue()
        {
            throw new NotImplementedException();
        }
    }
}