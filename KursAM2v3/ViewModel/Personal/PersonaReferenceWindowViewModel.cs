using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.View.Base;
using KursDomain;
using KursDomain.Documents.Systems;
using KursDomain.ICommon;
using KursDomain.Menu;
using KursDomain.References;

namespace KursAM2.ViewModel.Personal
{
    public class PersonaReferenceWindowViewModel : RSWindowViewModelBase
    {
        private readonly ALFAMEDIAEntities myCtx = GlobalOptions.GetEntities();
        private EmployeeViewModel myCurrentPersona;
        private User myCurrentUser;

        public PersonaReferenceWindowViewModel()
        {
            LeftMenuBar = MenuGenerator.BaseLeftBar(this, new Dictionary<MenuGeneratorItemVisibleEnum, bool>
            {
                [MenuGeneratorItemVisibleEnum.AddSearchlist] = false
            });
                    ;
            RightMenuBar = MenuGenerator.ReferenceRightBar(this);
            CurenciesCollection = GlobalOptions.ReferencesCache.GetCurrenciesAll().Cast<Currency>().ToList();
            RefreshData(null);
        }

        public ObservableCollection<EmployeeViewModel> PersonaCollection { set; get; } =
            new ObservableCollection<EmployeeViewModel>();

        public ObservableCollection<User> UserCollection { set; get; } = new ObservableCollection<User>();
        public ObservableCollection<User> UserDeleteCollection { set; get; } = new ObservableCollection<User>();

        public override bool IsCanSaveData => PersonaCollection.Any(_ => _.State != RowStatus.NotEdited) ||
                                              UserCollection.Any(
                                                  _ =>
                                                      _.State != RowStatus.NotEdited ||
                                                      UserDeleteCollection.Count > 0);

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public List<Currency> CurenciesCollection { set; get; }

        public EmployeeViewModel CurrentPersona
        {
            get => myCurrentPersona;
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (CurrentPersona?.State == RowStatus.Edited)
                    SaveData(myCurrentPersona);
                if (myCurrentPersona == value) return;
                myCurrentPersona = value;
                loadUsersForPersona();
                RaisePropertyChanged();
            }
        }

        public User CurrentUser
        {
            get => myCurrentUser;
            set
            {
                if (myCurrentUser == value) return;
                myCurrentUser = value;
                RaisePropertyChanged();
            }
        }

