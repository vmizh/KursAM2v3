using Data;
using KursDomain.References;
using KursDomain.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;

namespace KursAM2.Repositories
{
    public interface IBankRepozitories
    {
        List<BankViewModel> GetAllBanks();
    }

    public class BankRepozitories : GenericKursDBRepository<SD_44>, IBankRepozitories
    {
        public UnitOfWork<ALFAMEDIAEntities> UnitOfWork;

        public BankRepozitories(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            UnitOfWork = (UnitOfWork<ALFAMEDIAEntities>)unitOfWork;
        }

        public List<BankViewModel> GetAllBanks()
        {
            return Context.SD_44.ToList().Select(c => new BankViewModel(c)).ToList();
        }
    }
}
