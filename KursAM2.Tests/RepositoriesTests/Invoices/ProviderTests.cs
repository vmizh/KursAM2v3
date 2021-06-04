using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel.Invoices;
using Core.Invoices.EntityViewModel;
using Data;
using Data.Repository;
using KursAM2.Repositories.InvoicesRepositories;
using NUnit.Framework;

// ReSharper disable All

namespace KursAM2.Tests.RepositoriesTests.Invoices
{
    [TestFixture]
    public class ProviderTests : TestBase
    {
        GenericKursDBRepository<SD_26> genericProviderRepository;
        InvoiceProviderRepository invoiceProviderRepository;

        private void setRepository()
        {
            genericProviderRepository = new GenericKursDBRepository<SD_26>(UnitOfWork);
            invoiceProviderRepository = new InvoiceProviderRepository(UnitOfWork);
        }

        [Test]
        public void GetByDC()
        {
            setRepository();

            var d = genericProviderRepository.GetById(10260011751);
            var doc = new InvoiceProvider(d, UnitOfWork,false);
            Assert.AreNotEqual(d, null);
        }

        [Test]
        public void NewInvoice()
        {
            setRepository();
            var newEntity = new SD_26
            {
                DOC_CODE = -1,
                SF_POSTAV_DATE = DateTime.Today,
                SF_REGISTR_DATE = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.Name,
                SF_CRS_DC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                SF_POSTAV_NUM = null,
                Id = Guid.NewGuid(),
                SF_POST_DC = MainReferences.ActiveKontragents.First().Key,
                SF_RUB_SUMMA = 1000,
                SF_CRS_SUMMA = 1000,
                SF_PAY_FLAG = 0,
                SF_FACT_SUMMA = 1000,
                SF_PAY_COND_DC = 11790000001,
                TABELNUMBER = 2,
                SF_EXECUTED = 0,
                TD_26 = new List<TD_26>()
            };
            genericProviderRepository.Insert(newEntity);
            var newDoc = new InvoiceProvider(newEntity,UnitOfWork);
            var newRow = new InvoiceProviderRow
            {
                DocCode = newDoc.DocCode,
                Code = 1,
                Id = Guid.NewGuid(),
                DocId = newDoc.Id,
                SFT_POST_ED_IZM_DC = MainReferences.Units.First().Key,
                SFT_UCHET_ED_IZM_DC = MainReferences.Units.First().Key,
                SFT_ED_CENA = 1000,
                SFT_NEMENKL_DC = MainReferences.ALLNomenkls.Values
                    .FirstOrDefault(_ => _.Currency.DocCode == newDoc.Currency.DocCode).DocCode,
                SFT_KOL = 1,
                SFT_NDS_PERCENT = 18,
                SFT_IS_NAKLAD = 0,
                SFT_VKLUCH_V_CENU = 1,
                SFT_AUTO_FLAG = 0,
                SFT_TEXT = "test"
            };
            newDoc.Rows.Add(newRow);
            newDoc.Entity.TD_26.Add(newRow.Entity);
            UnitOfWork.CreateTransaction();
            var newDC = UnitOfWork.Context.SD_26.Any()
                ? UnitOfWork.Context.SD_26.Max(_ => _.DOC_CODE) + 1
                : 10260000001;
            try
            {
                newDoc.DocCode = newDC;
                foreach (var r in newDoc.Rows)
                {
                    r.DocCode = newDC;
                }

                UnitOfWork.Save();
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                Assert.Fail($"Не сохранен новый счет фактура поставщика. {ex.Message}");
            }

            var saveEntity = genericProviderRepository.GetById(newDC);
            var saveDoc = new InvoiceProvider(saveEntity,UnitOfWork, false);
            Assert.AreNotEqual(saveEntity, null, "Не загружается новый счет-фктура поставщика");
            Assert.AreEqual(saveDoc.Rows.Count, 1, "Строка не сохранилась");
            var row = saveDoc.Rows.First();
            var newCrsItem = new InvoiceProviderRowCurrencyConvertViewModel
            {
                Id = Guid.NewGuid(),
                DocCode = row.DocCode,
                Code = row.Code,
                NomenklId = row.Nomenkl.Id,
                Date = DateTime.Today,
                // ReSharper disable once PossibleInvalidOperationException
                OLdPrice = (decimal) row.SFT_ED_CENA,
                OLdNakladPrice = (row.SFT_SUMMA_NAKLAD ?? 0) != 0
                    ? Math.Round((decimal) row.SFT_ED_CENA +
                                 // ReSharper disable once PossibleInvalidOperationException
                                 (decimal) row.SFT_ED_CENA / (row.SFT_SUMMA_NAKLAD ?? 0), 2)
                    : Math.Round((decimal) row.SFT_ED_CENA, 2),
                Quantity = row.SFT_KOL,
                Rate = 1
            };
            row.CurrencyConvertRows.Add(newCrsItem);
            row.Entity.TD_26_CurrencyConvert.Add(newCrsItem.Entity);
            UnitOfWork.CreateTransaction();
            try
            {
                UnitOfWork.Save();
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                Assert.Fail("Не сохранен новый сохраненный счет фактура поставщика " +
                            $"с валютными таксировками. {ex.Message}");
            }

            var saveCrsEntity = genericProviderRepository.GetById(newDC);
            var saveCrsDoc = new InvoiceProvider(saveCrsEntity,UnitOfWork, false);
            Assert.AreNotEqual(saveCrsEntity, null, "Не загружается новый счет-фктура поставщика " +
                                                    "c валютными таксировками");
            Assert.AreEqual(saveCrsDoc.Rows.Count, 1, "Строка не сохранилась");
            Assert.AreEqual(saveCrsEntity.TD_26.First().TD_26_CurrencyConvert.Count, 1,
                "Не сохраниля и не загрузился строка валютной таксировки");

            var rc = saveCrsEntity.TD_26.First();
            var rcc = rc.TD_26_CurrencyConvert.First();
            rc.TD_26_CurrencyConvert.Remove(rcc);
            UnitOfWork.CreateTransaction();
            try
            {
                UnitOfWork.Save();
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                Assert.Fail($"Не удалена валютная таксировка сохраненный счет фактура поставщика. {ex.Message}");
            }

            saveCrsEntity = genericProviderRepository.GetById(newDC);
            
            Assert.AreNotEqual(saveCrsEntity.TD_26.First().TD_26_CurrencyConvert.Count, 1,
                "Не удалилась строка валютной таксировки");

            saveCrsDoc = invoiceProviderRepository.GetFullCopy(saveCrsDoc);
            UnitOfWork.CreateTransaction();
            try
            {
                newDC = UnitOfWork.Context.SD_26.Any()
                    ? UnitOfWork.Context.SD_26.Max(_ => _.DOC_CODE) + 1
                    : 10260000001;
                saveCrsDoc.DocCode = newDC;
                foreach (var r in saveCrsDoc.Rows)
                {
                    r.DocCode = newDC;
                }
                UnitOfWork.Save();
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                Assert.Fail($"Не сохранена копия счета фактура поставщика. {ex.Message}");
            }

            saveCrsEntity = genericProviderRepository.GetById(newDC);
            Assert.AreNotEqual(saveCrsEntity, null, "Не загружается copy счет-фктура поставщика");
            saveCrsDoc = new InvoiceProvider(saveCrsEntity, UnitOfWork,false);
            saveCrsDoc = invoiceProviderRepository.GetRequisiteCopy(saveCrsDoc);

            UnitOfWork.CreateTransaction();
            try
            {
                newDC = UnitOfWork.Context.SD_26.Any()
                    ? UnitOfWork.Context.SD_26.Max(_ => _.DOC_CODE) + 1
                    : 10260000001;
                saveCrsDoc.DocCode = newDC;
                foreach (var r in saveCrsDoc.Rows)
                {
                    r.DocCode = newDC;
                }
                UnitOfWork.Save();
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                Assert.Fail($"Не сохранена копия реквизитов счета фактура поставщика. {ex.Message}");
            }

            saveCrsEntity = genericProviderRepository.GetById(newDC);
            Assert.AreNotEqual(saveCrsEntity, null, "Не загружается copy счет-фктура поставщика");
            Assert.AreEqual(saveCrsEntity.TD_26.Count, 0, "Не удалились строки в счете-фактуре при копии реквизитов");

            #region Deleted

            UnitOfWork.CreateTransaction();
            try
            {
                //genericProviderRepository.Delete(saveCrsEntity);
                invoiceProviderRepository.Delete(saveCrsEntity);
                UnitOfWork.Save();
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                Assert.Fail($"Не удален новый сохраненный счет фактура поставщика. {ex.Message}");
            }

            var saveEntity2 = genericProviderRepository.GetById(newDC);
            Assert.AreEqual(saveEntity2, null, "Не удален новый счет-фктура поставщика");

            #endregion
        }

