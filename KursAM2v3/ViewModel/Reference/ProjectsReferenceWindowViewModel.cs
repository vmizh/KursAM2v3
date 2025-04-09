using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Data;
using DevExpress.Xpf.Grid;
using KursAM2.Dialogs;
using KursAM2.Repositories.Projects;
using KursAM2.View.KursReferences;
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
            RaisePropertyChanged();
        }
    }


    public override bool IsCanSaveData => IsCanSave();

    #endregion


    #region Methods

    private new bool IsCanSave()
    {
        if (Projects.Any(_ => _.State != RowStatus.NotEdited) ||
            DeletedProjects.Count > 0)
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
        try
        {
            foreach (var prj in myPojectRepository.LoadReference())
            {
                Projects.Add(new ProjectViewModel(prj));
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
        }
        catch (Exception ex)
        {
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

    public ICommand AddNewProjectCommand
    {
        get { return new Command(AddNewProject, _ => CurrentProject != null); }
    }

    private void AddNewProject(object obj)
    {
        var newRow = new ProjectViewModel
        {
            State = RowStatus.NewRow,
            Id = Guid.NewGuid(),
            ParentId = CurrentProject.ParentId,
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
