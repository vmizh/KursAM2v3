using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core;
using Core.EntityViewModel;
using Core.WindowsManager;
using KursAM2.Managers.Base;

namespace KursAM2.Managers
{
    public class CashInManager : DocumentManager<SD_33ViewModel>
    {
        public override List<SD_33ViewModel> GetDocuments()
        {
            throw new NotImplementedException();
        }

        public override List<SD_33ViewModel> GetDocuments(DateTime dateStart, DateTime dateEnd)
        {
            throw new NotImplementedException();
        }

        public override List<SD_33ViewModel> GetDocuments(DateTime dateStart, DateTime dateEnd, string searchText)
        {
            throw new NotImplementedException();
        }

        public override SD_33ViewModel Load()
        {
            throw new NotImplementedException();
        }

        public override SD_33ViewModel Load(decimal dc)
        {
            try
            { 
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var data = ctx.SD_33
                        .Include(_ => _.SD_114)
                        .Include(_ => _.SD_2)
                        .Include(_ => _.SD_22)
                        .Include(_ => _.SD_301)
                        .Include(_ => _.SD_3011)
                        .Include(_ => _.SD_3012)
                        .Include(_ => _.SD_3013)
                        .Include(_ => _.SD_303)
                        .Include(_ => _.SD_34)
                        .Include(_ => _.VD_46)
                        .Include(_ => _.SD_90)
                        .Include(_ => _.SD_84)
                        .Include(_ => _.SD_43)
                        .AsNoTracking()
                        .FirstOrDefault(_ => _.DOC_CODE == dc);
                    return new SD_33ViewModel(data);
                }
            }
            catch (Exception ex)
            {
                WindowManager.ShowError(ex);
            }
            return null;
        }

        public override SD_33ViewModel Load(Guid id)
        {
            throw new NotImplementedException();
        }

        public override SD_33ViewModel Save(SD_33ViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override bool IsChecked(SD_33ViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override SD_33ViewModel New()
        {
            throw new NotImplementedException();
        }

        public override SD_33ViewModel NewFullCopy(SD_33ViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override SD_33ViewModel NewRequisity(SD_33ViewModel doc)
        {
            throw new NotImplementedException();
        }

        public override void Delete(SD_33ViewModel doc)
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
    }
}