﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Core;
using Core.Repository.Base;
using Core.WindowsManager;
using Data;
using Data.Repository;
using KursAM2.Repositories.InvoicesRepositories;

namespace KursAM2.Repositories
{
    public interface IDistributeNakladRepository : IDocumentWithRowOperations<DistributeNaklad,
        DistributeNakladRow>
    {
        InvoiceProviderRepository InvoiceProviderRepository { set; get; }
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

        public DistributeNakladRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            InvoiceProviderRepository = new InvoiceProviderRepository(unitOfWork.Context);
        }

        public DistributeNakladRepository(ALFAMEDIAEntities context) : base(context)
        {
            InvoiceProviderRepository = new InvoiceProviderRepository(context);
        }

        public InvoiceProviderRepository InvoiceProviderRepository { get; set; }

        public DistributeNaklad GetById(Guid id)
        {
            return Context.DistributeNaklad.Include(_ => _.DistributeNakladRow)
                .Include(_ => _.DistributeNakladInvoices)
                .Include(_ => _.DistributeNakladRow.Select(x => x.DistributeNakladInfo))
                .SingleOrDefault(_ => _.Id == id);
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
            var rows = InvoiceProviderRepository.GetAllForNakladDistribute(MainReferences.GetCurrency(ent.CurrencyDC),
                null, null);
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
            var rows = InvoiceProviderRepository.GetAllForNakladDistribute(MainReferences.GetCurrency(ent.CurrencyDC),
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
            var rows = InvoiceProviderRepository.GetNakladInvoices(null, null);
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

        public List<DistributeNakladInvoices> GetDistributeInvoice(DistributeNaklad ent, DateTime start, DateTime end)
        {
            var ret = new List<DistributeNakladInvoices>();
            var rows = InvoiceProviderRepository.GetNakladInvoices(start, end);
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
            Context.DistributeNakladRow.RemoveRange(doc.DistributeNakladRow);
            Context.DistributeNakladInvoices.RemoveRange(doc.DistributeNakladInvoices);
            Context.DistributeNaklad.Remove(doc);
        }

        public void UpdateSFNaklad(DistributeNaklad doc)
        {
            foreach (var row in doc.DistributeNakladRow)
            {
                var t26 = Context.TD_26.FirstOrDefault(_ => _.Id == row.TovarInvoiceRowId);
                if (t26 == null) continue;
                var r = Context.DistributeNakladRow
                    .Where(_ => _.TovarInvoiceRowId == row.TovarInvoiceRowId);
                if (!r.Any()) continue;
                var s = r.Sum(_ => _.DistributeSumma);
                t26.SFT_SUMMA_NAKLAD = s;
                t26.SFT_ED_CENA_PRIHOD = t26.SFT_ED_CENA + s / t26.SFT_KOL;
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
    }
}