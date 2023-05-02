using Data;
using KursDomain.Documents.Refund;
using KursDomain.Repository;

namespace KursAM2.Repositories.Refund.RefundFromClient
{
    public class RefundFromClientRepository : GenericKursDBRepository<RefundFromClientViewModel>,
        IRefundFromClientRepository
    {
        public RefundFromClientRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
        }

        public RefundFromClientRepository(ALFAMEDIAEntities context) : base(context)
        {
        }
    }
}
