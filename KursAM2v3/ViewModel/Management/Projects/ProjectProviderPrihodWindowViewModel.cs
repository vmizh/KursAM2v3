using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.CommonReferences;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Xpf.Grid;
using KursAM2.View.Management;
using KursDomain;
using KursDomain.Documents.CommonReferences;
using KursDomain.ICommon;
using KursDomain.Menu;

namespace KursAM2.ViewModel.Management.Projects
{
    public class ProjectProviderPrihodWindowViewModel : RSWindowViewModelBase
    {
        private ProjectPrihodDistributeRow myCurrentDistRow;
        private ProjectPrihodDocRow myCurrentRowDocument;
        private Project myDefaultProject;
        private DateTime myEndDate;
        private bool myIsAll;
        private bool myIsNotUsluga;
        private bool myIsUsluga;
        private List<Project> myProjectList;
        private bool mySelectionMode;
        private DateTime myStartDate;
        private WindowManager winManager = new WindowManager();

        public ProjectProviderPrihodWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            EndDate = DateTime.Today;
            StartDate = EndDate.AddDays(-30);
            SelectionMode = true;
            IsAll = false;
            ProjectList = new List<Project>(MainReferences.Projects.Values.ToList());
        }

        public ProjectProviderPrihodWindowViewModel(Window form)
        {
            Form = form;
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            EndDate = DateTime.Today;
            StartDate = EndDate.AddDays(-30);
        }

        public ObservableCollection<ProjectPrihodDocRow> RowDocuments { set; get; } =
            new ObservableCollection<ProjectPrihodDocRow>();

        public ObservableCollection<ProjectPrihodDistributeRow> DistributeRows { set; get; } =
            new ObservableCollection<ProjectPrihodDistributeRow>();

        public ObservableCollection<ProjectPrihodDistributeRow> SelectedDistRows { set; get; } =
            new ObservableCollection<ProjectPrihodDistributeRow>();

        public ObservableCollection<ProjectPrihodDistributeRow> DeletedDistributeRows { set; get; } =
            new ObservableCollection<ProjectPrihodDistributeRow>();

        public ProjectPrihodDistributeRow CurrentDistRow
        {
            get => myCurrentDistRow;
            set
            {
                if (Equals(myCurrentDistRow,value)) return;
                myCurrentDistRow = value;
                IsUsluga = !myCurrentDistRow?.IsUsluga ?? false;
                IsNotUsluga = myCurrentDistRow?.IsUsluga ?? false;
                RaisePropertyChanged();
            }
        }

        public override bool IsCanSaveData => DataChecked();

        public bool SelectionMode
        {
            get => mySelectionMode;
            set
            {
                if (mySelectionMode == value) return;
                mySelectionMode = value;
                if (mySelectionMode)
                {
                    if (Form is ProjectProviderPrihodView frm)
                        frm.gridrowDocuments.SelectionMode = MultiSelectMode.None;
                }
                else
                {
                    if (Form is ProjectProviderPrihodView frm)
                        frm.gridrowDocuments.SelectionMode = MultiSelectMode.Row;
                }

                RaisePropertyChanged();
            }
        }

