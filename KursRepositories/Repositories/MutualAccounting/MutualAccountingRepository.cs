using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Core.ViewModel.Base;
using Data;
using Helper;
using KursDomain;
using KursDomain.Documents.Vzaimozachet;
using KursDomain.ICommon;
using KursDomain.WindowsManager.WindowsManager;
using KursRepositories.Repositories.Base;
using StackExchange.Redis;

namespace KursRepositories.Repositories.MutualAccounting
{
    public class MutualAccountingRepository : BaseRepository, IMutualAccountingRepository
    {
        public MutualAccountingRepository(ALFAMEDIAEntities context)
        {
            Context = context;
            redis = ConnectionMultiplexer.Connect(
                ConfigurationManager.AppSettings["redis.connection"]
            );
            mySubscriber = redis.GetSubscriber();
        }

        #region Fields

        private readonly decimal maxOldDC;
        public string CheckedInfo { set; get; }

        #endregion

        #region Methods

        public SD_110 Get(decimal dc)
        {
            return Context
                .SD_110.Include(_ => _.TD_110)
                .Include(_ => _.SD_111)
                .Include("TD_110.SD_26")
                .Include("TD_110.SD_301")
                .Include("TD_110.SD_3011")
                .Include("TD_110.SD_303")
                .Include("TD_110.SD_43")
                .Include("TD_110.SD_77")
                .Include("TD_110.SD_84")
                .AsNoTracking()
                .FirstOrDefault(_ => _.DOC_CODE == dc);
        }

        public IEnumerable<SD_110> GetForDates(DateTime dateStart, DateTime dateEnd, bool isConvert)
        {
            var data = Context
                .SD_110.Include(_ => _.TD_110)
                .Include(_ => _.SD_111)
                .Include("TD_110.SD_26")
                .Include("TD_110.SD_301")
                .Include("TD_110.SD_3011")
                .Include("TD_110.SD_303")
                .Include("TD_110.SD_43")
                .Include("TD_110.SD_77")
                .Include("TD_110.SD_84")
                .Where(_ =>
                    _.VZ_DATE >= dateStart
                    && _.VZ_DATE <= dateEnd
                    && _.SD_111.IsCurrencyConvert == isConvert
                )
                .AsNoTracking()
                .ToList();
            return data;
        }

        public IEnumerable<SD_110> GetForDates(DateTime dateStart, DateTime dateEnd)
        {
            var data = Context
                .SD_110.Include(_ => _.TD_110)
                .Include(_ => _.SD_111)
                .Include("TD_110.SD_26")
                .Include("TD_110.SD_301")
                .Include("TD_110.SD_3011")
                .Include("TD_110.SD_303")
                .Include("TD_110.SD_43")
                .Include("TD_110.SD_77")
                .Include("TD_110.SD_84")
                .Where(_ => _.VZ_DATE >= dateStart && _.VZ_DATE <= dateEnd)
                .AsNoTracking()
                .ToList();
            return data;
        }

        public SD_110ViewModel Load(decimal dc)
        {
            var ent = Context
                .SD_110.Include(_ => _.TD_110)
                .Include(_ => _.SD_111)
                .Include("TD_110.SD_26")
                .Include("TD_110.SD_301")
                .Include("TD_110.SD_3011")
                .Include("TD_110.SD_303")
                .Include("TD_110.SD_43")
                .Include("TD_110.SD_77")
                .Include("TD_110.SD_84")
                .AsNoTracking()
                .FirstOrDefault(_ => _.DOC_CODE == dc);
            if (ent == null)
                return new SD_110ViewModel();
            return new SD_110ViewModel(ent) { State = RowStatus.NotEdited };
        }

        private decimal GetMaxOldDC()
        {
            var d = GlobalOptions.SystemProfile.Profile.FirstOrDefault(_ =>
                _.SECTION == "MUTUAL_ACCOUNTING" && _.ITEM == "MAX_OLD_DC"
            );
            return d != null ? Convert.ToDecimal(d.ITEM_VALUE) : 0;
        }

        public bool CheckDocumentForOld(decimal dc)
        {
            return dc <= maxOldDC;
        }

