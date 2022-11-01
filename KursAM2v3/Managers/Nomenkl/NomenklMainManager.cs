using System.Collections.Generic;
using System.Linq;
using Core;
using Core.ViewModel.Base;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;

namespace KursAM2.Managers.Nomenkl
{
    public class NomenklMainManager
    {
        public List<NomenklMainViewModel> GetAll()
        {
            var ret = new List<NomenklMainViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                ret.AddRange(ctx.NomenklMain.Select(g => new NomenklMainViewModel
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
                    NomenklType = g.SD_119 != null ? new NomenklProductType(g.SD_119) : null,
                    ProductType = g.SD_50 != null ? new NomenklProductKind(g.SD_50) : null
                }));
            }

            return ret;
        }
    }
}
