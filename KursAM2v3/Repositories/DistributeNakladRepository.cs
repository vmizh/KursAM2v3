using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel.Base;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Invoices;
using Core.Invoices.EntityViewModel;
using Core.WindowsManager;
using Data;
using Data.Repository;

namespace KursAM2.Repositories
{
    public interface IDistributeNakladRepository : IDocumentWithRowOperations<DistributeNaklad,
        DistributeNakladRow>
    {
        DistributeNaklad GetById(Guid id);
        List<DistributeNaklad> GetAllByDates(DateTime dateStart, DateTime dateEnd);
        List<DistributeNakladRow> GetTovarFromInvoiceProviders(DistributeNaklad ent);
        List<DistributeNakladRow> GetTovarFromInvoiceProviders(DistributeNaklad ent, DateTime start, DateTime end);

        List<DistributeNakladInvoices> GetDistributeInvoice(DistributeNaklad ent);
        List<DistributeNakladInvoices> GetDistributeInvoice(DistributeNaklad ent, DateTime start, DateTime end);

        void Delete(DistributeNaklad doc);

        void UpdateSFNaklad(DistributeNaklad doc);

        DistributeNakladInvoices CreateNewInvoice(DistributeNaklad doc);

        void DistributeNakladRecalc(DistributeNaklad doc, List<DistributeNakladInvoices> invoices);

        InvoiceProvider GetInvoiceProviderById(Guid id);
        InvoiceProviderRowShort GetInvoiceRow(Guid id);
        InvoiceProviderRowCurrencyConvertViewModel GetTransferRow(Guid id);
        InvoiceProviderShort GetInvoiceHead(Guid id);
    }


    public class DistributeNakladRepository : GenericKursDBRepository<DistributeNaklad>, IDistributeNakladRepository
    {
        /// <summary>
        ///     Типы распределения накладных расходов
        /// </summary>
        public enum DistributeNakladTypeEnum
        {
            [Display(Name = "Нет распределния")] NotDistribute = 0,

            [Display(Name = "По цене")] PriceValue = 1,

            [Display(Name = "По сумме")] SummaValue = 2,

            [Display(Name = "По количеству")] QuantityValue = 3,

            [Display(Name = "По объему")] VolumeValue = 4,

            [Display(Name = "По весу")] WeightValue = 5,

            [Display(Name = "Вручную")] ManualValue = 6
        }

        public UnitOfWork<ALFAMEDIAEntities> UnitOfWork;

        public DistributeNakladRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            UnitOfWork = (UnitOfWork<ALFAMEDIAEntities>) unitOfWork;
        }

        public DistributeNakladRepository(ALFAMEDIAEntities context) : base(context)
        {
        }


        public DistributeNaklad GetById(Guid id)
        {
            return Context.DistributeNaklad.Include(_ => _.DistributeNakladRow)
                .Include(_ => _.DistributeNakladInvoices)
                .Include(_ => _.DistributeNakladRow.Select(x => x.DistributeNakladInfo))
                .SingleOrDefault(_ => _.Id == id);
        }

        public InvoiceProvider GetInvoiceProviderById(Guid id)
        {
            return new InvoiceProvider(Context.SD_26
                .Include(_ => _.TD_26)
                .Include("TD_26.TD_24")
                .Include(_ => _.SD_43)
                .Include(_ => _.SD_179)
                .Include(_ => _.SD_77)
                .Include(_ => _.SD_189)
                .Include(_ => _.SD_40)
                .Include("TD_26.SD_83")
                .Include("TD_26.SD_175")
                .Include("TD_26.SD_43")
                .Include("TD_26.SD_165")
                .Include("TD_26.SD_175")
                .Include("TD_26.SD_1751")
                .Include("TD_26.SD_26")
                .Include("TD_26.SD_261")
                .Include("TD_26.SD_301")
                .Include("TD_26.SD_303")
                .FirstOrDefault(_ => _.Id == id), new UnitOfWork<ALFAMEDIAEntities>());
        }

