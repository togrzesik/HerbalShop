using Sklep.Models.EntityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sklep.Controllers
{
    public class KomentarzController : Controller
    {
        public ActionResult Komentarze()
        {
            return PartialView();
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public string DodajKomentarz(string Comment, int Product)
        {
            if (User.Identity.Name == "")
                return "SIGNIN";
            if(!ProductManager.CheckProductExistsID(Product))
                return "ERROR";
        
            if (string.IsNullOrWhiteSpace(Comment) || Comment.Length < 3 || Comment.Length > 500)
                return "ERROR";

            bool isAdded = CommentManager.AddComment(User.Identity.Name, Product, Comment);
            if (isAdded)
                return "OK";
            else
                return "OK";
        }

        public ActionResult WyswietlKomentarz(int productID)
        {

            return PartialView(CommentManager.GetCommentFromProduct(productID));
        }
    }
}