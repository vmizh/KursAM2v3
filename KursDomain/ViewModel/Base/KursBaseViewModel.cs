using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DevExpress.Mvvm;
using KursDomain.ICommon;

namespace Core.ViewModel.Base
{
    internal interface IKursParentViewModel
    {
        KursBaseViewModel Parent { get; set; }
    }

    public abstract class KursBaseViewModel : ViewModelBase, IKursParentViewModel
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

        [DisplayName("Parent")]
        [Display(AutoGenerateField = false)]
        public KursBaseViewModel Parent { get; set; }

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

        public virtual void RaisePropertyAllChanged()
        {
            foreach (var prop in this.GetType().GetProperties())
            {
                RaisePropertyChanged(prop.Name);
            }
        }

        public virtual void SetChangeStatus(RowStatus state = RowStatus.Edited)
        {
            if (State != RowStatus.NewRow) State = state;
            if (Parent != null && Parent.State != RowStatus.NewRow)
                Parent.State = RowStatus.Edited;
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
