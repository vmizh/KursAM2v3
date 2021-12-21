using Data;
using Data.Repository;

namespace KursAM2.ViewModel.StockHolder
{
    public interface IStockHolderReestrRepository
    {
        GenericKursDBRepository<StockHolders> GenericRepository { set; get; }
    }


    public class StockHolderReestrRepository : GenericKursDBRepository<StockHolders>, IStockHolderReestrRepository
    {
        public StockHolderReestrRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            GenericRepository = new GenericKursDBRepository<StockHolders>(unitOfWork);
        }

        public StockHolderReestrRepository(ALFAMEDIAEntities context) : base(context)
        {
            GenericRepository = new GenericKursDBRepository<StockHolders>(context);
        }

        public GenericKursDBRepository<StockHolders> GenericRepository { get; set; }
    }
}