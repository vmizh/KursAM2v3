using System;
using System.Data.SqlClient;
using System.Linq;
using Core;
using NUnit.Framework;

namespace KursAM2.Tests
{
    [TestFixture]
    public class ConnectionTests
    {
        [Test]
        public void ConnectTest()
        {

            var sqlConnectionString = new SqlConnectionStringBuilder
            {
                DataSource = "172.16.1.1",
                InitialCatalog = "AlfaMedia2",
                UserID = "sysadm",
                Password = "19655691"
            }.ConnectionString;
            GlobalOptions.SqlConnectionString = sqlConnectionString;
            var ctx = GlobalOptions.GetEntities();
            Assert.AreNotEqual(ctx.EXT_USERS.Count(),0);
            foreach(var u in  ctx.EXT_USERS)
                Console.WriteLine(u.USR_NICKNAME);
            //GlobalOptions.MainReferences = new MainReferences();
            //GlobalOptions.MainReferences.Reset();
            //while (!MainReferences.IsReferenceLoadComplete)
            //{
            //}
        }
    }
}