using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Managers.Nomenkl;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Reference
{
    public class FormPayReferenceWindowViewModel : RSWindowViewModelBase
    {
        #region Fields

        private readonly UnitManager UnitManager = new UnitManager();

        #endregion

        #region Constructors

        public FormPayReferenceWindowViewModel()
        {
            WindowName = "Справочник форм оплаты";
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        #endregion

        #region Properties

        public ObservableCollection<PayFormViewModel> Rows { set; get; } = new ObservableCollection<PayFormViewModel>();

        public ObservableCollection<PayFormViewModel> DeletedRows { set; get; } =
            new ObservableCollection<PayFormViewModel>();

        public ObservableCollection<PayFormViewModel> SelectedRows { set; get; } =
            new ObservableCollection<PayFormViewModel>();

        private PayFormViewModel myCurrentRow;

        public PayFormViewModel CurrentRow
        {
            get => myCurrentRow;
            set
            {
                if (Equals(myCurrentRow, value)) return;
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
                        var newDC = ctx.SD_189.Any() ? ctx.SD_189.Max(_ => _.DOC_CODE) : 11890000001;
                        foreach (var d in DeletedRows)
                        {
                            if (ctx.SD_26.Any(_ => _.SF_PAY_COND_DC == d.DocCode)
                                || ctx.SD_84.Any(_ => _.SF_PAY_COND_DC == d.DocCode))
                                continue;
                            var oldItem = ctx.SD_189.FirstOrDefault(_ => _.DOC_CODE == d.DocCode);
                            if (oldItem != null) ctx.SD_189.Remove(oldItem);
                        }

                        foreach (var u in Rows)
                            switch (u.State)
                            {
                                case RowStatus.NewRow:
                                    newDC++;
                                    var sd179 = new SD_189
                                    {
                                        DOC_CODE = newDC,
                                        OOT_NALOG_PERCENT = 0,
                                        OOT_NALOG_S_PRODAZH = 0,
                                        OOT_NAME = u.Name,
                                        OOT_USL_OPL_DEF_DC = u.OOT_USL_OPL_DEF_DC
                                    };
                                    ctx.SD_189.Add(sd179);
                                    break;
                                case RowStatus.Edited:
                                    var old1 = ctx.SD_189.FirstOrDefault(_ => _.DOC_CODE == u.DocCode);
                                    if (old1 != null)
                                    {
                                        old1.OOT_NALOG_PERCENT = u.OOT_NALOG_PERCENT;
                                        old1.OOT_NALOG_S_PRODAZH = u.OOT_NALOG_S_PRODAZH;
                                        old1.OOT_NAME = u.Name;
                                        old1.OOT_USL_OPL_DEF_DC = u.OOT_USL_OPL_DEF_DC;
                                    }

                                    break;
                            }

                        ctx.SaveChanges();
                        tn.Commit();
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
            Rows.Add(new PayFormViewModel
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                Name = null,
                OOT_NALOG_PERCENT = 0,
                OOT_NALOG_S_PRODAZH = 0,
                OOT_USL_OPL_DEF_DC = 11790000001
            });
        }

        public override void RefreshData(object obj)
        {
            Rows.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = ctx.SD_189.ToList();
                foreach (var u in data)
                    Rows.Add(new PayFormViewModel(u)
                    {
                        State = RowStatus.NotEdited
                    });
            }
        }

        public override bool IsCanSaveData =>
            (Rows != null && Rows.Any(_ => _.State != RowStatus.NotEdited)) || DeletedRows.Count > 0;

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
            Rows.Add(new PayFormViewModel
            {
                DocCode = -1,
                State = RowStatus.NewRow,
                Name = CurrentRow.Name,
                OOT_NALOG_PERCENT = CurrentRow.OOT_NALOG_PERCENT,
                OOT_NALOG_S_PRODAZH = CurrentRow.OOT_NALOG_S_PRODAZH,
                OOT_USL_OPL_DEF_DC = CurrentRow.OOT_USL_OPL_DEF_DC
            });
        }

        public ICommand ItemsDeleteCommand
        {
            get { return new Command(ItemsDelete, _ => CurrentRow != null || SelectedRows.Count > 0); }
        }

        private void ItemsDelete(object obj)
        {
            var dList = new List<PayFormViewModel>();
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
