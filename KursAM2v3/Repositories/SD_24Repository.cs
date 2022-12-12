using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel.Base;
using Data;
using KursDomain.Repository;
using KursDomain;

namespace KursAM2.Repositories
{
    public static class MatearialDocumentType
    {
        [Display(Name = "Приходный складской ордер")]
        public const decimal OrderIn = 2010000001;

        [Display(Name = "Расходный складской ордер")]
        public const decimal OrderOut = 2010000003;

        [Display(Name = "Инвентаризационная ведомость")]
        public const decimal InventorySheet = 2010000005;

        [Display(Name = "Акт списания материалов")]
        public const decimal AktSpisaniya = 2010000010;

        [Display(Name = "Расходная накладная (без требования)")]
        public const decimal WayBill = 2010000012;

        [Display(Name = "Накладная на внутренее перемещение")]
        public const decimal NakladInner = 2010000014;

    }

    // ReSharper disable once InconsistentNaming
    public interface ISD_24Repository : IDocumentOperations<SD_24>
    {
        SD_24 GetByDC(decimal dc);
        SD_24 CreateNew(decimal docType);
        List<SD_24> GetWayBillAllByDates(DateTime dateStart, DateTime dateEnd);
        List<SD_24> GetPrihodOrderAllByDates(DateTime dateStart, DateTime dateEnd);
        List<SD_24> GetDocuments(decimal docType,DateTime dateStart, DateTime dateEnd);
    }

    // ReSharper disable once InconsistentNaming
    public class SD_24Repository : GenericKursDBRepository<SD_24>, ISD_24Repository
    {
        public SD_24Repository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
        }

        public SD_24Repository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public SD_24 CreateNew()
        {
            throw new NotImplementedException();
        }

        public SD_24 CreateCopy(SD_24 ent)
        {
            throw new NotImplementedException();
        }

        public SD_24 CreateCopy(Guid id)
        {
            throw new NotImplementedException();
        }

        public SD_24 CreateCopy(decimal dc)
        {
            var ret = GetByDC(dc);
            ret.DOC_CODE = -1;
            ret.DD_DATE = DateTime.Today;
            ret.Id = Guid.NewGuid();
            ret.CREATOR = GlobalOptions.UserInfo.NickName;
            foreach (var t in ret.TD_24)
            {
                t.DOC_CODE = -1;
                Context.Entry(t).State = EntityState.Added;
            }
            Context.Entry(ret).State = EntityState.Added;
            return ret;
        }

        public SD_24 CreateRequisiteCopy(SD_24 ent)
        {
            throw new NotImplementedException();
        }

        public SD_24 CreateRequisiteCopy(Guid id)
        {
            throw new NotImplementedException();
        }

        public SD_24 CreateRequisiteCopy(decimal dc)
        {
            var ret = GetByDC(dc);
            Context.Entry(ret).State = EntityState.Added;
            ret.DOC_CODE = -1;
            ret.DD_DATE = DateTime.Today;
            ret.Id = Guid.NewGuid();
            ret.CREATOR = GlobalOptions.UserInfo.NickName;
            ret.TD_24.Clear();
            
            return ret;

        }

        public SD_24 GetByDC(decimal dc)
        {
            return Context.SD_24
                .Include(_ => _.TD_24)
                .Include("TD_24.TD_26")
                .Include("TD_24.TD_26.SD_26")
                .Include("TD_24.SD_175")
                .Include("TD_24.SD_301")
                .Include("TD_24.SD_122")
                .Include("TD_24.SD_170")
                .Include("TD_24.SD_175")
                .Include("TD_24.SD_1751")
                .Include("TD_24.SD_2")
                .Include("TD_24.SD_254")
                .Include("TD_24.SD_27")
                .Include("TD_24.SD_301")
                .Include("TD_24.SD_3011")
                .Include("TD_24.SD_3012")
                .Include("TD_24.SD_303")
                .Include("TD_24.SD_384")
                .Include("TD_24.SD_43")
                .Include("TD_24.SD_83")
                .Include("TD_24.SD_831")
                .Include("TD_24.SD_832")
                .Include("TD_24.SD_84")
                .Include("TD_24.TD_73")
                .Include("TD_24.TD_9")
                .Include("TD_24.TD_84")
                .Include("TD_24.TD_26")
                .Include("TD_24.TD_241")
                .SingleOrDefault(_ => _.DOC_CODE == dc);
        }

        public SD_24 CreateNew(decimal docType)
        {
            return new SD_24
            {
                DOC_CODE = -1,
                DD_DATE = DateTime.Today,
                DD_TYPE_DC = docType,
                Id=Guid.NewGuid(),
                CREATOR = GlobalOptions.UserInfo.NickName,
                DD_IN_NUM = -1,
            };
        }

        public List<SD_24> GetWayBillAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            return Context.SD_24
                .Where(_ => _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd 
                                                   && _.DD_TYPE_DC == 2010000012).ToList();
        }

        public List<SD_24> GetPrihodOrderAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            return Context.SD_24
                .Include(_ => _.TD_24)
                .Include("TD_24.TD_26")
                .Include("TD_24.TD_26.SD_26")
                .Include("TD_24.SD_175")
                .Include("TD_24.SD_301")
                .Include("TD_24.SD_122")
                .Include("TD_24.SD_170")
                .Include("TD_24.SD_175")
                .Include("TD_24.SD_1751")
                .Include("TD_24.SD_2")
                .Include("TD_24.SD_254")
                .Include("TD_24.SD_27")
                .Include("TD_24.SD_301")
                .Include("TD_24.SD_3011")
                .Include("TD_24.SD_3012")
                .Include("TD_24.SD_303")
                .Include("TD_24.SD_384")
                .Include("TD_24.SD_43")
                .Include("TD_24.SD_83")
                .Include("TD_24.SD_831")
                .Include("TD_24.SD_832")
                .Include("TD_24.SD_84")
                .Include("TD_24.TD_73")
                .Include("TD_24.TD_9")
                .Include("TD_24.TD_84")
                .Include("TD_24.TD_26")
                .Include("TD_24.TD_241")
                .Where(_ => _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd
                                                   && _.DD_TYPE_DC == 2010000001).ToList();
        }

        public List<SD_24> GetDocuments(decimal docType, DateTime dateStart, DateTime dateEnd)
        {
            return Context.SD_24
                .Include(_ => _.TD_24)
                .Include("TD_24.TD_26")
                .Include("TD_24.TD_26.SD_26")
                .Include("TD_24.SD_175")
                .Include("TD_24.SD_301")
                .Include("TD_24.SD_122")
                .Include("TD_24.SD_170")
                .Include("TD_24.SD_175")
                .Include("TD_24.SD_1751")
                .Include("TD_24.SD_2")
                .Include("TD_24.SD_254")
                .Include("TD_24.SD_27")
                .Include("TD_24.SD_301")
                .Include("TD_24.SD_3011")
                .Include("TD_24.SD_3012")
                .Include("TD_24.SD_303")
                .Include("TD_24.SD_384")
                .Include("TD_24.SD_43")
                .Include("TD_24.SD_83")
                .Include("TD_24.SD_831")
                .Include("TD_24.SD_832")
                .Include("TD_24.SD_84")
                .Include("TD_24.TD_73")
                .Include("TD_24.TD_9")
                .Include("TD_24.TD_84")
                .Include("TD_24.TD_26")
                .Include("TD_24.TD_241")
                .Where(_ => _.DD_DATE >= dateStart && _.DD_DATE <= dateEnd
                                                   && _.DD_TYPE_DC == docType).ToList();
        }
    }
}
