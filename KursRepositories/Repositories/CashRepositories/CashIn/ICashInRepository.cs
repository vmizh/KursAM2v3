using KursRepositories.Repositories.Base;

namespace KursRepositories.Repositories.CashRepositories.CashIn
{
    public interface ICashInRepository : IBaseRepository
    {
        void UpdateSFClient(decimal sfDC);
    }
}
