using System;
using System.Linq;
using NUnit.Framework;
using Core.ViewModel.Common;

namespace Core.Tests
{
    [TestFixture]
    public class CurrencyRatesTests : TestBase
    {
        [Test]
        public void GetRateTest()
        {
            var rates = CurrencyRate.GetRate(DateTime.Today);
            Assert.AreNotEqual(rates.Count,0,"Не загружены курсы валют");
            decimal rate = CurrencyRate.GetCBRate(MainReferences.Currencies.Values.Single(_ => _.Name == "USD"),
                MainReferences.Currencies.Values.Single(_ => _.Name == "USD"), DateTime.Today);
            Assert.AreEqual(rate,1,"Неправильный курс для одинаковой валюты");
            rate = CurrencyRate.GetCBRate(MainReferences.Currencies.Values.Single(_ => _.Name == "USD"),
                MainReferences.Currencies.Values.Single(_ => _.Name == "RUR"), DateTime.Today);
            Assert.AreEqual(rate, 59.5436, "Неправильный курс для доллара и рубля");
            rate = CurrencyRate.GetCBRate(MainReferences.Currencies.Values.Single(_ => _.Name == "RUR"),
                MainReferences.Currencies.Values.Single(_ => _.Name == "USD"), DateTime.Today);
            Assert.AreEqual(rate, 59.5436, "Неправильный курс для рубля и доллара");

            rate = CurrencyRate.GetCBRate(MainReferences.Currencies.Values.Single(_ => _.Name == "USD"),
                MainReferences.Currencies.Values.Single(_ => _.Name == "EUR"), DateTime.Today);
            Assert.AreEqual(rate, 0.8546, "Неправильный курс для доллара и евро");
            rate = CurrencyRate.GetCBRate(MainReferences.Currencies.Values.Single(_ => _.Name == "EUR"),
                MainReferences.Currencies.Values.Single(_ => _.Name == "USD"), DateTime.Today);
            Assert.AreEqual(rate, 1.1702, "Неправильный курс для евро и доллара");
        }

        [Test]
        public void CurrencyEqualTest()
        {
            var f = MainReferences.Currencies.Values.Single(_ => _.Name == "RUR");
            var s = MainReferences.Currencies.Values.Single(_ => _.Name == "RUR");
            Assert.AreEqual(Equals(f, s),true,"Не сработал оператор эквивалентности");
        }
    }
}