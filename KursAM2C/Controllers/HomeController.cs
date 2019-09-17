using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Core;
using KursAM2C.Models;
using KursAM2C.Models.DataModel;
using KursAM2C.Models.Source;

namespace KursAM2C.Controllers
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

        public ActionResult About()
        {
            return View();
        }
        public ActionResult Test()
        {
            return View(GetCustomers());
        }
        public ActionResult GridViewPart()
        {
            return PartialView("GridViewPartial", GetCustomers());
        }

        public IEnumerable GetCustomers()
        {
            var c = new List<Customer>
            {
                new Customer()
                {
                    Id = Guid.NewGuid(),
                    City = "test",
                    CompanyName = "test",
                    ContactName = "test",
                    Country = "test",
                    Region = "test"
                },
                new Customer()
                {
                    Id = Guid.NewGuid(),
                    City = "test2",
                    CompanyName = "test2",
                    ContactName = "test2",
                    Country = "test2",
                    Region = "test2"
                },
                new Customer()
                {
                    Id = Guid.NewGuid(),
                    City = "test3",
                    CompanyName = "test3",
                    ContactName = "test3",
                    Country = "test3",
                    Region = "test3"
                }
            };
            return c;
        }
    }
}