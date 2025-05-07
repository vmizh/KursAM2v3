using KursRepositories.Repositories.Base;

namespace KursRepositories.Repositories.CashRepositories.CashOut
{
    public interface ICashOutRepository : IBaseRepository
    {
        void UpdateSFProvider(decimal sfDC);
    }
}
