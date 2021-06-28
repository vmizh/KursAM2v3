using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.AktSpisaniya
{
    class AktSpisaniyaRowViewModel: RSViewModelBase,IDataErrorInfo
    {
        #region Fields

        private AktSpisaniya_row myEntity;

        #endregion

        #region Methods

        private AktSpisaniya_row DefaultValue()
        {
            return new()
            {
                Id = Guid.NewGuid()
            };
        }

        #endregion

        #region Constructors

        public AktSpisaniyaRowViewModel()
        {
            Entity = DefaultValue();
        }

        public AktSpisaniyaRowViewModel(AktSpisaniya_row entity, AktSpisaniyaTitleViewModel parent = null)
        {
            Entity = entity ?? DefaultValue();
            Parent = parent;
        }

        #endregion

        #region Properties

        public AktSpisaniya_row Entity
        {
            get => myEntity;
            set
            {
                if(myEntity == value) 
                    return;
                myEntity = value;
                RaisePropertiesChanged();
            }
        }

        public override Guid Id
        {
            get => Entity.Id;
            set
            {
                if (Entity.Id == value)
                    return;
                Entity.Id = value;
                RaisePropertiesChanged();
            }
        }

        public Guid DocId
        {
            get => Entity.Doc_Id;
            set
            {
                if (Entity.Doc_Id == value)
                    return;
                Entity.Doc_Id = value;
                RaisePropertiesChanged();
            }
        }

        public Nomenkl Nomenkl
        {
            get => MainReferences.GetNomenkl(Entity.Nomenkl_DC);
            set
            {
                if(Entity.Nomenkl_DC > 0 && MainReferences.GetNomenkl(Entity.Nomenkl_DC) == value)
                    return;
                Entity.Nomenkl_DC = value?.DocCode ?? 0;
                RaisePropertiesChanged();
            }
        }
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


        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Quantity):
                        return Quantity <= 0 ? "Кол-во должно быть > 0" : null;
                    default:
                        return null;
                }
            }
        }

        public string Error { get; } = null;

        #endregion
    }
}
