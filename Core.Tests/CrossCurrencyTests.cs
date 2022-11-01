using System;
using Core.EntityViewModel.CommonReferences;
using KursDomain.Documents.CommonReferences;
using NUnit.Framework;

namespace Core.Tests
{
    [TestFixture]
    public class CrossCurrencyTests : TestBase
    {
        [Test]
        public void CrossCurrencyInitTest()
        {
            var crs = new CrossCurrencyRate();
            crs.SetRates(DateTime.Today);
            Assert.AreEqual(crs.CurrencyList.Count,4);
            foreach (var c in crs.CurrencyList)
            {
                Console.WriteLine($"{c.Currency} - {c.CurrencyEUR} / {c.CurrencyGBP} / {c.CurrencyRUB} / {c.CurrencyUSD}");
            }
        }
    }
}
