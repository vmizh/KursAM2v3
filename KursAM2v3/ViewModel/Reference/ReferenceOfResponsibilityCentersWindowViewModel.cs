using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.View.KursReferences;
using System;
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

        private SD_40ViewModel myCurrentCenter;
        private decimal myCenterCount;

        #endregion

        #region Properties

        public ObservableCollection<SD_40ViewModel> CenterCollection { set; get; } =
            new ObservableCollection<SD_40ViewModel>();


        public SD_40ViewModel CurrentCenter
        {
            get => myCurrentCenter;
            set
            {
                if (myCurrentCenter != null && myCurrentCenter.Equals(value)) return;
                myCurrentCenter = value;
                RaisePropertyChanged();
            }
        }

        public override bool IsCanSaveData => CenterCollection.Any(_ => _.State != RowStatus.NotEdited);

        public decimal CenterCount
        {
            get => myCenterCount + CenterCollection.Count(_ => _.State != RowStatus.NotEdited);
            set
            {
                if (myCenterCount == value)
                    myCenterCount++;
                myCenterCount = value;
                RaisePropertyChanged();
            }
        }

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
                    CenterCollection.Clear();
                    var newNumDocCode = ctx.SD_40.Any() ? ctx.SD_40.Max(_ => _.DOC_CODE) + 1 : 10400000001;
                    CenterCount = newNumDocCode;
                    foreach (var p in ctx.SD_40.ToList())
                        CenterCollection.Add(new SD_40ViewModel(p) { State = RowStatus.NotEdited });
                    if (CenterCollection.Count == 0)
                    {
                        var newRow = new SD_40ViewModel()
                        {
                            DOC_CODE = newNumDocCode,
                            CENT_FULLNAME = "Новый центр",
                            CENT_NAME = "Новый центр",
                            IS_DELETED = 0,
                            State = RowStatus.NotEdited
                        };
                        CenterCollection.Add(newRow);
                    }
                }

                RaisePropertiesChanged(nameof(CenterCollection));
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
                        foreach (var c in CenterCollection.Where(_ => _.State != RowStatus.NotEdited))
                            switch (c.State)
                            {
                                case RowStatus.NewRow:
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
                        ctx.SaveChanges();
                        transaction.Commit();

                        foreach (var с in CenterCollection)
                            с.myState = RowStatus.NotEdited;
                        CenterCount = 0;
                        MainReferences.Refresh();
                        RaisePropertiesChanged(nameof(IsCanSaveData));
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                    }
                }
            }
            RefreshData();
        }


        #region Commands

        public ICommand AddNewCenterCommand
        {
            get { return new Command(AddNewCenter, _ => true); }
        }

        private void AddNewCenter(object obj)
        {
            var newRow = new SD_40ViewModel()
            {
                State = RowStatus.NewRow,
                DOC_CODE = CenterCount,
                CENT_NAME = "Новый центр"

            };
            CenterCollection.Add(newRow);

            RaisePropertiesChanged(nameof(CenterCollection));
        }

        private bool IsCanAddCenter()
        {
            var pitem = CenterCollection.FirstOrDefault(_ => _.DOC_CODE == CurrentCenter.CENT_PARENT_DC);
            if (pitem == null)
                return true;
            var pitem2 = CenterCollection.FirstOrDefault(_ => _.CENT_PARENT_DC == pitem.DOC_CODE);
            if (pitem2 == null)
                return true;
            var pitem3 = CenterCollection.FirstOrDefault(_ => _.DOC_CODE == pitem2.CENT_PARENT_DC);
            if (pitem3 == null)
                return true;
            var pitem4 = CenterCollection.FirstOrDefault(_ => _.DOC_CODE == pitem3.CENT_PARENT_DC);
            if (pitem4 != null)
                return false;
            return false;
        }

        public ICommand AddNewSectionCenterCommand
        {
            get { return new Command(AddNewSectionCenter, _ => IsCanAddCenter() && CurrentCenter != null); }
        }

        private void AddNewSectionCenter(object obj)
        {
            var newRow = new SD_40ViewModel()
            {
                State = RowStatus.NewRow,
                CENT_NAME = "Новый центр",
                DOC_CODE = CenterCount,
                CENT_PARENT_DC = CurrentCenter.DOC_CODE,

            };
            CenterCollection.Add(newRow);

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
            CurrentCenter.CENT_PARENT_DC = null;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var tx = ctx.Database.BeginTransaction();
                try
                {
                    var centr = ctx.SD_40.FirstOrDefault(_ => _.DOC_CODE == CurrentCenter.DOC_CODE);
                    {
                        if (centr != null) centr.CENT_PARENT_DC = null;
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
        }



        public ICommand DeleteCenterCommand => new Command(DeleteCenter, _ => CurrentCenter != null);

        private void DeleteCenter(object obj)
        {
            var res = MessageBox.Show($"Вы уверены, что хотите удалить данный центр {CurrentCenter}?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        try
                        {
                            if (ctx.SD_40.Any(_ => _.CENT_PARENT_DC == CurrentCenter.DocCode))
                            {
                                WinManager.ShowWinUIMessageBox(
                                    $"В {CurrentCenter} существуют вложеные разделы. Удаление не возможно.",
                                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Stop);
                                return;
                            }

                            var old = ctx.SD_40.FirstOrDefault(_ => _.DOC_CODE == CurrentCenter.DOC_CODE);
                            if (old == null) return;
                            ctx.SD_40.Remove(old);
                            ctx.SaveChanges();
                            CenterCollection.Remove(CurrentCenter);
                        }
                        catch (Exception ex)
                        {
                            WindowManager.ShowError(ex);
                        }

                    }

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