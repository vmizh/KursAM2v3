using System;
using Core;
using Core.EntityViewModel;
using KursAM2.ViewModel.Finance;
using NUnit.Framework;

namespace KursAM2.Tests.MutualAccounting
{
    [TestFixture]
    public class MutualAccountingWindowViewModelTests : MutualTestBase
    {
        [Test]
        public void CreateMutualAccountingWindowViewModelTest()
        {
            var doc = manager.New();
            doc.MutualAccountingOldType = new SD_111ViewModel { DocCode = 11110000001 };
            doc.CREATOR = "sysadm";
            doc.VZ_DATE = DateTime.Today;
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
                VzaimoraschType = new SD_77ViewModel { DocCode = 10770000005 }
            });
            manager.AddRow(doc.Rows, new MutualAccountingDebitorCreditors.MutualAccountingCreditorViewModel
            {
                DocCode = doc.DocCode,
                VZT_DOC_DATE = doc.VZ_DATE,
                VZT_1MYDOLZH_0NAMDOLZH = 1,
                Kontragent = kontr1,
                VZT_CRS_DC = kontr1.BalansCurrency.DocCode,
                VzaimoraschType = new SD_77ViewModel { DocCode = 10770000005 },
                Parent = doc
            });
            if (manager.IsChecked(doc))
            {
                var docnew = manager.Save(doc);
                DocDC = docnew.DocCode;
            }
            var vm = new MutualAcountingWindowViewModel(DocDC);
            Assert.NotNull(vm, "Не возвратил документ");
            Assert.AreEqual(vm.Document.DocCode, DocDC, "Не правильно загружена вьюмодель акта взаимозачета");
            vm.AddNewCreditor(kontr0);
            Assert.AreEqual(vm.Document.Rows.Count, 3, "Не правильно добалена строка кредиторов");
            Assert.AreEqual(vm.CreditorCollection.Count, 2, "Не правильно добалена строка кредиторов");
            vm.AddNewDebitor(kontr1);
            Assert.AreEqual(vm.Document.Rows.Count, 4, "Не правильно добалена строка дебиторов");
            Assert.AreEqual(vm.DebitorCollection.Count, 2, "Не правильно добалена строка дебиторов");
            Assert.AreEqual(vm.CreditorCollection[1],vm.CurrentCreditor,"Не установлен текущий кредитор");
            Assert.AreEqual(vm.DebitorCollection[1], vm.CurrentDebitor, "Не установлен текущий дебитор");
            Assert.AreEqual(vm.CurrentDebitor.Currency, GlobalOptions.SystemProfile.MainCurrency,
                "Не сработал оператор эквивалентности");
        }
    }
}