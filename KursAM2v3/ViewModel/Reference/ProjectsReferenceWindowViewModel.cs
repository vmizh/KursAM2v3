using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Data;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using DevExpress.XtraCharts.Native;
using KursAM2.Dialogs;
using KursAM2.Repositories.Projects;
using KursAM2.View.KursReferences;
using KursAM2.ViewModel.Reference.Dialogs;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Reference;

public sealed class ProjectReferenceWindowViewModel : RSWindowViewModelBase
{
    #region Fields

    private ProjectViewModel myCurrentProject;
    private readonly IProjectRepository myPojectRepository = new ProjectRepository(new ALFAMEDIAEntities(GlobalOptions.SqlConnectionString));
    private ProjectGroupViewModel myCurrentGroupProject;
    private ProjectViewModel myCurrentLinkGroupProject;

    #endregion

    #region Constructors

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

    #endregion

    #region Properties

    public override string LayoutName => "ProjectReferenceWindowViewModel";
    public override string WindowName => "Справочник проектов";

    public ObservableCollection<ProjectViewModel> Projects { set; get; } =
        new ObservableCollection<ProjectViewModel>();

    public ObservableCollection<ProjectViewModel> SelectedProjects { set; get; } =
        new ObservableCollection<ProjectViewModel>();

    public ObservableCollection<ProjectViewModel> DeletedProjects { set; get; } =
        new ObservableCollection<ProjectViewModel>();

    public ObservableCollection<ProjectGroupViewModel> GroupProjects { set; get; } =
        new ObservableCollection<ProjectGroupViewModel>();

    public ObservableCollection<ProjectGroupViewModel> SelectedGroupProjects { set; get; } =
        new ObservableCollection<ProjectGroupViewModel>();

    public ObservableCollection<ProjectGroupViewModel> DeletedGroupProjects { set; get; } =
        new ObservableCollection<ProjectGroupViewModel>();

    public ObservableCollection<ProjectViewModel> LinkGroupProjects { set; get; } =
        new ObservableCollection<ProjectViewModel>();

    public List<Guid> GroupProjLinkDeleted { get; } = new List<Guid>();

    public ProjectViewModel CurrentProject
    {
        get => myCurrentProject;
        set
        {
            if (Equals(myCurrentProject, value)) return;
            myCurrentProject = value;
            RaisePropertyChanged();
        }
    }

    public ProjectGroupViewModel CurrentGroupProject
    {
        get => myCurrentGroupProject;
        set
        {
            if (Equals(myCurrentGroupProject, value)) return;
            myCurrentGroupProject = value;
            LinkGroupProjects.Clear();
            if(myCurrentGroupProject is not null)
                foreach (var prj in myCurrentGroupProject.Projects)
                {
                   LinkGroupProjects.Add(prj);
                }
            RaisePropertyChanged();
        }
    }

    public ProjectViewModel CurrentLinkGroupProject
    {
        get => myCurrentLinkGroupProject;
        set
        {
            if (Equals(myCurrentLinkGroupProject, value)) return;
            myCurrentLinkGroupProject = value;
            RaisePropertyChanged();
        }
    }


    public override bool IsCanSaveData => IsCanSave();

    #endregion
    
    #region Methods

    private new bool IsCanSave()
    {
        if (Projects.Any(_ => _.State != RowStatus.NotEdited) ||
            GroupProjects.Any(_ => _.State != RowStatus.NotEdited) ||
            DeletedProjects.Count > 0 || DeletedGroupProjects.Count > 0)
            return Projects.All(p => p.Check());
        
        return false;
    }

    public void SetResponsible()
    {
        var f = StandartDialogs.SelectEmployee();
        if (f != null)
            CurrentProject.Responsible = f;
    }

    #endregion

    #region Commands

    public override void RefreshData(object obj)
    {
        Projects.Clear();
        DeletedProjects.Clear();
        GroupProjects.Clear();
        DeletedGroupProjects.Clear();
        GroupProjLinkDeleted.Clear();
        try
        {
            foreach (var prj in myPojectRepository.LoadReference())
            {
                Projects.Add(new ProjectViewModel(prj){myState = RowStatus.NotEdited });
            }
            foreach (var grp in myPojectRepository.LoadGroups())
            {
                GroupProjects.Add(new ProjectGroupViewModel(grp) {myState = RowStatus.NotEdited });
            }
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
            myPojectRepository.BeginTransaction();
            var resultProj = myPojectRepository.SaveReference(Projects.Where(_ => _.State != RowStatus.NotEdited).Select(_ => _.Entity).ToList(),
                DeletedProjects.Any() ? DeletedProjects.Select(_ => _.Id).ToList() : null);
            var resultGroup = myPojectRepository.SaveGroups(GroupProjects.Where(_ => _.State != RowStatus.NotEdited).Select(_ => _.Entity).ToList(),
                DeletedGroupProjects.Any() ? DeletedGroupProjects.Select(_ => _.Id).ToList() : null, GroupProjLinkDeleted);
            
            if (!resultProj.Result || !resultGroup.Result)
            {
                WindowManager.ShowMessage($"{resultProj.ErrorText}\n{resultGroup.ErrorText}","Ошибка сохранения",MessageBoxImage.Error);
                return;
            }
            myPojectRepository.SaveChanges();
            myPojectRepository.CommitTransaction(); 
            foreach (var p in Projects)
            {
                p.myState = RowStatus.NotEdited;
            }
            foreach (var g in GroupProjects)
            {
                g.myState = RowStatus.NotEdited;
            }
            DeletedProjects.Clear();
            DeletedGroupProjects.Clear();
            GroupProjLinkDeleted.Clear();
        }
        catch (Exception ex)
        {
            myPojectRepository.RollbackTransaction();
            WindowManager.ShowError(ex);
        }
    }

