using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.ViewModel.Base;
using Core.WindowsManager;
using Data;
using KursAM2.Managers.Base;
using KursAM2.ViewModel.Reference;

namespace KursAM2.Managers
{
    public class AccessRightsManager : DocumentManager<EXT_USERSViewModel>
    {
        public override List<EXT_USERSViewModel> GetDocuments()
        {
            var ret = new List<EXT_USERSViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                ret.AddRange(ctx.EXT_USERS.ToList()
                    .Select(item => new EXT_USERSViewModel(item) {State = RowStatus.NotEdited}));
            }
            return ret;
        }

        public List<MAIN_DOCUMENT_GROUPViewModel> GetPermission()
        {
            var ret = new List<MAIN_DOCUMENT_GROUPViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var item in ctx.MAIN_DOCUMENT_GROUP.ToList())
                    ret.Add(new MAIN_DOCUMENT_GROUPViewModel(item) {State = RowStatus.NotEdited});
            }
            return ret;
        }

        public override List<EXT_USERSViewModel> GetDocuments(DateTime dateStart, DateTime dateEnd)
        {
            var ret = new List<EXT_USERSViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var item in ctx.EXT_USERS.ToList())
                    ret.Add(new EXT_USERSViewModel(item) {State = RowStatus.NotEdited});
            }
            return ret;
        }

        public override List<EXT_USERSViewModel> GetDocuments(DateTime dateStart, DateTime dateEnd, string searchText)
        {
            throw new NotImplementedException();
        }


        public override EXT_USERSViewModel Load()
        {
            throw new NotImplementedException();
        }

        public override EXT_USERSViewModel Load(decimal dc)
        {
            throw new NotImplementedException();
        }

        public override EXT_USERSViewModel Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public override EXT_USERSViewModel Save(EXT_USERSViewModel doc)
        {
            throw new NotImplementedException();
        }

        public void SaveBlock(EXT_USERSViewModel current)
        {
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var item = ctx.EXT_USERS.FirstOrDefault(_ => _.USR_ID == current.USR_ID);
                    if (item == null) return;
                    item.USR_ABORT_CONNECT = current.USR_ABORT_CONNECT;
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public void SaveCollection(IEnumerable<EXT_USERSViewModel> ReferenceCollection,
            IEnumerable<EXT_USERSViewModel> DeletedCountry)
        {
            if (ReferenceCollection == null)
                return;
            try
            {
                using (var ctx = GlobalOptions.GetEntities())
                {
                    foreach (var doc in ReferenceCollection)
                        switch (doc.State)
                        {
                            case RowStatus.NewRow:
                                ctx.EXT_USERS.Add(new EXT_USERS
                                {
                                    USR_ID = doc.USR_ID,
                                    USR_NICKNAME = doc.USR_NICKNAME,
                                    USR_FULLNAME = doc.USR_FULLNAME,
                                    TABELNUMBER = doc.TABELNUMBER,
                                    USR_NOTES = doc.USR_NOTES,
                                    USR_ABORT_CONNECT = doc.USR_ABORT_CONNECT
                                });

                                break;
                            case RowStatus.Edited:
                                var entity = ctx.Countries.FirstOrDefault(_ => _.Id == doc.Id);
                                ctx.Entry(entity).CurrentValues.SetValues(doc.Entity);
                                break;
                        }
                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
        }

        public override bool IsChecked(EXT_USERSViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override EXT_USERSViewModel New()
        {
            throw new NotImplementedException();
        }

        public override EXT_USERSViewModel NewFullCopy(EXT_USERSViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override EXT_USERSViewModel NewRequisity(EXT_USERSViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override void Delete(EXT_USERSViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override void Delete(decimal dc)
        {
            throw new NotImplementedException();
        }

        public override void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void SavePermission(TreeDocument curPermission, string login)
        {
            if (curPermission == null) return;
            using (var ctx = GlobalOptions.GetEntities())
            {
                if (!curPermission.IsCheked)
                {
                    var RemoveItem =
                        ctx.USER_FORMS_RIGHT.FirstOrDefault(_
                            => _.USER_NAME.ToUpper() == login.ToUpper() &&
                               _.FORM_ID == curPermission.BaseId);
                    if(RemoveItem != null)
                        ctx.USER_FORMS_RIGHT.Remove(RemoveItem);
                }
                else
                {
                    ctx.USER_FORMS_RIGHT.Add(new USER_FORMS_RIGHT
                    {
                        USER_NAME = login,
                        FORM_ID = (int)curPermission.BaseId
                    });
                }
                ctx.SaveChanges();

            }
        }
       
    }
}