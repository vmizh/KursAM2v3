using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Core;
using Core.EntityViewModel;
using KursAM2.Managers;
using KursAM2.ViewModel.Finance.Cash;

namespace DevExpress.Controllers
{
    public class CashBookController : Controller
    {
        private static CashManager Manager = new CashManager();

        // GET: CashBook
        [Authorize(Roles = "Управление,Менеджеры")]
        public ActionResult Index()
        {
            Refrash();
            return View();
        }

        private void Refrash()
        {
            Periods.Clear();
            Documents.Clear();
            MoneyRemains.Clear();
        }

        public ActionResult AddNewRowMainGrid(CashBookDocument item)
        {
            Documents.Add(item);
            return PartialView("CashGridPart", Documents);
        }

        public ActionResult DeleteRowMainGrid(CashBookDocument item)
        {
            Documents.Remove(item);
            return PartialView("CashGridPart", Documents);
        }

        public ActionResult EditRowMainGrid(CashBookDocument item)
        {
            return PartialView("CashGridPart", Documents);
        }

        public ActionResult PeriodsPart()
        {
            if (!string.IsNullOrEmpty(Request.Params["PeriodId"]))
            {
                LoadDocuments();
                GetRemains();
            }
            if (!string.IsNullOrEmpty(Request.Params["CashId"]))
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var datelst = ctx.SD_39.Where(_ => _.CA_DC == Convert.ToDecimal(Request.Params["CashId"]))
                        .Select(_ => _.DATE_CASS)
                        .Cast<DateTime>().Distinct().ToList();
                    Periods = new List<DatePeriod>(DatePeriod.GenerateIerarhy(datelst, PeriodIerarhy.YearMonthDay));
                }
            return PartialView("PeriodsViewPart", Periods);
        }

        public ActionResult SubCashGridPart()
        {
            return PartialView("SubCashGridPart", MoneyRemains);
        }

        public ActionResult MainGridPart()
        {
            return PartialView("CashGridPart", Documents);
        }

        #region Field

        public List<Cash> CashList { set; get; } =
            new List<Cash>(MainReferences.Cashs.Values.Where(_ => _.IsAccessRight));
        public List<DatePeriod> Periods = new List<DatePeriod>();
        public List<CashBookDocument> Documents = new List<CashBookDocument>();
        public List<MoneyRemains> MoneyRemains { set; get; } = new List<MoneyRemains>();

        #endregion

        #region Methods

        private void LoadDocuments()
        {
            Documents.Clear();
            if (Periods == null) return;
            foreach (var d in CashManager.LoadDocuments(
                CashList.FirstOrDefault(_ => _.DOC_CODE == Convert.ToDecimal(Request.Params["CashId"])),
                // ReSharper disable once PossibleNullReferenceException
                Periods.FirstOrDefault(_ => _.Id == Guid.Parse(Request.Params["PeriodId"])).DateStart,
                // ReSharper disable once PossibleNullReferenceException
                Periods.FirstOrDefault(_ => _.Id == Guid.Parse(Request.Params["PeriodId"])).DateEnd))
                Documents.Add(d);
            MainGridPart();
        }

        private void GetRemains()
        {
            MoneyRemains.Clear();
            foreach (var item in CashManager.GetRemains(
                CashList.FirstOrDefault(_ => _.DOC_CODE == Convert.ToDecimal(Request.Params["CashId"])),
                Periods.FirstOrDefault(_ => _.Id == Guid.Parse(Request.Params["PeriodId"])),
                new List<CashBookDocument>(Documents))) MoneyRemains.Add(item);
        }

        #endregion
    }
}