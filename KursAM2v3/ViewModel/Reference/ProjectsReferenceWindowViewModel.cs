using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.EntityViewModel.Employee;
using Core.Menu;
using Core.ViewModel.Base;
using Core.WindowsManager;
using KursAM2.Dialogs;
using KursAM2.View.KursReferences;

namespace KursAM2.ViewModel.Reference
{
    public class ProjectReferenceWindowViewModel : RSWindowViewModelBase
    {
        private Project myCurrentProject;

        public ProjectReferenceWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
        }

        public ProjectReferenceWindowViewModel(Window win) : this()
        {
            Form = win;
        }

        public ObservableCollection<Project> ProjectCollection { set; get; } =
            new ObservableCollection<Project>();

        public ObservableCollection<Project> DeletedProjectCollection { set; get; } =
            new ObservableCollection<Project>();

        public ObservableCollection<Employee> PersonaCollection { set; get; } = new ObservableCollection<Employee>();

        public Project CurrentProject
        {
            get => myCurrentProject;
            set
            {
                if (myCurrentProject != null && myCurrentProject.Equals(value)) return;
                myCurrentProject = value;
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
                    ProjectCollection.Clear();
                    foreach (var p in ctx.Projects.ToList())
                        ProjectCollection.Add(new Project(p) {State = RowStatus.NotEdited});
                    if (ProjectCollection.Count == 0)
                    {
                        var newRow = new Project
                        {
                            Id = Guid.NewGuid(),
                            Name = "Новый проект",
                            DateStart = DateTime.Today,
                            DateEnd = null,
                            IsClosed = false,
                            IsDeleted = false,
                            State = RowStatus.NotEdited
                        };
                        ProjectCollection.Add(newRow);
                        CurrentProject = newRow;
                    }
                }

                RaisePropertiesChanged(nameof(ProjectCollection));
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
                foreach (var p in DeletedProjectCollection)
                    p.Delete();
                foreach (var p in ProjectCollection.Where(_ => _.State != RowStatus.NotEdited))
                    p.Save(p);
                DeletedProjectCollection.Clear();
                foreach (var p in ProjectCollection.Where(_ => _.State != RowStatus.NotEdited))
                    p.myState = RowStatus.NotEdited;
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        private new bool IsCanSave()
        {
            if (ProjectCollection.Any(_ => _.State != RowStatus.NotEdited) ||
                DeletedProjectCollection.Count > 0)
                return ProjectCollection.All(p => p.Check());
            return false;
        }

        public void SetResponsible()
        {
            var f = StandartDialogs.SelectEmployee();
            if (f != null)
                CurrentProject.Responsible = f;
        }

        #region Commands

        public ICommand MoveToTopProjectCommand
        {
            get { return new Command(MoveToTopProject, _ => CurrentProject?.ParentId != null); }
        }

        private void MoveToTopProject(object obj)
        {
            CurrentProject.ParentId = null;
            using (var ctx = GlobalOptions.GetEntities())
            {
                var tx = ctx.Database.BeginTransaction();
                try
                {
                    var prj = ctx.Projects.FirstOrDefault(_ => _.Id == CurrentProject.Id);
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
            get { return new Command(AddNewProject, _ => CurrentProject != null); }
        }

        private void AddNewProject(object obj)
        {
            var newRow = new Project
            {
                State = RowStatus.NewRow,
                Id = Guid.NewGuid(),
                ParentId = CurrentProject.ParentId,
                DateStart = DateTime.Today
            };
            ProjectCollection.Add(newRow);
            CurrentProject = newRow;
        }

        private bool IsCanAddProject()
        {
            var pitem = ProjectCollection.FirstOrDefault(_ => _.Id == CurrentProject?.ParentId);
            if (pitem == null) return true;
            var pitem2 = ProjectCollection.FirstOrDefault(_ => _.ParentId == pitem.Id);
            if (pitem2 == null)
                return true;
            var pitem3 = ProjectCollection.FirstOrDefault(_ => _.Id == pitem2.ParentId);
            if (pitem3 == null) return true;
            var pitem4 = ProjectCollection.FirstOrDefault(_ => _.Id == pitem3.ParentId);
            if (pitem4 != null) return false;
            //var pitem3 = ProjectCollection.FirstOrDefault(_ => _.Id == pitem2.ParentId);
            return false;
        }

        public ICommand AddNewChildProjectCommand
        {
            get { return new Command(AddNewChildProject, _ => IsCanAddProject() && CurrentProject != null); }
        }

        private void AddNewChildProject(object obj)
        {
            var newRow = new Project
            {
                State = RowStatus.NewRow,
                Id = Guid.NewGuid(),
                ParentId = CurrentProject.Id,
                DateStart = DateTime.Today
            };
            ProjectCollection.Add(newRow);
            if (Form is ProjectReferenceView win)
                win.treeListView.FocusedNode.IsExpanded = true;
            CurrentProject = newRow;
        }

        public ICommand DeleteProjectCommand
        {
            get
            {
                return new Command(DeleteProject,
                    _ => CurrentProject != null && ProjectCollection.All(p => p.ParentId != CurrentProject.Id));
            }
        }

        private void DeleteProject(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var docs = ctx.ProjectsDocs.Where(_ => _.ProjectId == CurrentProject.Id);
                if (docs.Any())
                {
                    WinManager.ShowWinUIMessageBox("По проекту есть привязанные документы. Удалить проект нельзя.",
                        "Предупреждение"
                        , MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.None,
                        MessageBoxOptions.None);
                    return;
                }
            }

            if (CurrentProject.State != RowStatus.NewRow)
                DeletedProjectCollection.Add(CurrentProject);
            ProjectCollection.Remove(CurrentProject);
        }

        //    <MenuItem Header = "Добавить проект на текущем уровне" Command="{Binding AddNewProject}" />
        //<MenuItem Header = "Добавить подпроект" Command="{Binding AddNewChildProject}" />
        //<MenuItem Header = "Удалить проект" Command="{Binding DeleteProject}" />

        #endregion
    }
}