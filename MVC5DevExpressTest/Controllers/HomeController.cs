using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC5DevExpressTest.Models;

namespace MVC5DevExpressTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

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

        public ActionResult GridViewPartialDelete(Custome item)
        {
            return PartialView("GridViewPartial", item);
        }

        public ActionResult GridViewPartialAddNew(Custome item)
        {
            return PartialView("GridViewPartial", item);
        }

        public IEnumerable GetCustomers()
        {
            var c = new List<Custome>
            {
                new Custome()
                {
                    Id = Guid.NewGuid(),
                    City = "test",
                    CompanyName = "test",
                    Region = "test",
                    Country = "test",
                    ContactName = "test",
                },
                new Custome()
                {
                    Id = Guid.NewGuid(),
                    City = "test2",
                    CompanyName = "test2",
                    Region = "test2",
                    Country = "test2",
                    ContactName = "test2",
                },
                new Custome()
                {
                    Id = Guid.NewGuid(),
                    City = "test3",
                    CompanyName = "test3",
                    Region = "test3",
                    Country = "test3",
                    ContactName = "test3",
                },
            };
            return c;
        }
    }
}