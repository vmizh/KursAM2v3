using Core;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using Helper;
using KursAM2.View.KursReferences;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace KursAM2.ViewModel.Reference
{
    public class ReferenceOfResponsibilityCentersWindowViewModel : RSWindowViewModelBase
    {

        public ReferenceOfResponsibilityCentersWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        public ReferenceOfResponsibilityCentersWindowViewModel(Window win) : this()
        {
            Form = win;
        }

        #region Fields

        private CentrOfResponsibility myCurrentCenter;


        #endregion

        #region Properties

        public ObservableCollection<CentrOfResponsibility> CenterCollection { set; get; } =
            new ObservableCollection<CentrOfResponsibility>();


        public CentrOfResponsibility CurrentCenter
        {
            get => myCurrentCenter;
            set
            {
                if (myCurrentCenter == value)
                    return;
                myCurrentCenter = value;
                RaisePropertyChanged();

            }
        }

        public override bool IsCanSaveData => CenterCollection.Any(_ => _.State != RowStatus.NotEdited);

        public decimal DocCodeCounter = -1;
        private Dictionary<decimal?, decimal> DCvsChildNum = new Dictionary<decimal?, decimal>();
        #endregion

        public override void RefreshData(object obj)
        {
            DCvsChildNum.Clear();
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("Были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                    try
                    {
                        SaveData(null);
                    }
                    catch (Exception ex)
                    {
                        WinManager.ShowWinUIMessageBox(ex.Message, "Ошибка");
                    }
            }
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    CenterCollection.Clear();
                    foreach (var cent in ctx.SD_40.ToList())
                        CenterCollection.Add(new CentrOfResponsibility(cent)
                        {
                            State = RowStatus.NotEdited
                        });
                    RaisePropertyChanged(nameof(CenterCollection));
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void SaveData(object data)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        var newNumDocCode = ctx.SD_40.Any() ? ctx.SD_40.Max(_ => _.DOC_CODE) + 1 : 10400000001;
                        foreach (var c in CenterCollection.Where(_ => _.State != RowStatus.NotEdited).OrderByDescending(_ => _.DOC_CODE).ThenBy(_=>_.CENT_PARENT_DC))
                            if (c.CENT_PARENT_DC == null || c.CENT_PARENT_DC > 0)
                            {
                                switch (c.State)
                                {
                                    case RowStatus.NewRow:
                                        DCvsChildNum.Add(c.DOC_CODE, newNumDocCode);
                                        ctx.SD_40.Add(new SD_40
                                        {
                                            DOC_CODE = newNumDocCode,
                                            CENT_FULLNAME = c.CENT_FULLNAME,
                                            CENT_NAME = c.CENT_NAME,
                                            IS_DELETED = c.IS_DELETED,
                                            CENT_PARENT_DC = c.CENT_PARENT_DC
                                        });
                                        newNumDocCode++;
                                        break;
                                    case RowStatus.Edited:
                                        var old = ctx.SD_40.FirstOrDefault(_ => _.DOC_CODE == c.DOC_CODE);
                                        if (old == null) return;
                                        old.DOC_CODE = c.DOC_CODE;
                                        old.CENT_PARENT_DC = c.CENT_PARENT_DC;
                                        old.CENT_FULLNAME = c.CENT_FULLNAME;
                                        old.CENT_NAME = c.CENT_NAME;
                                        old.IS_DELETED = c.IS_DELETED;
                                        break;
                                }

                            }
                            else
                            {
                                DCvsChildNum.Add(c.DOC_CODE, newNumDocCode);
                                ctx.SD_40.Add(new SD_40
                                {
                                    DOC_CODE = newNumDocCode,
                                    CENT_FULLNAME = c.CENT_FULLNAME,
                                    CENT_NAME = c.CENT_NAME,
                                    IS_DELETED = c.IS_DELETED,
                                    CENT_PARENT_DC = DCvsChildNum[c.CENT_PARENT_DC]
                                });
                                newNumDocCode++;
                                
                            }

                        ctx.SaveChanges();
                        transaction.Commit();

                        foreach (var с in CenterCollection)
                            с.myState = RowStatus.NotEdited;
                        MainReferences.Refresh();
                        RaisePropertyChanged(nameof(IsCanSaveData));
                    }
                    catch (Exception ex)
                    {
                        if (transaction.UnderlyingTransaction.Connection != null)
                            transaction.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
            RefreshData(null);
        }


        #region Commands

        public ICommand AddNewCenterCommand
        {
            get { return new Command(AddNewCenter, _ => true); }
        }

        private void AddNewCenter(object obj)
        {
            var newRow = new CentrOfResponsibility()
            {
                State = RowStatus.NewRow,
                DOC_CODE = --DocCodeCounter,
                CENT_NAME = "Новый центр"
            };

            CenterCollection.Add(newRow);

            RaisePropertyChanged(nameof(CenterCollection));
        }

        private bool IsCanAddCenter()
        {
            if (CurrentCenter == null)
                return false;
            var centerItem = CenterCollection.FirstOrDefault(_ => _.DOC_CODE == CurrentCenter.CENT_PARENT_DC);
            if (centerItem == null)
                return true;
            var centerItem2 = CenterCollection.FirstOrDefault(_ => _.CENT_PARENT_DC == centerItem.DOC_CODE);
            if (centerItem2 == null)
                return true;
            var centerItem3 = CenterCollection.FirstOrDefault(_ => _.DOC_CODE == centerItem2.CENT_PARENT_DC);
            if (centerItem3 == null)
                return true;
            var centerItem4 = CenterCollection.FirstOrDefault(_ => _.DOC_CODE == centerItem3.CENT_PARENT_DC);
            if (centerItem4 != null)
                return false;
            return true;
        }

        public ICommand AddNewSectionCenterCommand
        {
            get { return new Command(AddNewSectionCenter, _ => IsCanAddCenter() && CurrentCenter != null); }
        }

        private void AddNewSectionCenter(object obj)
        {
            var newRow = new CentrOfResponsibility()
            {
                State = RowStatus.NewRow,
                CENT_NAME = "Новый центр",
                DOC_CODE = --DocCodeCounter,
                CENT_PARENT_DC = CurrentCenter.DOC_CODE,
            };

            CenterCollection.Add(newRow);
            RaisePropertyChanged(nameof(CenterCollection));

            if (Form is ReferenceOfResponsibilityCentersView win)
                win.treeListView.FocusedNode.IsExpanded = true;
            CurrentCenter = newRow;
        }


        public ICommand MoveToTopCenterCommand
        {
            get { return new Command(MoveToTopCenter, _ => CurrentCenter?.DOC_CODE != null); }
        }

        private void MoveToTopCenter(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var tx = ctx.Database.BeginTransaction();
                try
                {
                    var centr = ctx.SD_40.FirstOrDefault(_ => _.DOC_CODE == CurrentCenter.DOC_CODE);
                    {
                        if (centr != null)
                            centr.CENT_PARENT_DC = null;
                    }
                    ctx.SaveChanges();
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    WindowManager.ShowError(ex);
                }
            }
            RefreshData(null);
        }

        public ICommand DeleteCenterCommand
        {
            get { return new Command(DeleteCenter, _ => CurrentCenter != null); }
        }

        private void DeleteCenter(object obj)
        {
            var info = MessageBox.Show($"Вы уверены, что хотите удалить данный центр?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (info)
            {
                case MessageBoxResult.Yes:
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        using (var transaction = ctx.Database.BeginTransaction())
                        {
                            try
                            {
                                if (ctx.SD_40.Any(_ => _.CENT_PARENT_DC == CurrentCenter.DOC_CODE))
                                {
                                    WinManager.ShowWinUIMessageBox(
                                        $"В данном центре существуют вложеные разделы. Удаление не возможно.",
                                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }

                                var delCenter = ctx.SD_40.FirstOrDefault(_ => _.DOC_CODE == CurrentCenter.DOC_CODE);
                                if (delCenter == null)
                                    return;
                                ctx.Database.ExecuteSqlCommand(
                                    $"DELETE FROM HD_40 WHERE DOC_CODE = {CustomFormat.DecimalToSqlDecimal(delCenter.DOC_CODE)}");
                                ctx.SD_40.Remove(delCenter);
                                ctx.SaveChanges();
                                transaction.Commit();
                                CenterCollection.Remove(CurrentCenter);
                            }
                            catch (Exception ex)
                            {
                                if (transaction.UnderlyingTransaction.Connection != null)
                                    transaction.Rollback();
                                else
                                    transaction.Rollback();
                                WindowManager.ShowError(ex);
                            }
                        }
                    }
                    RaisePropertyChanged(nameof(CenterCollection));

                    break;
                case MessageBoxResult.No:
                    break;

            }
        }

        public override void CloseWindow(object form)
        {
            var vin = form as Window;
            if (IsCanSaveData)
            {
                var res = MessageBox.Show("Были внесены изменения, сохранить?", "Запрос",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (res == MessageBoxResult.Cancel)
                    return;
                if (res == MessageBoxResult.No)
                    vin?.Close();
                if (res != MessageBoxResult.Yes)
                    return;
                try
                {
                    SaveData(null);
                    vin?.Close();
                }
                catch (Exception ex)
                {
                    WinManager.ShowWinUIMessageBox(ex.Message, "Ошибка");
                }
            }
            else
            {
                vin?.Close();
            }
        }

        #endregion
    }
}