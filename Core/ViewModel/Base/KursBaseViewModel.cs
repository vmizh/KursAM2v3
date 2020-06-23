using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm;

namespace Core.ViewModel.Base
{
    public abstract class KursBaseViewModel : ViewModelBase
    {

        #region Fields

        #endregion

        #region Constructors

        #endregion

        #region Properties
        [DisplayName("Id")]
        [Display(AutoGenerateField = false)]
        public virtual Guid Id
        {
            get => GetValue<Guid>();
            set => SetValue(value);
        }

        [DisplayName("DocCode")]
        [Display(AutoGenerateField = false)]
        public virtual decimal DocCode
        {
            get => GetValue<decimal>();
            set => SetValue(value);
        }

        [DisplayName("State")]
        [Display(AutoGenerateField = false)]
        public RowStatus State
        {
            get => GetValue<RowStatus>();
            set => SetValue(value);
        }

        #endregion

        #region Methods

        public void SetChangeStatus(RowStatus state = RowStatus.Edited)
        {
            if (State == RowStatus.NewRow) return;
            State = state;
            RaisePropertyChanged();
        }

        public void SetChangeStatus(string propName, RowStatus state = RowStatus.Edited)
        {
            SetChangeStatus(state);
            RaisePropertyChanged(propName);
        }

        #endregion

        #region Commands

        #endregion
    }
}