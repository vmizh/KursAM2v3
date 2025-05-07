using Helper;
using KursRepositories.Repositories.Base;

namespace KursRepositories.Repositories.CashRepositories.CashIn
{
    public class CashInRepository : BaseRepository, ICashInRepository
    {
        public void UpdateSFClient(decimal sfDC)
        {
            Context.Database.ExecuteSqlCommand(
                $"EXEC [dbo].[GenerateSFClientCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(sfDC)}");
        }
    }
}
