using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core;
using NUnit.Framework;

namespace KursAM2.Tests.EntityViewModels
{
    [TestFixture]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class ProjectTests : TestBase
    {
        [Test]
        public void ProviderLoadTest()
        {
            var dclist = new List<Guid>(){
                Guid.Parse("ccd9e999-1fda-4d64-a027-1ee8bfef62e3"),
                Guid.Parse("47f55f9e-9233-4d28-9255-2168fee7c2d1")
            };
            using (var ent = GlobalOptions.GetEntities())
            {
                // ReSharper disable once UnusedVariable
                var dd = (from p in ent.ProjectProviderPrihod
                        from td24 in ent.TD_24
                        where dclist.Contains(p.ProjectId) && td24.Id == p.RowId
                        select p
                    ).ToList();

                var dataTovarPoluchen = (from prjDoc in ent.ProjectProviderPrihod
                    from td24 in ent.TD_24
                    from sd24 in ent.SD_24
                    from sd43 in ent.SD_43
                                         from td26 in ent.TD_26
                                         from sd83 in ent.SD_83
                                         from sd26 in ent.SD_26
                                         where td24.DOC_CODE == sd24.DOC_CODE
                          && dclist.Contains(prjDoc.ProjectId) && td24.Id == prjDoc.RowId
                          && sd43.DOC_CODE == sd24.DD_KONTR_OTPR_DC
                          && sd83.DOC_CODE == td24.DDT_NOMENKL_DC
                          && td26.DOC_CODE == td24.DDT_SPOST_DC && td26.CODE == td24.DDT_SPOST_ROW_CODE
                          && sd26.DOC_CODE == td26.DOC_CODE
                                         select prjDoc).ToList();
            }
        }
    }
}