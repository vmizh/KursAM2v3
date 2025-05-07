using System;
using System.Collections.Generic;
using KursDomain.Documents.Cash;
using KursRepositories.Repositories.Base;

namespace KursRepositories.Repositories.CashRepositories.CashBook
{
    public interface ICashBookRepository : IBaseRepository
    {
        void SetRemains(decimal cashDC, IEnumerable<CashStartRemains> remains);
        IEnumerable<DateTime> GetOperationDates(decimal cashDC);
    }
}
