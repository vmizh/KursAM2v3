using System;
using Core;
using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;
using Data;
using Helper;
using Newtonsoft.Json;

namespace KursAM2.ViewModel.Logistiks
{
    public class InventorySheetRowViewModel : RSViewModelData, IEntity<TD_24>
    {
        #region Constructors

        public InventorySheetRowViewModel(TD_24 entity)
        {
            Entity = entity ?? DefaultValue();
            LoadReference();
        }

        #endregion

        #region Fields

        #endregion

        #region Properties

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

        public Guid DocId
        {
            get => Entity.DocId;
            set
            {
                if (Entity.DocId == value)
                    return;
                Entity.DocId = value;
                RaisePropertiesChanged();
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

        public override int Code { 
            get => Entity.CODE;
            set
            {
                if (Entity.CODE == value) return;
                Entity.CODE = value;
                RaisePropertyChanged();
            } }

        public Nomenkl Nomenkl
        {
            get => Entity.DDT_NOMENKL_DC != 0 ? MainReferences.GetNomenkl(Entity.DDT_NOMENKL_DC) : null;
            set
            {
                if (MainReferences.GetNomenkl(Entity.DDT_NOMENKL_DC) == value) return;
                Entity.DDT_NOMENKL_DC = value?.DOC_CODE ?? 0;
                if (value != null)
                {
                    Entity.DDT_ED_IZM_DC = value.Unit.DocCode;
                    Entity.DDT_POST_ED_IZM_DC = value.Unit.DocCode;
                    Entity.DDT_CRS_DC = value.Currency.DocCode;
                }
                RaisePropertyChanged();
            }
        }

        public string NomenklNumber => Nomenkl?.NomenklNumber;
        public string NomenklName => Nomenkl?.Name;
        public string NomenklUnit => Nomenkl?.Unit?.Name;
        public string NomenklCrsName => Nomenkl?.Currency?.Name;

        public decimal QuantityCalc
        {
            get => (decimal)(Entity.DDT_OSTAT_STAR ?? 0);
            set
            {
                if ((decimal)(Entity.DDT_OSTAT_STAR ?? 0) == value) return;
                Entity.DDT_OSTAT_STAR = (double?)value;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(Summa));
            }
        }

        public decimal QuantityFact
        {
            get => (decimal)(Entity.DDT_OSTAT_NOV ?? 0);
            set
            {
                if ((decimal)(Entity.DDT_OSTAT_NOV ?? 0) == value) return;
                Entity.DDT_OSTAT_NOV = (double?)value;
                if (Difference > 0)
                {
                    Entity.DDT_KOL_PRIHOD = Difference;
                }
                else
                {
                    Entity.DDT_KOL_RASHOD = Difference;
                }
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(Difference));
                RaisePropertiesChanged(nameof(Summa));
            }
        }

        public decimal Difference => QuantityFact - QuantityCalc;

        

        public bool IsTaxExecuted
        {
            get => Entity.DDT_TAX_EXECUTED != 0;
            set
            {
                if (Entity.DDT_TAX_EXECUTED == (value ? 1 : 0)) return;
                Entity.DDT_TAX_EXECUTED = (short)(value ? 1 : 0);
                RaisePropertyChanged();
            }
        }

        public decimal Summa => Price * Difference;

        public decimal Price
        {
            get => Entity.DDT_TAX_CRS_CENA ?? 0;
            set
            {
                if ((Entity.DDT_TAX_CRS_CENA ?? 0) == value) return;
                Entity.DDT_TAX_CRS_CENA = value;
                RaisePropertyChanged();
                RaisePropertiesChanged(nameof(Summa));
            }
        }

        public TD_24 Entity { get; set; }

        #endregion

        #region Commands

        #endregion

        #region Methods

        private void LoadReference()
        {
        }

        public TD_24 DefaultValue()
        {
            return new TD_24
            {
                DOC_CODE = -1,
                CODE = -1
            };
        }

        public override object ToJson()
        {
            var res = new
            {
                Статус = CustomFormat.GetEnumName(State),
                Id,
                DocCode,
                Code,
                Номенклатурный_Номер = NomenklNumber,
                Номенклатура = NomenklName,
                Ед_Изм = NomenklUnit,
                Валюта = NomenklCrsName,
                Кол_расчетное = QuantityCalc,
                Кол_фактическое = QuantityFact,
                Разница = Difference,
                Таксировано =IsTaxExecuted,
                Цена = Price,
                Сумма = Summa,
                Примечание = Note

            };
            return JsonConvert.SerializeObject(res);
        }

        #endregion

        #region IDataErrorInfo

        #endregion
    }
}