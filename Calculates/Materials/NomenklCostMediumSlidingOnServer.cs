using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core;
using KursDomain;

namespace Calculates.Materials
{
    public class NomenklCostMediumSlidingOnServer : NomenklCostBase
    {
        public override ObservableCollection<NomenklCalcCostOperation> GetOperations(decimal nomDC, bool isCalcOnly = true, decimal? storeDC = null)
        {
            return null;
        }

        public override List<NomenklCalcCostOperation> Calc(ObservableCollection<NomenklCalcCostOperation> operList)
        {
            // ReSharper disable once UnusedVariable
            using (var ctx = GlobalOptions.GetEntities())
            {
                //ctx.Database.ExecuteSqlCommand("exec NomenklCalculateCostsForAll",new object[0]);
            }
            return new List<NomenklCalcCostOperation>();
        }

        public override void Save(IEnumerable<NomenklCalcCostOperation> operList)
        {
           
        }
    }
}
