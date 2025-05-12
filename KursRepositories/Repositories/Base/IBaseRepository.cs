using Data;

namespace KursRepositories.Repositories.Base
{
    public interface IBaseRepository
    {
        ALFAMEDIAEntities Context { set; get; }
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        void SaveChanges();
    }
}