        [Test]
        public void TestCopy()
        {
            setRepository();

            var d = genericProviderRepository.GetById(10260011751);
            Assert.AreNotEqual(d, null); 
            
            genericProviderRepository.Context.Entry(d).State = EntityState.Added;
            var newDC = genericProviderRepository.Context.SD_26.Max(_ => _.DOC_CODE) + 1;
            var newId = Guid.NewGuid(); 
            var doc = new InvoiceProvider(d, UnitOfWork,false);
            doc.DocCode = -1;
            doc.CREATOR = GlobalOptions.UserInfo.NickName;
            doc.SF_POSTAV_DATE = DateTime.Today;
            doc.SF_POSTAV_NUM = "copy";
            doc.Id = newId;
            var i = 1;
            foreach (var r in doc.Rows)
            {
                genericProviderRepository.Context.Entry(r.Entity).State = EntityState.Added;
                r.DocCode = -1;
                r.Code = i;
                r.Id = Guid.NewGuid();
                r.DocId = newId;
                r.CurrencyConvertRows.Clear();
                r.Entity.TD_26_CurrencyConvert.Clear();
                i++;
            }
            UnitOfWork.CreateTransaction();
            try
            {
                doc.DocCode = newDC;
                foreach (var r in doc.Rows)
                {
                    r.DocCode = newDC;
                }
                UnitOfWork.Save();
                UnitOfWork.Commit();
            }
            catch (Exception ex)
            {
                UnitOfWork.Rollback();
                Assert.Fail($"Не сохранена копия счет фактура поставщика. {ex.Message}");
            }
        }
    }
}