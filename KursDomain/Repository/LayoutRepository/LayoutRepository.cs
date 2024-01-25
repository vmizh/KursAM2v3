using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Data;
using KursDomain.Repository.Base;

namespace KursDomain.Repository.LayoutRepository
{
    public class LayoutRepository : KursGenericRepository<FormLayout, KursSystemEntities, Guid>, ILayoutRepository
    {
        public LayoutRepository(KursSystemEntities context) : base(context)
        {
        }

        public async Task<FormLayout> GetByFormNameAsync(string formName)
        {
            var res = await Context.FormLayout.FirstOrDefaultAsync(_ => _.FormName == formName && _.UserId == GlobalOptions.UserInfo.KursId);
            return res;
        }

        public async Task SaveLayoutAsync(string formName, string layout, string WinState)
        {
            var old = await GetByFormNameAsync(formName);
            if (old != null)
            {
                old.Layout = layout;
                old.UpdateDate = DateTime.Now;
                old.WindowState = WinState;
            }
            else
            {
                Context.Set<FormLayout>().Add(new FormLayout()
                {
                    Id = Guid.NewGuid(),
                    UserId = GlobalOptions.UserInfo.KursId,
                    FormName = formName,
                    ControlName = formName,
                    Layout = layout,
                    WindowState = WinState,
                    UpdateDate = DateTime.Now
                });
            } 
            await Context.SaveChangesAsync();
        }
    }
}
