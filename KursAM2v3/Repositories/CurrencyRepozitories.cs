using System.Collections.Generic;
using System.Linq;
using Data;
using KursDomain.Repository;
using KursDomain.References;

namespace KursAM2.Repositories
{
    public interface ICurrencyRepozitories
    {
        List<CurrencyViewModel> GetAllCurrencies();
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

        public List<CurrencyViewModel> GetAllCurrencies()
        {
            return Context.SD_301.ToList().Select(c => new CurrencyViewModel(c)).ToList();
        }
    }
}
