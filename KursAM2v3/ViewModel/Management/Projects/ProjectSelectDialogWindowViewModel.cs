﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.ViewModel.Base;
using DevExpress.Xpf.Grid.TreeList;
using KursAM2.View.Projects;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.References;
using KursRepositories.Repositories.Projects;

namespace KursAM2.ViewModel.Management.Projects;

public sealed class ProjectSelectDialogWindowViewModel : RSWindowViewModelBase
{
    #region Constructors

    public ProjectSelectDialogWindowViewModel(DocumentType documentType, decimal documentDC, string invoiceDesc,
        bool isCurrencyConvert)
    {
        DocumentType = documentType;
        DocumentDC = documentDC;
        IsCurrencyConvert = isCurrencyConvert;
        RefreshData(null);
        WindowName = $"Связать проекты со счетом {invoiceDesc}";
    }

    #endregion

    #region Fields

    private readonly IProjectRepository myRepository = new ProjectRepository(GlobalOptions.GetEntities());
    private ProjectLinkItem myCurrentProject;
    public readonly bool IsCurrencyConvert;
    private bool myIsShowAll;

    #endregion

    #region Properties

    public override string LayoutName => "ProjectSelectDialogWindowView";

    public bool IsShowAll
    {
        get => myIsShowAll;
        set
        {
            if (value == myIsShowAll) return;
            myIsShowAll = value;
            showProjects();
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<ProjectLinkItem> Projects { set; get; } = new ObservableCollection<ProjectLinkItem>();
    public ObservableCollection<ProjectLinkItem> AllProjects { set; get; } = new ObservableCollection<ProjectLinkItem>();

    public DocumentType DocumentType { get; set; }
    public decimal DocumentDC { set; get; }

    public ProjectLinkItem CurrentProject
    {
        set
        {
            if (Equals(value, myCurrentProject)) return;
            myCurrentProject = value;
            RaisePropertyChanged();
        }
        get => myCurrentProject;
    }

    #endregion

    #region Commands

    public ICommand ValueChangedCommad
    {
        get { return new Command(ValueChanged, _ => true); }
    }

    private void ValueChanged(object obj)
    {
        if (obj is not TreeListCellValueChangedEventArgs param) return;
        var ids = myRepository.GetAllTreeProjectIds(CurrentProject.Id);
        foreach (var id in ids.Where(_ => _ != CurrentProject.Id))
        {
            var p = Projects.Single(_ => _.Id == id);
            p.IsSelected = false;
        }
    }

    public override void Ok(object obj)
    {
        DialogResult = true;
        Form.Close();
    }

    public override void Cancel(object obj)
    {
        DialogResult = false;
        Form.Close();
    }

    public override void RefreshData(object d)
    {
        AllProjects.Clear();
        var oldPrjs = myRepository.GetDocumentsProjects(DocumentType, DocumentDC, IsCurrencyConvert);
        foreach (var prj in GlobalOptions.ReferencesCache.GetProjectsAll().Cast<Project>())
        {
            var newItem = new ProjectLinkItem
            {
                Project = prj,
                DateEnd = prj.DateEnd,
                DateStart = prj.DateStart,
                IsClosed = prj.IsClosed,
                Employee = prj.Employee as Employee,
                Note = prj.Notes
                
            };
            if (oldPrjs.Contains(newItem.Id)) newItem.IsSelected = true;
            AllProjects.Add(newItem);
            showProjects();
        }
    }

    private void showProjects()
    {
        Projects.Clear();
        foreach (var p in AllProjects)
        {
            if(IsShowAll || !p.IsClosed)
                Projects.Add(p);
        }
    }

    public override void UpdateVisualObjects()
    {
        base.UpdateVisualObjects();
        if (Form is not SelectProjectDialogView frm) return;
        foreach (var col in frm.gridProjects.Columns)
        {
            switch (col.FieldName)
            {
                case "IsSelected":
                case "IsMain":
                    col.ReadOnly = false;
                    break;
                case "State":
                    col.Visible = false;
                    break;
            }
        }
    }

    #endregion
}
