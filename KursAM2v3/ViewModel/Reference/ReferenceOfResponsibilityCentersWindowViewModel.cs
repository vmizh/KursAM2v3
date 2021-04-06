using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
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
        private CentrOfResponsibility myCurrentCenter;

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

        public ObservableCollection<CentrOfResponsibility> CenterCollection { set; get; } =
            new ObservableCollection<CentrOfResponsibility>();

        public ObservableCollection<CentrOfResponsibility> DeletedCenterCollection { set; get; } =
            new ObservableCollection<CentrOfResponsibility>();


        public CentrOfResponsibility CurrentCenter
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
                        CenterCollection.Add(new CentrOfResponsibility(p) {State = RowStatus.NotEdited});
                    if (CenterCollection.Count == 0)
                    {
                        var newRow = new CentrOfResponsibility()
                        {
                            DocCode = 1,
                            Name = "Новый центр",
                            FullName = "Новый центр",
                            IsDeleted = false,
                            CENT_PARENT_DC = 1,
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

        public void SetResponsible()
        {
            var f = StandartDialogs.SelectEmployee();
            if (f != null)
                CurrentCenter.Responsible = f;
        }

        #region Commands

        public ICommand MoveToTopProjectCommand
        {
            get { return new Command(MoveToTopProject, _ => CurrentCenter?.ParentId != null); }
        }

        private void MoveToTopProject(object obj)
        {
            CurrentCenter.ParentId = null;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var tx = ctx.Database.BeginTransaction();
                try
                {
                    var prj = ctx.Projects.FirstOrDefault(_ => _.Id == CurrentCenter.Id);
                    {
                        if (prj != null) prj.ParentId = null;
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

        public ICommand AddNewProjectCommand
        {
            get { return new Command(AddNewProject, _ => CurrentCenter != null); }
        }

        private void AddNewProject(object obj)
        {
            var newRow = new Project
            {
                State = RowStatus.NewRow,
                Id = Guid.NewGuid(),
                ParentId = CurrentCenter.ParentId,
                DateStart = DateTime.Today
            };
            CenterCollection.Add(newRow);
            CurrentCenter = newRow;
        }

        private bool IsCanAddProject()
        {
            var pitem = CenterCollection.FirstOrDefault(_ => _.Id == CurrentCenter?.ParentId);
            if (pitem == null) return true;
            var pitem2 = CenterCollection.FirstOrDefault(_ => _.ParentId == pitem.Id);
            if (pitem2 == null)
                return true;
            var pitem3 = CenterCollection.FirstOrDefault(_ => _.Id == pitem2.ParentId);
            if (pitem3 == null) return true;
            var pitem4 = CenterCollection.FirstOrDefault(_ => _.Id == pitem3.ParentId);
            if (pitem4 != null) return false;
            //var pitem3 = ProjectCollection.FirstOrDefault(_ => _.Id == pitem2.ParentId);
            return false;
        }

        public ICommand AddNewChildProjectCommand
        {
            get { return new Command(AddNewChildProject, _ => IsCanAddProject() && CurrentCenter != null); }
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
        }

        public ICommand DeleteProjectCommand
        {
            get
            {
                return new Command(DeleteProject,
                    _ => CurrentCenter != null && CenterCollection.All(p => p.ParentId != CurrentCenter.Id));
            }
        }

        private void DeleteProject(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var docs = ctx.ProjectsDocs.Where(_ => _.ProjectId == CurrentCenter.Id);
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