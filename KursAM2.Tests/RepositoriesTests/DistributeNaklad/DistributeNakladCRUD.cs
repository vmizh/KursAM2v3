using System;
using Core.Repository.Base;
using Data.Repository;
using KursAM2.Repositories;
using NUnit.Framework;

namespace KursAM2.Tests.RepositoriesTests.DistributeNaklad
{
    [TestFixture]
    public class DistributeNakladCRUD : TestBase
    {
        private IDistributeNakladRepository rep;
        // ReSharper disable once NotAccessedField.Local
        private GenericKursDBRepository<Data.DistributeNaklad> drep;

        [Test]
        public void Test()
        {
            rep = new DistributeNakladRepository(UnitOfWork.Context);
            drep = new GenericKursDBRepository<Data.DistributeNaklad>(UnitOfWork);
            var doc = rep.CreateNew();
            doc.Note = "test 1";
            UnitOfWork.CreateTransaction();
            UnitOfWork.Save();
            UnitOfWork.Commit();
            var d1 = rep.GetById(doc.Id);
            Assert.NotNull(d1, "Документ не найден");
            Console.WriteLine(d1.Note);
            var rows = rep.GetTovarFromInvoiceProviders(d1, new DateTime(2020, 1, 1), DateTime.Today);
            Assert.Greater(rows.Count, 0, "Не выбраны строки");
            if (rows.Count > 0)
            {
                var i = 1;
                foreach (var r in rows)
                {
                    if (i > 100) break;
                    var newRow = rep.CreateRowNew(d1);
                    newRow.TovarInvoiceRowId = r.TovarInvoiceRowId;
                    newRow.Note = r.Note;
                    i++;
                }
            }
            UnitOfWork.CreateTransaction();
            UnitOfWork.Save();
            UnitOfWork.Commit();
            var d2 = rep.GetById(doc.Id);
            Assert.Greater(d2.DistributeNakladRow.Count,0,"Товары не вставились");
            var nakladrows = rep.GetDistributeInvoice(d2);
            foreach (var nr in nakladrows)
            {
                var newInv = rep.CreateNewInvoice(d2);
                newInv.InvoiceId = nr.InvoiceId;
                newInv.Note = nr.Note;
            }
            UnitOfWork.CreateTransaction();
            UnitOfWork.Save();
            UnitOfWork.Commit();
            var d3 = rep.GetById(doc.Id);
            Assert.Greater(d3.DistributeNakladInvoices.Count, 0, "Инвойсы не вставились");
            
            rep.Delete(d3);
            UnitOfWork.CreateTransaction();
            UnitOfWork.Save();
            UnitOfWork.Commit();

            var ddel = rep.GetById(doc.Id);
            Assert.IsNull(ddel, "Документ не удален");
        }
    }
}