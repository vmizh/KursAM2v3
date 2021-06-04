using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel.Periods;
using Core.EntityViewModel.Systems;
using Core.Invoices.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;

namespace KursAM2.Managers
{
    public class PeriodGroupsRefenceManager
    {
        public ObservableCollection<PERIOD_CLOSEDViewModel> GetPeriods()
        {
            var result = new ObservableCollection<PERIOD_CLOSEDViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var col = ctx.PERIOD_CLOSED
                        .Include(_ => _.CLOSED_DOC_TYPE)
                        .ToList();
                    foreach (var item in col)
                    {
                        var newItem = new PERIOD_CLOSEDViewModel(item);
                        newItem.State = RowStatus.NotEdited;
                        result.Add(newItem);
                    }
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
            return result;
        }

        public ObservableCollection<PERIOD_GROUPSViewModel> GetGroups()
        {
            var result = new ObservableCollection<PERIOD_GROUPSViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var col = ctx.PERIOD_GROUPS.AsNoTracking()
                        .Include(_ => _.PERIOD_CLOSED_EXCLUDE)
                        .ToList();
                    foreach (var item in col)
                    {
                        var newItem = new PERIOD_GROUPSViewModel(item);
                        newItem.State = RowStatus.NotEdited;
                        result.Add(newItem);
                    }
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
            return result;
        }

        public ObservableCollection<PERIOD_GROUPS_USERSViewModel> GetGroupsUsers()
        {
            var result = new ObservableCollection<PERIOD_GROUPS_USERSViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.PERIOD_GROUPS_USERS
                        .Include(_ => _.EXT_USERS);
                    foreach (var item in data)
                    {
                        var newItem = new PERIOD_GROUPS_USERSViewModel(item);
                        newItem.State = RowStatus.NotEdited;
                        result.Add(newItem);
                    }
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
            return result;
        }

        public ObservableCollection<EXT_USERSViewModel> GetAllUsers()
        {
            var result = new ObservableCollection<EXT_USERSViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.EXT_USERS;
                    foreach (var item in data)
                    {
                        var newItem = new EXT_USERSViewModel(item);
                        newItem.State = RowStatus.NotEdited;
                        result.Add(newItem);
                    }
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
            return result;
        }

        public ObservableCollection<PeriodGroupsExludeViewModel> GetAllExcludePeriods()
        {
            var result = new ObservableCollection<PeriodGroupsExludeViewModel>();
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.PERIOD_CLOSED_EXCLUDE;
                    if (data.Any())
                        foreach (var item in data)
                        {
                            var newItem = new PeriodGroupsExludeViewModel(item);
                            newItem.State = RowStatus.NotEdited;
                            result.Add(newItem);
                        }
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
            return result;
        }

        public void SaveGroupUsers(ObservableCollection<PERIOD_GROUPS_USERSViewModel> doc,
            ObservableCollection<PERIOD_GROUPS_USERSViewModel> deletedDoc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.PERIOD_GROUPS_USERS;
                    foreach (var item in doc)
                        data.Add(new PERIOD_GROUPS_USERS
                        {
                            ID = Guid.NewGuid(),
                            USER_ID = item.USER_ID,
                            GROUP_ID = item.GROUP_ID
                        });
                    if (deletedDoc.Count > 0)
                        foreach (var i in deletedDoc)
                        {
                            var deletedItem = ctx.PERIOD_GROUPS_USERS.FirstOrDefault(_ => _.ID == i.Id);
                            if (deletedItem != null)
                                ctx.PERIOD_GROUPS_USERS.Remove(deletedItem);
                        }
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public void SaveExclude(ObservableCollection<PeriodGroupsExludeViewModel> doc)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var item in doc)
                    {
                        var d = ctx.PERIOD_CLOSED_EXCLUDE.FirstOrDefault(_ => _.ID == item.Id);
                        if (d != null)
                        {
                            d.DateFrom = item.DateFrom;
                            d.PERIOD_CLOSED = item.PERIOD_CLOSED;
                        }
                        else
                        {
                            ctx.PERIOD_CLOSED_EXCLUDE.Add(new PERIOD_CLOSED_EXCLUDE
                            {
                                ID = item.Id,
                                CLOSED_ID = item.CLOSED_ID,
                                DateFrom = item.DateFrom,
                                USER_GROUP_ID = item.USER_GROUP_ID,
                                DateExclude = item.DateExclude
                            });
                        }
                    }
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public void SavePeriodClosed(PERIOD_CLOSEDViewModel item)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var r = ctx.PERIOD_CLOSED.FirstOrDefault(_ => _.ID == item.Id);
                    // ReSharper disable once PossibleNullReferenceException
                    r.DateClosed = item.DateClosed;
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public void SavePeriodExlude(PeriodGroupsExludeViewModel item)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var doc = ctx.PERIOD_CLOSED_EXCLUDE.FirstOrDefault(_ => _.ID == item.Id);
                    if (doc != null)
                    {
                        doc.DateExclude = item.DateExclude;
                        doc.DateFrom = item.DateFrom;
                    }
                    else
                    {
                        ctx.PERIOD_CLOSED_EXCLUDE.Add(new PERIOD_CLOSED_EXCLUDE
                        {
                            CLOSED_ID = item.CLOSED_ID,
                            DateExclude = item.DateExclude,
                            DateFrom = item.DateFrom,
                            USER_GROUP_ID = item.USER_GROUP_ID,
                            ID = Guid.NewGuid()
                        });
                    }
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public void SaveGroup(ObservableCollection<PERIOD_GROUPSViewModel> periodGroups)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var item in periodGroups)
                    {
                        if (item.State == RowStatus.NewRow)
                            ctx.PERIOD_GROUPS.Add(new PERIOD_GROUPS
                            {
                                ID = item.Id,
                                NAME = item.Name
                            });
                        if (item.State == RowStatus.Edited)
                        {
                            var i = ctx.PERIOD_GROUPS.FirstOrDefault(_ => _.ID == item.Id);
                            if (i != null)
                                i.NAME = item.Name;
                        }
                    }
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public void DeleteGroup(ObservableCollection<PERIOD_GROUPSViewModel> periodGroupsDelete)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var item in periodGroupsDelete)
                    {
                        var del = ctx.PERIOD_GROUPS.FirstOrDefault(_ => _.ID == item.Id);
                        if (del != null)
                            ctx.PERIOD_GROUPS.Remove(del);
                        var delUser = ctx.PERIOD_GROUPS_USERS.Where(_ => _.GROUP_ID == item.Id);
                        if (delUser.Any())
                            foreach (var i in delUser)
                                ctx.PERIOD_GROUPS_USERS.Remove(i);
                    }
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }

        public void DeletedPeriodExclude(
            ObservableCollection<PeriodGroupsExludeViewModel> periodExcludeDeleteCollection)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var item in periodExcludeDeleteCollection)
                    {
                        var del = ctx.PERIOD_CLOSED_EXCLUDE.FirstOrDefault(_ => _.ID == item.Id);
                        if (del != null)
                            ctx.PERIOD_CLOSED_EXCLUDE.Remove(del);
                    }
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                WindowManager.ShowError(e);
            }
        }
    }
}