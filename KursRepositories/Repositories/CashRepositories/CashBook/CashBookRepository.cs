using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using KursDomain.Documents.Cash;
using KursDomain.WindowsManager.WindowsManager;
using KursRepositories.Repositories.Base;

namespace KursRepositories.Repositories.CashRepositories.CashBook
{
    public class CashBookRepository : BaseRepository, ICashBookRepository
    {
        public void SetRemains(decimal cashDC, IEnumerable<CashStartRemains> remains)
        {
            using (var transaction = Context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var d in remains)
                    {
                        if (d.Currency == null)
                            continue;
                        var old = Context.TD_22.FirstOrDefault(_ =>
                            _.DOC_CODE == d.DOC_CODE && _.CRS_DC == d.Currency.DocCode
                        );
                        if (old == null)
                        {
                            var newCode = Context.TD_22.Any(_ => _.DOC_CODE == d.DOC_CODE)
                                ? (int)Context.TD_22.Max(_ => _.CODE) + 1
                                : 1;
                            var newItem = new TD_22
                            {
                                DOC_CODE = cashDC,
                                CODE = newCode,
                                CRS_DC = d.CRS_DC,
                                DATE_START = d.DATE_START,
                                SUMMA_START = d.SUMMA_START,
                            };
                            Context.TD_22.Add(newItem);
                        }
                        else
                        {
                            old.DATE_START = d.DATE_START;
                            old.SUMMA_START = d.SUMMA_START;
                        }

                        Context.SaveChanges();
                        transaction.Commit();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    WindowManager.ShowError(ex);
                }
            }
        }

        public void SetRemains(decimal cashDC, IEnumerable remains)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DateTime> GetOperationDates(decimal cashDC)
        {
            var dates = new List<DateTime>();
            var d1 = Context.TD_22.Where(_ => _.DOC_CODE == cashDC).Select(_ => _.DATE_START);
            var dIn = Context
                .SD_33.Where(_ => _.CA_DC == cashDC)
                .Select(_ => _.DATE_ORD)
                .Distinct();
            var dOut = Context
                .SD_34.Where(_ => _.CA_DC == cashDC)
                .Select(_ => _.DATE_ORD)
                .Distinct();
            var dCrs = Context
                .SD_251.Where(_ => _.CH_CASH_DC == cashDC)
                .Select(_ => _.CH_DATE)
                .Distinct();
            foreach (var i in d1)
                dates.Add(i);
            foreach (var i in dIn)
                if (dates.All(_ => _ != i))
                    // ReSharper disable once PossibleInvalidOperationException
                    dates.Add((DateTime)i);
            foreach (var i in dOut)
                if (dates.All(_ => _ != i))
                    // ReSharper disable once PossibleInvalidOperationException
                    dates.Add((DateTime)i);
            foreach (var i in dCrs)
                if (dates.All(_ => _ != i))
                    dates.Add(i);
            return dates;
        }
    }
}
