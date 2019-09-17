using KursAM2.Managers;
using NUnit.Framework;

namespace KursAM2.Tests.Cash
{
    [TestFixture]
    public class CashInTests : TestBase
    {

        [Test]
        public void CashInOpen()
        {
            decimal dc = 10330000054;
            
            var doc = CashManager.LoadCashIn(dc);
            Assert.IsNotNull(doc,"Документ не загружен");
        }

    }
}