using System;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Common;
using KursAM2.ViewModel.Logistiks;
using NUnit.Framework;

namespace KursAM2.Tests
{
    [TestFixture]
    public class NomenklTransferTests : TestBase
    {
        [Test]
        public void CRUDTest()
        {
            var ctx = GlobalOptions.GetEntities();
            var newNomTrans = NomenklTransferViewModelExt.New();
            newNomTrans.Warehouse = new Warehouse(ctx.SD_27.First());
            Assert.AreNotEqual(newNomTrans.Warehouse,null);
            var iid = Guid.Parse("42615c6f-537b-48da-ab7d-d67e7a881ada");
            var noms = ctx.SD_83.Where(_ => _.MainId == iid).AsNoTracking();
            newNomTrans.Date = DateTime.Today;
            newNomTrans.Creator = GlobalOptions.UserInfo.NickName;
            newNomTrans.LastUpdate = DateTime.Now;
            newNomTrans.LastUpdater = GlobalOptions.UserInfo.NickName;
            var nomOut = new Nomenkl(noms.Single(_ => _.DOC_CODE == 10830003115));
            var nomIn = new Nomenkl(noms.Single(_ => _.DOC_CODE != 10830003115));
            var row1 = NomenklTransferRowViewModelExt.New(newNomTrans);
            row1.NomenklOut = nomOut;
            row1.NomenklIn = nomIn;
            row1.Quantity = 5;
            row1.IsAccepted = true;
            row1.LastUpdate = DateTime.Now;
            row1.LastUpdater = GlobalOptions.UserInfo.NickName;
            row1.Rate = 1;
            var row2 = NomenklTransferRowViewModelExt.New(newNomTrans);
            row2.NomenklOut = nomOut;
            row2.NomenklIn = nomIn;
            row2.Quantity = 5;
            row2.IsAccepted = true;
            row2.LastUpdate = DateTime.Now;
            row2.LastUpdater = GlobalOptions.UserInfo.NickName;
            row2.Rate = 1;
            var row3 = NomenklTransferRowViewModelExt.New(newNomTrans);
            row3.NomenklOut = nomOut;
            row3.NomenklIn = nomIn;
            row3.Quantity = 5;
            row3.IsAccepted = true;
            row3.LastUpdate = DateTime.Now;
            row3.LastUpdater = GlobalOptions.UserInfo.NickName;
            row3.Rate = 1;
            newNomTrans.Rows.Add(row1);
            newNomTrans.Rows.Add(row2);
            newNomTrans.Rows.Add(row3);
            newNomTrans.Save();
            var fromDB = ctx.NomenklTransfer.AsNoTracking()
                .Include(_ => _.NomenklTransferRow)
                .Include(_ => _.SD_27)
                .FirstOrDefault(_ => _.Id == newNomTrans.Id);
            Assert.AreNotEqual(fromDB,null,"Ничего не сохранилось");
            var updateDoc = new NomenklTransferViewModelExt(fromDB)
            {
                Date = new DateTime(2016, 1, 1),
                Note = "Привет"
            };
            var rowId = updateDoc.Rows.First().Id;
            updateDoc.Rows.First().Rate = 10;
            var delRow = updateDoc.Rows.Last();
            updateDoc.Rows.Remove(delRow);
            row3 = NomenklTransferRowViewModelExt.New(newNomTrans);
            row3.NomenklOut = nomOut;
            row3.NomenklIn = nomIn;
            row3.Quantity = 5;
            row3.IsAccepted = true;
            row3.LastUpdate = DateTime.Now;
            row3.LastUpdater = GlobalOptions.UserInfo.NickName;
            row3.Rate = 1;
            updateDoc.Rows.Add(row3);
            updateDoc.Save();
            fromDB = ctx.NomenklTransfer.Include(_ => _.NomenklTransferRow).AsNoTracking().FirstOrDefault(_ => _.Id == newNomTrans.Id);
            Assert.AreNotEqual(fromDB, null, "Ничего не сохранилось");
            // ReSharper disable once PossibleNullReferenceException
            Assert.AreEqual(fromDB.NomenklTransferRow.Count,3,"Не удалилась строка");
            Assert.AreEqual(fromDB.Date, new DateTime(2016, 1, 1));
            Assert.AreEqual(fromDB.Note, "Привет");
            Assert.AreEqual(fromDB.NomenklTransferRow.Single(_ => _.Id == rowId).Rate, 10, "Курс неверный");
            newNomTrans.Delete();
            var fromDB2 = ctx.NomenklTransfer.FirstOrDefault(_ => _.Id == newNomTrans.Id);
            Assert.AreEqual(fromDB2, null, "Не удалилось");


        }
    }
}