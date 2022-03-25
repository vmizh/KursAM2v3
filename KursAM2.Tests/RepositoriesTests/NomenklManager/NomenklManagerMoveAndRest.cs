using System;
using System.Linq;
using NUnit.Framework;

namespace KursAM2.Tests.RepositoriesTests.NomenklManager
{
    [TestFixture]
    public class NomenklManagerMoveAndRest : TestBase
    {
        private decimal NomenklDC { set; get; } = 10830053940;
        private decimal StoreDC { set; get; } = 10270000026;

        [Test]
        public void CountTests()
        {
            //var allCount = Managers.Nomenkl.NomenklManager.GetNomenklCount(NomenklDC);
            //Assert.AreEqual(allCount, 0, "Кол-во не равно 0");
            //var dateCount = Managers.Nomenkl.NomenklManager.GetNomenklCount(new DateTime(2021, 3, 17), NomenklDC);
            //Assert.AreEqual(dateCount, 4, "Кол-во не равно 4");
            //var allStoreCount = Managers.Nomenkl.NomenklManager.GetNomenklCount(NomenklDC, StoreDC);
            //Assert.AreEqual(allStoreCount, 0, "Кол-во не равно 0");
            //var alldateStoreCount =
            //    Managers.Nomenkl.NomenklManager.GetNomenklCount(new DateTime(2021, 3, 17), NomenklDC, StoreDC);
            //Assert.AreEqual(alldateStoreCount, 4, "Кол-во не равно 4");

            //var nomMove = Managers.Nomenkl.NomenklManager.GetNomenklMove(NomenklDC,
            //    new DateTime(2021, 3, 1), DateTime.Today, out var start);
            //Assert.AreNotEqual(nomMove.Count, 0, "Данные не вернулись");
            //Assert.AreEqual(start, 0, "Кол-во не равно 0");
            ////Console.WriteLine($@"Начало = {start}");
            ////foreach (var n in nomMove)
            ////{
            ////    Console.WriteLine($@"{n.NomDC} {n.NomNomenkl} {n.NomName} {n.Prihod} {n.Rashod} {n.Ostatok}");
            ////}

            //var nomMoveStore = Managers.Nomenkl.NomenklManager.GetNomenklMove(NomenklDC, StoreDC,
            //    new DateTime(2021, 2, 15), DateTime.Today, out start);
            //Assert.AreNotEqual(nomMoveStore.Count, 0, "Данные не вернулись");
            //Assert.AreEqual(start, 4, "Кол-во не равно 4");
            ////Console.WriteLine($@"Начало = {start} Склад = {StoreDC}");
            ////foreach (var n in nomMove)
            ////{
            ////    Console.WriteLine($@"{n.NomDC} {n.NomNomenkl} {n.NomName} {n.Prihod} {n.Rashod} {n.Ostatok} ");
            ////}

            //var nomMovePrice = Managers.Nomenkl.NomenklManager.GetNomenklMoveWithPrice(NomenklDC,
            //    new DateTime(2021, 2, 15), DateTime.Today, out start,
            //    out var startPrice, out var startPriceWithNaklad);
            //Assert.AreNotEqual(nomMovePrice.Count, 0, "Данные не вернулись");
            ////Assert.AreEqual(start,4,"Кол-во не равно 4");
            ////Console.WriteLine($@"Начало = {start} {startPrice} {startPriceWithNaklad}");
            ////foreach (var n in nomMovePrice)
            ////{
            ////    Console.WriteLine(
            ////        $@"{n.NomDC} {n.NomNomenkl} {n.NomName} {n.Prihod} {n.Rashod} {n.Ostatok} {n.Price} {n.PriceWithNaklad}");
            ////}

            //var nomMoveStorePrice =
            //    Managers.Nomenkl.NomenklManager.GetNomenklMoveWithPrice(NomenklDC, StoreDC,
            //        new DateTime(2021, 2, 15), DateTime.Today, out start,
            //        out startPrice, out startPriceWithNaklad);
            //Assert.AreNotEqual(nomMoveStorePrice.Count, 0, "Данные не вернулись");
            //Assert.AreEqual(start, 4, "Кол-во не равно 4");
            ////Console.WriteLine($@"Начало = {start} {startPrice} {startPriceWithNaklad} Склад={StoreDC}");
            ////foreach (var n in nomMoveStorePrice)
            ////{
            ////    Console.WriteLine(
            ////        $@"{n.NomDC} {n.NomNomenkl} {n.NomName} {n.Prihod} {n.Rashod} {n.Ostatok} {n.Price} {n.PriceWithNaklad}");
            ////}

            //var storeMove =
            //    Managers.Nomenkl.NomenklManager.GetStoreMove(new DateTime(2021, 2, 15), DateTime.Today,
            //        StoreDC);
            //Assert.AreNotEqual(storeMove.Count, 0, "Данные не вернулись");
            //var storsnames = storeMove.Select(_ => _.StoreName).Distinct();
            //foreach (var n in storeMove.OrderBy(_ => _.NomDC).ThenBy(_ => _.Date))
            //    Console.WriteLine(
            //        $@"{n.NomDC} {n.NomNomenkl} {n.NomName} {n.Start} {n.Prihod} {n.Rashod} {n.Ostatok}");

            //var storePriceMove =
            //    Managers.Nomenkl.NomenklManager.GetStoreMoveWithPrice(new DateTime(2021, 2, 15), DateTime.Today,
            //        StoreDC);
            //Assert.AreNotEqual(storePriceMove.Count, 0, "Данные не вернулись");
            //foreach (var n in storePriceMove.OrderBy(_ => _.NomDC).ThenBy(_ => _.Date))
            //    Console.WriteLine(
            //        $@"{n.NomDC} {n.NomNomenkl} {n.NomName} {n.Start} {n.Prihod} {n.Rashod} {n.Ostatok} {n.Price} {n.PriceWithNaklad}");
           // var data = Managers.Nomenkl.NomenklManager.GetAllStoreRemain(new DateTime(2020, 12, 31));
            //Assert.AreNotEqual(data.Count, 0, "Данные не вернулись");
            //foreach (var store in data.Select(_ => new {StoreDC = _.StoreDC, StoreName = _.StoreName}).Distinct())
            //{
            //    Console.WriteLine($@"{store.StoreDC} {store.StoreName}");
            //    foreach (var n in data.Where(_ => _.StoreDC == store.StoreDC).OrderBy(_ => _.NomDC).ThenBy(_ => _.Date))
            //        Console.WriteLine(
            //            $@"{n.NomDC} {n.NomNomenkl} {n.NomName} {n.Ostatok} {n.Price} {n.PriceWithNaklad}");
            //}

        }
    }
}