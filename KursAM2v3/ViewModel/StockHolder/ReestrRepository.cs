using Data;
using Data.Repository;

namespace KursAM2.ViewModel.StockHolder
{
    public interface IStockHolderRepository
    {
        GenericKursDBRepository<StockHolders> GenericRepository { set; get; }
    }


    public class StockHolderRepository : GenericKursDBRepository<StockHolders>, IStockHolderRepository
    {
        public StockHolderRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            GenericRepository = new GenericKursDBRepository<StockHolders>(unitOfWork);
        }

        public StockHolderRepository(ALFAMEDIAEntities context) : base(context)
        {
            GenericRepository = new GenericKursDBRepository<StockHolders>(context);
        }

        public GenericKursDBRepository<StockHolders> GenericRepository { get; set; }
    }

    public interface IStockHolderAccrualTypeRepository
    {
        GenericKursDBRepository<StockHolderAccrualType> GenericRepository { set; get; }
    }


    public class StockHolderAccrualTypeRepository : GenericKursDBRepository<StockHolderAccrualType>, IStockHolderAccrualTypeRepository
    {
        public StockHolderAccrualTypeRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            GenericRepository = new GenericKursDBRepository<StockHolderAccrualType>(unitOfWork);
        }

        public StockHolderAccrualTypeRepository(ALFAMEDIAEntities context) : base(context)
        {
            GenericRepository = new GenericKursDBRepository<StockHolderAccrualType>(context);
        }

        public GenericKursDBRepository<StockHolderAccrualType> GenericRepository { get; set; }
    }
}