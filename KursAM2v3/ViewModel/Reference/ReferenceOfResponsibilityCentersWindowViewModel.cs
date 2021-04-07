using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using Core.ViewModel.Common;
using Core.WindowsManager;
using Data;
using KursAM2.Dialogs;
using KursAM2.View.KursReferences;

namespace KursAM2.ViewModel.Reference
{
    public class ReferenceOfResponsibilityCentersWindowViewModel : RSWindowViewModelBase
    {
        private SD_40ViewModel myCurrentCenter;
        
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

        public ObservableCollection<SD_40ViewModel> CenterCollection { set; get; } =
            new ObservableCollection<SD_40ViewModel>();

        public ObservableCollection<SD_40ViewModel> DeletedCenterCollection { set; get; } =
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

        public override bool IsCanSaveData => IsCanSave();

        public override void RefreshData(object obj)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    CenterCollection.Clear();
                    foreach (var p in ctx.SD_40.ToList())
                        CenterCollection.Add(new SD_40ViewModel(p) {State = RowStatus.NotEdited});
                    if (CenterCollection.Count == 0)
                    {
                        var newRow = new SD_40ViewModel()
                        {
                            DOC_CODE = 1,
                            CENT_FULLNAME = "Новый центр",
                            CENT_NAME = "Новый центр",
                            CENT_PARENT_DC = 1,
                            IS_DELETED = 0,
                            State = RowStatus.NotEdited
                        };
                        CenterCollection.Add(newRow);
                        CurrentCenter = newRow;
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
            try
            {
                foreach (var p in DeletedCenterCollection)
                    p.Delete();
                foreach (var p in CenterCollection.Where(_ => _.State != RowStatus.NotEdited))
                    p.Save();
                DeletedCenterCollection.Clear();
                foreach (var p in CenterCollection.Where(_ => _.State != RowStatus.NotEdited))
                    p.myState = RowStatus.NotEdited;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        private new bool IsCanSave()
        {
            if (CenterCollection.Any(_ => _.State != RowStatus.NotEdited) ||
                DeletedCenterCollection.Count > 0)
                return CenterCollection.All(p => p.Check());
            return false;
        }

       
        #region Commands

        public ICommand MoveToTopProjectCommand
        {
            get { return new Command(MoveToTopProject, _ => CurrentCenter?.CENT_PARENT_DC != null); }
        }

        private void MoveToTopProject(object obj)
        {
            CurrentCenter.CENT_PARENT_DC = null;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var tx = ctx.Database.BeginTransaction();
                try
                {
                    var centr = ctx.SD_40.FirstOrDefault(_ => _.CENT_PARENT_DC == CurrentCenter.CENT_PARENT_DC);
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

        public ICommand AddNewCenterCommand
        {
            get { return new Command(AddNewCenter, _ => CurrentCenter != null); }
        }

        private void AddNewCenter(object obj)
        {
            var newRow = new SD_40ViewModel()
            {
                State = RowStatus.NewRow,
                CENT_FULLNAME = CurrentCenter.CENT_FULLNAME,
                CENT_NAME = CurrentCenter.CENT_NAME,
                CENT_PARENT_DC = CurrentCenter.CENT_PARENT_DC,
                DOC_CODE = CurrentCenter.DOC_CODE,
                IS_DELETED = 0
            };
            CenterCollection.Add(newRow);
            CurrentCenter = newRow;
        }

        private bool IsCanAddCenter()
        {
            var pitem = CenterCollection.FirstOrDefault(_ => _.CENT_PARENT_DC == CurrentCenter?.CENT_PARENT_DC);
            if (pitem == null)
                return true;
            var pitem2 = CenterCollection.FirstOrDefault(_ => _.CENT_PARENT_DC == pitem.CENT_PARENT_DC);
            if (pitem2 == null)
                return true;
            var pitem3 = CenterCollection.FirstOrDefault(_ => _.CENT_PARENT_DC == pitem2.CENT_PARENT_DC);
            if (pitem3 == null)
                return true;
            var pitem4 = CenterCollection.FirstOrDefault(_ => _.CENT_PARENT_DC == pitem3.CENT_PARENT_DC);
            if (pitem4 != null) 
                return false;
            return false;
        }

        /*public ICommand AddNewChildProjectCommand
        {
            get { return new Command(AddNewChildProject, _ => IsCanAddCenter() && CurrentCenter != null); }
        }

        private void AddNewChildProject(object obj)
        {
            var newRow = new Project
            {
                State = RowStatus.NewRow,
                Id = Guid.NewGuid(),
                ParentId = CurrentCenter.Id,
                DateStart = DateTime.Today
            };
            CenterCollection.Add(newRow);
            if (Form is ProjectReferenceView win)
                win.treeListView.FocusedNode.IsExpanded = true;
            CurrentCenter = newRow;
        }*/

        public ICommand DeleteCenterCommand
        {
            get
            {
                return new Command(DeleteCenter,
                    _ => CurrentCenter != null && CenterCollection.All(p => p.CENT_PARENT_DC != CurrentCenter.CENT_PARENT_DC));
            }
        }

        private void DeleteCenter(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var docs = ctx.SD_40.Where(_ => _.CENT_PARENT_DC == CurrentCenter.CENT_PARENT_DC);
                if (docs.Any())
                {
                    WinManager.ShowWinUIMessageBox("По проекту есть привязанные документы. Удалить проект нельзя.",
                        "Предупреждение"
                        , MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None,
                        MessageBoxOptions.None);
                    return;
                }
            }

            if (CurrentCenter.State != RowStatus.NewRow)
                DeletedCenterCollection.Add(CurrentCenter);
            CenterCollection.Remove(CurrentCenter);
        }

        //    <MenuItem Header = "Добавить проект на текущем уровне" Command="{Binding AddNewProject}" />
        //<MenuItem Header = "Добавить подпроект" Command="{Binding AddNewChildProject}" />
        //<MenuItem Header = "Удалить проект" Command="{Binding DeleteProject}" />

        #endregion
    }
}