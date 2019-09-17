using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Core;
using Core.ViewModel.Common;
using KursAM2C.Models.DataModel;

namespace KursAM2C.Controllers
{
    public class EmployeesController : Controller
    {
        private static List<Emploee> EmploeesCol { set; get; } = new List<Emploee>();
        // GET: Employees
        public ActionResult Employees()
        {
           if (!EmploeesCol.Any())
            using (var ctx = GlobalOptions.GetEntities())
            {
                foreach (var item in ctx.SD_2)
                {
                    EmploeesCol.Add(new Emploee
                    {
                        Tablenumber = item.TABELNUMBER,
                        LastName = item.NAME_LAST,
                        FirstName = item.NAME_FIRST,
                        SecondName = item.NAME_SECOND
                    });
                }
            }
            return View(EmploeesCol);
        }

        [HttpPost]
        public ActionResult Create(Emploee model)
        {
            var tabl = EmploeesCol.Max(_ => _.Tablenumber);
            EmploeesCol.Add(new Emploee
            {
                Tablenumber = tabl,
                LastName = model.LastName,
                FirstName = model.FirstName,
                SecondName = model.LastName,
            });
            return View("Employees", EmploeesCol); ;
        }

        public ActionResult Remove(int id)
        {
            var del = EmploeesCol.FirstOrDefault(_ => _.Tablenumber == id);
            EmploeesCol.Remove(del);
            return View("Employees", EmploeesCol);
        }
    }
}