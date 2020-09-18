using System.Collections.Generic;
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

        public override string Name
        {
            get => Entity.CENT_NAME;
            set
            {
                if (Entity.CENT_NAME == value) return;
                Entity.CENT_NAME = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public string FullName
        {
            get => Entity.CENT_FULLNAME;
            set
            {
                if (Entity.CENT_FULLNAME == value) return;
                Entity.CENT_FULLNAME = value;
                RaisePropertyChanged();
            }
        }

        [DataMember]
        public bool IsDeleted
        {
            get => Entity.IS_DELETED == 1;
            set
            {
                if (Entity.IS_DELETED == (value ? 1 : 0)) return;
                Entity.IS_DELETED = value ? 1 : 0;
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