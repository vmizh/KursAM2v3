using Core.EntityViewModel;
using Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Core.ViewModel.Base;

namespace Core.ViewModel.Common
{
    /// <summary>
    ///     Класс контракта для Центров ответственности
    /// </summary>
    [DataContract]
    public class CentrOfResponsibility : RSViewModelBase
    {
        #region Constructor

        public CentrOfResponsibility()
        {
            Entity = DefaultValue();
        }

        public CentrOfResponsibility(SD_40 entity)
        {
            Entity = entity ?? DefaultValue();
        }

        #endregion

        private SD_40 myEntity;
        [Display(AutoGenerateField = false)]
        public SD_40 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }

        #region Properties

        [Display(AutoGenerateField = false)]
        public override decimal DocCode
        {
            get => Entity.DOC_CODE;
            set
            {
                if (Entity.DOC_CODE == value)
                    return;
                Entity.DOC_CODE = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public decimal? CentParentDC
        {
            get => Entity.CENT_PARENT_DC;
            set
            {
                if (Entity.CENT_PARENT_DC == value)
                    return;
                Entity.CENT_PARENT_DC = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Код")]
        public override string Name
        {
            get => Entity.CENT_NAME;
            set
            {
                if (Entity.CENT_NAME == value)
                    return;
                Entity.CENT_NAME = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Полное наименование")]
        public string FullName
        {
            get => Entity.CENT_FULLNAME;
            set
            {
                if (Entity.CENT_FULLNAME == value)
                    return;
                Entity.CENT_FULLNAME = value;
                RaisePropertyChanged();
            }
        }

        [Display(Name = "Удален")]
        public int? IsDeleted
        {
            get => Entity.IS_DELETED;
            set
            {
                if (Entity.IS_DELETED == value)
                    return;
                Entity.IS_DELETED = value;
                RaisePropertyChanged();
            }
        }

        [Display(AutoGenerateField = false)]
        public SD_40 SD_402
        {
            get => Entity.SD_402;
            set
            {
                if (Entity.SD_402 == value) return;
                Entity.SD_402 = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Methods

         public void UpdateFrom(SD_40 ent)
        {
            FullName = ent.CENT_FULLNAME;
            Name = ent.CENT_NAME;
            CentParentDC = ent.CENT_PARENT_DC;
            IsDeleted= ent.IS_DELETED;
            SD_402 = ent.SD_402;
        }

         public SD_40 DefaultValue()
         {
             return new SD_40
             {
                 DOC_CODE = -1
             };
         }

        #endregion

        public override string ToString()
        {
            return Name;
        }

        public IList<CentrOfResponsibility> GetAllFromDataBase()
        {
            return GlobalOptions.GetEntities().SD_40.Select(item => new CentrOfResponsibility(item)).ToList();
        }
    }
}