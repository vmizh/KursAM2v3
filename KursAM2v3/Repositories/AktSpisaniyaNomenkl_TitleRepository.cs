using Core;
using Data;
using KursDomain.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using KursDomain;

namespace KursAM2.Repositories
{
    public interface IAktSpisaniyaNomenkl_TitleRepository
    {
        AktSpisaniyaNomenkl_Title AktSpisaniya { set; get; }

        AktSpisaniyaNomenkl_Title GetByGuidId(Guid id);

        AktSpisaniyaNomenkl_Title CreateNew();

        List<AktSpisaniyaNomenkl_Title> GetAllByDates(DateTime dateStart, DateTime dateEnd);

        List<AktSpisaniyaNomenkl_Title> GetAllByWarehouse(decimal warehouseDC);

        void Delete();
        void Delete(Guid id);
    }

    public class AktSpisaniyaNomenkl_TitleRepository : GenericKursDBRepository<AktSpisaniyaNomenkl_Title>, IAktSpisaniyaNomenkl_TitleRepository
    {
        public UnitOfWork<ALFAMEDIAEntities> UnitOfWork;
        public AktSpisaniyaNomenkl_TitleRepository(IUnitOfWork<ALFAMEDIAEntities> unitOfWork) : base(unitOfWork)
        {
            UnitOfWork = (UnitOfWork<ALFAMEDIAEntities>)unitOfWork;
        }

        public AktSpisaniyaNomenkl_TitleRepository(ALFAMEDIAEntities context) : base(context)
        {
        }

        public AktSpisaniyaNomenkl_Title AktSpisaniya { get; set; }

        public AktSpisaniyaNomenkl_Title GetByGuidId(Guid id)
        {
            return Context.AktSpisaniyaNomenkl_Title
                .Include(_ => _.SD_27)
                .Include(_ => _.AktSpisaniya_row)
                .FirstOrDefault(_ => _.Id == id);
        }

        public AktSpisaniyaNomenkl_Title CreateNew()
        {
            var item = new AktSpisaniyaNomenkl_Title()
            {
                Id = Guid.NewGuid(),
                Creator = GlobalOptions.UserInfo.NickName,
                Date_Doc = DateTime.Today,
            };
            Context.AktSpisaniyaNomenkl_Title.Add(item);
            return item;
        }

        public List<AktSpisaniyaNomenkl_Title> GetAllByDates(DateTime dateStart, DateTime dateEnd)
        {
            return Context.AktSpisaniyaNomenkl_Title.Where(_ => _.Date_Doc >= dateStart && _.Date_Doc <= dateEnd).ToList();
        }

        public List<AktSpisaniyaNomenkl_Title> GetAllByWarehouse(decimal warehouseDC)
        {
            return Context.AktSpisaniyaNomenkl_Title
                .Include(_ => _.SD_27)
                .Include(_ => _.AktSpisaniya_row)
                .Where(_ => _.Warehouse_DC == warehouseDC).ToList();
        }

        public void Delete()
        {
            if (AktSpisaniya == null) return;
            Delete(AktSpisaniya.Id);
        }

        public void Delete(Guid id)
        {
            foreach (var r in Context.AktSpisaniya_row.Where(_ => _.Doc_Id == id).ToList())
            {
                Context.AktSpisaniya_row.Remove(r);
            }
            var d = Context.AktSpisaniyaNomenkl_Title.FirstOrDefault(_ => _.Id == id);
            if (d != null)
                Context.AktSpisaniyaNomenkl_Title.Remove(d);
        }
    }
}
