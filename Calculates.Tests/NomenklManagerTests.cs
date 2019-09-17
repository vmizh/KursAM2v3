using System;
using System.Data.SqlClient;
using System.Linq;
using Calculates.Materials;
using Core;
using Helper;
using NUnit.Framework;

namespace Calculates.Tests
{
    [TestFixture]
    public class NomenklManagerTests
    {
        [SetUp]
        public void Connect()
        {
            var sqlConnectionString = new SqlConnectionStringBuilder
            {
                //DataSource = ds, //"172.16.1.1",
                //InitialCatalog = dbase, //"AlfaMedia",
                //UserID = usr, //"sa",
                //Password = pwd //",juk.,bnyfc"

                DataSource = "main8",
                InitialCatalog = "AlfaNew",
                UserID = "sa",
                Password = "CbvrfFhntvrf65"
            }.ConnectionString;
            GlobalOptions.SqlConnectionString = sqlConnectionString;
            GlobalOptions.UserInfo = new User
            {
                Name = "sysadm",
                NickName = "sysadm",
                Id = 6
            };
            GlobalOptions.SystemProfile = new SystemProfile
            {
                NomenklCalcType = NomenklCalcType.Standart
            };
            //if (MainReferences != null && MainReferences.IsReferenceLoadComplete)
            //    return ret;
            GlobalOptions.MainReferences = new MainReferences();
            GlobalOptions.MainReferences.Reset();
            while (!MainReferences.IsReferenceLoadComplete)
            {
            }
        }

