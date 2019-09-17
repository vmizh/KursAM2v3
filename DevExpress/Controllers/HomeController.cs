using System.Web.Mvc;
using DevExpress.Models;

namespace DevExpress.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Roles = "Управление,Менеджеры")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Menu()
        {
            return PartialView("_Menu", Globals.MenuItem);
        }
    }
}