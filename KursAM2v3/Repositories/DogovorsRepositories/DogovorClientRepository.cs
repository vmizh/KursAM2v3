using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core;
using Data;
using Data.Repository;

namespace KursAM2.Repositories.DogovorsRepositories
{
    public interface IDogovorClientRepository
    {
        DogovorClient GetByGuidId(Guid id);
        DogovorClient GetFullCopy(DogovorClient doc);
        DogovorClient GetRequisiteCopy(DogovorClient doc);
        List<DogovorClient> GetAllByDates(DateTime dateStart, DateTime dateEnd);
        DogovorClient CreateNew();
    }

    public class DogovorClientRepository : GenericKursDBRepository<DogovorClient>, IDogovorClientRepository
    {
        public UnitOfWork<ALFAMEDIAEntities> UnitOfWork;
        public DogovorClientRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            UnitOfWork = (UnitOfWork<ALFAMEDIAEntities>) unitOfWork;
        }

        public DogovorClientRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public DogovorClient GetByGuidId(Guid id)
        {
            return Context.DogovorClient
                .Include(_ => _.SD_102)
                .Include(_ => _.SD_179)
                .Include(_ => _.SD_189)
                .Include(_ => _.SD_43)
                .Include(_ => _.DogovorClientRow)
                .Include(_ => _.DogovorClientRow.Select(x => x.DogovorClientFact))
                .FirstOrDefault(_ => _.Id == id);
        }

        public DogovorClient GetFullCopy(DogovorClient doc)
        {
            throw new NotImplementedException();
        }

        public DogovorClient GetRequisiteCopy(DogovorClient doc)
        {
            throw new NotImplementedException();
        }

        public List<DogovorClient> GetAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            return Context.DogovorClient
                .Include(_ => _.SD_102)
                .Include(_ => _.SD_179)
                .Include(_ => _.SD_189)
                .Include(_ => _.SD_43)
                .Include(_ => _.DogovorClientRow)
                .Include(_ => _.DogovorClientRow.Select(x => x.DogovorClientFact))
                .Where(_ => _.DogDate >= dateStart && _.DogDate <= dateEnd).ToList();
        }

        public DogovorClient CreateNew()
        {
            return new DogovorClient
            {
                Id = Guid.NewGuid(),
                Creator = GlobalOptions.UserInfo.NickName,
                DogDate = DateTime.Today,
            };
        }
    }
}