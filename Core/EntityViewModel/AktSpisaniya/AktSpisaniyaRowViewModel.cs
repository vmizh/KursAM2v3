using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.ViewModel.Base;
using Data;

namespace Core.EntityViewModel.AktSpisaniya
{
    class AktSpisaniyaRowViewModel: RSViewModelBase
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

        public AktSpisaniyaRowViewModel(AktSpisaniya_row entity, AktSpisaniyaNomenkl_Title parent = null)
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

        #endregion
    }
}
