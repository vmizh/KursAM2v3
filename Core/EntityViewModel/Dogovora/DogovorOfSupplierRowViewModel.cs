using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.Dogovora
{

    public sealed class DogovorOfSupplierRowViewModel : RSViewModelBase, IDataErrorInfo, IEntity<DogovorOfSupplierRow>
    {
        #region Fields

        private DogovorOfSupplierRow myEntity;

        #endregion

        #region Properties

        [Display(AutoGenerateField = false)]
        public override string Name { get; set; }

        [Display(AutoGenerateField = false)]
        public DogovorOfSupplierRow Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
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

        [Display(AutoGenerateField = false)]
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

        [Display(AutoGenerateField = true, Name = "Ном.№")]
        public string NomenklNumber => Nomenkl?.NomenklNumber;

        [Display(AutoGenerateField = true, Name = "Номенклатура")]
        public Nomenkl Nomenkl
        {
            get => Entity.NomenklDC == 0 ? null : MainReferences.GetNomenkl(Entity.NomenklDC);
            set
            {
                if (MainReferences.GetNomenkl(Entity.NomenklDC) == value) return;
                Entity.NomenklDC = value?.DocCode ?? 0;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Кол-во")]
        [DisplayFormat(DataFormatString = "n4", ApplyFormatInEditMode = true)]
        public decimal Quantity
        {
            get => Entity.Quantity;
            set
            {
                if (Entity.Quantity == value) return;
                Entity.Quantity = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Ед.изм.")]
        public Unit Unit => Nomenkl?.Unit;

        [Display(AutoGenerateField = true, Name = "Цена")]
        [DisplayFormat(DataFormatString = "n2", ApplyFormatInEditMode = true)]
        public decimal Price
        {
            get => Entity.Price;
            set
            {
                if (Entity.Price == value) return;
                Entity.Price = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "НДС %")]
        [DisplayFormat(DataFormatString = "n2", ApplyFormatInEditMode = true)]
        public decimal NDSPercent
        {
            get => Entity.NDSPercent;
            set
            {
                if (Entity.NDSPercent == value) return;
                Entity.NDSPercent = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = true, Name = "Сумма")]
        [DisplayFormat(DataFormatString = "n2", ApplyFormatInEditMode = true)]
        public decimal Summa => Math.Round(Quantity * Price + Quantity * Price * NDSPercent / 100, 2);

        [Display(AutoGenerateField = true, Name = "Примечания")]
        public override string Note
        {
            get => Entity.Note;
            set
            {
                if (Entity.Note == value) return;
                Entity.Note = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Constructors

        public DogovorOfSupplierRowViewModel()
        {
            Entity = DefaultValue();
        }

        public DogovorOfSupplierRowViewModel(DogovorOfSupplierRow entity, DogovorOfSupplierViewModel parent = null)
        {
            Entity = entity ?? DefaultValue();
            Parent = parent;
        }

        #endregion

        #region Methods

        private DogovorOfSupplierRow DefaultValue()
        {
            return new()
            {
                Id = Guid.NewGuid()
            };
        }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    //case "Kontragent":
                    //    return Kontragent == null ? "Поставщик должен быть обязательно выбран" : null;
                    //case "DogType":
                    //    return DogType == null ? "Тип договора должен быть обязательно выбран" : null;
                    //case "PayCondition":
                    //    return PayCondition == null ? "Условия оплаты должны быть указаны" : null;
                    //case "FormOfPayment":
                    //    return DogType == null ? "Форма оплаты - обязательное поле" : null;
                    default:
                        return null;
                }
            }
        }
        [Display(AutoGenerateField = false)]
        public string Error => null;

        #endregion
    }
}