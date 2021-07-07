using System;
using System.Collections.Generic;
using System.Linq;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.CommonReferences.Kontragent;
using Data;
using Data.Repository;

namespace Core
{
    public interface IMainReferenceRepository
    {
        #region Currency

        List<Currency> CurrencyList { set; get; }
        public Currency GetCurrency(decimal dc);
        public Currency GetCurrency(Guid id);
        public List<Currency> GetAllCurrency();

        #endregion

        #region Kontragent

        List<Kontragent> KontragentList { set; get; }
        List<KontragentGroup> KontragentGroupList { set; get; }
        public Kontragent GetKontragent(decimal dc);
        public Kontragent GetKontragent(Guid id);
        public List<Kontragent> GetAllKontragent();
        public KontragentGroups GetKontragentGroup(decimal dc);
        public KontragentGroup GetKontragentGroup(Guid id);
        public List<KontragentGroup> GetAllKontragentGroup();

        #endregion
    }

    public class MainReferences2 : IMainReferenceRepository
    {
        public readonly UnitOfWork<ALFAMEDIAEntities> UnitOfWork =
            new(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));

        public IMainReferenceRepository MainReferenceRepository;

        #region Currency

        public List<Currency> CurrencyList { get; set; } = new();
        public Currency GetCurrency(decimal dc)
        {
            var c = CurrencyList.SingleOrDefault(_ => _.DOC_CODE == dc);
            if (c != null) return c;
            var d = UnitOfWork.Context.SD_301.FirstOrDefault(_ => _.DOC_CODE == dc);
            if (d == null) return null;
            var newItem = new Currency(d);
            CurrencyList.Add(newItem);
            return newItem;
        }
        public Currency GetCurrency(Guid id)
        {
            var c = CurrencyList.SingleOrDefault(_ => _.Id == id);
            if (c != null) return c;
            var d = UnitOfWork.Context.SD_301.FirstOrDefault(_ => _.Id == id);
            if (d == null) return null;
            var newItem = new Currency(d);
            CurrencyList.Add(newItem);
            return newItem;
        }
        public List<Currency> GetAllCurrency()
        {
            var cnt = UnitOfWork.Context.SD_301.Count();
            if (cnt == CurrencyList.Count) return CurrencyList;
            foreach (var d in UnitOfWork.Context.SD_301)
            {
                if (CurrencyList.Any(_ => _.Id == d.Id)) continue;
                CurrencyList.Add(new Currency(d));
            }

            return CurrencyList;
        }
        
        #endregion

        #region Kontragent

        public List<Kontragent> KontragentList { get; set; }
        public List<KontragentGroup> KontragentGroupList { get; set; }

        public Kontragent GetKontragent(decimal dc)
        {
            var c = KontragentList.SingleOrDefault(_ => _.DOC_CODE == dc);
            if (c != null) return c;
            var d = UnitOfWork.Context.SD_43.FirstOrDefault(_ => _.DOC_CODE == dc);
            if (d == null) return null;
            var newItem = new Kontragent(d);
            KontragentList.Add(newItem);
            return newItem;
        }
        public Kontragent GetKontragent(Guid id)
        {
            var c = KontragentList.SingleOrDefault(_ => _.Id == id);
            if (c != null) return c;
            var d = UnitOfWork.Context.SD_43.FirstOrDefault(_ => _.Id == id);
            if (d == null) return null;
            var newItem = new Kontragent(d);
            KontragentList.Add(newItem);
            return newItem;
        }
        public List<Kontragent> GetAllKontragent()
        {
            var cnt = UnitOfWork.Context.SD_43.Count();
            if (cnt == CurrencyList.Count) return KontragentList;
            foreach (var d in UnitOfWork.Context.SD_43)
            {
                if (KontragentList.Any(_ => _.Id == d.Id)) continue;
                KontragentList.Add(new Kontragent(d));
            }
            return KontragentList;
        }

        public KontragentGroups GetKontragentGroup(decimal dc)
        {
            throw new NotImplementedException();
        }

        public KontragentGroup GetKontragentGroup(Guid id)
        {
            throw new NotImplementedException();
        }

        public List<KontragentGroup> GetAllKontragentGroup()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}