        public bool IsAll
        {
            get => myIsAll;
            set
            {
                if (myIsAll == value) return;
                myIsAll = value;
                RefreshData(null);
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ListingTypeName));
            }
        }

        public string ListingTypeName => IsAll ? "Все" : "Не распределенные";

        public List<Project> ProjectList
        {
            get => myProjectList;
            set
            {
                if (myProjectList == value) return;
                myProjectList = value;
                RaisePropertyChanged();
            }
        }

        public DateTime StartDate
        {
            get => myStartDate;
            set
            {
                if (myStartDate == value) return;
                myStartDate = value;
                RaisePropertyChanged();
            }
        }

        public ProjectPrihodDocRow CurrentRowDocument
        {
            get => myCurrentRowDocument;
            set
            {
                if (IsCanSaveData)
                {
                    var res = MessageBox.Show("В документ были внесены изменения, сохранить?", "Запрос",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);
                    switch (res)
                    {
                        case MessageBoxResult.Yes:
                            SaveData(null);
                            break;
                        case MessageBoxResult.No:
                            DeletedDistributeRows.Clear();
                            DistributeRows.Clear();
                            break;
                        case MessageBoxResult.Cancel:
                            return;
                    }
                }

                if (Equals(myCurrentRowDocument,value)) return;
                myCurrentRowDocument = value;
                if (myCurrentRowDocument == null) return;
                if (!CurrentRowDocument.IsUsluga)
                {
                    DistributeRows.Clear();
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        foreach (var d in ctx.ProjectProviderPrihod.Where(_ => _.RowId == myCurrentRowDocument.Id)
                            .ToList())
                        {
                            var prj = MainReferences.Projects[d.ProjectId];
                            DistributeRows.Add(new ProjectPrihodDistributeRow
                            {
                                Id = d.Id,
                                RowId = d.RowId,
                                Price = CurrentRowDocument.Price,
                                Quantity = d.Quantity,
                                Date = CurrentRowDocument.Date,
                                Nomenkl = myCurrentRowDocument.Nomenkl,
                                NomenklNumber = myCurrentRowDocument.NomenklNumber,
                                Kontragent = myCurrentRowDocument.Kontragent,
                                NumNaklad = myCurrentRowDocument.NumNaklad,
                                NumAccount = myCurrentRowDocument.NumAccount,
                                Project = prj,
                                State = RowStatus.NotEdited,
                                Note = d.Note,
                                IsUsluga = false,
                                Summa = myCurrentRowDocument.Summa
                            });
                        }
                    }
                }
                else
                {
                    DistributeRows.Clear();
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        foreach (var d in ctx.ProjectProviderUslugi.Where(_ => _.RowId == myCurrentRowDocument.Id)
                            .ToList())
                        {
                            var prj = MainReferences.Projects[d.ProjectId];
                            DistributeRows.Add(new ProjectPrihodDistributeRow
                            {
                                Id = d.Id,
                                RowId = d.RowId,
                                Summa = d.Summa,
                                Date = CurrentRowDocument.Date,
                                Nomenkl = myCurrentRowDocument.Nomenkl,
                                NomenklNumber = myCurrentRowDocument.NomenklNumber,
                                Kontragent = myCurrentRowDocument.Kontragent,
                                NumNaklad = myCurrentRowDocument.NumNaklad,
                                NumAccount = myCurrentRowDocument.NumAccount,
                                Project = prj,
                                State = RowStatus.NotEdited,
                                Note = d.Note,
                                IsUsluga = true
                            });
                        }
                    }
                }

                foreach (var r in DistributeRows) r.myState = RowStatus.NotEdited;
                RaisePropertyChanged();
            }
        }

        public bool IsUsluga
        {
            get => myIsUsluga;
            set
            {
                if (myIsUsluga == value) return;
                myIsUsluga = value;
                RaisePropertyChanged();
            }
        }

        public bool IsNotUsluga
        {
            get => myIsNotUsluga;
            set
            {
                if (myIsNotUsluga == value) return;
                myIsNotUsluga = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<ProjectPrihodDocRow> SelectedItems { set; get; } =
            new ObservableCollection<ProjectPrihodDocRow>();

        public DateTime EndDate
        {
            get => myEndDate;
            set
            {
                if (myEndDate == value) return;
                myEndDate = value;
                RaisePropertyChanged();
            }
        }

        public Project DefaultProject
        {
            get => myDefaultProject;
            set
            {
                if (Equals(myDefaultProject,value)) return;
                myDefaultProject = value;
                RaisePropertyChanged();
            }
        }

        private bool DataChecked()
        {
            if (DistributeRows.Count == 0) return DeletedDistributeRows.Count != 0;
            return DistributeRows.Any(_ => _.State == RowStatus.Edited) ||
                   DistributeRows.Any(_ => _.State == RowStatus.NewRow)
                   && DistributeRows.All(_ => _.Project != null);
        }

        public override void SaveData(object data)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (DeletedDistributeRows.Count > 0)
                    foreach (var d in DeletedDistributeRows)
                        if (!d.IsUsluga)
                        {
                            var delold = ctx.ProjectProviderPrihod.FirstOrDefault(_ => _.Id == d.Id);
                            if (delold != null)
                                ctx.ProjectProviderPrihod.Remove(delold);
                        }
                        else
                        {
                            var delold = ctx.ProjectProviderUslugi.FirstOrDefault(_ => _.Id == d.Id);
                            if (delold != null)
                                ctx.ProjectProviderUslugi.Remove(delold);
                        }

                foreach (var d in DistributeRows.Where(_ => _.State != RowStatus.NotEdited))
                    if (!d.IsUsluga)
                        switch (d.State)
                        {
                            case RowStatus.NewRow:
                                ctx.ProjectProviderPrihod.Add(new ProjectProviderPrihod
                                {
                                    Id = d.Id,
                                    Quantity = d.Quantity,
                                    RowId = d.RowId,
                                    ProjectId = d.Project.Id,
                                    Note = d.Note
                                });
                                break;
                            case RowStatus.Edited:
                                var old = ctx.ProjectProviderPrihod.FirstOrDefault(_ => _.Id == d.Id);
                                if (old != null)
                                {
                                    old.Quantity = d.Quantity;
                                    old.Note = d.Note;
                                    old.ProjectId = d.Project.Id;
                                }

                                break;
                        }
                    else
                        switch (d.State)
                        {
                            case RowStatus.NewRow:
                                ctx.ProjectProviderUslugi.Add(new ProjectProviderUslugi
                                {
                                    Id = d.Id,
                                    Summa = d.Summa,
                                    RowId = d.RowId,
                                    ProjectId = d.Project.Id,
                                    Note = d.Note
                                });
                                break;
                            case RowStatus.Edited:
                                var old = ctx.ProjectProviderUslugi.FirstOrDefault(_ => _.Id == d.Id);
                                if (old != null)
                                {
                                    old.Summa = d.Summa;
                                    old.Note = d.Note;
                                    old.ProjectId = d.Project.Id;
                                }

                                break;
                        }

                ctx.SaveChanges();
            }

            foreach (var d in DistributeRows) d.myState = RowStatus.NotEdited;
        }

        public override void RefreshData(object obj)
        {
            if (Form is ProjectProviderPrihodView frm)
                if (SelectionMode && frm.gridrowDocuments.SelectionMode != MultiSelectMode.None)
                    frm.gridrowDocuments.SelectionMode = MultiSelectMode.None;
            RowDocuments.Clear();
            DistributeRows.Clear();
            DeletedDistributeRows.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                var data = (from td24 in ctx.TD_24
                    from sd24 in ctx.SD_24
                    from td26 in ctx.TD_26
                    from sd26 in ctx.SD_26
                    from sd83 in ctx.SD_83
                    from sd175 in ctx.SD_175
                    //join prj in ctx.ProjectProviderPrihod on td24.Id equals prj.RowId
                    where td24.DOC_CODE == sd24.DOC_CODE
                          && td26.DOC_CODE == td24.DDT_SPOST_DC && td26.CODE == td24.DDT_SPOST_ROW_CODE
                          && sd26.DOC_CODE == td26.DOC_CODE
                          && sd24.DD_DATE >= StartDate && sd24.DD_DATE <= EndDate
                          && td24.DDT_NOMENKL_DC == sd83.DOC_CODE
                          && sd83.NOM_ED_IZM_DC == sd175.DOC_CODE
                    //&& prj.RowId == td24.Id
                    select new ProjectPrihodDocRow
                    {
                        Id = td24.Id,
                        AccountDC = sd26.DOC_CODE,
                        NakladDC = sd24.DOC_CODE,
                        Creator = sd24.CREATOR,
                        Kontragent = sd24.DD_OTRPAV_NAME,
                        Nomenkl = sd83.NOM_NAME,
                        NomenklNumber = sd83.NOM_NOMENKL,
                        Unit = sd175.ED_IZM_NAME,
                        Date = sd24.DD_DATE,
                        NumAccount = sd26.SF_IN_NUM + "/" + sd26.SF_POSTAV_NUM,
                        NumNaklad = sd24.DD_IN_NUM + "/" + sd24.DD_EXT_NUM,
                        Price = (decimal) td26.SFT_ED_CENA,
                        Quantity = td24.DDT_KOL_PRIHOD,
                        IsUsluga = false
                        //DistributeQuantity = prj.Sum(_ => _.Quantity),
                    }).ToList();
                var udata = (from td26 in ctx.TD_26
                    from sd26 in ctx.SD_26
                    from sd83 in ctx.SD_83
                    from sd175 in ctx.SD_175
                    from sd43 in ctx.SD_43
                    where sd26.DOC_CODE == td26.DOC_CODE
                          && sd43.DOC_CODE == sd26.SF_POST_DC
                          && sd83.DOC_CODE == td26.SFT_NEMENKL_DC && sd83.NOM_0MATER_1USLUGA == 1
                          && sd26.SF_POSTAV_DATE >= StartDate && sd26.SF_POSTAV_DATE <= EndDate
                          && sd175.DOC_CODE == sd83.NOM_ED_IZM_DC
                    select new ProjectPrihodDocRow
                    {
                        Id = td26.Id,
                        AccountDC = sd26.DOC_CODE,
                        NakladDC = null,
                        Creator = sd26.CREATOR,
                        Kontragent = sd43.NAME,
                        Nomenkl = sd83.NOM_NAME,
                        NomenklNumber = sd83.NOM_NOMENKL,
                        Unit = sd175.ED_IZM_NAME,
                        Date = sd26.SF_POSTAV_DATE,
                        NumAccount = sd26.SF_IN_NUM + "/" + sd26.SF_POSTAV_NUM,
                        NumNaklad = null,
                        Price = (decimal) td26.SFT_ED_CENA,
                        Quantity = td26.SFT_KOL,
                        IsUsluga = true
                    }).ToList();
                var uprjs = ctx.ProjectProviderUslugi.ToList();
                var prjs = ctx.ProjectProviderPrihod.ToList();
                foreach (var d in data)
                {
                    var prjrows = prjs.Where(_ => _.RowId == d.Id);
                    d.DistributeQuantity = prjrows.Any() ? prjrows.Sum(_ => _.Quantity) : 0;
                    d.DistributeSumma = d.Price * d.DistributeQuantity;
                    if (IsAll)
                        RowDocuments.Add(d);
                    else if (d.Quantity > d.DistributeQuantity)
                        RowDocuments.Add(d);
                }

                foreach (var d in udata)
                {
                    var uprjrows = uprjs.Where(_ => _.RowId == d.Id);
                    d.DistributeSumma = uprjrows.Any() ? uprjrows.Sum(_ => _.Summa) : 0;
                    if (IsAll)
                        RowDocuments.Add(d);
                    else if (d.Summa > d.DistributeSumma)
                        RowDocuments.Add(d);
                }
            }
        }

        #region Commands

        public ICommand AddDistributeRowLinkCommand
        {
            get
            {
                return new Command(AddDistributeRowLink,
                    _ => CurrentRowDocument != null);
            }
        }

        private void AddDistributeRowLink(object obj)
        {
            if (SelectedItems == null) return;
            foreach (var s in SelectedItems)
            {
                if (s.Summa <= s.DistributeSumma) continue;
                DistributeRows.Add(new ProjectPrihodDistributeRow
                {
                    Id = Guid.NewGuid(),
                    RowId = s.Id,
                    Price = s.Price,
                    Quantity = s.IsUsluga ? 1 : s.Quantity - s.DistributeQuantity,
                    Nomenkl = s.Nomenkl,
                    NomenklNumber = s.NomenklNumber,
                    Kontragent = s.Kontragent,
                    NumNaklad = s.NumNaklad,
                    NumAccount = s.NumAccount,
                    Project = DefaultProject,
                    IsUsluga = s.IsUsluga,
                    State = RowStatus.NewRow,
                    Summa = s.IsUsluga ? s.Summa - s.DistributeSumma : (s.Quantity - s.DistributeQuantity) * s.Price
                });
                s.DistributeQuantity = DistributeRows.Where(_ => _.RowId == s.Id).Sum(v => v.Quantity);
                if (s.IsUsluga) s.DistributeSumma = DistributeRows.Where(_ => _.RowId == s.Id).Sum(v => v.Summa);
                RaisePropertyChanged(nameof(DistributeRows));
            }
        }

        public ICommand DistributeRowChanged
        {
            get { return new Command(DistributeRow, _ => CurrentDistRow != null); }
        }

        private void DistributeRow(object obj)
        {
            if (CurrentDistRow.State == RowStatus.NotEdited)
                CurrentDistRow.State = RowStatus.Edited;
            if (!(obj is CellValueChangedEventArgs arg)) return;
            if (arg.Column.FieldName != "Quantity" && arg.Column.FieldName != "Summa") return;
            if ((decimal) arg.Value <= 0)
            {
                CurrentDistRow.Quantity = (decimal) arg.OldValue;
                return;
            }

            var curdoc = RowDocuments.FirstOrDefault(_ => _.Id == CurrentDistRow.RowId);
            if (curdoc == null) return;
            if (!curdoc.IsUsluga)
            {
                var c = curdoc.DistributeQuantity + ((decimal) arg.Value - (decimal) arg.OldValue);
                if (c > curdoc.Quantity)
                {
                    CurrentDistRow.Quantity = (decimal) arg.OldValue;
                    CurrentDistRow.Summa = CurrentDistRow.Quantity * CurrentDistRow.Price;
                }
                else
                {
                    curdoc.DistributeQuantity = c;
                }
            }
            else
            {
                var c = DistributeRows.Where(_ => _.RowId == CurrentRowDocument.Id).Sum(s => s.Summa);
                if (curdoc.Summa - c < 0)
                    CurrentDistRow.Summa = (decimal) arg.OldValue;
                else
                    curdoc.DistributeSumma =
                        DistributeRows.Where(_ => _.RowId == CurrentRowDocument.Id).Sum(s => s.Summa);
            }

            RaisePropertyChanged(nameof(RowDocuments));
        }

        public ICommand RemoveProjectLinkCommand
        {
            get { return new Command(RemoveProjectLink, _ => SelectedDistRows.Count > 0 || CurrentDistRow != null); }
        }

        private void RemoveProjectLink(object obj)
        {
            var listId = new List<Guid>();
            if (SelectedDistRows.Count > 0)
                foreach (var s in SelectedDistRows)
                    listId.Add(s.Id);
            if (listId.All(_ => _ != CurrentDistRow.Id)) listId.Add(CurrentDistRow.Id);
            foreach (var id in listId)
            {
                var v = DistributeRows.FirstOrDefault(_ => _.Id == id);
                if (v == null) continue;
                {
                    var docRow = RowDocuments.FirstOrDefault(_ => _.Id == v.RowId);
                    if (docRow != null)
                        docRow.DistributeQuantity -= v.Quantity;
                    if (v.State != RowStatus.NewRow)
                        DeletedDistributeRows.Add(v);
                    DistributeRows.Remove(v);
                }
            }
        }

        #endregion
    }
}
