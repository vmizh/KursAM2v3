using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core;
using Core.EntityViewModel.Systems;
using Core.Invoices.EntityViewModel;
using Core.Menu;
using Core.ViewModel.Base;
using KursAM2.Managers;
using KursAM2.View.KursReferences;

namespace KursAM2.ViewModel.Reference
{
    public class AccessRightsViewModel : RSWindowViewModelBase
    {
        public readonly AccessRightsManager manager = new AccessRightsManager();
        private TreeDocument myCurrentStruct;
        private EXT_USERSViewModel myCurrentUser;
        private bool myIsShowAll;

        public AccessRightsViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this);
            RightMenuBar = MenuGenerator.StandartInfoRightBar(this);
            //ReSharper disable once VirtualMemberCallInConstructor 
            DocumentStruct = new ObservableCollection<TreeDocument>();
            RefreshData(null);
        }

        public ObservableCollection<EXT_USERSViewModel> UserCollection { set; get; } =
            new ObservableCollection<EXT_USERSViewModel>();

        public ObservableCollection<EXT_USERSViewModel> ActualUsers { set; get; } =
            new ObservableCollection<EXT_USERSViewModel>();

        public ObservableCollection<TreeDocument> DocumentStruct { set; get; }

        public ObservableCollection<TreeDocument> ActualDocumentStruct { set; get; } =
            new ObservableCollection<TreeDocument>();

        public ObservableCollection<USER_FORMS_RIGHTViewModel> Permissions { set; get; } =
            new ObservableCollection<USER_FORMS_RIGHTViewModel>();

        public override bool IsCanSaveData => UserCollection.Any(
            _ => _.State != RowStatus.NotEdited);

        public EXT_USERSViewModel CurrentUser
        {
            set
            {
                // ReSharper disable once RedundantCheckBeforeAssignment
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentUser != value)
                    myCurrentUser = value;
                IsCHekedPermissionUser();
                RaisePropertiesChanged();
            }
            get => myCurrentUser;
        }

        public TreeDocument CurrentStruct
        {
            set
            {
                // ReSharper disable once RedundantCheckBeforeAssignment
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (myCurrentStruct != value)
                    myCurrentStruct = value;
                RaisePropertiesChanged();
            }
            get => myCurrentStruct;
        }

        public ICommand SaveRowCommand
        {
            get { return new Command(SaveRow, _ => true); }
        }

        public ICommand SavePermissionCommand
        {
            get { return new Command(SavePermission, _ => true); }
        }

        public bool IsShowAll
        {
            set
            {
                if (myIsShowAll == value) return;
                myIsShowAll = value;
                ShowUserInWindow();
            }
            get => myIsShowAll;
        }

        private void SaveRow(object obj)
        {
            var user =
                UserCollection.FirstOrDefault(_ => _.USR_NICKNAME.ToUpper() == CurrentUser.USR_NICKNAME.ToUpper());
            if (user == null) return;
            user.UserBlock = CurrentUser.UserBlock;
            manager.SaveBlock(CurrentUser);
            ShowUserInWindow();
            RaisePropertyChanged(nameof(ActualUsers));
        }

        private void SavePermission(object obj)
        {
            if (CurrentStruct == null) return;
            if (CurrentStruct.ParentId == null)
            {
                foreach (var p in DocumentStruct.Where(_ => _.ParentId == CurrentStruct.Id))
                    if (p.IsCheked != CurrentStruct?.IsCheked)
                    {
                        p.IsCheked = CurrentStruct.IsCheked;
                        manager.SavePermission(p, CurrentUser.USR_NICKNAME);
                    }

                var currentTreeListNode = ((AccessRightsView) Form).treeListPermissionStructView.FocusedNode;
                if (currentTreeListNode != null)
                {
                    currentTreeListNode.IsExpanded = false;
                    currentTreeListNode.IsExpanded = true;
                }

                foreach (var item in DocumentStruct)
                {
                    if (item.ParentId != null) continue;
                    item.ColorState = 1;
                    if (DocumentStruct.FirstOrDefault(_ => item.Id == _.ParentId && _.IsCheked) != null)
                        item.ColorState = 2;
                    if (DocumentStruct.FirstOrDefault(_ => item.Id == _.ParentId && _.IsCheked == false) == null)
                        item.ColorState = 3;
                }

                RaisePropertyChanged(nameof(DocumentStruct));
                return;
            }

            manager.SavePermission(myCurrentStruct, CurrentUser.USR_NICKNAME);
            RaisePropertyChanged(nameof(DocumentStruct));
        }

        public override void RefreshData(object obj)
        {
            UserCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var i in ctx.EXT_USERS.ToList())
                {
                    var newItem = new EXT_USERSViewModel(i);
                    newItem.USR_NICKNAME = newItem.USR_NICKNAME.Trim();
                    UserCollection.Add(newItem);
                }

                RaisePropertyChanged(nameof(UserCollection));
            }

            ShowUserInWindow();
        }

        private void LoadStruct(string userName, bool resetData)
        {
            using (var ctx = GlobalOptions.GetEntities())
            {
                Permissions.Clear();
                foreach (var item in ctx.USER_FORMS_RIGHT.Where(_ => _.USER_NAME.ToUpper() == userName.ToUpper())
                    .ToList())
                    Permissions.Add(new USER_FORMS_RIGHTViewModel(item));
                if (!resetData)
                    DocumentStruct = new ObservableCollection<TreeDocument>();
                else
                    DocumentStruct = new ObservableCollection<TreeDocument>();
                foreach (var item in ctx.MAIN_DOCUMENT_GROUP.AsNoTracking())
                    DocumentStruct.Add(new TreeDocument
                    {
                        Id = Guid.NewGuid(),
                        BaseId = item.ID,
                        Name = item.NAME,
                        Note = item.NOTES
                    });
                foreach (var item in ctx.MAIN_DOCUMENT_ITEM.AsNoTracking())
                {
                    var newDoc = new TreeDocument
                    {
                        Id = Guid.NewGuid(),
                        BaseParentId = item.GROUP_ID,
                        BaseId = item.ID,
                        Name = item.NAME,
                        Note = item.NOTES,
                        ParentId = DocumentStruct.Where(_ => _.BaseParentId == null)
                            .FirstOrDefault(_ => _.BaseId == item.GROUP_ID)
                            ?.Id,
                        IsCheked = Permissions.FirstOrDefault(_ => _.USER_NAME.ToUpper() == userName.ToUpper() &&
                                                                   _.FORM_ID == item.ID) !=
                                   null
                    };
                    DocumentStruct.Add(newDoc);
                }

                foreach (var item in DocumentStruct)
                {
                    if (item.ParentId != null) continue;
                    item.ColorState = 1;
                    if (DocumentStruct.FirstOrDefault(_ => item.Id == _.ParentId && _.IsCheked) != null)
                        item.ColorState = 2;
                    if (DocumentStruct.FirstOrDefault(_ => item.Id == _.ParentId && _.IsCheked == false) == null)
                        item.ColorState = 3;
                }

                RaisePropertyChanged(nameof(DocumentStruct));
            }
        }

        //private void LoadStruct(string userName)
        //{
        //    using (var ctx = GlobalOptions.GetEntities())
        //    {
        //        Permissions.Clear();
        //        foreach (var item in ctx.USER_FORMS_RIGHT.Where(_ => _.USER_NAME.ToUpper() == userName.ToUpper()).ToList())
        //        {
        //            Permissions.Add(new USER_FORMS_RIGHTViewModel(item));
        //        }
        //        if(DocumentStruct.Count > 0)
        //            DocumentStruct.Clear();
        //        else
        //            DocumentStruct = new ObservableCollection<TreeDocument>();
        //        foreach (var item in ctx.MAIN_DOCUMENT_GROUP.AsNoTracking())
        //        {
        //            DocumentStruct.Add(new TreeDocument
        //            {
        //                Id = Guid.NewGuid(),
        //                BaseId = item.ID,
        //                Name = item.NAME,
        //                Note = item.NOTES
        //            });
        //        }
        //        foreach (var item in ctx.MAIN_DOCUMENT_ITEM.AsNoTracking())
        //        {
        //            var newDoc = new TreeDocument
        //            {
        //                Id = Guid.NewGuid(),
        //                BaseParentId = item.GROUP_ID,
        //                BaseId = item.ID,
        //                Name = item.NAME,
        //                Note = item.NOTES,
        //                ParentId = DocumentStruct.Where(_ => _.BaseParentId == null)
        //                    .FirstOrDefault(_ => _.BaseId == item.GROUP_ID)
        //                    .Id,
        //                IsCheked = Permissions.FirstOrDefault(_ => _.USER_NAME.ToUpper() == userName.ToUpper() &&
        //                                                  _.FORM_ID == item.ID) !=
        //                           null
        //            };
        //            DocumentStruct.Add(newDoc);
        //        }
        //        RaisePropertyChanged(nameof(DocumentStruct));
        //    }

        //}

        private void IsCHekedPermissionUser()
        {
            if (CurrentUser == null) return;
            var treelststruct = ((AccessRightsView) Form).treeListPermissionStruct;
            if (treelststruct != null)
                LoadStruct(CurrentUser.USR_NICKNAME.Trim().ToUpper(),
                    treelststruct.Columns.Any(_ => (string) _.HeaderCaption == _.FieldName));
        }

        public override void CloseWindow(object form)
        {
            var vin = form as Window;
            vin?.Close();
        }

        public void ShowUserInWindow()
        {
            ActualUsers.Clear();
            if (IsShowAll)
                foreach (var n in UserCollection)
                    ActualUsers.Add(n);
            else
                foreach (var n in UserCollection.Where(_ => !_.UserBlock))
                    ActualUsers.Add(n);
            RaisePropertiesChanged(nameof(ActualUsers));
        }

        public override void ResetLayout(object form)
        {
            //List<TreeDocument> data = new List<TreeDocument>(DocumentStruct);
            DocumentStruct.Clear();
            base.ResetLayout(form);
            LoadStruct(CurrentUser?.USR_NICKNAME, true);
        }
    }
}