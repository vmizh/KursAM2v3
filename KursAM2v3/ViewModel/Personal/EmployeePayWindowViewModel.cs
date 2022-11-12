using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.View.Personal;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Personal
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class EmployeePayWindowViewModel : RSWindowViewModelBase
    {
        private readonly bool isPersonaItog = true;
        private EmployeePayMainViewModel myCurrentEmploee;
        private EmployeePayDocumentViewModel myCurrentPayDocument;
        private NachEmployeeForPeriod myCurrentPeriod;
        private EmployeePayMainViewModel mySelectEmployee;

        public EmployeePayWindowViewModel()
        {
            Documents = new ObservableCollection<EmployeePayDocumentViewModel>();
            EmployeeMain = new ObservableCollection<EmployeePayMainViewModel>();
            Periods = new ObservableCollection<NachEmployeeForPeriod>();
            DocumentsForEmployee = new ObservableCollection<EmployeePayDocumentViewModel>();
            DocumentsForPeriod = new ObservableCollection<EmployeePayDocumentViewModel>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = GetRightMenu();
        }

        public EmployeePayWindowViewModel(bool isPersItog)
        {
            Documents = new ObservableCollection<EmployeePayDocumentViewModel>();
            EmployeeMain = new ObservableCollection<EmployeePayMainViewModel>();
            Periods = new ObservableCollection<NachEmployeeForPeriod>();
            DocumentsForEmployee = new ObservableCollection<EmployeePayDocumentViewModel>();
            DocumentsForPeriod = new ObservableCollection<EmployeePayDocumentViewModel>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = GetRightMenu();
            isPersonaItog = isPersItog;
        }

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<EmployeePayDocumentViewModel> DocumentsForPeriod { get; set; }
        public ObservableCollection<EmployeePayDocumentViewModel> Documents { set; get; }
        public ObservableCollection<EmployeePayDocumentViewModel> DocumentsForEmployee { set; get; }
        public ObservableCollection<EmployeePayMainViewModel> EmployeeMain { get; }

        public NachEmployeeForPeriod CurrentPeriod
        {
            get => myCurrentPeriod;
            set
            {
                if (myCurrentPeriod == value) return;
                myCurrentPeriod = value;
                UpdateDocumentsForPeriod(myCurrentPeriod);
                RaisePropertyChanged();
            }
        }

        public EmployeePayMainViewModel CurrentEmploee
        {
            get => myCurrentEmploee;
            set
            {
                if (Equals(myCurrentEmploee, value)) return;
                myCurrentEmploee = value;
                UpdatePeriods(myCurrentEmploee);
                RaisePropertyChanged();
            }
        }

        public EmployeePayMainViewModel SelectEmployee
        {
            get => mySelectEmployee;
            set
            {
                if (Equals(mySelectEmployee, value)) return;
                mySelectEmployee = value;
                RaisePropertyChanged();
            }
        }

        public EmployeePayDocumentViewModel CurrentPayDocument
        {
            get => myCurrentPayDocument;
            set
            {
                if (Equals(myCurrentPayDocument, value)) return;
                myCurrentPayDocument = value;
                RaisePropertyChanged();
            }
        }

        public ICommand PayDocumentOpenCommand
        {
            get { return new Command(PayDocumentOpen, _ => IsDocPayVedomost); }
        }

        public bool IsDocPayVedomost => CurrentPayDocument != null && CurrentPayDocument.PayType == "Начисление";

        // ReSharper disable once CollectionNeverQueried.Global
        public ObservableCollection<NachEmployeeForPeriod> Periods { set; get; }

        private void PayDocumentOpen(object obj)
        {
            if (CurrentPayDocument == null) return;
            switch (CurrentPayDocument.PayType)
            {
                case "Начисление":
                    var pr = new PayRollVedomostWindowViewModel(CurrentPayDocument.Id, CurrentEmploee?.Employee);
                    var form = new PayRollVedomost { Owner = Application.Current.MainWindow, DataContext = pr };
                    form.Show();
                    break;
            }
        }

        private ObservableCollection<MenuButtonInfo> GetRightMenu()
        {
            return new ObservableCollection<MenuButtonInfo>
            {
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuRefresh"] as ControlTemplate,
                    ToolTip = "Обновить",
                    Command = RefreshDataCommand
                },
                new MenuButtonInfo
                {
                    Alignment = Dock.Right,
                    HAlignment = HorizontalAlignment.Right,
                    Content = Application.Current.Resources["menuExit"] as ControlTemplate,
                    ToolTip = "Закрыть документ",
                    Command = CloseWindowCommand
                }
            };
        }

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

        // ReSharper disable once FunctionComplexityOverflow
        public void DocumentsLoad(DateTime? dt, bool isAll = false)
        {
            if (dt == null) dt = DateTime.Today;
            Documents.Clear();
            List<decimal> persRight;
            using (var ent = GlobalOptions.GetEntities())
            {
                try
                {
                    if (isAll)
                        persRight = ent.SD_2.Select(_ => _.DOC_CODE).ToList();
                    else
                        persRight =
                            ent.EMP_USER_RIGHTS.Where(
                                    t => t.USER.ToUpper() == GlobalOptions.UserInfo.Name.ToUpper())
                                .Select(d => d.EMP_DC)
                                .ToList();
                    foreach (var nach in ent.EMP_PR_ROWS.Include(_ => _.EMP_PR_DOC)
                                 .Where(
                                     d =>
                                         d.EMP_PR_DOC.IS_TEMPLATE == 0 && persRight.Any(t => t == d.EMP_DC) &&
                                         d.NachDate <= dt)
                                 .ToList())
                    {
                        var newItem = new EmployeePayDocumentViewModel
                        {
                            Employee = GlobalOptions.ReferencesCache.GetEmployee(nach.EMP_DC) as Employee,
                            Crs = GlobalOptions.ReferencesCache.GetCurrency(nach.CRS_DC) as Currency,
                            DocDate = nach.NachDate ?? nach.EMP_PR_DOC.Date,
                            Note = nach.NOTES,
                            Summa = nach.SUMMA,
                            DocSumma = nach.SUMMA,
                            SummaEmp = nach.SUMMA,
                            PlatSummaEmp = 0,
                            PlatDocNotes = nach.NOTES,
                            PlatDocName = "Платежная ведомость",
                            PayType = "Начисление",
                            NachRUB =
                                ((IName)GlobalOptions.ReferencesCache.GetCurrency(nach.CRS_DC)).Name == "RUB" ||
                                ((IName)GlobalOptions.ReferencesCache.GetCurrency(nach.CRS_DC)).Name == "RUR"
                                    ? nach.SUMMA
                                    : 0,
                            NachUSD = ((IName)GlobalOptions.ReferencesCache.GetCurrency(nach.CRS_DC)).Name == "USD" ? nach.SUMMA : 0,
                            NachEUR = ((IName)GlobalOptions.ReferencesCache.GetCurrency(nach.CRS_DC)).Name == "EUR" ? nach.SUMMA : 0,
                            RUB = 0,
                            USD = 0,
                            EUR = 0,
                            Id = nach.ID
                        };
                        Documents.Add(newItem);
                    }

                    foreach (var p in ent.SD_34.AsNoTracking()
                                 .Where(t => t.NCODE == 100 && t.DATE_ORD <= dt)
                                 .Where(p => p.TABELNUMBER != null)
                                 .ToList())
                        try
                        {
                            //if (p.CRS_KOEF == null) continue;
                            var per =
                                GlobalOptions.ReferencesCache.GetEmployee(p.TABELNUMBER) as Employee;
                            if (p.CRS_DC == null) continue;
                            var crs = GlobalOptions.ReferencesCache.GetCurrency(p.CRS_DC) as Currency;
                            if (p.DATE_ORD == null) continue;
                            if (per == null) continue;
                            if (per.TabelNumber == 25 && (p.DOC_CODE == 10340051230 || p.DOC_CODE == 10340051240
                                                          || p.DOC_CODE == 10340051656
                                                          || p.DOC_CODE == 10340051674
                                                      )
                                                      && isPersonaItog)
                                continue;
                            var s = new EmployeePayDocumentViewModel
                            {
                                Employee = per,
                                Crs = crs,
                                DocDate = p.DATE_ORD.Value,
                                DocSumma = p.SUMM_ORD ?? 0,
                                PlatSumma = p.SUMM_ORD ?? 0,
                                PlatSummaEmp =
                                    Math.Round(CalcSummaWithRate(((IDocCode)per.Currency).DocCode, p.CRS_DC.Value,
                                        p.SUMM_ORD ?? 0,
                                        p.CRS_KOEF ?? 1), 2),
                                SummaEmp = 0,
                                PlatDocNotes = p.NOTES_ORD,
                                PlatDocName = $"Расходный кассовый ордер № {p.NUM_ORD}",
                                PayType = "Выплата",
                                NachRUB = 0,
                                NachUSD = 0,
                                NachEUR = 0,
                                // ReSharper disable PossibleInvalidOperationException
                                RUB =
                                    (decimal)
                                    (crs.Name == "RUB" || crs.Name == "RUR"
                                        ? p.SUMM_ORD
                                        : 0),
                                USD = (decimal)(crs.Name == "USD" ? p.SUMM_ORD : 0),
                                EUR = (decimal)(crs.Name == "EUR" ? p.SUMM_ORD : 0)
                                // ReSharper restore PossibleInvalidOperationException
                            };
                            if (persRight.Any(t => t == s.Employee.DocCode))
                                Documents.Add(s);
                        }
                        catch (Exception ex)
                        {
                            WindowManager.ShowError(ex);
                        }

                    var maxDate = dt;
                    var minDate = dt;
                    if (Documents.Count > 0)
                    {
                        maxDate = Documents.Max(t => t.DocDate);
                        minDate = Documents.Min(t => t.DocDate);
                    }

                    CurrencyRate.LoadCBrates((DateTime)minDate, (DateTime)maxDate);
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }
            }
        }

        public void UpdateDocumentsForPeriod(NachEmployeeForPeriod item)
        {
            var tempList = new List<EmployeePayDocumentViewModel>();
            DocumentsForPeriod.Clear();
            if (item == null) return;
            foreach (
                var r in DocumentsForEmployee
                    .Where(_ => _.DocDate >= item.DateStart && _.DocDate <= item.DateEnd))
            {
                var old = DocumentsForPeriod.FirstOrDefault(_ =>
                    _.PayType == r.PayType && _.DocDate == r.DocDate && _.Crs.DocCode == r.Crs.DocCode);
                if (old == null)
                {
                    var newItem = new EmployeePayDocumentViewModel
                    {
                        Employee = r.Employee,
                        Crs = r.Crs,
                        DocDate = r.DocDate,
                        DocSumma = r.DocSumma,
                        PlatSumma = r.PlatSumma,
                        PlatSummaEmp = r.PlatSummaEmp,
                        Summa = r.Summa,
                        SummaEmp = r.SummaEmp,
                        PlatDocNotes = r.PlatDocNotes,
                        PlatDocName = r.PayType == "Выплата"
                            ? "№" + r.PlatDocName.Replace("Расходный кассовый ордер № ", string.Empty)
                            : r.PlatDocName,
                        PayType = r.PayType,
                        NachRUB = r.NachRUB,
                        NachUSD = r.NachUSD,
                        NachEUR = r.NachEUR,
                        // ReSharper disable PossibleInvalidOperationException
                        RUB = r.RUB,
                        USD = r.USD,
                        EUR = r.EUR
                    };
                    tempList.Add(newItem);
                }
                else
                {
                    if (old.PayType == "Выплата")
                    {
                        var s = r.PlatDocName.Replace("Расходный кассовый ордер № ", string.Empty);
                        old.PlatDocName += ", №" + s;
                    }

                    old.DocSumma += r.DocSumma;
                    old.NachEUR += r.NachEUR;
                    old.NachRUB += r.NachRUB;
                    old.NachUSD += r.NachUSD;
                    old.EUR += r.EUR;
                    old.USD += r.USD;
                    old.RUB += r.RUB;
                    old.PlatSumma += r.PlatSumma;
                    old.PlatSummaEmp += r.PlatSummaEmp;
                    old.Summa += r.Summa;
                    old.SummaEmp += r.SummaEmp;
                }
            }

            foreach (var d in tempList) DocumentsForPeriod.Add(d);
            RaisePropertyChanged(nameof(DocumentsForPeriod));
        }

        public void UpdatePeriods(EmployeePayMainViewModel item)
        {
            if (item == null) return;
            DocumentsForEmployee.Clear();
            var dates = new List<DateTime>();
            foreach (
                var r in
                Documents.Where(t => t.Employee.DocCode == item.Employee.DocCode))
            {
                if (dates.All(t => t != r.DocDate))
                    dates.Add(r.DocDate);
                DocumentsForEmployee.Add(r);
            }

            var dPeriods = DatePeriod.GenerateIerarhy(dates, PeriodIerarhy.YearMonth);
            var datesSource = dPeriods.Select(p => new NachEmployeeForPeriod
                {
                    Id = p.Id,
                    ParentId = p.ParentId,
                    Name = p.Name,
                    DateStart = p.DateStart,
                    DateEnd = p.DateEnd,
                    Start = 0,
                    In = 0,
                    Out = 0,
                    End = 0,
                    CrsName = item.CrsName
                })
                .ToList();
            foreach (var r in datesSource)
            {
                r.Start = Math.Round(DocumentsForEmployee.Where(t => t.DocDate < r.DateStart)
                                         .Sum(
                                             s =>
                                                 CurrencyRate.GetSummaRate(s.Crs.DocCode,
                                                     ((IDocCode)s.Employee.Currency).DocCode,
                                                     s.DocDate,
                                                     s.Summa))
                                     - DocumentsForEmployee.Where(t => t.DocDate < r.DateStart)
                                         .Sum(
                                             s => s.PlatSummaEmp), 2);
                r.In =
                    Math.Round(DocumentsForEmployee.Where(t => t.DocDate >= r.DateStart && t.DocDate <= r.DateEnd)
                        .Sum(
                            s =>
                                CurrencyRate.GetSummaRate(s.Crs.DocCode, ((IDocCode)s.Employee.Currency).DocCode,
                                    s.DocDate,
                                    s.Summa)), 2);
                r.Out =
                    Math.Round(DocumentsForEmployee.Where(t => t.DocDate >= r.DateStart && t.DocDate <= r.DateEnd)
                        .Sum(
                            s => s.PlatSummaEmp), 2);
                r.End = r.Start + r.In - r.Out;
                switch (r.CrsName)
                {
                    case CurrencyCode.RUBName:
                    case CurrencyCode.RURName:
                        r.StartRUB = r.Start;
                        r.EndRUB = r.End;
                        break;
                    case CurrencyCode.USDName:
                        r.StartUSD = r.Start;
                        r.EndUSD = r.End;
                        break;
                    case CurrencyCode.EURName:
                        r.StartEUR = r.Start;
                        r.EndEUR = r.End;
                        break;
                }

                r.RUB =
                    Math.Round(
                        DocumentsForEmployee.Where(
                                t => t.DocDate >= r.DateStart && t.DocDate <= r.DateEnd)
                            .Sum(s => s.RUB), 2);
                r.USD =
                    Math.Round(
                        DocumentsForEmployee.Where(
                                t => t.DocDate >= r.DateStart && t.DocDate <= r.DateEnd)
                            .Sum(s => s.USD), 2);
                r.EUR =
                    Math.Round(
                        DocumentsForEmployee.Where(
                                t => t.DocDate >= r.DateStart && t.DocDate <= r.DateEnd)
                            .Sum(s => s.EUR), 2);
                r.NachRUB =
                    Math.Round(
                        DocumentsForEmployee.Where(
                                t => t.DocDate >= r.DateStart && t.DocDate <= r.DateEnd)
                            .Sum(s => s.NachRUB), 2);
                r.NachUSD =
                    Math.Round(
                        DocumentsForEmployee.Where(
                                t => t.DocDate >= r.DateStart && t.DocDate <= r.DateEnd)
                            .Sum(s => s.NachUSD), 2);
                r.NachEUR =
                    Math.Round(
                        DocumentsForEmployee.Where(
                                t => t.DocDate >= r.DateStart && t.DocDate <= r.DateEnd)
                            .Sum(s => s.NachEUR), 2);
            }

            Periods = new ObservableCollection<NachEmployeeForPeriod>(datesSource.OrderByDescending(_ => _.DateStart));
            RaisePropertyChanged(nameof(Periods));
            var form = Form as PersonalPaysView;
            var bands = form?.treePeriods.Bands.FirstOrDefault(_ => _.Name == "bandt2");
            if (bands == null) return;
            foreach (var p in bands.Bands)
                if ((string)p.Header == "RUB" || (string)p.Header == "USD" || (string)p.Header == "EUR")
                    p.Visible = CurrentEmploee.CrsName == (string)p.Header;
        }

        public void LoadForAll(DateTime date)
        {
            var tempMain = new ObservableCollection<EmployeePayMainViewModel>();
            DocumentsLoad(date, true);
            foreach (
                var nach in
                Documents.Where(
                    nach => tempMain.All(t => t.Employee.DocCode != nach.Employee.DocCode)))
                tempMain.Add(new EmployeePayMainViewModel
                {
                    Employee = nach.Employee,
                    DolgSumma = 0,
                    PlatSumma = 0,
                    SummaNach = 0,
                    DateLastOper = DateTime.MinValue
                });
            foreach (var emp in tempMain)
            {
                var emp1 = emp;
                var s =
                    Documents.Where(t => t.Employee.DocCode == emp1.Employee.DocCode);
                foreach (var nach in s)
                {
                    if (nach.DocDate > emp.DateLastOper)
                        emp.DateLastOper = nach.DocDate;
                    emp.SummaNach += CurrencyRate.GetSummaRate(nach.Crs.DocCode,
                        ((IDocCode)emp.Employee.Currency).DocCode,
                        nach.DocDate,
                        nach.Summa);
                    emp.PlatSumma += nach.PlatSummaEmp;
                    emp.DolgSumma = emp.SummaNach - emp.PlatSumma;
                }
            }

            EmployeeMain.Clear();
            foreach (var t in tempMain.Where(_ => _.DolgSumma != 0 || _.PlatSumma != 0 || _.SummaNach != 0))
                EmployeeMain.Add(t);
        }

        public override void RefreshData(object obj)
        {
            CurrentEmploee = null;
            CurrentPeriod = null;
            CurrentPayDocument = null;
            var tempMain = new ObservableCollection<EmployeePayMainViewModel>();
            var d = obj as DateTime?;
            DocumentsLoad(obj != null ? d : null);
            if (Documents.Count == 0) return;
            foreach (
                var nach in
                Documents.Where(
                    nach => tempMain.All(t => t.Employee.DocCode != nach.Employee.DocCode)))
                tempMain.Add(new EmployeePayMainViewModel
                {
                    Employee = nach.Employee,
                    DolgSumma = 0,
                    PlatSumma = 0,
                    SummaNach = 0,
                    DateLastOper = DateTime.MinValue
                });
            foreach (var emp in tempMain)
            {
                var emp1 = emp;
                var s =
                    Documents.Where(t => t.Employee.DocCode == emp1.Employee.DocCode);
                foreach (var nach in s)
                {
                    if (nach.DocDate > emp.DateLastOper)
                        emp.DateLastOper = nach.DocDate;
                    emp.SummaNach += CurrencyRate.GetSummaRate(nach.Crs.DocCode,
                        ((IDocCode)emp.Employee.Currency).DocCode,
                        nach.DocDate,
                        nach.Summa);
                    emp.PlatSumma += nach.PlatSummaEmp;
                    emp.DolgSumma = emp.SummaNach - emp.PlatSumma;
                }
            }

            EmployeeMain.Clear();
            foreach (var t in tempMain)
                EmployeeMain.Add(t);
            RaisePropertyChanged(nameof(EmployeeMain));
            //if (mySelEmp == null) return;
        }
    }
}
