using System.Data.SqlClient;
using System.Linq;
using Core;
using Data;
using Data.Repository;
using Helper;
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
            MainReferences.Reset();
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }

            GlobalOptions.SystemProfile = new SystemProfile
            {
                NationalCurrency = MainReferences.Currencies.Values.Single(_ => _.Name == "RUR"),
                MainCurrency = MainReferences.Currencies.Values.Single(_ => _.Name == "RUR")
            };
            entities = GlobalOptions.GetEntities();
            UnitOfWork = new UnitOfWork<ALFAMEDIAEntities>(entities);
        }

        [TearDown]
        public void DeleteDocument()
        {
            //manager.Delete(DocDC);
        }

        public decimal DocDC = -1;
        protected UnitOfWork<ALFAMEDIAEntities> UnitOfWork = new UnitOfWork<ALFAMEDIAEntities>();

        private ALFAMEDIAEntities entities;
    }
}