        public List<DistributeNaklad> GetAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            return Context.DistributeNaklad.Include(_ => _.DistributeNakladRow)
                .Include(_ => _.DistributeNakladRow.Select(x => x.DistributeNakladInfo))
                .Where(_ => _.DocDate >= dateStart && _.DocDate <= dateEnd)
                .AsNoTracking()
                .ToList();
        }

        public List<DistributeNakladRow> GetTovarFromInvoiceProviders(DistributeNaklad ent)
        {
            var ret = new List<DistributeNakladRow>();
            var rows = GetAllForNakladDistribute(MainReferences.GetCurrency(ent.CurrencyDC),
                DateTime.MinValue, DateTime.MaxValue);
            foreach (var inv in rows)
            foreach (var r in inv.Rows)
            {
                if (r.IsUsluga) continue;
                if (ret.Any(_ => _.TovarInvoiceRowId == r.Id)) continue;
                ret.Add(new DistributeNakladRow
                {
                    Id = Guid.NewGuid(),
                    TovarInvoiceRowId = r.Id,
                    Note = r.Note
                });
            }

            return ret;
        }

        public List<DistributeNakladRow> GetTovarFromInvoiceProviders(DistributeNaklad ent, DateTime start,
            DateTime end)
        {
            var ret = new List<DistributeNakladRow>();
            var rows = GetAllForNakladDistribute(MainReferences.GetCurrency(ent.CurrencyDC),
                start, end);
            foreach (var inv in rows)
            foreach (var r in inv.Rows)
            {
                if (r.IsUsluga) continue;
                if (ret.Any(_ => _.TovarInvoiceRowId == r.Id)) continue;
                ret.Add(new DistributeNakladRow
                {
                    Id = Guid.NewGuid(),
                    TovarInvoiceRowId = r.Id,
                    Note = r.Note
                });
            }

            return ret;
        }

        public List<DistributeNakladInvoices> GetDistributeInvoice(DistributeNaklad ent)
        {
            var ret = new List<DistributeNakladInvoices>();
            var rows = GetNakladInvoices(DateTime.MinValue, DateTime.MaxValue);
            foreach (var r in rows)
                ret.Add(new DistributeNakladInvoices
                {
                    Id = Guid.NewGuid(),
                    DocId = ent.Id,
                    InvoiceId = r.Id,
                    Note = r.Note,
                    DistributeType = 0,
                    DistributeNaklad = ent
                });
            return ret;
        }

        public InvoiceProviderRowShort GetInvoiceRow(Guid id)
        {
            var item = Context.TD_26.Include(_ => _.TD_24)
                .Include(_ => _.SD_83)
                .Include(_ => _.SD_175)
                .Include(_ => _.SD_43)
                .Include(_ => _.SD_165)
                .Include(_ => _.SD_175)
                .Include(_ => _.SD_1751)
                .Include(_ => _.SD_26)
                .Include(_ => _.SD_261)
                .Include(_ => _.SD_301)
                .Include(_ => _.SD_303)
                .SingleOrDefault(_ => _.Id == id);
            if (item != null) return new InvoiceProviderRowShort(item);
            return null;
        }

        public InvoiceProviderRowCurrencyConvertViewModel GetTransferRow(Guid id)
        {
            var item = Context.TD_26_CurrencyConvert.Include(_ => _.TD_26)
                .SingleOrDefault(_ => _.Id == id);
            return new InvoiceProviderRowCurrencyConvertViewModel(item);
        }

        public List<DistributeNakladInvoices> GetDistributeInvoice(DistributeNaklad ent, DateTime start, DateTime end)
        {
            var ret = new List<DistributeNakladInvoices>();
            var rows = GetNakladInvoices(start, end);
            foreach (var r in rows)
                ret.Add(new DistributeNakladInvoices
                {
                    Id = Guid.NewGuid(),
                    DocId = ent.Id,
                    InvoiceId = r.Id,
                    Note = r.Note,
                    DistributeType = 0,
                    DistributeNaklad = ent
                });
            return ret;
        }

        public new void Delete(DistributeNaklad doc)
        {
            var rlist = new List<DistributeNakladInfo>();
            foreach (var r in doc.DistributeNakladRow) rlist.AddRange(r.DistributeNakladInfo);
            Context.DistributeNakladInfo.RemoveRange(rlist);
            foreach (var r in doc.DistributeNakladRow)
            {
                if (r.TransferRowId != null)
                {
                    var old = Context.TD_26_CurrencyConvert.FirstOrDefault(_ => _.Id == r.TransferRowId);
                    if (old != null)
                    {
                        if (old.SummaWithNaklad - r.DistributeSumma >= old.Summa)
                        {
                            old.SummaWithNaklad = old.SummaWithNaklad - r.DistributeSumma;
                        }
                        else old.SummaWithNaklad = old.Summa;

                        old.PriceWithNaklad = old.SummaWithNaklad / old.Quantity;
                    }
                }

                if (r.TovarInvoiceRowId != null)
                {
                    var old = Context.TD_26.FirstOrDefault(_ => _.Id == r.TovarInvoiceRowId);
                    if (old != null)
                    {
                        if (old.SFT_SUMMA_NAKLAD - r.DistributeSumma >= 0)
                        {
                            old.SFT_SUMMA_NAKLAD = old.SFT_SUMMA_NAKLAD - r.DistributeSumma;
                        }
                        else old.SFT_SUMMA_NAKLAD = 0;
                    }
                }
            }
            Context.DistributeNakladRow.RemoveRange(doc.DistributeNakladRow);
            Context.DistributeNakladInvoices.RemoveRange(doc.DistributeNakladInvoices);
            Context.DistributeNaklad.Remove(doc);
            Context.SaveChanges();
        }

        public void UpdateSFNaklad(DistributeNaklad doc)
        {
            foreach (var row in doc.DistributeNakladRow)
            {
                var t26 = Context.TD_26.FirstOrDefault(_ => _.Id == row.TovarInvoiceRowId);
                if (t26 == null)
                {
                    var conv = Context.TD_26_CurrencyConvert.FirstOrDefault(_ => _.Id == row.TransferRowId);
                    if (conv == null) continue;
                    var s = row.DistributeSumma;
                    conv.PriceWithNaklad = conv.Price + s / conv.Quantity;
                    conv.SummaWithNaklad = conv.Summa + s;
                }
                else
                {
                    var s = row.DistributeSumma;
                    t26.SFT_SUMMA_NAKLAD = s;
                    t26.SFT_ED_CENA_PRIHOD = t26.SFT_ED_CENA + s / t26.SFT_KOL;
                }
            }
        }

        public DistributeNakladInvoices CreateNewInvoice(DistributeNaklad doc)
        {
            var newItem = new DistributeNakladInvoices
            {
                Id = Guid.NewGuid(),
                DocId = doc.Id,
                DistributeType = 0,
                DistributeNaklad = doc
            };
            doc.DistributeNakladInvoices.Add(newItem);
            return newItem;
        }

        public void DistributeNakladRecalc(DistributeNaklad doc, List<DistributeNakladInvoices> invoices)
        {
            if (doc.DistributeNakladRow.Count == 0) return;
            foreach (var n in doc.DistributeNakladRow)
            foreach (var inv in invoices)
            {
                var nr = n.DistributeNakladInfo.FirstOrDefault(_ => _.RowId == n.Id
                                                                    && _.InvoiceNakladId == inv.Id);
                if (nr != null)
                {
                    nr.NakladSumma = 0;
                    nr.Rate = inv.Rate;
                    nr.DistributeSumma = 0;
                }
                else
                {
                    n.DistributeNakladInfo.Add(new DistributeNakladInfo
                    {
                        Id = Guid.NewGuid(),
                        InvoiceNakladId = inv.Id,
                        DistributeNakladRow = n,
                        NakladSumma = 0,
                        DistributeSumma = 0,
                        InvoiceCrsDC = inv.CrsDC,
                        Rate = 1,
                        RowId = n.Id
                    });
                }
            }

            foreach (var invoice in invoices)
            {
                decimal allSumma = 0;
                switch ((DistributeNakladTypeEnum) invoice.DistributeType)
                {
                    case DistributeNakladTypeEnum.ManualValue:
                        break;
                    case DistributeNakladTypeEnum.NotDistribute:
                        break;
                    case DistributeNakladTypeEnum.PriceValue:
                        allSumma = doc.DistributeNakladRow.Sum(_ => _.Price);
                        break;
                    case DistributeNakladTypeEnum.QuantityValue:
                        allSumma = doc.DistributeNakladRow.Sum(_ => _.Quantity);
                        break;
                    case DistributeNakladTypeEnum.SummaValue:
                        allSumma = doc.DistributeNakladRow.Sum(_ => _.Price * _.Quantity);
                        break;
                    case DistributeNakladTypeEnum.VolumeValue:
                        allSumma = 0;
                        break;
                    case DistributeNakladTypeEnum.WeightValue:
                        allSumma = 0;
                        break;
                }

                foreach (var r in doc.DistributeNakladRow)
                {
                    var nprow = r.DistributeNakladInfo.FirstOrDefault(_ => _.InvoiceNakladId == invoice.Id);
                    if (nprow == null) continue;

                    switch ((DistributeNakladTypeEnum) invoice.DistributeType)
                    {
                        case DistributeNakladTypeEnum.ManualValue:
                            break;
                        case DistributeNakladTypeEnum.NotDistribute:
                            break;
                        case DistributeNakladTypeEnum.PriceValue:
                            nprow.NakladSumma = r.Price * (invoice.SummaInvoice - invoice.SummaDistribute) / allSumma;
                            break;
                        case DistributeNakladTypeEnum.QuantityValue:
                            nprow.NakladSumma =
                                r.Quantity * (invoice.SummaInvoice - invoice.SummaDistribute) / allSumma;
                            break;
                        case DistributeNakladTypeEnum.SummaValue:
                            nprow.NakladSumma = r.Quantity * r.Price *
                                (invoice.SummaInvoice - invoice.SummaDistribute) / allSumma;
                            break;
                        case DistributeNakladTypeEnum.VolumeValue:
                            break;
                        case DistributeNakladTypeEnum.WeightValue:
                            break;
                    }

                    // ReSharper disable once PossibleInvalidOperationException
                    nprow.DistributeSumma = (decimal) nprow.NakladSumma * invoice.Rate;
                }
            }
        }

        public InvoiceProviderShort GetInvoiceHead(Guid id)
        {
            var item = Context.SD_26
                .Include(_ => _.TD_26)
                .Include(_ => _.SD_43)
                .Include(_ => _.SD_179)
                .Include(_ => _.SD_77)
                .Include(_ => _.SD_189)
                .Include(_ => _.SD_40)
                .SingleOrDefault(_ => _.Id == id);
            return new InvoiceProviderShort(item, new UnitOfWork<ALFAMEDIAEntities>());
        }

        public DistributeNaklad CreateNew()
        {
            var entity = new DistributeNaklad
            {
                Id = Guid.NewGuid(),
                CurrencyDC = GlobalOptions.SystemProfile.NationalCurrency.DocCode,
                Creator = GlobalOptions.UserInfo.Name,
                DocDate = DateTime.Today,
                DocNum = Context.DistributeNaklad.Any() ? (Context.DistributeNaklad.Count() + 1).ToString() : "1"
            };
            Context.DistributeNaklad.Add(entity);
            return entity;
        }

        public DistributeNaklad CreateCopy(DistributeNaklad oldentity)
        {
            var entity = new DistributeNaklad
            {
                Id = Guid.NewGuid(),
                CurrencyDC = oldentity.CurrencyDC,
                Creator = GlobalOptions.UserInfo.Name,
                DocDate = DateTime.Today,
                DocNum = Context.DistributeNaklad.Any() ? Context.DistributeNaklad.Count().ToString() + 1 : "1",
                Note = oldentity.Note
            };
            Context.DistributeNaklad.Add(entity);
            return entity;
        }

        public DistributeNaklad CreateCopy(Guid id)
        {
            var oldentity = Context.DistributeNaklad.SingleOrDefault(_ => _.Id == id);
            if (oldentity == null) return null;
            var entity = new DistributeNaklad
            {
                Id = Guid.NewGuid(),
                CurrencyDC = oldentity.CurrencyDC,
                Creator = GlobalOptions.UserInfo.Name,
                DocDate = DateTime.Today,
                DocNum = Context.DistributeNaklad.Any() ? Context.DistributeNaklad.Count().ToString() + 1 : "1",
                Note = oldentity.Note
            };
            Context.DistributeNaklad.Add(entity);
            return entity;
        }

        public DistributeNaklad CreateCopy(decimal dc)
        {
            WindowManager.ShowFunctionNotReleased();
            return null;
        }

        public DistributeNaklad CreateRequisiteCopy(DistributeNaklad oldentity)
        {
            return CreateCopy(oldentity);
        }

        public DistributeNaklad CreateRequisiteCopy(Guid id)
        {
            return CreateCopy(id);
        }

        public DistributeNaklad CreateRequisiteCopy(decimal dc)
        {
            return CreateCopy(dc);
        }

        public DistributeNakladRow CreateRowNew(DistributeNaklad head)
        {
            var newItem = new DistributeNakladRow
            {
                Id = Guid.NewGuid(),
                DocId = head.Id,
                DistributeNaklad = head
            };
            head.DistributeNakladRow.Add(newItem);
            return newItem;
        }

        public DistributeNakladRow CreateRowCopy(DistributeNakladRow oldent)
        {
            throw new NotImplementedException();
        }

        public void DeleteRow(DistributeNakladRow ent)
        {
            throw new NotImplementedException();
        }

        public void LoadRow(DistributeNaklad ent, DistributeNakladRow rowent)
        {
            throw new NotImplementedException();
        }

        public List<InvoiceProviderShort> GetAllForNakladDistribute(Currency crs, DateTime dateStart,
            DateTime dateEnd)
        {
            var ret = new List<InvoiceProviderShort>();
            var data = Context.SD_26
                .Include(_ => _.TD_26)
                .Include("TD_26.TD_24")
                .Include(_ => _.SD_43)
                .Include(_ => _.SD_179)
                .Include(_ => _.SD_77)
                .Include(_ => _.SD_189)
                .Include(_ => _.SD_40)
                .Include("TD_26.SD_83")
                .Include("TD_26.SD_175")
                .Include("TD_26.SD_43")
                .Include("TD_26.SD_165")
                .Include("TD_26.SD_175")
                .Include("TD_26.SD_1751")
                .Include("TD_26.SD_26")
                .Include("TD_26.SD_261")
                .Include("TD_26.SD_301")
                .Include("TD_26.SD_303")
                .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd
                                                          && _.SF_CRS_DC == crs.DocCode && _.SF_ACCEPTED == 1)
                .ToList();

            var invdc = Context.TD_26_CurrencyConvert
                .Include(_ => _.TD_26)
                .Include(_ => _.TD_26.SD_26)
                .Where(_ => _.TD_26.SD_26.SF_POSTAV_DATE >= dateStart &&
                            _.TD_26.SD_26.SF_POSTAV_DATE <= dateEnd).ToList();
            foreach (var dc in invdc)
            {
                var doc = Context.SD_26
                    .Include(_ => _.TD_26)
                    .Include("TD_26.TD_24")
                    .Include("TD_26.TD_26_CurrencyConvert")
                    .Include(_ => _.SD_43)
                    .Include(_ => _.SD_179)
                    .Include(_ => _.SD_77)
                    .Include(_ => _.SD_189)
                    .Include(_ => _.SD_40)
                    .Include("TD_26.SD_83")
                    .Include("TD_26.SD_175")
                    .Include("TD_26.SD_43")
                    .Include("TD_26.SD_165")
                    .Include("TD_26.SD_175")
                    .Include("TD_26.SD_1751")
                    .Include("TD_26.SD_26")
                    .Include("TD_26.SD_261")
                    .Include("TD_26.SD_301")
                    .Include("TD_26.SD_303")
                    .FirstOrDefault(_ => _.DOC_CODE == dc.DOC_CODE);
                if (doc != null) ret.Add(new InvoiceProviderShort(doc, UnitOfWork));
            }

            foreach (var d in data) ret.Add(new InvoiceProviderShort(d, new UnitOfWork<ALFAMEDIAEntities>()));

            return ret;
        }

        public List<InvoiceProviderShort> GetNakladInvoices(DateTime dateStart,
            DateTime dateEnd)
        {
            var ret = new List<InvoiceProviderShort>();
            var data = Context.SD_26
                .Where(_ => _.SF_POSTAV_DATE >= dateStart && _.SF_POSTAV_DATE <= dateEnd
                                                          && _.IsInvoiceNakald == true
                                                          && (_.NakladDistributedSumma ?? 0) <
                                                          _.SF_KONTR_CRS_SUMMA && _.SF_ACCEPTED == 1)
                .ToList();
            foreach (var d in data) ret.Add(new InvoiceProviderShort(d, UnitOfWork));

            return ret;
        }
    }
}