using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm;
using DevExpress.Xpf.Core;
using KursAM2.View.Personal;

namespace KursAM2.ViewModel.Personal
{
    public sealed class PayRollVedomostWindowViewModel : RSWindowViewModelBase
    {
        public PayRollVedomostEmployeeViewModel Employee;
        private bool isChange;
        private PayRollVedomostEmployeeViewModel myCurrentEmployee;
        private PayRollVedomostEmployeeRowViewModel myCurrentNach;
        private DateTime myDate;
        private bool myIsTemplate;
        private PayrollType myMyCurrentPayrollType;
        private Visibility myIsCanDateChange;

        public PayRollVedomostWindowViewModel()
        {
            Employees = new ObservableCollection<PayRollVedomostEmployeeViewModel>();
            Employees.CollectionChanged += Employees_CollectionChanged;
            RemoveEmployees = new ObservableCollection<PayRollVedomostEmployeeViewModel>();
            PayrollTypeCollection = new ObservableCollection<EMP_PAYROLL_TYPEViewModel>();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartDocWithDeleteRightBar(this);
            var prType = GlobalOptions.GetEntities().EMP_PAYROLL_TYPE.ToList();
            foreach (var pr in prType)
                PayrollTypeCollection.Add(new EMP_PAYROLL_TYPEViewModel(pr));
            CurrencyCollection = MainReferences.Currencies.Values.ToList();
        }

        public PayRollVedomostWindowViewModel(string id) : this()
        {
            Id = Guid.Parse(id);
            myState = RowStatus.NotEdited;
            //RefreshData(null);
        }

        public PayRollVedomostWindowViewModel(string id, Employee emp)
            : this(id)
        {
            if (emp != null)
                Employee = Employees.SingleOrDefault(_ => _.Employee.DocCode == emp.DocCode);
        }

        public PayRollVedomostWindowViewModel(Guid id)
            : this()
        {
            Id = id;
            RefreshData(null);
        }

        public PayRollVedomostWindowViewModel(Guid id, Employee emp)
            : this(id)
        {
            if (emp != null)
                Employee = Employees.SingleOrDefault(_ => _.Employee.DocCode == emp.DocCode);
        }

        public ICommand AddNewEmployeeCommand => new DelegateCommand(AddNewEmployee);
        public ICommand ShowMessageBoxCommand => new DelegateCommand(ShowMessageBox);

