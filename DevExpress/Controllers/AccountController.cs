using System;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Core;
using DevExpress.Models.DataModel;
using DevExpress.Models.logicModel;
using Helper;

namespace DevExpress.Controllers
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
                InitializationConnect(model.Name, model.Password);
                HttpCookie AuthCookie;
                AuthCookie = FormsAuthentication.GetAuthCookie(model.Name, true);
                //Устаревание кук
                AuthCookie.Expires = DateTime.Now.AddDays(1);
                //Добавляем куки в ответ
                Response.Cookies.Add(AuthCookie);
                //FormsAuthentication.SetAuthCookie(model.Name, true);
                MenuSource.GetMenuItem(model.Name);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (e.Message == "The underlying provider failed on Open.")
                    ModelState.AddModelError("", @"Неправильный пароль или имя пользователя.");
                else
                    throw;
            }
            return View(model);
        }

        public static void InitializationConnect(string psw, string login)
        {
            GlobalOptions.SqlConnectionString = GetConnectionString(psw, login);
            GlobalOptions.GetEntities();
            if (GlobalOptions.UserInfo == null)
            {
                GlobalOptions.UserInfo = new User
                {
                    Id = 6
                };
                var reffer = new MainReferences();
                reffer.Reset();
            }
        }

        public static string GetConnectionString(string user, string pwd)
        {
            var strConn = new SqlConnectionStringBuilder
            {
                DataSource = "172.16.0.1",
                InitialCatalog = "EcoOndol",
                UserID = user,
                Password = pwd
            };
            return strConn.ConnectionString;
        }
    }
}