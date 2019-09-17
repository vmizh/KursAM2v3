using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Core;
using KursAM2C.Models;
using KursAM2C.Providers;

namespace KursAM2C.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model)
        {
            GlobalOptions.DataBaseName = "EcoOndol";
            //GlobalOptions.DatabaseColor = SelectedDataSource.Color;
            //Проверка логина и пароля(на SQL сервере)
            try
            {
                ConnectProviders.InitializationConnect();
                using (var ctx = GlobalOptions.GetEntities())
                {
                    var usr = ctx.EXT_USERS
                        .FirstOrDefault(_ => _.USR_NICKNAME.ToUpper() == model.Name.ToUpper());
                    if (usr != null)
                    {
                        FormsAuthentication.SetAuthCookie(model.Name, true);
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (e.Message == "The underlying provider failed on Open.")
                {
                    ModelState.AddModelError("", "Неправильный пароль или имя пользователя.");
                }
                else
                {
                    throw;
                }
            }
            return View(model);
        }
    }
}