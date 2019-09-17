using System.Data.SqlClient;
using System.Linq;
using Core;
using Helper;
using KursAM2.Managers;
using KursAM2.Managers.Base;
using NUnit.Framework;

namespace KursAM2.Tests
{
    [TestFixture]
    public class MutualTestBase
    {
        public decimal DocDC = -1;
        protected MutualAccountingManager manager;

        [SetUp]
        public void SetcConnect()
        {
            var sqlConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = "172.16.0.1",
                InitialCatalog = "EcoOndol",
                UserID = "sysadm",
                Password = "19655691"
            }.ConnectionString;
            GlobalOptions.SqlConnectionString = sqlConnectionString;
            GlobalOptions.UserInfo = new User
            {
                Name = "sysadm",
                NickName = "sysadm"
            };
            GlobalOptions.MainReferences = new MainReferences();
            GlobalOptions.MainReferences.Reset();
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }
            GlobalOptions.SystemProfile = new SystemProfile
            {
                NationalCurrency = MainReferences.Currencies.Values.Single(_ => _.Name == "RUR"),
                MainCurrency = MainReferences.Currencies.Values.Single(_ => _.Name == "RUR"),

            };
            manager = new MutualAccountingManager();
    }
        [TearDown]
        public void DeleteDocument()
        {
            manager.Delete(DocDC);
        }
    }

    [TestFixture]
    public class TestBase
    {
        public decimal DocDC = -1;

        [SetUp]
        public void SetcConnect()
        {
            var sqlConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = "172.16.0.1",
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
            GlobalOptions.MainReferences = new MainReferences();
            GlobalOptions.MainReferences.Reset();
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }
            GlobalOptions.SystemProfile = new SystemProfile
            {
                NationalCurrency = MainReferences.Currencies.Values.Single(_ => _.Name == "RUR"),
                MainCurrency = MainReferences.Currencies.Values.Single(_ => _.Name == "RUR"),

            };
        }
        [TearDown]
        public void DeleteDocument()
        {
            //manager.Delete(DocDC);
        }
    }
}