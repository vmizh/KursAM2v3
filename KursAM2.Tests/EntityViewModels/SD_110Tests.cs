using System;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.MutualAccounting;
using NUnit.Framework;
 // ReSharper disable InconsistentNaming
namespace KursAM2.Tests.EntityViewModels
{
    [TestFixture]
   
    public class SD_110Tests : TestBase
    {
        [Test]
        public void CRUDSD_110()
        {
            var item1 = new SD_110ViewModel
            {
                State = RowStatus.NewRow,
                CREATOR = GlobalOptions.UserInfo.Name,
                VZ_DATE = DateTime.Today,
                MutualAccountingOldType = new SD_111ViewModel
                {
                    DocCode = 11110000001,
                    IsCurrencyConvert = false,
                },
                Note = "Test1",
            };
            /* 10430000100
  10430000103*/
            var r1 = new TD_110ViewModel
            { 
                DocCode = -1,
                Code = 1,
                VZT_1MYDOLZH_0NAMDOLZH = 0,
                VZT_CRS_DC = MainReferences.GetKontragent(10430000100).BalansCurrency.DocCode,
                VZT_CRS_SUMMA = 1000,
                VZT_DOC_DATE = item1.VZ_DATE,
                VZT_DOC_NUM = "000",
                VZT_KONTR_DC = MainReferences.GetKontragent(10430000100).DocCode,
                VZT_DOC_NOTES = "Test Row 1",
                VZT_KONTR_CRS_RATE = 1,
                VZT_KONTR_CRS_DC = MainReferences.GetKontragent(10430000100).BalansCurrency.DocCode,
                VZT_VZAIMOR_TYPE_DC = 10770000005,
                State = RowStatus.NewRow
            };
            r1.UpdateFrom(r1.Entity);
            var r2 = new TD_110ViewModel
            {
                DocCode = -1,
                Code = 1,
                VZT_1MYDOLZH_0NAMDOLZH = 1,
                VZT_CRS_DC = MainReferences.GetKontragent(10430000103).BalansCurrency.DocCode,
                VZT_CRS_SUMMA = 1000,
                VZT_DOC_DATE = item1.VZ_DATE,
                VZT_DOC_NUM = "000",
                VZT_KONTR_DC = MainReferences.GetKontragent(10430000103).DocCode,
                VZT_DOC_NOTES = "Test Row 1",
                VZT_KONTR_CRS_RATE = 1,
                VZT_KONTR_CRS_DC = MainReferences.GetKontragent(10430000103).BalansCurrency.DocCode,
                VZT_VZAIMOR_TYPE_DC = 10770000005,
                State = RowStatus.NewRow
            };
            item1.Rows.Add(r1);
            item1.Rows.Add(r2);
            item1.UpdateTo(item1.Entity);
            item1.Save(item1.Entity);
            var dc = item1.DocCode;
            var item1Old = new SD_110ViewModel
            {
                LoadCondition = new EntityLoadCodition
                {
                    IsShort = false
                }
            };
            item1Old.Load(dc);
            Assert.AreEqual(item1.Note, item1Old.Note,
                "Тип акта взаимозачета не загружен или не сохранен в базе данных");
            item1Old.Note = "Test2";
            var rRow = item1Old.Rows.FirstOrDefault(_ => _.Code == 1);
            var rRow2 = item1Old.Rows.FirstOrDefault(_ => _.Code == 2);
            if (rRow2 != null) rRow2.VZT_CRS_SUMMA = -1;
            item1Old.DeletedRow(rRow);
            item1Old.Save();
            var item2Old = new SD_110ViewModel();
            item2Old.Load(dc);
            Assert.AreEqual(item2Old.Rows.Count,1,"Удаление строк не прошло");
            var r = item2Old.Rows.First().VZT_CRS_SUMMA;
            Assert.AreEqual(r,-1,"Ошибка в обновлении строк");
            Assert.AreNotEqual(item1.Note, item2Old.Note, "Тип акта взаимозачета не обновился");
            item2Old.Delete();
            var item3Old = item2Old.Load(item1.DocCode);
            Assert.Null(item3Old, "Тип акта взаимозачета не удален из базы данных");
        }
    }
}