        //[Test]
        public void GetRemains()
        {
            var data = NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2016, 9, 1));
            Assert.NotNull(data);
            Assert.AreNotEqual(data.Count, 0,
                "NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2016, 9, 1))");
            //var data2 = NomenklCalculationManager.GetCalculateOperations(10830001234);
            Assert.NotNull(data, "NomenklCalculationManager.GetCalculateOperations(10830001234)");
            Assert.AreNotEqual(data.Count, 0, "NomenklCalculationManager.GetCalculateOperations(10830001234)");
            var data3 = NomenklCalculationManager.GetNomenklAllStoreRemains(new DateTime(2016, 9, 1), 10830001234);
            Assert.NotNull(data3,
                "NomenklCalculationManager.GetNomenklAllStoreRemains(new DateTime(2016, 9, 1),10830001234)");
            Assert.AreEqual(data3.Count, 0,
                "NomenklCalculationManager.GetNomenklAllStoreRemains(new DateTime(2016, 9, 1),10830001234)");
            var data4 = NomenklCalculationManager.GetNomenklAllStoreRemains(new DateTime(2016, 9, 1), 10830044945);
            Assert.NotNull(data4,
                "NomenklCalculationManager.GetNomenklAllStoreRemains(new DateTime(2016, 9, 1),10830044945)");
            Assert.AreNotEqual(data4.Count, 0,
                "NomenklCalculationManager.GetNomenklAllStoreRemains(new DateTime(2016, 9, 1),10830044945)");
            //var data5 = NomenklCalculationManager.GetNomenklStoreRemain(new DateTime(2016, 9, 1), 10830044945, 10270000033);
            Assert.NotNull(data4,
                "NomenklCalculationManager.GetNomenklStoreRemain(new DateTime(2016, 9, 1), 10830044945, 10270000033)");
            Assert.AreNotEqual(data4.Count, 0,
                "NomenklCalculationManager.GetNomenklStoreRemain(new DateTime(2016, 9, 1), 10830044945, 10270000033)");
            var data6 = NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2016, 9, 1));
            Assert.NotNull(data6, "NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2016, 9, 1))");
            Assert.AreNotEqual(data6.Count, 0,
                "NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2016, 9, 1))");
            var data7 = NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2016, 9, 1), 10270000033);
            Assert.NotNull(data7,
                "NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2016, 9, 1), 10270000033)");
            Assert.AreNotEqual(data7.Count, 0,
                "NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2016, 9, 1), 10270000033)");
        }

        [Test]
        public void RemainAllMoveTest()
        {
            var data = NomenklCalculationManager.NomenklRemains(new DateTime(2019, 1, 1),
                new DateTime(2019, 7, 31));
            //Assert.NotNull(data, "NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2019, 7, 31))");
            foreach (var r in data)
                Console.WriteLine(
                    $"Nom {r.Item1} Skald {r.Item2} Start {r.Item3} In {r.Item4} Out {r.Item5} End {r.Item6} ");
        }

        [Test]
        public void RemainNomMoveTest()
        {
            var data = NomenklCalculationManager.NomenklRemain(new DateTime(2019, 1, 1),
                new DateTime(2019, 7, 31), 10830035404);
            //Assert.NotNull(data, "NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2019, 7, 31))");
            foreach (var r in data)
                Console.WriteLine($"Skald {r.Item1} Start {r.Item2} In {r.Item3} Out {r.Item4} End {r.Item5} ");
        }

        [Test]
        public void RemainNomSladTest()
        {
            var quantity = NomenklCalculationManager.NomenklRemain(new DateTime(2019, 7, 31), 10830000380, 10270000001);
            //Assert.NotNull(data, "NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2019, 7, 31))");
            Console.WriteLine(quantity);
        }


        [Test]
        public void RemainNomTest()
        {
            var data = NomenklCalculationManager.NomenklRemain(new DateTime(2019, 7, 31), 10830000380);
            Assert.NotNull(data, "NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2019, 7, 31))");
            foreach (var r in data) Console.WriteLine($"Skald {r.Item1} Rems {r.Item2}");
        }

        [Test]
        public void RemainStoreMoveTest()
        {
            var data = NomenklCalculationManager.NomenklRemains(new DateTime(2019, 1, 1),
                new DateTime(2019, 7, 31), 10270000001);
            //Assert.NotNull(data, "NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2019, 7, 31))");
            foreach (var r in data)
                Console.WriteLine($"Nom {r.Item1}  Start {r.Item2} In {r.Item3} Out {r.Item4} End {r.Item5} ");
        }

        [Test]
        public void RemainStoreTest()
        {
            var data = NomenklCalculationManager.NomenklRemains(new DateTime(2019, 7, 31), 10270000001);
            Assert.NotNull(data, "NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2019, 7, 31))");
            foreach (var r in data) Console.WriteLine($"Nom {r.Item1} Rems {r.Item2}");
        }

        [Test]
        public void RemainTest()
        {
            var data = NomenklCalculationManager.NomenklRemains(new DateTime(2019, 7, 31));
            Assert.NotNull(data, "NomenklCalculationManager.GetNomenklStoreRemains(new DateTime(2019, 7, 31))");
            foreach (var r in data) Console.WriteLine($"Nom {r.Item1} Sklad {r.Item2} Rems {r.Item3}");
        }

        [Test]
        public void RemainMoveWithPrice()
        {
            var start = new DateTime(2019, 1, 1);
            var end = new DateTime(2019, 07, 31);
            var data = NomenklCalculationManager.NomenklRemains(start, end);
            foreach (var r in data.Where(_ => _.Item4 != 0 || _.Item5 != 0))
            {
                Tuple<decimal, decimal> sumOut = Tuple.Create(0m, 0m), sumIn = Tuple.Create(0m, 0m);
                if (r.Item5 != 0 || r.Item4 != 0)
                {
                    var d = NomenklCalculationManager.NomenklMoveSum(start, end, r.Item1);
                    decimal sIn1 = 0, sIn2 = 0, sOut1 = 0, sOut2 = 0;
                    foreach (var _ in d)
                    {
                        if (_ == null) continue;
                        sIn1 += _.Item2;
                        sIn2 += _.Item3;
                        sOut1 += _.Item4;
                        sOut2 += _.Item5;
                    }

                    sumIn = Tuple.Create(sIn1, sIn2);
                    sumOut = Tuple.Create(sOut1, sOut2);
                }
                /*10830046166
10270000026*/
                var prcStart = NomenklCalculationManager.NomenklPrice(start, r.Item1) ?? Tuple.Create(0m, 0m);
                var prcEnd = NomenklCalculationManager.NomenklPrice(end, r.Item1) ?? Tuple.Create(0m, 0m);
                Console.WriteLine(
                    $"Nom {r.Item1} Skald {r.Item2} Start {r.Item3} ({prcStart.Item1:n2}/{prcStart.Item2:n2}) " +
                    $"In {r.Item4} ({sumIn.Item1:n2}/{sumIn.Item2:n2}) Out {r.Item5} ({sumOut.Item1:n2}/{sumOut.Item2:n2}) End {r.Item6} ({prcEnd.Item1:n2}/{prcEnd.Item2:n2}) ");
            }
        }

        [Test]
        public void RemainStoreMoveWithPrice()
        {
            var start = new DateTime(2019, 1, 1);
            var end = new DateTime(2019, 07, 31);
            var data = NomenklCalculationManager.NomenklRemains(start, end, 10270000026);
            foreach (var r in data.Where(_ => _.Item4 != 0 || _.Item5 != 0))
            {
                Tuple<decimal, decimal> sumOut = Tuple.Create(0m, 0m), sumIn = Tuple.Create(0m, 0m);
                if (r.Item5 != 0 || r.Item4 != 0)
                {
                    var d = NomenklCalculationManager.NomenklMoveSum(start, end, r.Item1, 10270000026);
                    decimal sIn1 = 0, sIn2 = 0, sOut1 = 0, sOut2 = 0;
                    foreach (var _ in d)
                    {
                        if (_ == null) continue;
                        sIn1 += _.Item2;
                        sIn2 += _.Item3;
                        sOut1 += _.Item4;
                        sOut2 += _.Item5;
                    }

                    sumIn = Tuple.Create(sIn1, sIn2);
                    sumOut = Tuple.Create(sOut1, sOut2);
                }
                /*10830046166
10270000026*/
                var prcStart = NomenklCalculationManager.NomenklPrice(start, r.Item1) ?? Tuple.Create(0m, 0m);
                var prcEnd = NomenklCalculationManager.NomenklPrice(end, r.Item1) ?? Tuple.Create(0m, 0m);
                Console.WriteLine(
                    $"Nom {r.Item1} Start {r.Item2} ({prcStart.Item1:n2}/{prcStart.Item2:n2}) " +
                    $"In {r.Item3} ({sumIn.Item1:n2}/{sumIn.Item2:n2}) Out {r.Item4} ({sumOut.Item1:n2}/{sumOut.Item2:n2}) End {r.Item5} ({prcEnd.Item1:n2}/{prcEnd.Item2:n2}) ");
            }

        }

        [Test]
        public void RemainStoreMoveWithPrice2()
        {
            var start = new DateTime(2000, 1, 1);
            var end = new DateTime(2019, 07, 31);
            var data = NomenklCalculationManager.NomenklMoveSum2(start, end, 10270000026);
            foreach (var r in data)
            {
                Console.WriteLine($"Nom {r.NomDC} Start {r.Start:n2} ({r.PriceStart:n2}/{r.PriceStartWithNaklad:n2}) " +
                                  $"In {r.In:n2} ({r.SumIn:n2}/{r.SumInWithNaklad:n2}) Out {r.Out:n2} ({r.SumOut}/{r.SumOutWithNaklad}) " +
                                  $"End {r.End:n2} ({r.PriceEnd}/{r.PriceEndWithNaklad})");

            }

        }
    }
}