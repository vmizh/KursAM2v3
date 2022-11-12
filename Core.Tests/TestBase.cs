using System.Data.SqlClient;
using System.Linq;
using Helper;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;
using NUnit.Framework;

namespace Core.Tests
{
    [TestFixture]
    public class TestBase
    {
        [SetUp]
        public void SetConnect()
        {
            var sqlConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = "172.16.1.1",
                InitialCatalog = "KomSpecProject",
                UserID = "sysadm",
                Password = "19655691"
            }.ConnectionString;
            GlobalOptions.SqlConnectionString = sqlConnectionString;
            GlobalOptions.UserInfo = new User
            {
                Name = "sysadm",
                NickName = "sysadm"
            };
            GlobalOptions.SystemProfile = new SystemProfile
            {
                NationalCurrency =
                    GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "RUR") as Currency
            };
        }
    }
}
