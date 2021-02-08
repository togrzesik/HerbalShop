using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNet.Identity;
using Sklep.Models;
using Sklep.Models.db;
using Sklep.Models.EntityManager;
using Sklep.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sklep.Controllers
{
    public class ZamowieniaController : BaseController
    {
        // GET: Zamowienia
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ZlozZamowienie()
        {
            if (User.Identity.Name == "")
            {
                TempData["titleModal"] = "Złożenie zamówienia";
                TempData["contentModal"] = "Jeżeli chcesz złożyć zamówienie, zaloguj się";
                return RedirectToAction("Index", "Koszyk");
            }

            HttpCookie cookies;
            if (HttpContext.Request.Cookies.Get("koszyk") == null)    // spradzenie czy uzytkownik posiada ciasteczko "koszyk"
            {
                return RedirectToAction("Index", "Koszyk");
            }
            if (Request.Cookies["koszyk"]["p"] == null)               // sprawdzenie czy istnieje jakis produkt w ciasteczku "koszyk"
            {
                return RedirectToAction("Index", "Koszyk");
            }
            string cookiesID = Request.Cookies["koszyk"].Value.ToString();           // pobranie indywidualnego numeru z ciasteczka
            string[] splitID = cookiesID.Split('&');
            cookiesID = splitID[0];
            if (User.Identity.Name != cookiesID)          // sprawdzenie czy ciasteczko koszyk nalezy do uzytkownika zalogowanego
            {
                return RedirectToAction("Index", "Home");
            }
            string[] productsID = Request.Cookies["koszyk"]["p"].Split(',');         // pobranie ID produktow z ciasteczka
            string[] quantity = Request.Cookies["koszyk"]["q"].Split(',');           // pobranie ilosci produktow z ciasteczka
            string productName;
            int productQuantity, orderID;
            bool submitOrder = OrderManager.SubmitOrder(productsID, quantity, User.Identity.Name, out productName, out productQuantity, out orderID);
            if (!submitOrder)
            {
                TempData["titleModal"] = "Złożenie zamówienia";
                TempData["contentModal"] = "Nie ma wystarczającej ilości produktu <br /> <b>" + productName + "</b> <br /> Liczba dostępnych sztuk: <b>" + productQuantity + "</b>";
                return RedirectToAction("Index", "Koszyk");
            }

           
            TempData["orderID"] = orderID;
            return RedirectToAction("wyborDostawy");
        }

        [Authorize]
        public ActionResult wyborDostawy()
        {
            List<SelectListItem> deliveriesList = OrderManager.GetDeliveriesList();
            ViewData["deliveriesList"] = (IEnumerable<SelectListItem>)deliveriesList;
/*            int deliveryID = Int32.Parse(Request["de  liveriesList"]);
            if(deliveryID == 1)
            {
                ViewBag.Delivery = 1;
            }*/
            usuniecieKoszykaUzytkownika(User.Identity.Name);
            DeliveryAddressViewModel deliveryAddressViewModel = UserManager.GetUserAddress(User.Identity.Name);

            return View(deliveryAddressViewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Podsumowanie(DeliveryAddressViewModel deliveryAddressViewModel)
        {

            if (ModelState.IsValid)
            {
                int deliveryID = Int32.Parse(Request["deliveriesList"]);
                int status;
                SummaryOrderViewModel summaryOrderViewModel = OrderManager.GetOrderToSummary(deliveryAddressViewModel, deliveryID, User.Identity.Name, out status);
                ViewBag.Status = status;
                OrderDetailsView orderDetails = new OrderDetailsView();
                orderDetails.orderProductList = summaryOrderViewModel.orderProductList;
                orderDetails.TotalOrderPriceB = summaryOrderViewModel.TotalOrderPriceB;
                orderDetails.DeliveryPriceB = summaryOrderViewModel.DeliveryPriceB;
                orderDetails.NameDelivery = summaryOrderViewModel.NameDelivery;
                ViewData["payUData"] = OrderManager.GetPayUFrom(orderDetails);
                return View(summaryOrderViewModel);
                
            }
            else
            {
                // TODO: usuniecie zamowienia z BD
                return RedirectToAction("Index", "Koszyk");
            }
        }

        public void usuniecieKoszykaUzytkownika(string login)
        {
            HttpCookie cookies;
            if (HttpContext.Request.Cookies.Get("koszyk") == null)                  // spradzenie czy uzytkownik posiada ciasteczko "koszyk"
            {
                return; // brak koszyka
            }
            cookies = Request.Cookies["koszyk"];
            string cookiesID = Request.Cookies["koszyk"].Value.ToString();           // pobranie indywidualnego numeru z ciasteczka
            string[] splitID = cookiesID.Split('&');
            cookiesID = splitID[0];
            if (cookiesID == login)
            {
                cookies.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookies);
                return;
            }
        }

        public ActionResult PayUSuccess()
        {
            return View();
        }

    }
}