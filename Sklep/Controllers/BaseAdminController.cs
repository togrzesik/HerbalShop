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
    public class BaseAdminController : Controller
    {
        // GET: BaseAdmin
        [Authorize]
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            ViewBag.is_Admin = UserManager.CheckUserIsAdmin(User.Identity.Name);
     

        }

        protected override void ExecuteCore()
        {

            base.ExecuteCore();
        }

    }
}