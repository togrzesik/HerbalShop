using Sklep.Models;
using Sklep.Models.db;
using Sklep.Models.EntityManager;
using Sklep.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sklep.Controllers
{
    public class KoszykController : BaseController
    {
        // GET: Koszyk
        public ActionResult Index()
        {
            if (HttpContext.Request.Cookies.Get("koszyk") == null || Request.Cookies["koszyk"]["p"] == null)     // sprawdzenie czy uzytkownik posiada ciasteczko "koszyk" i czy w nim sa jakies produkty
            {
                ViewBag.Status = 0;
                return View();
            }
            
            decimal OrderPriceN = 0M, OrderPriceB = 0M;
            string[] productsID = Request.Cookies["koszyk"]["p"].Split(',');                   // pobranie ID produktow z ciasteczka
            string[] quantity = Request.Cookies["koszyk"]["q"].Split(',');                     // pobranie ilosci produktow z ciasteczka

            List<ShoppingCartView> productsList = ProductManager.GetProductsToShoppingCart(productsID, quantity, ref OrderPriceN, ref OrderPriceB);
            if(TempData["Message"] != null)   // gdy, wybrano "zloz zamowienie", w ktorym ilosc produktu jest wieksza niz dostepna w bazie danych
                ModelState.AddModelError("", TempData["Message"].ToString());

            ViewBag.OrderPriceN = OrderPriceN.ToString("C2"); 
            ViewBag.OrderPriceB = OrderPriceB.ToString("C2");
            if (TempData["contentModal"] != null)
            {
                ViewBag.titleModal = TempData["titleModal"].ToString();
                ViewBag.contentModal = TempData["contentModal"].ToString();
                ViewBag.display = "1";
            }
            return View(productsList);
        }

        // Get: Koszyk/DodajDoKoszyka
        public ActionResult DodajDoKoszyka(int ProductID)
        {
            HttpCookie cookies;
            if (HttpContext.Request.Cookies.Get("koszyk") == null)                  // spradzenie czy uzytkownik posiada ciasteczko "koszyk"
            {
                if(User.Identity.Name != "")   // sprawdzenie czy jest zalogowany
                {
                    cookies = new HttpCookie("koszyk", User.Identity.Name);         // przypisanie ciasteczka do uzytkownika 
                    cookies.Expires.AddDays(1);                                     // ustawienie waznosci ciasteczka na 1 dzien
                    Response.Cookies.Add(cookies);
                }
                else    // gdy nie jest zalogowany
                {
                    cookies = new HttpCookie("koszyk", Guid.NewGuid().ToString());      // nadanie indywidualnego numeru
                    cookies.Expires.AddDays(1);                                         // ustawienie waznosci ciasteczka na 1 dzien
                    Response.Cookies.Add(cookies);
                }
            }     
            cookies = Request.Cookies["koszyk"];                                    // pobranie starego ciasteczka
            cookies.Expires.AddDays(1);

            if(Request.Cookies["koszyk"]["p"] == null)          // jezeli nie ma produktow w ciasteczku to dodajemy produkt
            {
                cookies.Values.Add("p", ProductID.ToString());  // klucz to "p", wartoscia sa ID produktu, np. p : 2,4,6,3
                cookies.Values.Add("q", "1");                   // wartoscia klucza "q" jest wybrana ilosc danego produktu
                HttpContext.Response.SetCookie(cookies);
                return RedirectToAction("Index", "Koszyk");
            }

            string[] productsID = Request.Cookies["koszyk"]["p"].Split(',');
            foreach(string item in productsID)                  // szukamy czy wybrany produkt jest w ciasteczku, jezeli tak to go nie dodajemy
            {
                if(Int32.Parse(item) == ProductID)
                {
                    HttpContext.Response.SetCookie(cookies);
                    return RedirectToAction("Index", "Koszyk");
                }
            }
            
            cookies.Values.Add("p", ProductID.ToString());      // klucz to "p", wartoscia sa ID produktu, np. p : 2,4,6,3
            cookies.Values.Add("q", "1");                       // wartoscia klucza "q" jest wybrana ilosc danego produktu
            HttpContext.Response.SetCookie(cookies);
            return RedirectToAction("Index", "Koszyk");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DodajDoKoszykaIlosc(ProductView productView)          // wybrana ilosc produktu z widoku szczegolowego danego produktu
        {
            if (productView.QuantityChoose > productView.Quantity)
            {
                TempData["Message"] = "Wybrana ilość jest za duża";
                return RedirectToAction("SzczegolyProduktu", "Produkt", new { productID = productView.ProductID });
            }

            HttpCookie cookies;
            if (HttpContext.Request.Cookies.Get("koszyk") == null)                  // spradzenie czy uzytkownik posiada ciasteczko "koszyk"
            {
                if (User.Identity.Name != "")   // sprawdzenie czy jest zalogowany
                {
                    cookies = new HttpCookie("koszyk", User.Identity.Name);         // przypisanie ciasteczka do uzytkownika 
                    cookies.Expires.AddDays(1);                                     // ustawienie waznosci ciasteczka na 1 dzien
                    Response.Cookies.Add(cookies);
                }
                else    // gdy nie jest zalogowany
                {
                    cookies = new HttpCookie("koszyk", Guid.NewGuid().ToString());      // nadanie indywidualnego numeru
                    cookies.Expires.AddDays(1);                                         // ustawienie waznosci ciasteczka na 1 dzien
                    Response.Cookies.Add(cookies);
                }
            }
            cookies = Request.Cookies["koszyk"];                                    // pobranie starego ciasteczka
            cookies.Expires.AddDays(1);

            if (Request.Cookies["koszyk"]["p"] == null)          // jezeli nie ma produktow w ciasteczku to dodajemy produkt
            {
                cookies.Values.Add("p", productView.ProductID.ToString());  // klucz to "p", wartoscia sa ID produktu, np. p : 2,4,6,3
                cookies.Values.Add("q", productView.QuantityChoose.ToString());                   // wartoscia klucza "q" jest wybrana ilosc danego produktu
                HttpContext.Response.SetCookie(cookies);
                return RedirectToAction("Index", "Koszyk");
            }

            string[] productsID = Request.Cookies["koszyk"]["p"].Split(',');
            foreach (string item in productsID)                  // szukamy czy wybrany produkt jest w ciasteczku, jezeli tak to go nie dodajemy
            {
                if (Int32.Parse(item) == productView.ProductID)
                {
                    HttpContext.Response.SetCookie(cookies);
                    return RedirectToAction("Index", "Koszyk");
                }
            }

            cookies.Values.Add("p", productView.ProductID.ToString());      // klucz to "p", wartoscia sa ID produktu, np. p : 2,4,6,3
            cookies.Values.Add("q", productView.QuantityChoose.ToString());                       // wartoscia klucza "q" jest wybrana ilosc danego produktu
            HttpContext.Response.SetCookie(cookies);
            return RedirectToAction("Index", "Koszyk");
        }


        public ActionResult UsunZKoszyka(int ProductID)
        {
            
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
            
            string[] productsID = Request.Cookies["koszyk"]["p"].Split(',');         // pobranie ID produktow z ciasteczka
            string[] quantity = Request.Cookies["koszyk"]["q"].Split(',');           // pobranie ilosci produktow z ciasteczka

            cookies = new HttpCookie("koszyk", cookiesID);                           // utworzenie ciasteczka o takiej samej nazwie, ktore podmieni stare ciasteczko
            cookies.Expires.AddDays(1);                                                     
            Response.Cookies.Add(cookies);
            
           
            for (int i = 0; i< productsID.Length; i++)
            {                
                if (Int32.Parse(productsID[i]) == ProductID)                         // nie dodajemy do nowego ciasteczka produktu wybranego przez uzytkownika 
                {
                    continue;
                }
                cookies.Values.Add("p", productsID[i]);                              // przepisujemy ze starego ciasteczka do nowego ciasteczka produkty  
                cookies.Values.Add("q", quantity[i]);                                
            }
            HttpContext.Response.SetCookie(cookies);
            cookiesID = Request.Cookies["koszyk"].Value.ToString();
            return RedirectToAction("Index", "Koszyk");
        }


        [HttpPost]
        public ActionResult ZmienIlosc(IList<ShoppingCartView> sCV)
        {
            
            if (ModelState.IsValid)
            {
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

                cookies = new HttpCookie("koszyk", cookiesID);                           // utworzenie ciasteczka o takiej samej nazwie, ktore podmieni stare ciasteczko
                cookies.Expires.AddDays(1);
                Response.Cookies.Add(cookies);

                foreach(ShoppingCartView pr in sCV)
                {
                    System.Diagnostics.Debug.WriteLine("co to" + pr.ProductName);
                    cookies.Values.Add("p", pr.ProductID.ToString());                    // uzupelnienie ciasteczka "koszyk" o ProductID oraz Quantity          
                    if (pr.QuantityChoose <= pr.QuantityDB && pr.QuantityChoose >= 1)                              // sprawdzenie czy wybrana liczba sztuk jest mniejsza lub rowna ilosci sztuk w bazie danych
                        cookies.Values.Add("q", pr.QuantityChoose.ToString());
                    else if (pr.QuantityChoose <= 0)
                        cookies.Values.Add("q", 1.ToString());
                    else
                        cookies.Values.Add("q", pr.QuantityDB.ToString());               // jezeli nie, ustalamy maksymalna ilosc dostepna w bazie danych
                }
                HttpContext.Response.SetCookie(cookies);
                cookiesID = Request.Cookies["koszyk"].Value.ToString();
            }
            return RedirectToAction("Index", "Koszyk");
        }
    }
}