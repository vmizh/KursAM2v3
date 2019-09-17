using System;
using System.Linq;
using Core;
using Core.EntityViewModel;
using KursAM2.ViewModel.Finance;
using NUnit.Framework;

namespace KursAM2.Tests.MutualAccounting
{
    [TestFixture]
    public class MutualAccountingTests : MutualTestBase
    {
        [Test]
        public void LoadMutualAccountingTest()
        {
            //var manager = new MutualAccountingManager();
            var doc = manager.Load(11100000001);
            Assert.NotNull(doc,"Не возвратил документ");
            Assert.AreEqual(doc.VZ_NUM, 1);
        }

        [Test]
        public void MutualAccountingGRUDTest()
        {
            var doc = manager.New();
            doc.MutualAccountingOldType = new SD_111ViewModel {DocCode = 11110000001};
            var kontr0 = MainReferences.GetKontragent(10430000007);
            var kontr1 = MainReferences.GetKontragent(10430000084);
            manager.AddRow(doc.Rows, new MutualAccountingDebitorCreditors.MutualAccountingCreditorViewModel
            {
                DocCode = doc.DocCode,
                VZT_DOC_DATE = doc.VZ_DATE,
                Parent = doc,
                VZT_1MYDOLZH_0NAMDOLZH = 0,
                Kontragent = kontr0,
                VZT_CRS_DC = kontr0.BalansCurrency.DocCode,
                VzaimoraschType = new SD_77ViewModel {DocCode = 10770000005}
            });
            manager.AddRow(doc.Rows, new MutualAccountingDebitorCreditors.MutualAccountingCreditorViewModel
            {
                DocCode = doc.DocCode,
                VZT_DOC_DATE = doc.VZ_DATE,
                VZT_1MYDOLZH_0NAMDOLZH = 1,
                Kontragent = kontr1,
                VZT_CRS_DC = kontr1.BalansCurrency.DocCode,
                VzaimoraschType = new SD_77ViewModel {DocCode = 10770000005},
                Parent = doc
            });
            if (manager.IsChecked(doc))
            {
                var docnew = manager.Save(doc);
                var docnew2 = manager.Load(docnew.DocCode);
                Assert.AreEqual(docnew.DocCode, docnew2.DocCode, "Не правильно сохранился акт взаимозачета");
                var d = new DateTime(2017, 7, 1);
                docnew2.VZ_DATE = d;
                manager.Save(docnew2);
                var docnew3 = manager.Load(docnew2.DocCode);
                Assert.AreEqual(docnew3.VZ_DATE, d, "Не правильно обновился акт взаимозачета");
                var r = docnew3.Rows.First(_ => _.Code == 1);
                r.Note = "12345";
                manager.AddRow(docnew3.Rows, new MutualAccountingDebitorCreditors.MutualAccountingCreditorViewModel
                {
                    DocCode = doc.DocCode,
                    VZT_DOC_DATE = doc.VZ_DATE,
                    VZT_1MYDOLZH_0NAMDOLZH = 1,
                    Kontragent = kontr1,
                    VZT_CRS_DC = kontr1.BalansCurrency.DocCode,
                    VzaimoraschType = new SD_77ViewModel {DocCode = 10770000005},
                    Parent = docnew3
                });
                manager.Save(docnew3);
                var docnew4 = manager.Load(docnew3.DocCode);
                Assert.AreEqual(docnew4.Rows.Count, 3, "Акт взаимозачета - не вставилась новая строка");
                var r2 = docnew4.Rows.First(_ => _.Code == 1);
                Assert.AreEqual(r.Note, r2.Note, "Акт взаимозачета - неправильно обновились строки");
                var docnew5 = manager.Load(docnew4.DocCode);
                manager.DeleteRow(docnew5.Rows, docnew5.DeletedRows, docnew5.Rows.First());
                manager.Save(docnew5);
                var docnew6 = manager.Load(docnew5.DocCode);
                Assert.AreEqual(docnew6.Rows.Count, 2, "Акт взаимозачета - Не удалилась строка");
                var docnew7 = manager.Load(docnew6.DocCode);
                var docnew8 = manager.NewRequisity(docnew6);
                var docnewFull = manager.NewFullCopy(docnew8);
                manager.Delete(docnew6.DocCode);
                var deletedDoc = manager.Load(docnew6.DocCode);
                Assert.IsNull(deletedDoc, "Акт взаимозачета - не удалился докумет");
                Assert.AreNotEqual(docnew7.DocCode, docnew8.DocCode,
                    "Акт взаимозачета - Неправильно встал код в копировании реквизитов");
                Assert.AreEqual(DateTime.Today, docnew8.VZ_DATE,
                    "Акт взаимозачета - неправильно перенеслась дата в копировании реквизитов");
                var docnew9 = manager.Save(docnew8);
                //var docnew9 = manager.Load(newDC2);
                Assert.IsNotNull(docnew9, "Акт взаимозачета - Не сохранился документ созданый копией реквизитов");
                // ReSharper disable once RedundantAssignment
                deletedDoc = manager.Load(docnew9.DocCode);
                // ReSharper disable once HeuristicUnreachableCode
                manager.Delete(docnew9.DocCode);
                deletedDoc = manager.Load(docnew9.DocCode);
                Assert.IsNull(deletedDoc, "Акт взаимозачета - не удалился докумет");
                Assert.AreNotEqual(docnew7.DocCode, docnewFull.DocCode,
                    "Акт взаимозачета - Неправильно встал код в копировании копии");
                Assert.AreEqual(DateTime.Today, docnewFull.VZ_DATE,
                    "Акт взаимозачета - неправильно перенеслась дата в копировании копии");
                Assert.AreEqual(docnew8.Rows.Count, docnewFull.Rows.Count,
                    "Акт взаимозачета - неправильно перенеслись строки в копировании копии");
                var docnewFull2 = manager.Save(docnewFull);
                var docnewFull3 = manager.Load(docnewFull2.DocCode);
                Assert.IsNotNull(docnewFull3, "Акт взаимозачета - не сохранилсяь полная копия акта ");
                Assert.AreEqual(docnewFull3.Rows.Count, docnewFull2.Rows.Count,
                    "Акт взаимозачета - неправильно перенеслись строки в копировании копии");
                manager.Delete(docnewFull3.DocCode);
                deletedDoc = manager.Load(docnewFull3.DocCode);
                Assert.IsNull(deletedDoc, "Акт взаимозачета - не удалился докумет");
            }
            else
            {
                Console.WriteLine(manager.CheckedInfo);
            }
        }
    }
}