using System;
using System.Linq;
using Core.EntityViewModel.CommonReferences;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.References;
using NUnit.Framework;

namespace Core.Tests
{
    [TestFixture]
    public class CurrencyRatesTests : TestBase
    {
        [Test]
        public void GetRateTest()
        {
            var rates = CurrencyRate.GetRate(DateTime.Today);
            Assert.AreNotEqual(rates.Count, 0, "Не загружены курсы валют");
            decimal rate = CurrencyRate.GetCBRate(
                GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "USD") as Currency,
                GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "USD") as Currency,
                DateTime.Today);
            Assert.AreEqual(rate, 1, "Неправильный курс для одинаковой валюты");
            rate = CurrencyRate.GetCBRate(
                GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "USD") as Currency,
                GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "RUR") as Currency,
                DateTime.Today);
            Assert.AreEqual(rate, 59.5436, "Неправильный курс для доллара и рубля");
            rate = CurrencyRate.GetCBRate(
                GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "RUR") as Currency,
                GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "USD") as Currency,
                DateTime.Today);
            Assert.AreEqual(rate, 59.5436, "Неправильный курс для рубля и доллара");

            rate = CurrencyRate.GetCBRate(
                GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "USD") as Currency,
                GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "EUR") as Currency,
                DateTime.Today);
            Assert.AreEqual(rate, 0.8546, "Неправильный курс для доллара и евро");
            rate = CurrencyRate.GetCBRate(
                GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "EUR") as Currency,
                GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "USD") as Currency,
                DateTime.Today);
            Assert.AreEqual(rate, 1.1702, "Неправильный курс для евро и доллара");
        }

        [Test]
        public void CurrencyEqualTest()
        {
            var f = GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "RUR");
            var s = GlobalOptions.ReferencesCache.GetCurrenciesAll().Single(_ => ((IName) _).Name == "RUR");
            Assert.AreEqual(Equals(f, s), true, "Не сработал оператор эквивалентности");
        }
    }
}
