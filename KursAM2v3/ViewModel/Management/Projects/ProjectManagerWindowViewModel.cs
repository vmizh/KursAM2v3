using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Repositories.Projects;
using KursDomain;
using KursDomain.Menu;
using KursDomain.References;
using MoreLinq;

namespace KursAM2.ViewModel.Management.Projects;

public sealed class ProjectManagerWindowViewModel : RSWindowViewModelBase
{
    #region Fields

    private readonly IProjectRepository myProjectRepository;
    private readonly ALFAMEDIAEntities myContext;

    private Project myCurrentProject;
    private bool myIsAllProject;

    #endregion

    #region Constructors

    public ProjectManagerWindowViewModel()
    {
        LeftMenuBar = MenuGenerator.BaseLeftBar(this);
        RightMenuBar = MenuGenerator.RefreshExitOnlyRightBar(this);

        myContext = GlobalOptions.GetEntities();
        myProjectRepository = new ProjectRepository(myContext);
        RefreshData(null);
    }

 
    #endregion

    #region Properties

    public override string WindowName => "Управление проектами";
    public override string LayoutName => "ProjectManagerWindowViewModel";

    public ObservableCollection<Project> Projects { set; get; } = new ObservableCollection<Project>();
    


    public Project CurrentProject
    {
        set
        {
            if (Equals(value, myCurrentProject)) return;
            myCurrentProject = value;
            RaisePropertyChanged();
        }
        get => myCurrentProject;
    }

    public bool IsAllProject
    {
        set
        {
            if (value == myIsAllProject) return;
            myIsAllProject = value;
            RaisePropertyChanged();
        }
        get => myIsAllProject;
    }

    #endregion

    #region Methods


    #endregion

    #region Commands

    public override void RefreshData(object obj)
    {
        try
        {
            Projects.Clear();
            foreach (var prj in myProjectRepository.LoadReference().OrderBy(_ => _.Name))
            {
                var newItem = new Project();
                newItem.LoadFromEntity(prj, GlobalOptions.ReferencesCache);
                Projects.Add(newItem);
            }
        }
        catch (Exception ex)
        {
            WindowManager.ShowError(ex);
        }
    }

    #endregion
}
