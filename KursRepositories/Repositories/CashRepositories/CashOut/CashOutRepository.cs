using Helper;
using KursRepositories.Repositories.Base;

namespace KursRepositories.Repositories.CashRepositories.CashOut
{
    public class CashOutRepository : BaseRepository, ICashOutRepository
    {
        public void UpdateSFProvider(decimal sfDC)
        {
            Context.Database.ExecuteSqlCommand(
                $"EXEC [dbo].[GenerateSFProviderCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(sfDC)}");
        }
    }
}
