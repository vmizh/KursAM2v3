using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.EntityViewModel.Dogovora;
using Core.EntityViewModel.Invoices;
using Data;
using Data.Repository;
using KursAM2.Repositories.DogovorsRepositories;
using KursAM2.ViewModel.Dogovora;

namespace KursAM2.Repositories
{
    public interface IAktSpisaniyaNomenkl_TitleRepository
    {
        AktSpisaniyaNomenkl_Title AktSpisaniya { set; get; }

        AktSpisaniyaNomenkl_Title GetByGuidId(Guid id);
        AktSpisaniyaNomenkl_Title GetFullCopy(AktSpisaniyaNomenkl_Title doc);
        AktSpisaniyaNomenkl_Title GetRequisiteCopy(AktSpisaniyaNomenkl_Title doc);
        AktSpisaniyaNomenkl_Title CreateNew();

        // List<LinkDocumentInfo> GetLinkDocuments();

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

        public AktSpisaniyaNomenkl_Title AktSpisaniya { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public AktSpisaniyaNomenkl_Title CreateNew()
        {
            throw new NotImplementedException();
        }

        public AktSpisaniyaNomenkl_Title GetByGuidId(Guid id)
        {
            throw new NotImplementedException();
        }

        public AktSpisaniyaNomenkl_Title GetFullCopy(AktSpisaniyaNomenkl_Title doc)
        {
            throw new NotImplementedException();
        }

        public AktSpisaniyaNomenkl_Title GetRequisiteCopy(AktSpisaniyaNomenkl_Title doc)
        {
            throw new NotImplementedException();
        }
    }
}
