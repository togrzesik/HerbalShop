using Sklep.Models;
using Sklep.Models.db;
using Sklep.Models.EntityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sklep.Controllers
{
    [RequireHttps]
    public class BaseController : Controller, InterfaceKoszyk
    {
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            ViewBag.IloscSztuk = sprawdzIlosc();
            ViewBag.IsAdmin = UserManager.CheckUserIsAdmin(User.Identity.Name);

        }

        protected override void ExecuteCore()
        {
            base.ExecuteCore();
        }
        
        public int sprawdzIlosc()
        {
            if (HttpContext.Request.Cookies.Get("koszyk") == null || Request.Cookies["koszyk"]["p"] == null)     // sprawdzenie czy uzytkownik posiada ciasteczko "koszyk" i czy w nim sa jakies produkty
            {
                return 0;
            }
            string[] productsID = Request.Cookies["koszyk"]["p"].Split(',');              // pobranie ID produktow z ciasteczka
            return productsID.Length;
        }
    }
}