        public sealed override void RefreshData(object obj)
        {if (IsCanSaveData)
            {
                var res = MessageBox.Show("В документ внесены изменения. Сохранить?", "Запрос",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                switch (res)
                {
                    case MessageBoxResult.Yes:
                        SaveData(null);
                        break;
                    case MessageBoxResult.No:
                        foreach (var entity in myCtx.ChangeTracker.Entries()) entity.Reload();
                        break;
                }
            }
            PersonaCollection.Clear();
            UserCollection.Clear();
            try
            {
                foreach (var s in myCtx.SD_2.Include(_ => _.SD_301).ToList())
                    PersonaCollection.Add(new EmployeeViewModel(s) { State = RowStatus.NotEdited });
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        // ReSharper disable once InconsistentNaming
        private void loadUsersForPersona()
        {
            UserCollection.Clear();
            if (CurrentPersona == null) return;
            try
            {
                foreach (
                    var usr in
                    myCtx.EMP_USER_RIGHTS.Where(_ => _.EMP_DC == CurrentPersona.DocCode)
                        .ToList()
                        .Select(
                            s => myCtx.EXT_USERS.FirstOrDefault(_ => _.USR_NICKNAME.ToUpper() == s.USER.ToUpper()))
                        .Where(usr => usr != null))
                    UserCollection.Add(new User(usr) { State = RowStatus.NotEdited });
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override void SaveData(object data)
        {
            var WinManager = new WindowManager();
            if (PersonaCollection.All(_ => _.State == RowStatus.NotEdited)) return;
            using (var ctxsave = GlobalOptions.GetEntities())
            {
                using (var tnx = ctxsave.Database.BeginTransaction())
                {
                    try
                    {
                        switch (CurrentPersona.State)
                        {
                            case RowStatus.Edited:
                                var ed = ctxsave.SD_2.FirstOrDefault(_ => _.DOC_CODE == CurrentPersona.DocCode);
                                if (ed == null) return;
                                ed.NAME_FIRST = CurrentPersona.FirstName;
                                ed.NAME_LAST = CurrentPersona.LastName;
                                ed.NAME_SECOND = CurrentPersona.SecondName;
                                ed.CHANGE_DATE = DateTime.Now;
                                ed.DELETED = CurrentPersona.DELETED ?? 0;
                                ed.STATUS_NOTES = CurrentPersona.StatusNotes;
                                ed.NAME = CurrentPersona.Name;
                                if (ed.crs_dc != CurrentPersona.Currency.DocCode)
                                    if (!(ctxsave.SD_33.Any(_ => _.TABELNUMBER == CurrentPersona.TabelNumber)
                                          || ctxsave.SD_34.Any(_ => _.TABELNUMBER == CurrentPersona.TabelNumber)))
                                        ed.crs_dc = CurrentPersona.Currency.DocCode;

                                break;
                            case RowStatus.NewRow:
                                if (CurrentPersona.TabelNumber == 0 || CurrentPersona.Currency == null)
                                {
                                    WinManager.ShowWinUIMessageBox(
                                        "Табельный номер и валюта должны быть обязательно указаны.", "Ошибка",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }

                                var newDC = ctxsave.SD_2.Any() ? ctxsave.SD_2.Max(_ => _.DOC_CODE) + 1 : 10020000001;
                                ctxsave.SD_2.Add(new SD_2
                                {
                                    DOC_CODE = newDC,
                                    NAME_FIRST = CurrentPersona.FirstName,
                                    NAME_LAST = CurrentPersona.LastName,
                                    NAME_SECOND = CurrentPersona.SecondName,
                                    CHANGE_DATE = DateTime.Now,
                                    DELETED = CurrentPersona.DELETED ?? 0,
                                    STATUS_NOTES = CurrentPersona.StatusNotes,
                                    NAME = CurrentPersona.ToString(),
                                    TABELNUMBER = CurrentPersona.TabelNumber,
                                    crs_dc = CurrentPersona.Currency.DocCode,
                                    ID = Guid.NewGuid().ToString().Replace("-", string.Empty),
                                    OLD = 0
                                });
                                break;
                        }

                        foreach (var uu in UserDeleteCollection.Distinct())
                        {
                            var udel =
                                ctxsave.EMP_USER_RIGHTS.FirstOrDefault(
                                    _ =>
                                        _.EMP_DC == CurrentPersona.DocCode &&
                                        _.USER.ToUpper() == uu.NickName.ToUpper());
                            if (udel != null)
                                ctxsave.EMP_USER_RIGHTS.Remove(udel);
                        }

                        foreach (var u in UserCollection)
                        {
                            var old = ctxsave.EMP_USER_RIGHTS.FirstOrDefault(_ => _.EMP_DC == CurrentPersona.DocCode
                                && _.USER == u.NickName);
                            if (old != null) continue;
                            ctxsave.EMP_USER_RIGHTS.Add(new EMP_USER_RIGHTS
                            {
                                EMP_DC = CurrentPersona.DocCode,
                                USER = u.NickName
                            });
                        }

                        ctxsave.SaveChanges();
                        tnx.Commit();
                        foreach (var pers in PersonaCollection)
                        {
                            pers.myState = RowStatus.NotEdited;
                        }
                        RefreshData(null);
                    }
                    catch (Exception ex)
                    {
                        tnx.Rollback();
                        WindowManager.ShowError(ex);
                    }
                }
            }
        }

        #region Command

        public ICommand AddNewPersona
        {
            get { return new Command(addNewPersona, _ => true); }
        }

        // ReSharper disable once InconsistentNaming
        private void addNewPersona(object obj)
        {
            var newPersona = new EmployeeViewModel
            {
                IsDeleted = false,
                CHANGE_DATE = DateTime.Now,
                Currency = GlobalOptions.ReferencesCache.GetCurrency(GlobalOptions.SystemProfile.EmployeeDefaultCurrency
                    .DocCode) as Currency,
                State = RowStatus.NewRow,
                Id = Guid.NewGuid()
            };
            UserCollection.Clear();
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (ctx.SD_2.Any())
                    newPersona.TabelNumber = ctx.SD_2.Max(_ => _.TABELNUMBER) + 1;
                else
                    newPersona.TabelNumber = 1;
            }

            PersonaCollection.Add(newPersona);
            RaisePropertyChanged(nameof(PersonaCollection));
            RaisePropertyChanged(nameof(CurrentPersona));
            CurrentPersona = newPersona;
        }

        public ICommand AddNewUser
        {
            get { return new Command(addNewUser, _ => true); }
        }

        // ReSharper disable once InconsistentNaming
        private void addNewUser(object obj)
        {
            var ctxnew = new PersonaAddUserForRights();
            var dlg = new SelectDialogView { DataContext = ctxnew };
            dlg.ShowDialog();
            if (!ctxnew.DialogResult) return;
            ctxnew.CurrentRow.State = RowStatus.NewRow;
            if (UserCollection.Any(_ => _.NickName.ToUpper() == ctxnew.CurrentRow.NickName.ToUpper())) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                ctxnew.CurrentRow.TabelNumber = ctx.SD_2.Max(_ => _.TABELNUMBER) + 1;
            }

            UserCollection.Add(ctxnew.CurrentRow);
            CurrentUser = ctxnew.CurrentRow;
            if (CurrentPersona != null && CurrentPersona.State != RowStatus.NewRow)
                CurrentPersona.State = RowStatus.Edited;
            RaisePropertyChanged(nameof(CurrentUser));
        }

        public ICommand DeleteUser
        {
            get { return new Command(deleteUser, _ => true); }
        }

        // ReSharper disable once InconsistentNaming
        private void deleteUser(object obj)
        {
            if (CurrentUser?.State == RowStatus.NewRow)
            {
                UserCollection.Remove(CurrentUser);
                return;
            }

            CurrentPersona.State = RowStatus.Edited;
            UserDeleteCollection.Add(CurrentUser);
            UserCollection.Remove(CurrentUser);
        }

        #endregion
    }
}