        public SD_110ViewModel Save(SD_110ViewModel doc)
        {
            using (var tran = Context.Database.BeginTransaction())
            {
                try
                {
                    var isConvert = GlobalOptions
                        .ReferencesCache.GetMutualSettlementType(doc.VZ_TYPE_DC)
                        .IsCurrencyConvert;
                    switch (doc.State)
                    {
                        case RowStatus.NewRow:
                            var newDC = Context.SD_110.Any()
                                ? Context.SD_110.Max(_ => _.DOC_CODE) + 1
                                : 11100000001;
                            doc.VZ_NUM = Context.SD_110.Any()
                                ? Context.SD_110.Max(_ => _.VZ_NUM) + 1
                                : 1;
                            doc.DocCode = newDC;
                            Context.SD_110.Add(
                                new SD_110
                                {
                                    DOC_CODE = newDC,
                                    VZ_NUM = doc.Entity.VZ_NUM,
                                    VZ_DATE = doc.Entity.VZ_DATE,
                                    VZ_TYPE_DC = doc.Entity.VZ_TYPE_DC,
                                    VZ_PRIBIL_UCH_CRS_SUM = doc.Entity.VZ_PRIBIL_UCH_CRS_SUM,
                                    VZ_NOTES = doc.Entity.VZ_NOTES,
                                    CREATOR = doc.Entity.CREATOR,
                                    VZ_LEFT_UCH_CRS_SUM = doc.Entity.VZ_LEFT_UCH_CRS_SUM,
                                    VZ_RIGHT_UCH_CRS_SUM = doc.Entity.VZ_RIGHT_UCH_CRS_SUM,
                                    CurrencyFromDC = doc.Entity.CurrencyFromDC,
                                    CurrencyToDC = doc.Entity.CurrencyToDC,
                                }
                            );
                            foreach (var r in doc.Rows)
                            {
                                r.DocCode = newDC;
                                Context.TD_110.Add(
                                    new TD_110
                                    {
                                        DOC_CODE = newDC,
                                        CODE = r.Entity.CODE,
                                        VZT_CRS_SUMMA = r.Entity.VZT_CRS_SUMMA,
                                        VZT_CRS_DC = r.Entity.VZT_CRS_DC,
                                        VZT_SPOST_DC = r.Entity.VZT_SPOST_DC,
                                        VZT_SFACT_DC = r.Entity.VZT_SFACT_DC,
                                        VZT_DOC_DATE = isConvert
                                            ? doc.Entity.VZ_DATE
                                            : r.Entity.VZT_DOC_DATE,
                                        VZT_DOC_NUM = r.Entity.VZT_DOC_NUM,
                                        VZT_DOC_NOTES = r.Entity.VZT_DOC_NOTES,
                                        VZT_KONTR_DC = r.Entity.VZT_KONTR_DC,
                                        VZT_CRS_POGASHENO = r.Entity.VZT_CRS_POGASHENO,
                                        VZT_UCH_CRS_POGASHENO = r.Entity.VZT_UCH_CRS_POGASHENO,
                                        VZT_UCH_CRS_RATE = r.Entity.VZT_UCH_CRS_RATE,
                                        VZT_VZAIMOR_TYPE_DC = r.Entity.VZT_VZAIMOR_TYPE_DC,
                                        VZT_1MYDOLZH_0NAMDOLZH = r.Entity.VZT_1MYDOLZH_0NAMDOLZH,
                                        VZT_KONTR_CRS_DC = r.Entity.VZT_KONTR_CRS_DC,
                                        VZT_KONTR_CRS_RATE = r.Entity.VZT_KONTR_CRS_RATE,
                                        VZT_KONTR_CRS_SUMMA = r.Entity.VZT_KONTR_CRS_SUMMA,
                                        VZT_SHPZ_DC = r.Entity.VZT_SHPZ_DC,
                                    }
                                );
                            }

                            break;
                        case RowStatus.Edited:
                        case RowStatus.NotEdited:
                            var entity = Context.SD_110.FirstOrDefault(_ =>
                                _.DOC_CODE == doc.DocCode
                            );
                            if (entity == null)
                                return null;
                            Context.Entry(entity).CurrentValues.SetValues(doc.Entity);
                            foreach (var r in doc.DeletedRows)
                            {
                                var delRow = Context.TD_110.FirstOrDefault(_ =>
                                    _.DOC_CODE == r.DocCode && _.CODE == r.Code
                                );
                                if (delRow == null)
                                    continue;
                                Context.TD_110.Remove(delRow);
                                var prows = Context
                                    .ProviderInvoicePay.Where(_ =>
                                        _.VZDC == r.DocCode && _.VZCode == r.Code
                                    )
                                    .ToList();
                                if (prows.Count <= 0)
                                    continue;
                                foreach (var p in prows)
                                    Context.ProviderInvoicePay.Remove(p);
                            }

                            foreach (var r in doc.Rows)
                            {
                                r.VZT_DOC_DATE = isConvert
                                    ? doc.Entity.VZ_DATE
                                    : r.Entity.VZT_DOC_DATE;
                                switch (r.State)
                                {
                                    case RowStatus.Edited:
                                    case RowStatus.NotEdited:
                                        var row = Context.TD_110.FirstOrDefault(_ =>
                                            _.DOC_CODE == doc.DocCode && _.CODE == r.Code
                                        );
                                        if (row == null)
                                            break;
                                        row.VZT_CRS_SUMMA = r.Entity.VZT_CRS_SUMMA;
                                        row.VZT_CRS_DC = r.Entity.VZT_CRS_DC;
                                        row.VZT_SPOST_DC = r.Entity.VZT_SPOST_DC;
                                        row.VZT_SFACT_DC = r.Entity.VZT_SFACT_DC;
                                        row.VZT_DOC_DATE = isConvert
                                            ? doc.Entity.VZ_DATE
                                            : r.Entity.VZT_DOC_DATE;
                                        row.VZT_DOC_NUM = r.Entity.VZT_DOC_NUM;
                                        row.VZT_DOC_NOTES = r.Entity.VZT_DOC_NOTES;
                                        row.VZT_KONTR_DC = r.Entity.VZT_KONTR_DC;
                                        row.VZT_CRS_POGASHENO = r.Entity.VZT_CRS_POGASHENO;
                                        row.VZT_UCH_CRS_POGASHENO = r.Entity.VZT_UCH_CRS_POGASHENO;
                                        row.VZT_UCH_CRS_RATE = r.Entity.VZT_UCH_CRS_RATE;
                                        row.VZT_VZAIMOR_TYPE_DC = r.Entity.VZT_VZAIMOR_TYPE_DC;
                                        row.VZT_1MYDOLZH_0NAMDOLZH =
                                            r.Entity.VZT_1MYDOLZH_0NAMDOLZH;
                                        row.VZT_KONTR_CRS_DC = r.Entity.VZT_KONTR_CRS_DC;
                                        row.VZT_KONTR_CRS_RATE = r.Entity.VZT_KONTR_CRS_RATE;
                                        row.VZT_KONTR_CRS_SUMMA = r.Entity.VZT_KONTR_CRS_SUMMA;
                                        row.VZT_SHPZ_DC = r.Entity.VZT_SHPZ_DC;
                                        break;
                                    case RowStatus.NewRow:
                                        Context.TD_110.Add(
                                            new TD_110
                                            {
                                                DOC_CODE = r.DocCode,
                                                CODE = r.Code,
                                                VZT_CRS_SUMMA = r.Entity.VZT_CRS_SUMMA,
                                                VZT_CRS_DC = r.Entity.VZT_CRS_DC,
                                                VZT_SPOST_DC = r.Entity.VZT_SPOST_DC,
                                                VZT_SFACT_DC = r.Entity.VZT_SFACT_DC,
                                                VZT_DOC_DATE = isConvert
                                                    ? doc.Entity.VZ_DATE
                                                    : r.Entity.VZT_DOC_DATE,
                                                VZT_DOC_NUM = r.Entity.VZT_DOC_NUM,
                                                VZT_DOC_NOTES = r.Entity.VZT_DOC_NOTES,
                                                VZT_KONTR_DC = r.Entity.VZT_KONTR_DC,
                                                VZT_CRS_POGASHENO = r.Entity.VZT_CRS_POGASHENO,
                                                VZT_UCH_CRS_POGASHENO =
                                                    r.Entity.VZT_UCH_CRS_POGASHENO,
                                                VZT_UCH_CRS_RATE = r.Entity.VZT_UCH_CRS_RATE,
                                                VZT_VZAIMOR_TYPE_DC = r.Entity.VZT_VZAIMOR_TYPE_DC,
                                                VZT_1MYDOLZH_0NAMDOLZH =
                                                    r.Entity.VZT_1MYDOLZH_0NAMDOLZH,
                                                VZT_KONTR_CRS_DC = r.Entity.VZT_KONTR_CRS_DC,
                                                VZT_KONTR_CRS_RATE = r.Entity.VZT_KONTR_CRS_RATE,
                                                VZT_KONTR_CRS_SUMMA = r.Entity.VZT_KONTR_CRS_SUMMA,
                                                VZT_SHPZ_DC = r.Entity.VZT_SHPZ_DC,
                                            }
                                        );
                                        break;
                                }
                            }

                            break;
                    }

                    foreach (var r in doc.Rows)
                        if (r.SFProvider == null)
                        {
                            var prow = Context.ProviderInvoicePay.FirstOrDefault(_ =>
                                _.VZDC == r.DocCode && _.VZCode == r.Code
                            );
                            if (prow != null)
                            {
                                Context.ProviderInvoicePay.Remove(prow);
                                Context.Database.ExecuteSqlCommand(
                                    $"EXEC [dbo].[GenerateSFProviderCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(prow.DocDC)}"
                                );
                            }
                        }
                        else
                        {
                            var prow = Context.ProviderInvoicePay.FirstOrDefault(_ =>
                                _.VZDC == r.DocCode && _.VZCode == r.Code
                            );
                            if (prow != null)
                            {
                                // ReSharper disable once PossibleInvalidOperationException
                                prow.Summa = (decimal)r.VZT_CRS_SUMMA;
                            }
                            else
                            {
                                var newItem = new ProviderInvoicePay
                                {
                                    Id = Guid.NewGuid(),
                                    // ReSharper disable once PossibleInvalidOperationException
                                    Summa = (decimal)r.VZT_CRS_SUMMA,
                                    DocDC = r.SFProvider.DocCode,
                                    Rate = 1,
                                    VZDC = r.DocCode,
                                    VZCode = r.Code,
                                };
                                Context.ProviderInvoicePay.Add(newItem);
                            }
                        }

                    Context.SaveChanges();
                    foreach (var row in doc.Rows)
                    {
                        if (row.VZT_SPOST_DC != null)
                            Context.Database.ExecuteSqlCommand(
                                $"EXEC [dbo].[GenerateSFProviderCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(row.VZT_SPOST_DC)}"
                            );
                        if (row.VZT_SFACT_DC != null)
                            Context.Database.ExecuteSqlCommand(
                                $"EXEC [dbo].[GenerateSFClientCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(row.VZT_SFACT_DC)}"
                            );
                    }

                    foreach (var row in doc.DeletedRows)
                    {
                        if (row.VZT_SPOST_DC != null)
                            Context.Database.ExecuteSqlCommand(
                                $"EXEC [dbo].[GenerateSFProviderCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(row.VZT_SPOST_DC)}"
                            );
                        if (row.VZT_SFACT_DC != null)
                            Context.Database.ExecuteSqlCommand(
                                $"EXEC [dbo].[GenerateSFClientCash] @SFDocDC = {CustomFormat.DecimalToSqlDecimal(row.VZT_SFACT_DC)}"
                            );
                    }

                    doc.myState = RowStatus.NotEdited;
                    foreach (var r in doc.Rows)
                    {
                        r.myState = RowStatus.NotEdited;
                        r.RaisePropertyChanged("State");
                    }

                    tran.Commit();
                    doc.RaisePropertyChanged("State");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    WindowManager.ShowError(ex);
                    return null;
                }
            }

            return doc;
        }

