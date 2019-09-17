using System.Collections.Generic;
using System.Linq;
using Core;
using Core.ViewModel.Base;
using Core.ViewModel.Common;

namespace KursAM2.Managers
{
    public class NomenklMainManager
    {
        public List<NomenklMain> GetAll()
        {
            var ret = new List<NomenklMain>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                ret.AddRange(ctx.NomenklMain.Select(g => new NomenklMain
                {
                    State = RowStatus.NotEdited,
                    Name = g.Name,
                    CategoryDC = g.CategoryDC,
                    CountryId = g.CountryId,
                    Id = g.Id,
                    FullName = g.FullName,
                    IsDelete = g.IsDelete,
                    NomenklNumber = g.NomenklNumber,
                    Note = g.Note,
                    IsUsluga = g.IsUsluga,
                    IsNakladExpense = g.IsNakladExpense,
                    NomenklType = g.SD_119 != null ? new NomenklType(g.SD_119) : null,
                    ProductType = g.SD_50 != null ? new NomenklProductViewModel(g.SD_50) : null
                }));
            }
            return ret;
        }
    }
}