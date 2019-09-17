using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Data;
using KursAM2.Managers.Base;
using KursAM2.ViewModel.Finance.Invoices;

namespace KursAM2.Managers
{
    public class InvoiceSupplierManager : DocumentManager<SD_26ViewModel>
    {
        /// <summary>
        ///     Тип загрузки строк инвойса
        /// </summary>
        public enum TypeLoadInvoiceRows
        {
            All,
            WithoutNaklad,
            NakladOnly
        }

        public override List<SD_26ViewModel> GetDocuments()
        {
            throw new NotImplementedException();
        }

        public override List<SD_26ViewModel> GetDocuments(DateTime dateStart, DateTime dateEnd)
        {
            List<SD_26> docs;
            var ret = new List<SD_26ViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                docs =
                    ctx.SD_26
                        .Where(_ => (_.SF_POSTAV_DATE >= dateStart) && (_.SF_POSTAV_DATE <= dateEnd))
                        .ToList();
            }
            ret.AddRange(docs.Select(d => new SD_26ViewModel(d)));
            return ret;
        }

        public override List<SD_26ViewModel> GetDocuments(DateTime dateStart, DateTime dateEnd,
            string searchText)
        {
            List<SD_26> docs;
            var ret = new List<SD_26ViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                docs =
                    ctx.SD_26.Include(_ => _.SD_43).Include(_ => _.SD_301)
                        .Include(_ => _.TD_26)
                        .Where(_ => (_.SF_POSTAV_DATE >= dateStart) && (_.SF_POSTAV_DATE <= dateEnd)
                                    &&
                                    (_.SD_43.NAME.ToUpper().Contains(searchText.ToUpper()) ||
                                     _.SD_301.CRS_SHORTNAME.ToUpper().Contains(searchText.ToUpper())))
                        .ToList();
            }
            var noms = new List<SD_26>();
            foreach (var n in docs.Where(_ => _.TD_26.Select(item => MainReferences.GetNomenkl(item.SFT_NEMENKL_DC))
                .Any(nom => nom.NomenklNumber.ToUpper().Contains(searchText.ToUpper())
                            || nom.Name.ToUpper().Contains(searchText.ToUpper()))))
            {
                noms.Add(n);
                break;
            }
            foreach (
                var n in
                from n in noms let dcs = docs.Where(_ => _.DOC_CODE == n.DOC_CODE) where !dcs.Any() select n)
                docs.Add(n);
            ret.AddRange(docs.Select(d => new SD_26ViewModel(d)));
            return ret;
        }

        public List<TD_26ViewModel> GetRows(decimal dc,
            TypeLoadInvoiceRows typeLoad = TypeLoadInvoiceRows.All)
        {
            var ret = new List<TD_26ViewModel>();
            List<TD_26> rows;
            using (var ctx = GlobalOptions.GetEntities())
            {
                rows =
                    ctx.TD_26
                        .Where(_ => _.DOC_CODE == dc)
                        .ToList();
                switch (typeLoad)
                {
                    case TypeLoadInvoiceRows.All:
                        ret.AddRange(rows.Select(r => new TD_26ViewModel(r)));
                        break;
                    case TypeLoadInvoiceRows.WithoutNaklad:
                        ret.AddRange(
                            rows.Where(_ => _.SFT_IS_NAKLAD != 1).Select(r => new TD_26ViewModel(r)));
                        break;
                    case TypeLoadInvoiceRows.NakladOnly:
                        ret.AddRange(
                            rows.Where(_ => _.SFT_IS_NAKLAD == 1).Select(r => new TD_26ViewModel(r)));
                        break;
                }
            }

            return ret;
        }

        public override SD_26ViewModel Save(SD_26ViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override bool IsChecked(SD_26ViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override SD_26ViewModel New()
        {
            throw new NotImplementedException();
        }

        public override SD_26ViewModel NewFullCopy(SD_26ViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override SD_26ViewModel NewRequisity(SD_26ViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override void Delete(SD_26ViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override SD_26ViewModel Load()
        {
            throw new NotImplementedException();
        }

        public override SD_26ViewModel Load(decimal docCode)
        {
            throw new NotImplementedException();
        }

        public override SD_26ViewModel Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public override void Delete(decimal docCode)
        {
            throw new NotImplementedException();
        }

        public override void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public SD_26ViewModel CreateNew()
        {
            throw new NotImplementedException();
        }

        public SD_26ViewModel CreateNew(SD_26ViewModel doc,
            DocumentCopyType copyType = DocumentCopyType.Empty)
        {
            throw new NotImplementedException();
        }

        public SD_26ViewModel CreateNew(decimal docCode,
            DocumentCopyType copyType = DocumentCopyType.Empty)
        {
            throw new NotImplementedException();
        }

        public SD_26ViewModel CreateNew(Guid id, DocumentCopyType copyType = DocumentCopyType.Empty)
        {
            throw new NotImplementedException();
        }
    }
}