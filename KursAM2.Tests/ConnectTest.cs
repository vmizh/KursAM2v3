using System;
using Core;
using KursAM2.Repositories;
using NUnit.Framework;

namespace KursAM2.Tests
{
    [TestFixture]
    public class ConnectTest : TestBase
    {
        private IDistributeNakladRepository rep;

        [Test]
        public void TestConnect()
        {
            rep = new DistributeNakladRepository(GlobalOptions.GetEntities());
            var d = rep.GetAllByDates(new DateTime(2000, 1, 1),
                DateTime.Today);
            Console.WriteLine(d.Count);
        }
    }
}