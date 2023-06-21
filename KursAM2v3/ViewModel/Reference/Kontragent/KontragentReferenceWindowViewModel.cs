using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Transactions;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using DevExpress.Mvvm.Xpf;
using KursAM2.View.DialogUserControl.ViewModel;
using KursAM2.View.KursReferences.KontragentControls;
using KursDomain;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Reference.Kontragent
{
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public class KontragentReferenceWindowViewModel : RSWindowViewModelBase
    {
        private KontragentViewModel _myCurrentKontragent;
        private KontragentGroupViewModel myCurrentGroup;
        private bool myIsAllKontragent;
        private bool myIsGroupEnabled;
        private bool myIsShowDeleted;

        public KontragentReferenceWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.RightBarOfKontragentReferens(this);
            // ReSharper disable once VirtualMemberCallInConstructor
            RefreshData(null);
            IsGroupEnabled = true;
        }

        public override string WindowName => "Справочник контрагентов";
        public override string LayoutName => "KontragentReferenceWindowViewModel";


        //public List<Kontragent> Kontragents => MainReferences.AllKontragents.Values.ToList();
        public ObservableCollection<KontragentViewModel> Kontragents { set; get; } =
            new ObservableCollection<KontragentViewModel>();

        public ObservableCollection<KontragentViewModel> KontragentsInGroup { set; get; } =
            new ObservableCollection<KontragentViewModel>();

        public ObservableCollection<KontragentGroupViewModel>
            Groups { set; get; } =
            new ObservableCollection<KontragentGroupViewModel>();

        public KontragentViewModel CurrentKontragent
        {
            get => _myCurrentKontragent;
            set
            {
                if (Equals(_myCurrentKontragent, value)) return;
                _myCurrentKontragent = value;
                RaisePropertyChanged();
            }
        }

        public KontragentGroupViewModel CurrentGroup
        {
            get => myCurrentGroup;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentGroup == value) return;
                myCurrentGroup = value;
                CurrentKontragent = null;
                if (myCurrentGroup != null)
                    SetKontragentsInGroup();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsNewEnabled));
            }
        }

        public bool IsShowDeleted
        {
            get => myIsShowDeleted;
            set
            {
                if (myIsShowDeleted == value) return;
                myIsShowDeleted = value;
                RaisePropertyChanged();
            }
        }

        public bool IsAllKontragent
        {
            get => myIsAllKontragent;
            set
            {
                if (myIsAllKontragent == value) return;
                myIsAllKontragent = value;
                if (myIsAllKontragent)
                    GetAllKontragent();
                else
                    SetKontragentsInGroup();
                RaisePropertyChanged();
            }
        }

        public bool IsNewEnabled
        {
            get
            {
                return myCurrentGroup != null &&
                       Groups.FirstOrDefault(_ => _.EG_PARENT_ID == myCurrentGroup.EG_ID) == null;
            }
        }

        public ICommand TreeAddInRootCommand
        {
            get { return new Command(TreeAddInRoot, param => true); }
        }

        public ICommand TreeAddChildCommand
        {
            get { return new Command(TreeAddChild, param => true); }
        }

        public ICommand TreeAddOneLevelCommand
        {
            get { return new Command(TreeAddOneLevel, param => true); }
        }

        public ICommand TreeEditCommand
        {
            get { return new Command(TreeEdit, param => true); }
        }

        public ICommand TreeDeleteCommand
        {
            get
            {
                return new Command(GroupDelete, param => CurrentGroup != null &&
                                                         Groups.All(_ => _.EG_PARENT_ID != CurrentGroup.EG_ID &&
                                                                         KontragentsInGroup.Count == 0));
            }
        }

        private void SetKontragentsInGroup()
        {
            try
            {
                KontragentsInGroup.Clear();
                if (CurrentGroup == null || CurrentGroup.EG_ID == -1) return;
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data =
                        ctx.SD_43
                            .Include(_ => _.SD_301)
                            .Include(_ => _.SD_2)
                            .Include(_ => _.UD_43)
                            .Include(_ => _.SD_148)
                            .Include(_ => _.SD_43_GRUZO)
                            .Include(_ => _.TD_43)
                            .AsNoTracking().Where(_ => _.EG_ID == CurrentGroup.EG_ID).ToList();
                    foreach (var i in data)
                    {
                        if (!IsShowDeleted && i.DELETED == 1) continue;
                        KontragentsInGroup.Add(new KontragentViewModel(i));
                    }

                    if (CurrentGroup.EG_ID == 0)
                    {
                        var data2 =
                            ctx.SD_43
                                .Include(_ => _.SD_301)
                                .Include(_ => _.SD_2)
                                .Include(_ => _.UD_43)
                                .Include(_ => _.SD_148)
                                .Include(_ => _.SD_43_GRUZO)
                                .Include(_ => _.TD_43)
                                .AsNoTracking().Where(_ => _.EG_ID == 0 || _.EG_ID == null).ToList();
                        foreach (var k in data2.Where(k => IsShowDeleted || k.DELETED != 1))
                            KontragentsInGroup.Add(new KontragentViewModel(k));

                        RaisePropertyChanged(nameof(KontragentsInGroup));
                    }

                    RaisePropertyChanged(nameof(KontragentsInGroup));
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        private void SearchKontragent()
        {
            try
            {
                KontragentsInGroup.Clear();
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data =
                        ctx.SD_43
                            .Include(_ => _.SD_301)
                            .Include(_ => _.SD_2)
                            .Include(_ => _.UD_43)
                            .Include(_ => _.SD_148)
                            .Include(_ => _.SD_43_GRUZO)
                            .Include(_ => _.TD_43)
                            .AsNoTracking().Where(_ => _.NAME.ToUpper().Contains(SearchText.ToUpper())
                                                       || _.NAME_FULL.ToUpper().Contains(SearchText.ToUpper()))
                            .ToList();
                    foreach (var i in data) KontragentsInGroup.Add(new KontragentViewModel(i));
                    RaisePropertyChanged(nameof(KontragentsInGroup));
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public void GetAllKontragent()
        {
            try
            {
                KontragentsInGroup.Clear();
                using (var ctx = GlobalOptions.GetEntities())
                {
                    List<SD_43> data;
                    if (IsShowDeleted)
                        data = ctx.SD_43
                            .Include(_ => _.UD_43)
                            .Include(_ => _.SD_148)
                            .Include(_ => _.SD_43_GRUZO)
                            .Include(_ => _.TD_43)
                            .Include("TD_43.SD_44")
                            .ToList();
                    else
                        data = ctx.SD_43
                            .Include(_ => _.UD_43)
                            .Include(_ => _.SD_148)
                            .Include(_ => _.SD_43_GRUZO)
                            .Include(_ => _.TD_43)
                            .Include("TD_43.SD_44")
                            .Where(_ => _.DELETED != 1)
                            .ToList();
                    foreach (var i in data)
                        KontragentsInGroup.Add(new KontragentViewModel(i));
                }

                RaisePropertyChanged(nameof(KontragentsInGroup));
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public override void RefreshData(object data)
        {
            Groups.Clear();

            foreach (var item in GlobalOptions.GetEntities().UD_43.Where(_ => (_.EG_DELETED ?? 0) != 1))
                Groups.Add(new KontragentGroupViewModel(item));
            if (Groups.All(_ => _.EG_ID != 0))
                Groups.Add(new KontragentGroupViewModel
                {
                    Entity = new UD_43
                    {
                        EG_ID = 0,
                        EG_NAME = "Вне групп"
                    }
                });
            RaisePropertyChanged(nameof(Groups));
        }

        private void TreeAddInRoot(object obj)
        {
            var category = new NameNoteViewModel
            {
                HeaderName = "Наименование",
                HeaderNote = "Примечания",
                Name = "",
                Note = ""
            };
            var res = DialogService.ShowDialog(MessageBoxButton.OKCancel,
                "Новая категория", "NameNoteUC", category);
            if (res != MessageBoxResult.OK) return;
            var newgrp = new KontragentGroupViewModel
            {
                Name = category.Name,
                State = RowStatus.NewRow
            };
            if (!GroupSave(newgrp)) return;
            Groups.Add(newgrp);
            CurrentGroup = newgrp;
        }

        private void TreeAddChild(object obj)
        {
            var category = new NameNoteViewModel
            {
                HeaderName = "Наименование",
                HeaderNote = "Примечания",
                Name = "",
                Note = ""
            };
            var res = DialogService.ShowDialog(MessageBoxButton.OKCancel,
                "Новая категория", "NameNoteUC", category);
            if (res != MessageBoxResult.OK) return;
            var newgrp = new KontragentGroupViewModel
            {
                Name = category.Name,
                ParentDC = CurrentGroup.DocCode,
                State = RowStatus.NewRow
            };
            if (!GroupSave(newgrp)) return;
            Groups.Add(newgrp);
            CurrentGroup = newgrp;
        }

        private void TreeAddOneLevel(object obj)
        {
            var category = new NameNoteViewModel
            {
                HeaderName = "Наименование",
                HeaderNote = "Примечания",
                Name = "",
                Note = ""
            };
            var res = DialogService.ShowDialog(MessageBoxButton.OKCancel, "Новая категория", "NameNoteUC", category);
            if (res != MessageBoxResult.OK) return;
            var newgrp = new KontragentGroupViewModel
            {
                Name = category.Name,
                ParentDC = CurrentGroup.ParentDC,
                State = RowStatus.NewRow
            };
            if (!GroupSave(newgrp)) return;
            Groups.Add(newgrp);
            CurrentGroup = newgrp;
        }

        private void TreeEdit(object obj)
        {
            var group = new NameNoteViewModel
            {
                HeaderName = "Наименование",
                HeaderNote = "Примечания",
                Name = CurrentGroup?.Name,
                Note = CurrentGroup?.Note
            };
            var res = DialogService.ShowDialog(MessageBoxButton.OKCancel,
                "Редактирование категории", "NameNoteUC",
                group);
            if (res != MessageBoxResult.OK) return;
            if (CurrentGroup == null) return;
            var oldCat = new KontragentGroupViewModel(CurrentGroup.Entity);
            CurrentGroup.Name = group.Name;
            CurrentGroup.Note = group.Note;
            if (GroupSave(CurrentGroup)) return;
            CurrentGroup.Name = oldCat.Name;
            CurrentGroup.Note = oldCat.Note;
            CurrentGroup.State = RowStatus.NotEdited;
        }

        private bool GroupSave(KontragentGroupViewModel cat)
        {
            var newId = cat.EG_ID;
            using (var ctx = GlobalOptions.GetEntities())
            {
                using (var tnx = new TransactionScope())
                {
                    try
                    {
                        switch (cat.State)
                        {
                            case RowStatus.Edited:
                                var oldcat = ctx.UD_43.SingleOrDefault(_ => _.EG_ID == cat.EG_ID);
                                if (oldcat == null) return false;
                                oldcat.EG_NAME = cat.Name;
                                oldcat.EG_PARENT_ID = cat.EG_PARENT_ID;
                                break;
                            case RowStatus.NewRow:
                                newId = ctx.UD_43.Any() ? ctx.UD_43.Max(_ => _.EG_ID) : 0;
                                if (newId == 0)
                                    newId = 1;
                                else newId = newId + 1;
                                var newItem = new UD_43
                                {
                                    EG_ID = newId,
                                    EG_PARENT_ID = cat.EG_PARENT_ID,
                                    EG_NAME = cat.Name
                                };
                                ctx.UD_43.Add(newItem);
                                break;
                        }

                        ctx.SaveChanges();
                        tnx.Complete();
                        cat.EG_ID = newId;
                        cat.State = RowStatus.NotEdited;
                    }
                    catch (Exception ex)
                    {
                        WindowManager.ShowError(ex);
                        return false;
                    }
                }
            }

            return true;
        }


        #region Commands

        public override bool IsDocumentOpenAllow => CurrentKontragent != null;
        public override bool IsCanDocNew => CurrentGroup != null;
        public override bool IsDocNewCopyAllow => CurrentKontragent != null;
        public override bool IsDocDeleteAllow => CurrentKontragent != null;

        public override void DocumentOpen(object obj)
        {
            var form = new KontragentCardView { Owner = Application.Current.MainWindow };
            var ctx = new KontragentCardWindowViewModel(CurrentKontragent.DocCode) { Form = form };
            form.DataContext = ctx;
            form.ShowDialog();
            SetKontragentsInGroup();
        }

        public override void DocNewEmpty(object obj)
        {
            var WinManager = new WindowManager();
            var form = new KontragentCardView { Owner = Application.Current.MainWindow };
            if (CurrentGroup == null)
            {
                WinManager.ShowWinUIMessageBox("Не выбрана категория.", "Ощибка");
                return;
            }

            var ctx = new KontragentCardWindowViewModel(-1, CurrentGroup.EG_ID, false) { Form = form };
            form.DataContext = ctx;
            form.ShowDialog();
            SetKontragentsInGroup();
        }

        public override void DocNewCopy(object obj)
        {
            var form = new KontragentCardView { Owner = Application.Current.MainWindow };
            var ctx =
                new KontragentCardWindowViewModel(CurrentKontragent.DocCode, CurrentGroup.EG_ID, true)
                    { Form = form };
            form.DataContext = ctx;
            form.ShowDialog();
            SetKontragentsInGroup();
        }

        public ICommand NewCopyKontragentCommand
        {
            get { return new Command(NewCopyKontragent, _ => CurrentKontragent != null); }
        }

        public ICommand GroupAddCommand
        {
            get { return new Command(GroupAdd, param => CurrentGroup?.EG_ID >= 0); }
        }

        public ICommand GroupDeleteCommand
        {
            get
            {
                return new Command(GroupDelete, param => CurrentGroup != null
                                                         && KontragentsInGroup.Count == 0
                                                         && Groups.All(_ => _.EG_PARENT_ID != CurrentGroup.EG_ID));
            }
        }

        public ICommand DocumentOpenMenuCommand
        {
            get { return new Command(DocumentOpen, param => CurrentKontragent != null); }
        }

        private void GroupDelete(object obj)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                var firstOrDefault = ctx.UD_43.FirstOrDefault(_ => _.EG_ID == CurrentGroup.EG_ID);
                if (firstOrDefault != null)
                    firstOrDefault.EG_DELETED = 1;
                ctx.SaveChanges();
                Groups.Remove(CurrentGroup);
            }
        }

        private void GroupAdd(object obj)
        {
            var item = new KontragentGroupViewModel
            {
                EG_PARENT_ID = CurrentGroup.EG_ID,
                Id = Guid.NewGuid(),
                EG_ID = -1
            };
            Groups.Add(item);
            CurrentGroup = Groups.FirstOrDefault(_ => _.Id == item.Id);
        }

        public override void DocDelete(object form)
        {
            var res = MessageBox.Show("Вы уверены, что хотите удалить документ?", "Запрос",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    using (var ctx = GlobalOptions.GetEntities())
                    {
                        var firstOrDefault =
                            ctx.SD_43.FirstOrDefault(_ => _.DOC_CODE == CurrentKontragent.DOC_CODE);
                        if (firstOrDefault != null)
                            firstOrDefault.DELETED = 1;
                        ctx.SaveChanges();
                        if (GlobalOptions.ReferencesCache.GetKontragent(CurrentKontragent.DOC_CODE) is
                            KursDomain.References.Kontragent orDefault)
                            orDefault
                                .IsDeleted = true;
                        KontragentsInGroup.Remove(CurrentKontragent);
                        RaisePropertyChanged(nameof(KontragentsInGroup));
                    }

                    CloseWindow(null);
                    break;
                case MessageBoxResult.No:
                    return;
            }
        }

        private void NewCopyKontragent(object obj)
        {
            WindowManager.ShowFunctionNotReleased();
        }

        public ICommand KontragentAddCommand
        {
            get { return new Command(KontragentAdd, _ => CurrentGroup != null); }
        }

        private void KontragentAdd(object obj)
        {
            var form = new KontragentCardView { Owner = Application.Current.MainWindow };
            var ctx = new KontragentCardWindowViewModel
            {
                Kontragent = new KontragentViewModel
                {
                    State = RowStatus.NewRow,
                    Group = CurrentGroup,
                    IsBalans = true,
                    START_BALANS = DateTime.Today
                },
                Form = form
            };
            form.DataContext = ctx;
            form.ShowDialog();
            SetKontragentsInGroup();
        }

        public ICommand KontragentAddCopyCommand
        {
            get { return new Command(KontragentAddCopy, _ => CurrentKontragent != null); }
        }

        private void KontragentAddCopy(object obj)
        {
            SD_43 doc = null;
            using (var context = GlobalOptions.GetEntities())
            {
                doc = context.SD_43.Include(_ => _.TD_43)
                    .Include(_ => _.UD_43)
                    .Include(_ => _.SD_148)
                    .Include(_ => _.SD_43_GRUZO)
                    .Include("TD_43.SD_44")
                    .AsNoTracking()
                    .FirstOrDefault(_ => _.DOC_CODE == CurrentKontragent.DOC_CODE);
                if (doc == null) return;
            }

            var form = new KontragentCardView { Owner = Application.Current.MainWindow };
            var ctx = new KontragentCardWindowViewModel
            {
                Kontragent = new KontragentViewModel(doc),
                Form = form
            };
            ctx.Kontragent.DOC_CODE = -1;
            ctx.Kontragent.BalansCurrency = ctx.Currencies.FirstOrDefault(_ => _.DocCode == doc.VALUTA_DC);
            ctx.CurrentCurrencies = ctx.Kontragent.BalansCurrency;
            ctx.Kontragent.Name += "(новый)";
            //ctx.Kontragent.FullName = string.IsNullOrWhiteSpace(ctx.Kontragent.FullName) ? null : ctx.Kontragent.FullName + "(новый)";
            ctx.Kontragent.DELETED = 0;
            //ctx.Kontragent.NAME_FULL = string.IsNullOrWhiteSpace(ctx.Kontragent.NAME_FULL) ? null : ctx.Kontragent.NAME_FULL + "(новый)";
            ctx.Kontragent.START_SUMMA = 0;
            ctx.Kontragent.State = RowStatus.NewRow;
            ctx.Kontragent.Group = CurrentGroup;
            ctx.Kontragent.IsBalans = true;
            ctx.Kontragent.START_BALANS = DateTime.Today;
            form.DataContext = ctx;
            form.ShowDialog();
            SetKontragentsInGroup();
        }

        public ICommand KontragentDeleteCommand
        {
            get { return new Command(KontragentDelete, _ => CurrentKontragent != null); }
        }

        private void KontragentDelete(object obj)
        {
            WindowManager.ShowFunctionNotReleased();
        }

        public override bool IsCanSearch => !string.IsNullOrEmpty(SearchText);

        public override void Search(object obj)
        {
            SearchKontragent();
        }

        public bool IsGroupEnabled
        {
            get => myIsGroupEnabled;
            set
            {
                if (myIsGroupEnabled == value) return;
                myIsGroupEnabled = value;
                KontragentsInGroup.Clear();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(KontragentsInGroup));
            }
        }

        public override string SearchText
        {
            get => mySearchText;
            set
            {
                if (mySearchText == value) return;
                mySearchText = value;
                RaisePropertyChanged();
                IsGroupEnabled = string.IsNullOrEmpty(mySearchText);
                CurrentGroup = null;
            }
        }

        #endregion
    }
}
