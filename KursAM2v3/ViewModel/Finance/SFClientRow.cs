using System;
using System.Globalization;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Common;
using Data;

namespace KursAM2.ViewModel.Finance
{
    public class SFClientRow : TD_84ViewModel
    {
        private decimal myCurrentRemains;


        private decimal myRest;


        private decimal myShipped;

        public SFClientRow()
        {
        }

        public SFClientRow(TD_84 entity)
            : base(entity)
        {
        }

        public Nomenkl Nomenkl
        {
            get => MainReferences.GetNomenkl(Entity.SFT_NEMENKL_DC);
            set
            {
                if (Entity.SFT_NEMENKL_DC == value.DOC_CODE) return;
                Entity.SFT_NEMENKL_DC = value.DOC_CODE;
                RaisePropertyChanged();
            }
        }

        public string NomenklNumber => Nomenkl.NomenklNumber;
        public bool IsUsluga => Nomenkl.NOM_0MATER_1USLUGA == 1;

        public Country Country
        {
            get
            {
                if (Entity.SFT_COUNTRY_CODE == null) return new Country {Name = Entity.SFT_STRANA_PROIS};
                if (MainReferences.Countries.ContainsKey(Convert.ToDecimal(Entity.SFT_COUNTRY_CODE.Trim())))
                    return MainReferences.Countries[Convert.ToDecimal(Entity.SFT_COUNTRY_CODE.Trim())];
                return null;
            }
            set
            {
                if (value == null)
                {
                    Entity.SFT_COUNTRY_CODE = null;
                }
                else
                {
                    if (Entity.SFT_COUNTRY_CODE == Convert.ToString(value.Iso, CultureInfo.CurrentCulture)) return;
                    Entity.SFT_COUNTRY_CODE = Convert.ToString(value.Iso, CultureInfo.CurrentCulture);
                    Entity.SFT_STRANA_PROIS = value.Name;
                }
                RaisePropertyChanged();
            }
        }

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

        /// <summary>
        ///     текущие остатки на складах
        /// </summary>
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
    }
}