using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Managers.Nomenkl;

namespace KursAM2.ViewModel.Reference
{
    public class UsagePayReferenceWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private readonly UnitManager UnitManager = new UnitManager();

        #endregion

        #region Constructors

        public UsagePayReferenceWindowViewModel()
        {
            WindowName = "Справочник условий оплаты";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        #endregion

        #region Properties

        public ObservableCollection<PayCondition> Rows { set; get; } = new ObservableCollection<PayCondition>();
        public ObservableCollection<PayCondition> DeletedRows { set; get; } = new ObservableCollection<PayCondition>();
        public ObservableCollection<PayCondition> SelectedRows { set; get; } = new ObservableCollection<PayCondition>();
        private PayCondition myCurrentRow;

        public PayCondition CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (myCurrentRow != null && myCurrentRow.Equals(value)) return;
                myCurrentRow = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Commands

        public override void SaveData(object data)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tn = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var newDC = ctx.SD_179.Any() ? ctx.SD_179.Max(_ => _.DOC_CODE) : 11790000001;
                        foreach (var d in DeletedRows)
                        {
                            if (ctx.SD_26.Any(_ => _.SF_PAY_COND_DC == d.DocCode)
                                || ctx.SD_84.Any(_ => _.SF_PAY_COND_DC == d.DocCode))
                                continue;
                            var oldItem = ctx.SD_179.FirstOrDefault(_ => _.DOC_CODE == d.DocCode);
                            if (oldItem != null) ctx.SD_179.Remove(oldItem);
                        }

                        foreach (var u in Rows)
                            switch (u.State)
                            {
                                case RowStatus.NewRow:
                                    newDC++;
                                    var sd179 = new SD_179
                                    {
                                        DOC_CODE = newDC,
                                        DEFAULT_VALUE = u.DEFAULT_VALUE,
                                        PT_DAYS_OPL = u.PT_DAYS_OPL,
                                        PT_NAME = u.PT_NAME
                                    };
                                    ctx.SD_179.Add(sd179);
                                    break;
                                case RowStatus.Edited:
                                    var old1 = ctx.SD_179.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old1 != null)
                                    {
                                        old1.DEFAULT_VALUE = u.DEFAULT_VALUE;
                                        old1.PT_DAYS_OPL = u.PT_DAYS_OPL;
                                        old1.PT_NAME = u.Name;
                                    }

                                    break;
                            }

                        ctx.SaveChanges();
                        tn.Commit();
                        MainReferences.PayConditions.Clear();
                        foreach (var item in ctx.SD_179.AsNoTracking().ToList())
                            MainReferences.PayConditions.Add(item.DOC_CODE,
                                new PayCondition(item) {State = RowStatus.NotEdited});
                        foreach (var r in Rows)
                            r.myState = RowStatus.NotEdited;
                        DeletedRows.Clear();
                    }
                    catch (Exception ex)
                    {
                        tn.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        public ICommand ItemNewEmptyCommand
        {
            get { return new Command(ItemNewEmpty, _ => true); }
        }

        private void ItemNewEmpty(object obj)
        {
            Rows.Add(new PayCondition
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                Name = null,
                DEFAULT_VALUE = 1,
                PT_DAYS_OPL = 1
            });
        }

        public override void RefreshData(object obj)
        {
            Rows.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.SD_179.ToList();
                foreach (var u in data)
                    Rows.Add(new PayCondition(u)
                    {
                        State = RowStatus.NotEdited
                    });
            }
        }

        public override bool IsCanSaveData =>
            Rows != null && Rows.Any(_ => _.State != RowStatus.NotEdited) || DeletedRows.Count > 0;

        //    ItemNewCopyCommand
        //}" />
        //</MenuItem>
        //<Separator />
        //<MenuItem Header = "Удалить выделенные строки" Command="{Binding ItemsDeleteCommand}"
        public ICommand ItemNewCopyCommand
        {
            get { return new Command(ItemNewCopy, _ => CurrentRow != null); }
        }

        private void ItemNewCopy(object obj)
        {
            Rows.Add(new PayCondition
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                Name = CurrentRow.Name,
                DEFAULT_VALUE = 1,
                PT_DAYS_OPL = CurrentRow.PT_DAYS_OPL
            });
        }

        public ICommand ItemsDeleteCommand
        {
            get { return new Command(ItemsDelete, _ => CurrentRow != null || SelectedRows.Count > 0); }
        }

        private void ItemsDelete(object obj)
        {
            var dList = new List<PayCondition>();
            foreach (var r in SelectedRows)
                dList.Add(r);
            foreach (var row in dList)
                if (row.State == RowStatus.NewRow)
                {
                    Rows.Remove(row);
                }
                else
                {
                    DeletedRows.Add(row);
                    Rows.Remove(row);
                }
        }

        #endregion
    }
}