using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.EntityViewModel.Dogovora;
using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;
using Data;
using DevExpress.Charts.Native;
using DevExpress.XtraSpreadsheet.Commands;

namespace Core.EntityViewModel.AktSpisaniya
{
    class AktSpisaniyaTitleViewModel : RSWindowViewModelBase, IEntity<AktSpisaniyaNomenkl_Title>
    {
        #region Fields

        private AktSpisaniyaNomenkl_Title myEntity;

        #endregion

        #region Methods

        private AktSpisaniyaNomenkl_Title DefaultValue()
        {
            return new()
            {
                Id = Guid.NewGuid()
            };
        }

        public List<AktSpisaniyaNomenkl_Title> LoadList()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Constructor

        public AktSpisaniyaTitleViewModel()
        {
            Entity = DefaultValue();
        }

        public AktSpisaniyaTitleViewModel(AktSpisaniyaNomenkl_Title entity, RowStatus state = RowStatus.NotEdited)
        {
            Entity = entity ?? DefaultValue();

            foreach (var akt in Entity.AktSpisaniya_row)
            {
                Rows.Add(new AktSpisaniyaRowViewModel(akt)
                {
                    Parent = this,
                    myState = state
                });
                myState = state;
            }
        }

        #endregion

        #region Properties



        public bool IsAccessRight { get; set; }

        public ObservableCollection<AktSpisaniyaRowViewModel> Rows { set; get; } = new();

        public AktSpisaniyaNomenkl_Title Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value)
                    return;
                myEntity = value;
                RaisePropertiesChanged();
            }
        }

        public Warehouse Warehouse
        {
            get => MainReferences.g
            set
            {
                if(Entity.Warehouse_DC == value)
                    return;
                Entity.Warehouse_DC = value;
                RaisePropertiesChanged();
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
    }
}
