using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Core.EntityViewModel;
using Data;

namespace Core.ViewModel.Common
{
    /// <summary>
    ///     Класс контракта для Центров ответственности
    /// </summary>
    [DataContract]
    public class CentrOfResponsibility : SD_40ViewModel
    {
        public CentrOfResponsibility()
        {
        }

        public CentrOfResponsibility(SD_40 doc)
            : base(doc)
        {
        }
        /*public CentrOfResponsibility(SD_40 entity)
        {
            Entity = entity;
        }

        private SD_40 myEntity;

        public SD_40 Entity
        {
            get => myEntity;
            set
            {
                if (myEntity == value) return;
                myEntity = value;
                RaisePropertyChanged();
            }
        }*/

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