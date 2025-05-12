using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Core.ViewModel.Base;
using DevExpress.Xpf.Grid;
using KursAM2.View.Base;
using KursDomain;
using KursDomain.Menu;
using KursDomain.References;
using KursRepositories.Repositories.Projects;

namespace KursAM2.ViewModel.Reference.Dialogs;

public class ProjectSelectDialog : RSWindowViewModelBase
{
    #region Constructors

    public ProjectSelectDialog(List<Guid> excludeIds)
    {
        myExcludeIds = excludeIds ?? new List<Guid>();
        IsDialog = true;
        LeftMenuBar = MenuGenerator.BaseLeftBar(this);
        RefreshData();
    }

    #endregion

    #region Fields

    private List<Guid> myExcludeIds;
    private readonly IProjectRepository myProjectRepository = new ProjectRepository(GlobalOptions.GetEntities());
    private ProjectViewModel myCurrentRow;

    #endregion

    #region Properties

    public UserControl CustomDataUserControl { set; get; } = new TableBaseUC();
    public override string LayoutName => "ProjectSelectDialog";

    public ObservableCollection<ProjectViewModel> Rows { set; get; } = new ObservableCollection<ProjectViewModel>();

    public ObservableCollection<ProjectViewModel> SelectedRows { set; get; } =
        new ObservableCollection<ProjectViewModel>();

    public ProjectViewModel CurrentRow
    {
        get => myCurrentRow;
        set
        {
            if (Equals(value, myCurrentRow)) return;
            myCurrentRow = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Command

    public override void RefreshData()
    {
        Rows.Clear();
        foreach (var prj in myProjectRepository.LoadReference())
        {
            if(myExcludeIds.All(_ => _ != prj.Id))
                Rows.Add(new ProjectViewModel(prj));
        }
    }

    public override void UpdateVisualObjects()
    {
        if (CustomDataUserControl is TableBaseUC frm)
        {
            frm.gridRows.SelectionMode = MultiSelectMode.Row;
            frm.tableViewRows.NavigationStyle = GridViewNavigationStyle.Cell;
        }
    }

    #endregion
}
