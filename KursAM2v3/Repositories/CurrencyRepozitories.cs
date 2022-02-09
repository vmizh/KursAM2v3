using System.Collections.Generic;
using System.Linq;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Data;
using Data.Repository;

namespace KursAM2.Repositories
{
    public interface ICurrencyRepozitories
    {
        List<CurrencyRef> GetAllCurrencies();
    }

    public class CurrencyRepozitories : GenericKursDBRepository<SD_301>, ICurrencyRepozitories 
    {
        public UnitOfWork<ALFAMEDIAEntities> UnitOfWork;
        public CurrencyRepozitories(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            UnitOfWork = (UnitOfWork<ALFAMEDIAEntities>)unitOfWork;
        }

        public CurrencyRepozitories(ALFAMEDIAEntities context) : base(context)
        {
        }

        public List<CurrencyRef> GetAllCurrencies()
        {
            return Context.SD_301.ToList().Select(c => new CurrencyRef(c)
            {
                myState = RowStatus.NotEdited
            }).ToList();
        }
    }
}