        public bool IsChecked(SD_110ViewModel doc)
        {
            CheckedInfo = null;
            if (doc == null)
            {
                CheckedInfo = "Документ не найден";
                return false;
            }

            var info = new StringBuilder();
            var ret = true;
            if (doc.VZ_DATE == DateTime.MinValue)
            {
                info.Append("Дата не установлена.\n");
                ret = false;
            }

            if (doc.CreditorCurrency == null)
            {
                info.Append("Не установлена валюта кредитора.\n");
                ret = false;
            }

            if (doc.DebitorCurrency == null)
            {
                info.Append("Не установлена валюта дебитора.\n");
                ret = false;
            }

            if (doc.VZ_TYPE_DC < 1111000001)
            {
                info.Append("Не установлен тип зачета.\n");
                ret = false;
            }

            if (string.IsNullOrWhiteSpace(doc.CREATOR))
            {
                info.Append("Не Указан создатель документа.\n");
                ret = false;
            }

            if (doc.Rows != null && doc.Rows.Count > 0)
                foreach (var r in doc.Rows)
                {
                    if (r.DocCode != doc.DocCode)
                    {
                        info.Append("Doc-Code неправильный.\n");
                        ret = false;
                    }

                    var res = IsRowChecked(r);
                    if (res.Item1)
                        continue;
                    info.Append(res.Item2);
                    ret = false;
                }

            CheckedInfo = info.ToString();
            return ret;
        }

