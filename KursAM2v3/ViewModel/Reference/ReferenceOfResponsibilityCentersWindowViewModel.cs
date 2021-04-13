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
        private Dictionary<decimal?, decimal> DocCodeToParentDC = new Dictionary<decimal?, decimal>();
        #endregion

        public override void RefreshData(object obj)
        {
            
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
                    DocCodeToParentDC.Clear();
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
                        foreach (var c in CenterCollection.Where(_ => _.State != RowStatus.NotEdited).OrderByDescending(_ => _.DocCode).ThenBy(_=>_.CentParentDC))
                            if (c.CentParentDC == null || c.CentParentDC > 0)
                            {
                                switch (c.State)
                                {
                                    case RowStatus.NewRow:
                                        DocCodeToParentDC.Add(c.DocCode, newNumDocCode);
                                        ctx.SD_40.Add(new SD_40
                                        {
                                            DOC_CODE = newNumDocCode,
                                            CENT_FULLNAME = c.FullName,
                                            CENT_NAME = c.Name,
                                            IS_DELETED = c.IsDeleted,
                                            CENT_PARENT_DC = c.CentParentDC
                                        });
                                        newNumDocCode++;
                                        break;
                                    case RowStatus.Edited:
                                        var old = ctx.SD_40.FirstOrDefault(_ => _.DOC_CODE == c.DocCode);
                                        if (old == null) return;
                                        old.DOC_CODE = c.DocCode;
                                        old.CENT_PARENT_DC = c.CentParentDC;
                                        old.CENT_FULLNAME = c.FullName;
                                        old.CENT_NAME = c.Name;
                                        old.IS_DELETED = c.IsDeleted;
                                        break;
                                }

                            }
                            else
                            {
                                DocCodeToParentDC.Add(c.DocCode, newNumDocCode);
                                ctx.SD_40.Add(new SD_40
                                {
                                    DOC_CODE = newNumDocCode,
                                    CENT_FULLNAME = c.FullName,
                                    CENT_NAME = c.Name,
                                    IS_DELETED = c.IsDeleted,
                                    CENT_PARENT_DC = DocCodeToParentDC[c.CentParentDC]
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
                DocCode = --DocCodeCounter,
                Name = "Новый центр"
            };

            CenterCollection.Add(newRow);

            RaisePropertyChanged(nameof(CenterCollection));
        }

        private bool IsCanAddCenter()
        {
            if (CurrentCenter == null)
                return false;
            var centerItem = CenterCollection.FirstOrDefault(_ => _.DocCode == CurrentCenter.CentParentDC);
            if (centerItem == null)
                return true;
            var centerItem2 = CenterCollection.FirstOrDefault(_ => _.CentParentDC == centerItem.DocCode);
            if (centerItem2 == null)
                return true;
            var centerItem3 = CenterCollection.FirstOrDefault(_ => _.DocCode == centerItem2.CentParentDC);
            if (centerItem3 == null)
                return true;
            var centerItem4 = CenterCollection.FirstOrDefault(_ => _.DocCode == centerItem3.CentParentDC);
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
                Name = "Новый центр",
                DocCode = --DocCodeCounter,
                CentParentDC = CurrentCenter.DocCode,
            };

            CenterCollection.Add(newRow);
            RaisePropertyChanged(nameof(CenterCollection));

            if (Form is ReferenceOfResponsibilityCentersView win)
                win.treeListView.FocusedNode.IsExpanded = true;
            CurrentCenter = newRow;
        }


        public ICommand MoveToTopCenterCommand
        {
            get { return new Command(MoveToTopCenter, _ => CurrentCenter?.DocCode != null); }
        }

        private void MoveToTopCenter(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var tx = ctx.Database.BeginTransaction();
                try
                {
                    var centr = ctx.SD_40.FirstOrDefault(_ => _.DOC_CODE == CurrentCenter.DocCode);
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
                                if (ctx.SD_40.Any(_ => _.CENT_PARENT_DC == CurrentCenter.DocCode))
                                {
                                    WinManager.ShowWinUIMessageBox(
                                        $"В данном центре существуют вложеные разделы. Удаление не возможно.",
                                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }

                                var delCenter = ctx.SD_40.FirstOrDefault(_ => _.DOC_CODE == CurrentCenter.DocCode);
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