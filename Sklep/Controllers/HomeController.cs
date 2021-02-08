using Sklep.Models.db;
using Sklep.Models.EntityManager;
using Sklep.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Sklep.Models;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Text;

namespace Sklep.Controllers
{
    public class HomeController : BaseController
    {

        
        public ActionResult Index()
        {
            
            List<ProductRecommededView> products = ProductManager.GetProductsRecommended();
            if(products == null)
            {
                ViewBag.Ilosc = 0;
                ModelState.AddModelError("", "Brak produktów polecanych");
                return View();
            }
            return View(products);
        }

        public ActionResult ProductsBestseller()
        {
            List<ProductRecommededView> recommended = ProductManager.GetProductsBestseller();
            if(recommended !=  null)
            { 
                return PartialView(recommended);
            }
            else
            {
                ViewBag.Ilosc = 0;
                return PartialView();
            }
        }

        public ActionResult Kontakt()
        {
            if (TempData["contentModal"] != null)
            {
                ViewBag.titleModal = TempData["titleModal"].ToString();
                ViewBag.contentModal = TempData["contentModal"].ToString();
                ViewBag.display = "1";
                return View("Kontakt");
            }
            return View();
        }

        [HttpPost]
        public ActionResult WyslijPytanie(ContactView contactView)
        {
            if(ModelState.IsValid)
            {
                EmailService.sendQuestion(contactView.Email, contactView.Content);
                TempData["titleModal"] = "Powodzenie";
                TempData["contentModal"] = "Wiadomość została wysłana";
                return RedirectToAction("Kontakt");
            }
            else
            {
                if(contactView.Email == null)
                {
                    TempData["titleModal"] = "Niepowodzenie";
                    TempData["contentModal"] = "Pole e-mail nie może być puste";
                    return RedirectToAction("Kontakt");
                }
                var email = new System.Net.Mail.MailAddress(contactView.Email);
                if (email.Address != contactView.Email)
                {
                    TempData["titleModal"] = "Niepowodzenie";
                    TempData["contentModal"] = "Podano zły adres e-mail";
                    return RedirectToAction("Kontakt");
                }
                if(contactView.Content == null)
                {
                    TempData["titleModal"] = "Niepowodzenie";
                    TempData["contentModal"] = "Treść wiadomości nie może być pusta";
                    return RedirectToAction("Kontakt");
                }
            }
            return RedirectToAction("Kontakt");
        }

        public ActionResult Regulamin()
        {
            return View();
        }

        public ActionResult KosztyDostawy()
        {
            List<DeliveryView> deliveriesList = new List<DeliveryView>();
            using (sklepEntities db = new sklepEntities())
            {
                var deliveriesDB = db.Deliveries;
                foreach (var item in deliveriesDB)
                {
                    DeliveryView delivery = new DeliveryView();
                    delivery.DeliveryName = item.Name;
                    delivery.DeliveryPriceN = (item.PriceN).ToString("C2");
                    delivery.DeliveryPriceB = ((1M + item.Vat) * item.PriceN).ToString("C2");
                    delivery.DeliveryVAT = item.Vat;

                    deliveriesList.Add(delivery);
                }
            }
            return View(deliveriesList);
        }
    }
}