        public SD_110ViewModel New()
        {
            var ret = new SD_110ViewModel
            {
                DocCode = -1,
                VZ_NUM = -1,
                VZ_DATE = DateTime.Today,
                CREATOR = GlobalOptions.UserInfo.NickName,
                Rows = new ObservableCollection<TD_110ViewModel>(),
                IsOld = false,
                State = RowStatus.NewRow,
            };
            return ret;
        }

        public SD_110ViewModel NewFullCopy(SD_110ViewModel doc)
        {
            var newDoc = NewRequisity(doc);
            foreach (var r in doc.Rows)
            {
                var newRow = new TD_110ViewModel(r.Entity)
                {
                    DocCode = -1,
                    Code = r.Code,
                    VZT_DOC_DATE = DateTime.Today,
                    State = RowStatus.NewRow,
                };
                newDoc.Rows.Add(newRow);
            }

            newDoc.State = RowStatus.NewRow;
            return newDoc;
        }

        public SD_110ViewModel NewRequisity(SD_110ViewModel doc)
        {
            var newDoc = New();
            newDoc.DocCode = -1;
            newDoc.VZ_NUM = -1;
            newDoc.CREATOR = GlobalOptions.UserInfo.NickName;
            newDoc.MutualAccountingOldType = doc.MutualAccountingOldType;
            newDoc.DebitorCurrency = doc.DebitorCurrency;
            newDoc.CreditorCurrency = doc.CreditorCurrency;
            return newDoc;
        }

