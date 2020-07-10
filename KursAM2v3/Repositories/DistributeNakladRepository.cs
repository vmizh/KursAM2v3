using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.Repository.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using KursAM2.ViewModel.Finance.DistributeNaklad;

namespace KursAM2.Repositories
{
    public interface IDistributeNakladRepository : IDocumentWithRowOperations<DistributeNaklad, 
        DistributeNakladRow>
    {
        DistributeNaklad GetById(Guid id);
        List<DistributeNaklad> GetAllByDates(DateTime dateStart, DateTime dateEnd);

        List<DistributeNakladInvoiceViewModel> GetInvoiceProviders(DistributeNakladViewModel vm);
        void UpdateProviderInvoices(DistributeNakladViewModel vm);
    }


    public class DistributeNakladRepository : GenericKursRepository<DistributeNaklad>, IDistributeNakladRepository
    {
        /// <summary>
        /// Типы распределения накладных расходов
        /// </summary>
        public enum DistributeNakladTypeEnum
        {
            [Display(Name = "Нет распределния")]
            NotDistribute = 0,

            [Display(Name = "По цене")]
            PriceValue = 1,

            [Display(Name = "По сумме")]
            SummaValue = 2,

            [Display(Name = "По количеству")]
            QuantityValue = 3,

            [Display(Name = "По объему")]
            VolumeValue = 4,

            [Display(Name = "По весу")]
            WeightValue = 5,

            [Display(Name = "Вручную")]
            ManualValue = 6,

        }

        public DistributeNakladRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
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

        public List<DistributeNaklad> GetAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            return Context.DistributeNaklad.Include(_ => _.DistributeNakladRow)
                .Include(_ => _.DistributeNakladRow.Select(x => x.DistributeNakladInfo))
                .Where(_ => _.DocDate >= dateStart && _.DocDate <= dateEnd)
                .AsNoTracking()
                .ToList();
        }

        public List<DistributeNakladInvoiceViewModel> GetInvoiceProviders(DistributeNakladViewModel vm)
        {
            List<DistributeNakladInvoiceViewModel> ret = new List<DistributeNakladInvoiceViewModel>();
            var data = Context.SD_26
                .Include(_ => _.TD_26)
                .Include("TD_26.SD_83")
                .Where(_ => _.IsInvoiceNakald == true 
                            && _.NakladDistributedSumma < _.SF_KONTR_CRS_SUMMA
                            && _.TD_26.All(s => s.SD_83.NOM_0MATER_1USLUGA == 1)
                            )
                .OrderByDescending(_ => _.SF_POSTAV_DATE);
            foreach (var d in data)
            {
                var distrSumma = Context.DistributeNakladInfo
                    .Where(_ => _.InvoiceNakladId == d.Id).Sum(_ => _.NakladSumma) ?? 0;
                ret.Add(new DistributeNakladInvoiceViewModel(
                    Context.DistributeNakladInvoices.Add(new DistributeNakladInvoices
                    {
                        DocId = vm.Id,
                        Id = Guid.NewGuid(),
                        InvoiceId = d.Id,
                        DistributeNaklad = vm.Entity,
                        DistributeType = 0 
                        
                    }))
                {
                    Invoice = d,
                    Summa = d.SF_KONTR_CRS_SUMMA ?? 0,
                    SummaDistribute = distrSumma,
                });
            }
            return ret;
        }

        public void UpdateProviderInvoices(DistributeNakladViewModel vm)
        {
            foreach (var inv in vm.NakladInvoices)
            {
                inv.Invoice.NakladDistributedSumma = inv.SummaDistribute;
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
                Id = Guid.NewGuid()
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