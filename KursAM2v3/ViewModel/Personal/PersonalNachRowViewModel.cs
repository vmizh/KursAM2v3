using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Core.ViewModel.Base.Column;
using Core.WindowsManager;
using KursDomain.References;

namespace KursAM2.ViewModel.Personal
{
    public class PersonalNachRowViewModel : RSViewModelBase, IViewModel<NachForEmployeeRowModelOld>
    {
        public PersonalNachRowViewModel()
        {
            TableViewInfo = new GridTableViewInfo();
            TableViewInfo.Generate(typeof(NachForEmployeeRowModelOld));
            Source = new ObservableCollection<NachForEmployeeRowModelOld>();
            SourceAll = new ObservableCollection<NachForEmployeeRowModelOld>();
            DeletedItems = new List<NachForEmployeeRowModelOld>();
        }

        public GridTableViewInfo TableViewInfo { get; set; }

        private decimal CalcSummaWithRate(decimal dcDest, decimal dcSrc, decimal summa, decimal rate)
        {
            if (dcDest == dcSrc)
                return summa;
            if (rate == 0)
                return -1;
            if (dcDest == CurrencyCode.USD && dcSrc == CurrencyCode.EUR)
                return summa * rate;
            if (dcDest == CurrencyCode.EUR && dcSrc == CurrencyCode.USD)
                return summa * rate;
            return summa / rate;
        }

        public void Load()
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                try
                {
                    SourceAll.Clear();
                    // ReSharper disable once UnusedVariable
                    var crsList =
                        ent.SD_301.Select(
                                s => new Currency { DocCode = s.DOC_CODE, Name = s.CRS_SHORTNAME })
                            .ToList();
                    var persRight =
                        ent.EMP_USER_RIGHTS.Where(
                                t => t.USER.ToUpper() == GlobalOptions.UserInfo.Name.ToUpper())
                            .Select(d => d.EMP_DC)
                            .ToList();
                    var pers = MainReferences.Employees.Values.Where(p => persRight.Any(t => t == p.DocCode)).ToList();
                    foreach (var nach in ent.EMP_PR_ROWS.Include("EMP_PR_DOC")
                                 .Include("SD_301")
                                 .Where(d => d.EMP_PR_DOC.IS_TEMPLATE == 0)
                                 .Select(s => new NachForEmployeeRowModelOld
                                 {
                                     Employee = MainReferences.Employees[s.EMP_DC],
                                     Crs = new Currency { DocCode = s.SD_301.DOC_CODE, Name = s.SD_301.CRS_SHORTNAME },
                                     DocDate = s.EMP_PR_DOC.Date,
                                     Notes = s.NOTES,
                                     Summa = s.SUMMA,
                                     PlatDocNotes = s.NOTES,
                                     PlatDocName = "Платежная ведомость",
                                     PayType =
                                         new PayrollType { DocCode = s.PR_TYPE_DC, Name = s.EMP_PAYROLL_TYPE.Name }
                                 })
                                 .Where(nach => persRight.Any(t => t == nach.Employee.DocCode)))
                    {
                        nach.Employee = pers.FirstOrDefault(t => t.DocCode == nach.Employee.DocCode);
                        SourceAll.Add(nach);
                    }

                    foreach (var p in ent.SD_34.Where(t => t.NCODE == 100))
                    {
                        var s = new NachForEmployeeRowModelOld
                        {
                            Employee =
                                MainReferences.Employees.Values.SingleOrDefault(_ => _.TabelNumber == p.TABELNUMBER),
                            Crs = MainReferences.Currencies[p.SD_301.DOC_CODE],
                            DocDate = p.DATE_ORD.Value,
                            PlatSumma = p.SUMM_ORD ?? 0,
                            Rate = p.CRS_KOEF.Value,
                            PlatSummaEmp =
                                CalcSummaWithRate(
                                    // ReSharper disable once PossibleNullReferenceException
                                    MainReferences.Employees.Values.SingleOrDefault(
                                            _ => _.TabelNumber == p.TABELNUMBER)
                                        .DocCode, p.CRS_DC.Value,
                                    p.SUMM_ORD ?? 0,
                                    p.CRS_KOEF.Value),
                            PlatDocNotes = p.NOTES_ORD,
                            PlatDocName = $"Расходный кассовый ордер № {p.NUM_ORD}"
                        };
                        if (persRight.Any(t => t == s.Employee.DocCode))
                            SourceAll.Add(s);
                    }

                    var maxDate = DateTime.Today;
                    var minDate = DateTime.Today;
                    if (SourceAll.Count > 0)
                    {
                        maxDate = SourceAll.Max(t => t.DocDate);
                        minDate = SourceAll.Min(t => t.DocDate);
                    }

                    CurrencyRate.LoadCBrates(minDate, maxDate);
                    foreach (var row in SourceAll)
                    {
                        row.Rate = CurrencyRate.GetSummaRate(row.Crs.DocCode, row.Employee.Currency.DocCode,
                            row.DocDate,
                            1);
                        row.SummaEmp = CurrencyRate.GetSummaRate(row.Crs.DocCode, row.Employee.Currency.DocCode,
                            row.DocDate,
                            row.Summa);
                        if (row.DocDate <= new DateTime(2013, 7, 31)) continue;
                        row.Rate = CurrencyRate.GetRate(row.Crs.DocCode, row.Employee.Currency.DocCode, row.DocDate);
                        if (row.Employee.TabelNumber == 38 && row.PlatSumma == 100000)
                            row.PlatSummaEmp = CurrencyRate.GetSummaRate(row.Crs.DocCode,
                                row.Employee.Currency.DocCode, row.DocDate, row.PlatSumma);
                        row.PlatSummaEmp = CurrencyRate.GetSummaRate(row.Crs.DocCode, row.Employee.Currency.DocCode,
                            row.DocDate, row.PlatSumma);
                    }
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(null, ex);
                }
            }
        }

        #region IViewModel<NachForEmployeeRowModelOld> Members

        public ObservableCollection<NachForEmployeeRowModelOld> Source { get; set; }
        public ObservableCollection<NachForEmployeeRowModelOld> SourceAll { get; set; }
        public List<NachForEmployeeRowModelOld> DeletedItems { get; set; }

        #endregion
    }
}