        public void Delete(SD_110ViewModel doc)
        {
            try
            {
                var d = Context
                    .SD_110.Include(_ => _.TD_110)
                    .FirstOrDefault(_ => _.DOC_CODE == doc.DocCode);
                var sfClientDCList = new List<decimal>();
                var sfProviderDCList = new List<decimal>();
                var items = new List<(decimal, int)>();
                if (d == null)
                    return;
                foreach (var r in d.TD_110)
                {
                    items.Add((r.DOC_CODE, r.CODE));
                    if (r.VZT_SFACT_DC != null)
                        sfClientDCList.Add(r.VZT_SFACT_DC.Value);

                    if (r.VZT_SPOST_DC != null)
                        sfProviderDCList.Add(r.VZT_SPOST_DC.Value);
                }

                foreach (
                    var pd in items
                        .Select(item =>
                            Context.ProviderInvoicePay.FirstOrDefault(_ =>
                                _.VZDC == item.Item1 && _.VZCode == item.Item2
                            )
                        )
                        .Where(pd => pd != null)
                )
                    Context.ProviderInvoicePay.Remove(pd);
                Context.SD_110.Remove(d);
                Context.SaveChanges();
                if (sfClientDCList.Count > 0)
                    foreach (var sfClientDC in sfClientDCList)
                        Context.Database.ExecuteSqlCommand(
                            $"EXEC dbo.GenerateSFClientCash {CustomFormat.DecimalToSqlDecimal(sfClientDC)}"
                        );

                if (sfProviderDCList.Count > 0)
                    foreach (var sfProviderDC in sfProviderDCList)
                        Context.Database.ExecuteSqlCommand(
                            $"EXEC dbo.GenerateSFProviderCash {CustomFormat.DecimalToSqlDecimal(sfProviderDC)}"
                        );

                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public void Delete(decimal dc)
        {
            try
            {
                var doc = Context
                    .SD_110.Include(_ => _.TD_110)
                    .FirstOrDefault(_ => _.DOC_CODE == dc);
                if (doc != null)
                {
                    Context.SD_110.Remove(doc);
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public TD_110ViewModel AddRow(
            ObservableCollection<TD_110ViewModel> rows,
            TD_110ViewModel row,
            // ReSharper disable once OptionalParameterHierarchyMismatch
            short dolg = 0
        )
        {
            if (rows == null)
                return null;
            if (row == null)
                row = new TD_110ViewModel
                {
                    VZT_DOC_DATE = DateTime.Today,
                    Code = rows.Count + 1,
                    VZT_DOC_NUM = (rows.Count + 1).ToString(),
                    VZT_1MYDOLZH_0NAMDOLZH = dolg,
                    VZT_CRS_POGASHENO = 0,
                    VZT_UCH_CRS_POGASHENO = 0,
                    VZT_CRS_SUMMA = 0,
                    VZT_KONTR_CRS_RATE = 1,
                    VZT_KONTR_CRS_SUMMA = 0,
                    VZT_UCH_CRS_RATE = 1,
                    State = RowStatus.NewRow,
                };
            else
                row = new TD_110ViewModel
                {
                    VZT_DOC_DATE = row.VZT_DOC_DATE,
                    Code = rows.Count + 1,
                    VZT_DOC_NUM = (rows.Count + 1).ToString(),
                    VZT_1MYDOLZH_0NAMDOLZH = row.VZT_1MYDOLZH_0NAMDOLZH,
                    VZT_CRS_POGASHENO = row.VZT_CRS_POGASHENO,
                    VZT_UCH_CRS_POGASHENO = row.VZT_UCH_CRS_POGASHENO,
                    VZT_CRS_SUMMA = row.VZT_CRS_SUMMA,
                    VZT_KONTR_CRS_RATE = row.VZT_KONTR_CRS_RATE,
                    VZT_KONTR_CRS_SUMMA = row.VZT_KONTR_CRS_SUMMA,
                    VZT_UCH_CRS_RATE = row.VZT_UCH_CRS_RATE,
                    State = RowStatus.NewRow,
                };
            rows.Add(row);
            return row;
        }

        public TD_110ViewModel DeleteRow(
            ObservableCollection<TD_110ViewModel> rows,
            ObservableCollection<TD_110ViewModel> deletedrows,
            TD_110ViewModel row
        )
        {
            //var drow = Rows.FirstOrDefault(_ => _.Code == row.Code);
            if (row != null)
            {
                if (row.State == RowStatus.NewRow)
                {
                    rows.Remove(row);
                    return row;
                }

                deletedrows.Add(row);
                rows.Remove(row);
                if (row.Parent is RSViewModelBase prnt)
                    prnt.State =
                        prnt.State == RowStatus.NewRow ? RowStatus.NewRow : RowStatus.Edited;
            }

            return row;
        }

        public Tuple<bool, string> IsRowChecked(TD_110ViewModel r)
        {
            var ret = true;
            var info = new StringBuilder();
            if (r.Code < 1)
            {
                info.Append("Код строки не установлен.\n");
                ret = false;
            }

            if (r.VZT_CRS_DC < 3010000001)
            {
                info.Append("Валюта не установлена.\n");
                ret = false;
            }

            if (r.VZT_DOC_DATE == DateTime.MinValue)
            {
                info.Append("Не установлена дата.\n");
                ret = false;
            }

            if (r.VZT_VZAIMOR_TYPE_DC < 10770000001)
            {
                info.Append("Тип взаиморасчета неправильная.\n");
                ret = false;
            }

            if (r.VZT_1MYDOLZH_0NAMDOLZH < 0 || r.VZT_1MYDOLZH_0NAMDOLZH > 1)
            {
                info.Append("Типы операций не правильные.\n");
                ret = false;
            }

            CheckedInfo = "\n" + info;
            return Tuple.Create(ret, info.ToString());
        }

        #endregion
    }
}
