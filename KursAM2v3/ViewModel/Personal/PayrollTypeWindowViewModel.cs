using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;

namespace KursAM2.ViewModel.Personal
{
    public sealed class PayrollTypeWindowViewModel : RSWindowViewModelBase
    {
        private PayrollType myCurrentType;
        public ALFAMEDIAEntities myDataContext;

        public PayrollTypeWindowViewModel()
        {
            myDataContext = GlobalOptions.GetEntities();
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            RefreshData(null);
        }

        public ObservableCollection<PayrollType> Rows { set; get; } = new ObservableCollection<PayrollType>();
        public ObservableCollection<PayrollType> DeletedRows { set; get; } = new ObservableCollection<PayrollType>();

        public PayrollType CurrentType
        {
            get => myCurrentType;
            set
            {
                if (myCurrentType != null && myCurrentType.Equals(value)) return;
                myCurrentType = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsCanSaveData
            => Rows.Count > 0 && Rows.Any(_ => _.State != RowStatus.NotEdited) || DeletedRows.Count > 0;

        public ICommand AddRowCommand
        {
            get { return new Command(AddRow, _ => true); }
        }

        public bool IsRowCanDelete
        {
            get
            {
                if (CurrentType == null) return false;
                try
                {
                    return myDataContext.EMP_PR_ROWS.Where(_ => _.PR_TYPE_DC == CurrentType.DocCode).ToList().Count <=
                           0;
                }
                catch (Exception ex)
                {
                    WindowManager.ShowError(ex);
                }

                return false;
            }
        }

        public ICommand DeleteRowCommand
        {
            get { return new Command(DeleteRow, _ => IsRowCanDelete); }
        }

        public override void SaveData(object data)
        {
            using (var tnx = myDataContext.Database.BeginTransaction())
            {
                try
                {
                    if (DeletedRows.Count > 0)
                        foreach (
                            var d in
                            DeletedRows.Select(
                                    row => myDataContext.EMP_PAYROLL_TYPE.FirstOrDefault(_ => _.ID == row.ID))
                                .Where(d => d != null))
                            myDataContext.EMP_PAYROLL_TYPE.Remove(d);
                    foreach (var row in Rows.Where(_ => _.State != RowStatus.NotEdited))
                    {
                        if (row.State == RowStatus.Edited)
                        {
                            var d = myDataContext.EMP_PAYROLL_TYPE.FirstOrDefault(_ => _.ID == row.ID);
                            if (d == null) continue;
                            d.Name = row.Name;
                            d.Type = row.Type ? 1 : 0;
                        }

                        if (row.State == RowStatus.NewRow)
                        {
                            var newDC = myDataContext.EMP_PAYROLL_TYPE.Count() > 0
                                ? myDataContext.EMP_PAYROLL_TYPE.Max(_ => _.DOC_CODE) + 1
                                : 19000000001;
                            myDataContext.EMP_PAYROLL_TYPE.Add(new EMP_PAYROLL_TYPE
                            {
                                ID = row.ID,
                                DOC_CODE = newDC,
                                Name = row.Name,
                                Type = row.Type ? 1 : 0
                            });
                        }
                    }

                    myDataContext.SaveChanges();
                    tnx.Commit();
                    Refresh();
                }
                catch (Exception ex)
                {
                    tnx.Rollback();
                    WindowManager.ShowError(ex);
                }
            }
        }

        private void AddRow(object obj)
        {
            Rows.Add(new PayrollType
            {
                ID = Guid.NewGuid().ToString().ToUpper().Replace("-", string.Empty),
                DocCode = -1,
                Type = true,
                State = RowStatus.NewRow
            });
        }

        private void DeleteRow(object obj)
        {
            if (CurrentType.State != RowStatus.NewRow)
                DeletedRows.Add(CurrentType);
            Rows.Remove(CurrentType);
        }

        public override void RefreshData(object o)
        {
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("� �������� ���� ������� ���������, ���������?", "������",
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

            Refresh();
        }

        private void Refresh()
        {
            try
            {
                Rows.Clear();
                DeletedRows.Clear();
                foreach (var d in myDataContext.EMP_PAYROLL_TYPE.ToList())
                    Rows.Add(new PayrollType(d)
                    {
                        State = RowStatus.NotEdited
                    });
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }
    }
}