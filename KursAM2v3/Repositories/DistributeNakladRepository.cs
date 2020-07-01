using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.Repository.Base;
using Core.WindowsManager;
using Data;
using KursAM2.ViewModel.Finance.DistributeNaklad;

namespace KursAM2.Repositories
{
    public interface IDistributeNakladRepository : IDocumentWithRowOperations<DistributeNaklad, DistributeNakladRow>
    {
        DistributeNaklad GetById(Guid id);
        List<DistributeNaklad> GetAllByDates(DateTime dateStart, DateTime dateEnd);
    }
    public class DistributeNakladRepository : GenericKursRepository<DistributeNaklad>, IDistributeNakladRepository
    {
        public DistributeNakladRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
        }

        public DistributeNakladRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public DistributeNaklad GetById(Guid id)
        {
            return Context.DistributeNaklad.Include(_ => _.DistributeNakladRow)
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