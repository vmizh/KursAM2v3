using System.Collections.Generic;
using System.Linq;
using Core;
using Core.EntityViewModel.NomenklManagement;
using Core.ViewModel.Base;
using KursDomain;
using KursDomain.Documents.NomenklManagement;
using KursDomain.ICommon;
using KursDomain.References;
using NomenklProductType = KursDomain.References.NomenklProductType;

namespace KursAM2.Managers.Nomenkl
{
    public class NomenklMainManager
    {
        public List<NomenklMainViewModel> GetAll()
        {
            var ret = new List<NomenklMainViewModel>();
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var g in ctx.NomenklMain.ToList())
                {
                    var newItem = new NomenklMainViewModel
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
                        NomenklType = g.SD_119 != null
                            ? GlobalOptions.ReferencesCache.GetNomenklType(g.SD_119.DOC_CODE) as NomenklType
                            : null,
                    };
                    newItem.ProductType = new ProductType();
                    newItem.ProductType.LoadFromEntity(g.SD_50);
                    ret.Add(newItem);
                }
            }

            return ret;
        }
    }
}