        // ReSharper disable once UnusedMember.Local
        private IDialogService DialogService => GetService<IDialogService>();
        private IMessageBoxService MessageBoxService => GetService<IMessageBoxService>();
        public ObservableCollection<PayRollVedomostEmployeeViewModel> Employees { set; get; }
        public ObservableCollection<PayRollVedomostEmployeeViewModel> RemoveEmployees { set; get; }
        public ObservableCollection<EMP_PAYROLL_TYPEViewModel> PayrollTypeCollection { set; get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public List<Currency> CurrencyCollection { set; get; }

        public PayRollVedomostEmployeeViewModel CurrentEmployee
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentEmployee == value) return;
                myCurrentEmployee = value;
                RaisePropertyChanged();
            }
            get => myCurrentEmployee;
        }

        public PayRollVedomostEmployeeRowViewModel CurrentNach
        {
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentNach == value) return;
                myCurrentNach = value;
                RaisePropertyChanged();
            }
            get => myCurrentNach;
        }

        public override bool IsDocDeleteAllow => true;

        public override string Name
        {
            get => base.Name;
            set
            {
                if (base.Name == value) return;
                base.Name = value;
                isChange = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsCanSaveData));
            }
        }

        public PayrollType CurrentPayrollType
        {
            set
            {
                if (myMyCurrentPayrollType != null && myMyCurrentPayrollType.Equals(value)) return;
                myMyCurrentPayrollType = value;
                RaisePropertyChanged();
            }
            get => myMyCurrentPayrollType;
        }

        public DateTime Date
        {
            set
            {
                if (myDate == value) return;
                myDate = value;
                isChange = true;
                IsCanDateChange = Visibility.Visible;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsCanSaveData));
            }
            get => myDate;
        }

        public bool IsTemplate
        {
            set
            {
                if (myIsTemplate == value) return;
                myIsTemplate = value;
                isChange = true;
                RaisePropertyChanged();
            }
            get => myIsTemplate;
        }

        public Visibility IsCanDateChange
        {
            set
            {
                if (myIsCanDateChange == value) return;
                myIsCanDateChange = value;
                isChange = true;
                RaisePropertyChanged();
            }
            get => myIsCanDateChange;
        }

        //public Visibility IsCanDateChange => State == RowStatus.NewRow ? Visibility.Visible : Visibility.Collapsed;

        private void AddNewEmployee()
        {
            var ex = Employees.Select(d => d.Employee.DocCode).ToList();
            var ctx = new PersonalDialogSelectWindowViewModel(ex);
            ctx.RefreshData(null);
            var dlgSelect = new DialogSelectPersona
            {
                DataContext = ctx,
                Owner = Application.Current.MainWindow
            };
            dlgSelect.ShowDialog();
            if (dlgSelect.DialogResult == null || !dlgSelect.DialogResult.Value) return;
            //var res = DialogService.ShowDialog(MessageBoxButton.OKCancel, "Добавить сотрудников", "EmploeeSelectUC", ctx);
            //if (res != MessageBoxResult.OK) return;
            foreach (var newEmp in ctx.SelectedCollection.Where(
                    item => Employees.All(t => t.Employee.DocCode != item.Persona.DocCode))
                .Select(item => new PayRollVedomostEmployeeViewModel
                {
                    State = RowStatus.NewRow,
                    Employee = item.Persona,
                    EURSumma = (decimal) 0.0,
                    RUBSumma = (decimal) 0.0,
                    USDSumma = (decimal) 0.0,
                    Rows = PayrollTypeCollection.FirstOrDefault(_ => _.DocCode == 19000000001) != null
                        ? new ObservableCollection<PayRollVedomostEmployeeRowViewModel>
                        {
                            new PayRollVedomostEmployeeRowViewModel
                            {
                                Id = Id,
                                RowId = Guid.NewGuid(),
                                Name = item.Name,
                                State = RowStatus.NewRow,
                                Employee = item.Persona,
                                Crs = MainReferences.Currencies[GlobalOptions.SystemProfile.MainCurrency.DocCode],
                                PRType = PayrollTypeCollection.Single(_ =>
                                    _.DocCode == GlobalOptions.SystemProfile.DafaultPayRollType.DocCode),
                                Summa = 0,
                                Rate = 0,
                                NachDate = Date

                            }
                        }
                        : new ObservableCollection<PayRollVedomostEmployeeRowViewModel>()
                }))
            {
                newEmp.Rows[0].State = RowStatus.NewRow;
                Employees.Add(newEmp);
                CurrentEmployee = newEmp;
            }
        }

        private void ShowMessageBox()
        {
            MessageBoxService.Show("MessageBox", "Test", MessageBoxButton.OKCancel, MessageBoxImage.Information);
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnusedParameter.Local
        private void Save(object obj)
        {
            SaveData(null);
            RaisePropertyChanged(nameof(IsCanSaveData));
        }

        private void Employees_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Employees));
            RaisePropertyChanged(nameof(IsCanSaveData));
        }

        public static PayRollVedomostWindowViewModel CreateNew()
        {
            var newVed = new PayRollVedomostWindowViewModel
            {
                Id = Guid.NewGuid(), Date = DateTime.Today,
                State = RowStatus.NewRow,
                IsCanDateChange = Visibility.Visible
            };
            return newVed;
        }

        public PayRollVedomostWindowViewModel Copy()
        {
            var newVed = new PayRollVedomostWindowViewModel(Id)
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Today,
                Name = string.Empty,
                State = RowStatus.NewRow,
                IsCanDateChange = Visibility.Visible
            };
            newVed.RemoveEmployees.Clear();
            foreach (var emp in newVed.Employees)
            {
                emp.State = RowStatus.NewRow;
                emp.RemoveRows.Clear();
                foreach (var r in emp.Rows)
                {
                    r.State = RowStatus.NewRow;
                    r.Id = newVed.Id;
                    r.RowId = Guid.NewGuid();
                    r.NachDate = DateTime.Today;
                }
            }

            return newVed;
        }

        public void CalcEmployee(PayRollVedomostEmployeeViewModel emp)
        {
            if (emp == null) return;
            emp.EURSumma = emp.Rows.Where(_ => _.Crs.Name == "EUR").Sum(_ => _.Summa);
            emp.USDSumma = emp.Rows.Where(_ => _.Crs.Name == "USD").Sum(_ => _.Summa);
            emp.RUBSumma = emp.Rows.Where(_ => _.Crs.Name == "RUB" || _.Crs.Name == "RUR").Sum(_ => _.Summa);
            RaisePropertyChanged(nameof(Employees));
        }

        public void CalcEmployee()
        {
            if (CurrentEmployee == null) return;
            CurrentEmployee.EURSumma = CurrentEmployee.Rows.Where(_ => _.Crs.Name == "EUR").Sum(_ => _.Summa);
            CurrentEmployee.USDSumma = CurrentEmployee.Rows.Where(_ => _.Crs.Name == "USD").Sum(_ => _.Summa);
            CurrentEmployee.RUBSumma =
                CurrentEmployee.Rows.Where(_ => _.Crs.Name == "RUB" || _.Crs.Name == "RUR").Sum(_ => _.Summa);
            RaisePropertyChanged(nameof(Employees));
        }

        private void row_StateChanged(object sender, StateEventArgs e)
        {
            if (e.Row?.Parent != null)
            {
                CalcEmployee(CurrentEmployee);
                RaisePropertyChanged(nameof(Employees));
            }
        }

        #region Command

        public override void CloseWindow(object form)
        {
            if (IsCanSaveData)
            {
                var res = DXMessageBox.Show(
                    "В справочник внесены изменения, сохранить?",
                    "Запрос",
                    MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Cancel) return;
                if (res == MessageBoxResult.Yes)
                {
                    SaveData(null);
                    var frm = form as Window;
                    frm?.Close();
                }

                if (res == MessageBoxResult.No)
                {
                    var frm = form as Window;
                    frm?.Close();
                }
            }

            var frm1 = form as Window;
            frm1?.Close();
            //base.CloseWindow(Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive));
        }

        public override void DocNewEmpty(object form)
        {
            var ved = CreateNew();
            var frm = new PayRollVedomost {Owner = Application.Current.MainWindow, DataContext = ved};
            frm.Show();
        }

        public override void DocNewCopyRequisite(object form)
        {
            var dtx = Copy();
            var frm = new PayRollVedomost {Owner = Application.Current.MainWindow, DataContext = dtx};
            frm.Show();
            foreach (var e in dtx.Employees)
            {
                e.Rows.Clear();
                var nach = new PayRollVedomostEmployeeRowViewModel
                {
                    Id = dtx.Id,
                    RowId = Guid.NewGuid(),
                    Name = e.Name,
                    State = RowStatus.NewRow,
                    Employee = e.Employee,
                    Crs = MainReferences.Currencies[GlobalOptions.SystemProfile.MainCurrency.DocCode],
                    PRType = dtx.PayrollTypeCollection.Single(_ =>
                        _.DocCode == GlobalOptions.SystemProfile.DafaultPayRollType.DocCode),
                    Summa = 0,
                    Rate = 0
                };
                e.Rows.Add(nach);
                e.RaisePropertyChanged("Rows");
                e.CalcSumma();
            }
        }

        public override void DocNewCopy(object form)
        {
            var dtx = Copy();
            dtx.myState = RowStatus.NewRow;
            var frm = new PayRollVedomost {Owner = Application.Current.MainWindow, DataContext = dtx};
            frm.Show();
        }

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("Действительно хотите удалить ведомость?", "Запрос",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    using (var ent = GlobalOptions.GetEntities())
                    {
                        if(ent.EMP_PR_DOC.Include(_ => _.EMP_PR_ROWS)
                            .Any(_ => _.ID == Id.ToString().Replace("-", string.Empty)))
                        {
                            var d =
                                ent.EMP_PR_DOC.Include(_ => _.EMP_PR_ROWS)
                                    .Single(_ => _.ID == Id.ToString().Replace("-", string.Empty).ToUpper());
                            if (d != null)
                            {
                                var rows = d.EMP_PR_ROWS.ToList();
                                foreach (var r in rows)
                                    ent.EMP_PR_ROWS.Remove(r);
                                ent.EMP_PR_DOC.Remove(d);
                            }

                            ent.SaveChanges();
                        }
                        var frm = form as Window;
                        frm?.Close();
                    }

                    break;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }
        }

        public ICommand DateChangeCommand
        {
            get { return new Command(SetDate, _ => true); }
        }

        private void SetDate(object obj)
        {
            foreach (var emp in Employees)
            {
                foreach (var r in emp.Rows)
                {
                    r.NachDate = Date;
                }
            }
        }

        public override void RefreshData(object o)
        {
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            using (var ent = GlobalOptions.GetEntities())
            {
                var idd = Id.ToString().Replace("-", string.Empty).ToUpper();
                var doc = ent.EMP_PR_DOC.SingleOrDefault(_ => _.ID == idd);
                if (doc == null) return;
                Name = doc.Notes;
                Date = doc.Date;
                IsTemplate = doc.IS_TEMPLATE == 1;
                CurrencyRate.LoadCBrates(Date.AddDays(-1), Date);
                Employees.Clear();
                try
                {
                    var docs = ent.EMP_PR_ROWS
                        .Include(_ => _.EMP_PAYROLL_TYPE)
                        .Where(_ => _.ID == idd)
                        .ToList();
                    var emps = new List<decimal>();
                    foreach (var dd in docs.Where(dd => !emps.Exists(_ => _ == dd.EMP_DC)))
                        emps.Add(dd.EMP_DC);
                    foreach (var newEmp in from dc in emps
                        select MainReferences.Employees[dc]
                        into emp
                        where emp != null
                        select new PayRollVedomostEmployeeViewModel
                        {
                            Employee = emp,
                            Name = emp.Name,
                            EURSumma = 0,
                            USDSumma = 0,
                            RUBSumma = 0,
                            Rows = new ObservableCollection<PayRollVedomostEmployeeRowViewModel>()
                        })
                    {
                        foreach (var row in from d in docs.Where(_ => _.EMP_DC == newEmp.Employee.DocCode)
                            let crs1 = MainReferences.Currencies[d.CRS_DC]
                            select new PayRollVedomostEmployeeRowViewModel
                            {
                                Summa = d.SUMMA,
                                PRType = PayrollTypeCollection.Single(_ => _.DocCode == d.PR_TYPE_DC),
                                Crs = crs1,
                                Note = d.NOTES,
                                RowId = Guid.Parse(d.ROW_ID),
                                Rate = CurrencyRate.GetRate(crs1.DocCode, newEmp.Employee.Currency.DocCode, Date),
                                NachEmpRate =
                                    CurrencyRate.GetSummaRate(crs1.DocCode, newEmp.Employee.Currency.DocCode, Date,
                                        d.SUMMA),
                                Parent = newEmp,
                                NachDate = d.NachDate ?? DateTime.Today
                            })
                        {
                            row.StateChanged += row_StateChanged;
                            row.myState = RowStatus.NotEdited;
                            newEmp.Rows.Add(row);
                        }

                        newEmp.EURSumma = newEmp.Rows.Where(_ => _.Crs.Name == "EUR").Sum(_ => _.Summa);
                        newEmp.USDSumma = newEmp.Rows.Where(_ => _.Crs.Name == "USD").Sum(_ => _.Summa);
                        newEmp.RUBSumma =
                            newEmp.Rows.Where(_ => _.Crs.Name == "RUB" || _.Crs.Name == "RUR").Sum(_ => _.Summa);
                        newEmp.myState = RowStatus.NotEdited;
                        Employees.Add(newEmp);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            foreach (var e in Employees)
            {
                foreach (var r in e.Rows)
                    r.myState = RowStatus.NotEdited;
                e.myState = RowStatus.NotEdited;
            }

            State = RowStatus.NotEdited;
            isChange = false;
            IsCanDateChange = Visibility.Collapsed;
            //RaisePropertyChanged(nameof(Employees));
            //RaisePropertyChanged(nameof(IsCanSaveData));
        }

        public override void SaveData(object o)
        {
            using (var ent = GlobalOptions.GetEntities())
            {
                using (var tnx = new TransactionScope())
                {
                    try
                    {
                        var id = Id.ToString().Replace("-", string.Empty).ToUpper();
                        var deletesNach = new List<PayRollVedomostEmployeeRowViewModel>();
                        foreach (var e in RemoveEmployees) deletesNach.AddRange(e.RemoveRows);
                        foreach (
                            var e in
                            Employees.Where(_ => _.State != RowStatus.NotEdited).Where(e => e.RemoveRows.Count > 0)
                        )
                            deletesNach.AddRange(e.RemoveRows);
                        foreach (var dn in deletesNach)
                        {
                            var delNach = ent.EMP_PR_ROWS.FirstOrDefault(_ =>
                                _.ROW_ID == dn.RowId.ToString().Replace("-", string.Empty).ToUpper());
                            if (delNach != null)
                                ent.EMP_PR_ROWS.Remove(delNach);
                        }

                        var doc = ent.EMP_PR_DOC.SingleOrDefault(_ => _.ID == id);
                        if (doc == null)
                        {
                            var newDoc = new EMP_PR_DOC
                            {
                                Creator = GlobalOptions.UserInfo.NickName,
                                Notes = Name,
                                Date = Date,
                                ID = id,
                                IS_TEMPLATE = IsTemplate ? 1 : 0,
                                CrsRates = "",
                                DCOld = null,
                                EXT_Notes = Name,
                                EMP_PR_ROWS = new Collection<EMP_PR_ROWS>()
                            };
                            ent.EMP_PR_DOC.Add(newDoc);
                            foreach (var e in Employees)
                            foreach (var item in e.Rows)
                                ent.EMP_PR_ROWS.Add(new EMP_PR_ROWS
                                {
                                    ID = id,
                                    ROW_ID = item.RowId.ToString().Replace("-", string.Empty).ToUpper(),
                                    // ReSharper disable PossibleInvalidOperationException
                                    CRS_DC = item.Crs.DocCode,
                                    EMP_DC = e.Employee.DocCode,
                                    PR_TYPE_DC = item.PRType.DocCode,
                                    // ReSharper restore PossibleInvalidOperationException
                                    NOTES = item.Note,
                                    SUMMA = item.Summa,
                                    RATE = item.Rate,
                                    RR = "",
                                    NachDate = item.NachDate
                                });
                        }
                        else
                        {
                            doc.IS_TEMPLATE = IsTemplate ? 1 : 0;
                            doc.Date = Date;
                            doc.Notes = Name;
                            doc.EXT_Notes = Name;
                            foreach (var e in Employees.Where(t => t.State != RowStatus.NotEdited))
                            foreach (var item in e.Rows)
                                switch (item.State)
                                {
                                    case RowStatus.NewRow:
                                        ent.EMP_PR_ROWS.Add(new EMP_PR_ROWS
                                        {
                                            ID = id,
                                            ROW_ID = item.RowId.ToString().Replace("-", string.Empty)
                                                .ToUpper(),
                                            // ReSharper disable PossibleInvalidOperationException
                                            CRS_DC = item.Crs.DocCode,
                                            EMP_DC = e.Employee.DocCode,
                                            PR_TYPE_DC = item.PRType.DocCode,
                                            // ReSharper restore PossibleInvalidOperationException
                                            NOTES = item.Note,
                                            SUMMA = item.Summa,
                                            RATE = item.Rate,
                                            RR = "",
                                            NachDate = item.NachDate
                                        });
                                        break;
                                    case RowStatus.Edited:
                                        var empType =
                                            ent.EMP_PR_ROWS.ToList()
                                                .SingleOrDefault(t =>
                                                    t.ID == id && Guid.Parse(t.ROW_ID) == item.RowId);
                                        // ReSharper disable PossibleNullReferenceException
                                        // ReSharper disable PossibleInvalidOperationException
                                        if (empType == null)
                                        {
                                            ent.EMP_PR_ROWS.Add(new EMP_PR_ROWS
                                            {
                                                ID = id,
                                                ROW_ID = item.RowId.ToString().Replace("-", string.Empty)
                                                    .ToUpper(),
                                                // ReSharper disable PossibleInvalidOperationException
                                                CRS_DC = item.Crs.DocCode,
                                                EMP_DC = e.Employee.DocCode,
                                                PR_TYPE_DC = item.PRType.DocCode,
                                                // ReSharper restore PossibleInvalidOperationException
                                                NOTES = item.Note,
                                                SUMMA = item.Summa,
                                                RATE = item.Rate,
                                                RR = "",
                                                NachDate = item.NachDate
                                            });
                                        }
                                        else
                                        {
                                            empType.CRS_DC = item.Crs.DocCode;
                                            empType.EMP_DC = e.Employee.DocCode;
                                            empType.PR_TYPE_DC = item.PRType.DocCode;
                                            // ReSharper restore PossibleInvalidOperationException
                                            empType.NOTES = item.Note;
                                            empType.SUMMA = item.Summa;
                                            empType.RATE = item.Rate;
                                            empType.RR = "";
                                            empType.NachDate = item.NachDate;
                                            // ReSharper restore PossibleNullReferenceException
                                            //ctx.UpdateObject(empType);
                                        }

                                        break;
                                    case RowStatus.NotEdited:
                                        break;
                                    case RowStatus.Deleted:
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                        }

                        ent.SaveChanges();
                        tnx.Complete();
                        RaisePropertiesChanged(nameof(Employees));
                        isChange = false;
                        myState = RowStatus.NotEdited;
                        foreach (var emp in Employees)
                        {
                            emp.myState = RowStatus.NotEdited;

                            foreach (var n in emp.Rows)
                            {
                                n.myState = RowStatus.NotEdited;
                                n.RaisePropertyChanged("State");
                            }

                            emp.RaisePropertyChanged("Rows");
                        }
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                    }
                }
            }

            RemoveEmployees.Clear();
            foreach (var e in Employees)
            {
                e.State = RowStatus.NotEdited;
                e.RemoveRows.Clear();
                foreach (var r in e.Rows)
                    r.State = RowStatus.NotEdited;
            }

            myState = RowStatus.NotEdited;
            foreach (var e in Employees)
                e.myState = RowStatus.NotEdited;
            RaisePropertyChanged(nameof(Employees));
            RaisePropertyChanged(nameof(IsCanSaveData));
        }

        public override bool IsCanSaveData => isChange || Employees.Any(_ => _.State != RowStatus.NotEdited) ||
                                              RemoveEmployees.Count > 0;

        public ICommand AddNachCommand
        {
            get { return new Command(AddNach, param => CurrentEmployee != null); }
        }

        private void AddNach(object obj)
        {
            var newNach = new PayRollVedomostEmployeeRowViewModel
            {
                
                Id = CurrentEmployee.Id,
                RowId = Guid.NewGuid(),
                Summa = 0,
                Crs = MainReferences.Currencies[GlobalOptions.SystemProfile.EmployeeDefaultCurrency.DocCode],
                Parent = CurrentEmployee,
                NachDate = DateTime.Today,
                State = RowStatus.NewRow
            };
            CalcEmployee(CurrentEmployee);
            newNach.StateChanged += row_StateChanged;
            CurrentEmployee.Rows.Add(newNach);
        }

        public ICommand DeleteNachCommand
        {
            get { return new Command(DeleteNach, param => CurrentNach != null); }
        }

        private void DeleteNach(object obj)
        {
            if (CurrentEmployee == null) return;
            if (CurrentNach != null)
            {
                CurrentEmployee.RemoveRow(CurrentNach);
                CalcEmployee(CurrentEmployee);
            }
        }

        public ICommand RefreshCommand
        {
            get { return new Command(Refresh, param => true); }
        }

        private void Refresh(object obj)
        {
            RefreshData(null);
        }

        public ICommand DeleteEmployeeCommand
        {
            get { return new Command(DeleteEmployee, param => true); }
        }

        private void DeleteEmployee(object obj)
        {
            if (CurrentEmployee != null)
            {
                if (CurrentEmployee.State != RowStatus.NewRow)
                    RemoveEmployees.Add(CurrentEmployee);
                foreach (var r in CurrentEmployee.Rows) CurrentEmployee.RemoveRows.Add(r);
                //CurrentEmployee.State = RowStatus.Deleted;
                var item = Employees.FirstOrDefault(_ => _.Employee.TabelNumber == CurrentEmployee.TabelNumber);
                Employees.Remove(item);
            }

            RaisePropertyChanged(nameof(Employees));
        }

        public override bool IsDocNewCopyAllow => true;

        #endregion
    }
}