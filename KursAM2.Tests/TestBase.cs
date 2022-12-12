using System.Data.SqlClient;
using System.Linq;
using Data;
using Helper;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;
using KursDomain.Repository;
using NUnit.Framework;

namespace KursAM2.Tests
{
    [TestFixture]
    public class TestBase
    {
        [SetUp]
        public void SetConnect()
        {
            var sqlConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = "127.0.0.1",
                InitialCatalog = "AlfaTest",
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
                    GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "RUR") as Currency,
                MainCurrency =
                    GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "RUR") as Currency,
            };
            entities = GlobalOptions.GetEntities();
            UnitOfWork = new KursDomain.Repository.UnitOfWork<ALFAMEDIAEntities>(entities);
        }

        [TearDown]
        public void DeleteDocument()
        {
            //manager.Delete(DocDC);
        }

        public decimal DocDC = -1;
        protected KursDomain.Repository.UnitOfWork<ALFAMEDIAEntities> UnitOfWork = new KursDomain.Repository.UnitOfWork<ALFAMEDIAEntities>();

        private ALFAMEDIAEntities entities;
    }
}