    public ICommand DeleteManagerCommand
    {
        get { return new Command(ManagerSetToNull, _ => CurrentProject != null && CurrentProject.Responsible is not null); }
    }

    private void ManagerSetToNull(object obj)
    {
        CurrentProject.Responsible = null;
        if (Form is ProjectReferenceView frm)
        {
            frm.tableViewProjects.CloseEditor();
        }
    }

    public ICommand AddProjectCommand
    {
        get { return new Command(AddNewProject, _ => true); }
    }

    private void AddNewProject(object obj)
    {
        var newRow = new ProjectViewModel
        {
            State = RowStatus.NewRow,
            Id = Guid.NewGuid(),
            ParentId = null,
            DateStart = DateTime.Today
        };
        Projects.Add(newRow);
        CurrentProject = newRow;
    }

    public ICommand DeleteProjectCommand
    {
        get
        {
            return new Command(DeleteProject,
                _ => CurrentProject != null);
        }
    }

    private void DeleteProject(object obj)
    {
        var WinManager = new WindowManager();
        if (CurrentProject.State != RowStatus.NewRow)
            DeletedProjects.Add(CurrentProject);
        Projects.Remove(CurrentProject);
    }

    public ICommand AddGroupProjectCommand
    {
        get { return new Command(AddGroupProject, _ => true); }
    }

    private void AddGroupProject(object obj)
    {
        GroupProjects.Add(new ProjectGroupViewModel
        {
            Id = Guid.NewGuid(),
            Name = "Новая группа",
            State = RowStatus.NewRow
        });
    }

    public ICommand DeleteGroupProjectCommand
    {
        get { return new Command(DeleteGroupProject, _ => CurrentGroupProject is not null); }
    }

    private void DeleteGroupProject(object obj)
    {
        if(CurrentGroupProject.State != RowStatus.NewRow)
            DeletedGroupProjects.Add(CurrentGroupProject);
        GroupProjects.Remove(CurrentGroupProject);
    }

    public ICommand AddLinkGroupProjectCommand
    {
        get { return new Command(AddLinkGroupProject, _ => CurrentGroupProject is not null); }
    }

    private void AddLinkGroupProject(object obj)
    {
        var ctx = new ProjectSelectDialog(CurrentGroupProject.Projects.Select(_ => _.Id).ToList());
        var service = this.GetService<IDialogService>("DialogServiceUI");
        if (service.ShowDialog(MessageButton.OKCancel, "Выбор проектов.", ctx) == MessageResult.Cancel) return;
        foreach (var prj in ctx.SelectedRows)
        {
            LinkGroupProjects.Add(prj);
            CurrentGroupProject.Projects.Add(prj);
            CurrentGroupProject.Entity.ProjectGroupLink.Add(new ProjectGroupLink()
            {
                Id = Guid.NewGuid(),
                GroupId = CurrentGroupProject.Id,
                ProjectId = prj.Id
            });
        }
        if(CurrentGroupProject.myState != RowStatus.NewRow)
            CurrentGroupProject.myState = RowStatus.Edited;
    }

    public ICommand DeleteLinkGroupProjectCommand
    {
        get { return new Command(DeleteLinkGroupProject, _ => CurrentLinkGroupProject is not null); }
    }

    private void DeleteLinkGroupProject(object obj)
    {
        LinkGroupProjects.Remove(CurrentLinkGroupProject);
        var link = CurrentGroupProject.Entity.ProjectGroupLink.FirstOrDefault(_ =>
            _.ProjectId == CurrentLinkGroupProject.Id);
        if (link is not null)
        {
            CurrentGroupProject.Entity.ProjectGroupLink.Remove(link);
            if(CurrentGroupProject.myState != RowStatus.NewRow)
                CurrentGroupProject.myState = RowStatus.Edited;
            GroupProjLinkDeleted.Add(link.Id);
        }
    }


    public override void UpdateVisualObjects()
    {
        base.UpdateVisualObjects();
        if (Form is ProjectReferenceView frm)
        {
            frm.gridGroupProjects.TotalSummary.Clear();
            foreach (var col in frm.gridProjects.Columns)
            {
                switch (col.FieldName)
                {
                    case "Name":
                        frm.gridProjects.TotalSummary.Add(new GridSummaryItem()
                        {
                            FieldName = col.FieldName,
                            SummaryType = SummaryItemType.Count,
                            //Alignment = GridSummaryItemAlignment.Right,
                            DisplayFormat = "{0:n0}",
                            ShowInColumn = col.FieldName
                        });
                        break;
                }
            }
        }

    }

    #endregion
}
