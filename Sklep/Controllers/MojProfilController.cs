using Sklep.Models;
using Sklep.Models.db;
using Sklep.Models.EntityManager;
using Sklep.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Sklep.Controllers
{
    [RequireHttps]
    [Authorize]
    public class MojProfilController : BaseController
    {
        // GET: MojProfil
        public ActionResult Index()
        {
            if (TempData["contentModal"] != null)
            {
                ViewBag.titleModal = TempData["titleModal"].ToString(); 
                ViewBag.contentModal = TempData["contentModal"].ToString();
                ViewBag.display = "1";
            }
            return View(UserManager.GetAllDataToView(User.Identity.Name));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ZmienDanePersonalne(ChangeDataInMyProfileViewModel changeData)
        {
            if (ModelState.IsValid)
            {
               
                bool changePersonalData = UserManager.ChangePersonalData(User.Identity.Name, changeData.changePersonalDataViewModel.Name, changeData.changePersonalDataViewModel.Surname, changeData.changePersonalDataViewModel.PhoneNumber);
                if (changePersonalData)
                {
                    
                    TempData["titleModal"] = "Sukces";
                    TempData["contentModal"] = "Zmieniono dane personalne";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["titleModal"] = "Niepowodzenie";
                    TempData["contentModal"] = "Podaj inne dane personalne";

                    return RedirectToAction("Index");
                }
            }
            ViewBag.titleModal = "Niepowodzenie";
            ViewBag.contentModal = "Nie podano wszystkich niezbędnych danych";
            ViewBag.display = "1";
            return View("Index", UserManager.GetAllDataToView(User.Identity.Name));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ZmienEmail(ChangeDataInMyProfileViewModel changeData)
        {
            if (ModelState.IsValid)
            {

                var email = new System.Net.Mail.MailAddress(changeData.changeEmailViewModel.Email); // zmien na wyrazenie regularne
                if (email.Address != changeData.changeEmailViewModel.Email)
                {
                    TempData["titleModal"] = "Niepowodzenie";
                    TempData["contentModal"] = "Adres e-mail podano w nieprawidłowej formie";
                    return RedirectToAction("Index");
                }
                var code = UserManager.GenerateLink(User.Identity.Name, changeData.changeEmailViewModel.Email);
                string callbackUrl = Url.Action(
                   "Potwierdzenie", "Logowanie",
                   new { userId = User.Identity.Name, code = code },
                   Request.Url.Scheme);
                bool changeEmailFlag = UserManager.ChangeUserEmail(User.Identity.Name, changeData.changeEmailViewModel.Email, callbackUrl, code);
                if (changeEmailFlag)
                {
                    usuniecieKoszykaUzytkownika(User.Identity.Name);
                    FormsAuthentication.SignOut();
                    TempData["titleModal"] = "Sukces";
                    TempData["contentModal"] = "Na podany e-mail został wysłany link aktywacyjny";
                    return RedirectToAction("Index", "Logowanie");
                }
                else
                {
                    TempData["titleModal"] = "Niepowodzenie";
                    TempData["contentModal"] = "Podaj inny adres e-mail";
                    
                    return RedirectToAction("Index");
                }
            }
            ViewBag.titleModal = "Niepowodzenie";
            ViewBag.contentModal = "Nie podano adresu e-mail";
            ViewBag.display = "1";
            return View("Index", UserManager.GetAllDataToView(User.Identity.Name));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ZmienHaslo(ChangeDataInMyProfileViewModel changeData)
        {
            System.Diagnostics.Debug.WriteLine("weszło zmiana hasa "+ ModelState.IsValid);
            if (ModelState.IsValid)
            {
                if(changeData.changePasswordViewModel.ActualPassword == null)
                {
                    TempData["titleModal"] = "Niepowodzenie";
                    TempData["contentModal"] = "Aktualne hasło nie zgadza się";
                    return RedirectToAction("Index");
                }
                
                if (changeData.changePasswordViewModel.NewPassword == null || changeData.changePasswordViewModel.NewPassword.Length < 8 || changeData.changePasswordViewModel.NewPassword.Length > 200)
                {
                    TempData["titleModal"] = "Niepowodzenie";
                    TempData["contentModal"] = "Nowe hasło musi liczyć od 8 do 200 znaków";
                    return RedirectToAction("Index");
                }
                Regex rgx = new Regex("^(?=.*[0-9])(?=.*[A-Z])(?=.*[a-z])(?=\\S+$).{8,200}$");
                if (!rgx.IsMatch(changeData.changePasswordViewModel.NewPassword))
                {
                    ModelState.AddModelError("", "Hasło musi mieć jedną wielką, jedną małą literę oraz cyfrę");   // poinformowanie uzytkownika o niepoprawnym formacie hasla
                    TempData["titleModal"] = "Niepowodzenie";
                    TempData["contentModal"] = "Nowe hasło musi mieć jedną wielką, jedną małą literę oraz cyfrę";
                    return RedirectToAction("Index");
                }
                if (changeData.changePasswordViewModel.NewPassword1 == null || (changeData.changePasswordViewModel.NewPassword != changeData.changePasswordViewModel.NewPassword1) )
                {
                    TempData["titleModal"] = "Niepowodzenie";
                    TempData["contentModal"] = "Nowe hasła nie są identyczne";
                    return RedirectToAction("Index");
                }

                bool changePasswordFlag = UserManager.changeUserPassword(User.Identity.Name, changeData.changePasswordViewModel.ActualPassword, changeData.changePasswordViewModel.NewPassword);

                if (changePasswordFlag)
                {
                    TempData["titleModal"] = "Sukces";
                    TempData["contentModal"] = "Hasło zostało zmienione";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["titleModal"] = "Niepowodzenie";
                    TempData["contentModal"] = "Aktualne hasło nie zgadza się";
                    return RedirectToAction("Index");
                }
            }
            ViewBag.titleModal = "Niepowodzenie";
            ViewBag.contentModal = "Nie wypełniono wszystkich pól";
            ViewBag.display = "1";
            return View("Index", UserManager.GetAllDataToView(User.Identity.Name));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ZmienAdresWysylkowy(ChangeDataInMyProfileViewModel changeData)
        {
            if (ModelState.IsValid)
            {
                bool changeDeliveryAddressFlag = UserManager.changeDeliveryAddress(User.Identity.Name, changeData.deliveryAddressViewModel);
                if (changeDeliveryAddressFlag)
                {
                    TempData["titleModal"] = "Sukces";
                    TempData["contentModal"] = "Adres wysyłkowy został zmieniony";
                    return RedirectToAction("Index");
                }
                else { 
                    TempData["titleModal"] = "Niepowodzenie";
                    TempData["contentModal"] = "Proszę jeszcze raz wprowadzić adres do wysyłki";
                    return RedirectToAction("Index");
                }
            }
            ViewBag.titleModal = "Niepowodzenie";
            ViewBag.contentModal = "Nie wypełniono wszystkich pól";
            ViewBag.display = "1";
            return View("Index", UserManager.GetAllDataToView(User.Identity.Name));
        }


        public ActionResult BiezaceZamowienia()
        {
            return View(OrderManager.GetActualOrders(User.Identity.Name));
        }

        public ActionResult SzczegolyZamowienia(int orderID)
        {
            OrderDetailsView orderDetails = OrderManager.GetOrderDetails(User.Identity.Name, orderID);
            if (orderDetails == null)                               // gdy brak dostepu do zamowienia
                return RedirectToAction("Index");

        
                ViewBag.orderID = orderDetails.OrderID;
                if (orderDetails.Status == "Oczekiwanie na wpłatę")
                {
                    ViewBag.Niezaplacono = 1;
                    ViewData["payUData"] = OrderManager.GetPayUFrom(orderDetails);
                }

                return View(orderDetails);
           
        }

        public ActionResult PobierzZamowienie(int orderID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Orders order = db.Orders.Where(x => x.OrderID == orderID).SingleOrDefault();
                if(order == null)
                {
                    return RedirectToAction("BiezaceZamowienia");
                }
                string userNameFromOrder = db.Users.Where(x => x.UserID == order.UserID).Select(x => x.UserName).SingleOrDefault();
                if(User.Identity.Name != userNameFromOrder)
                {
                    return RedirectToAction("BiezaceZamowienia");
                }
                string filename = "Z_" + order.OrderID + "_" + userNameFromOrder;
                string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Zamowienia\\" + filename + ".pdf";
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = filename,
                    Inline = true,
                };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(filedata, contentType);
            }
                
        }

        public ActionResult PobierzFakture(int orderID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Orders order = db.Orders.Where(x => x.OrderID == orderID).SingleOrDefault();
                if (order == null)
                {
                    return RedirectToAction("BiezaceZamowienia");
                }
                string userNameFromOrder = db.Users.Where(x => x.UserID == order.UserID).Select(x => x.UserName).SingleOrDefault();
                if (User.Identity.Name != userNameFromOrder)
                {
                    return RedirectToAction("BiezaceZamowienia");
                }
                string filename = "FV_" + order.OrderID + "_" + userNameFromOrder;
                string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Faktury\\" + filename + ".pdf";
                byte[] filedata = System.IO.File.ReadAllBytes(filepath);
                string contentType = MimeMapping.GetMimeMapping(filepath);

                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = filename,
                    Inline = true,
                };

                Response.AppendHeader("Content-Disposition", cd.ToString());

                return File(filedata, contentType);
            }

        }
        public ActionResult HistoriaZamowien()
        {
            return View(OrderManager.GetHistoryOrders(User.Identity.Name));
        }
        public ActionResult SzczegolyZamowieniaHistoria(int orderID)
        {
            OrderDetailsView orderDetails = OrderManager.GetOrderDetails(User.Identity.Name, orderID);
            if (orderDetails == null)                               // gdy brak dostepu do zamowienia
                return RedirectToAction("Index");
            ViewBag.orderID = orderDetails.OrderID;
            return View(orderDetails);

        }


        public ActionResult UsunKonto(string name = "")
        {
            if (name == "")
            {
                ViewBag.Name = User.Identity.Name;
                return View();
            }
            else if (name == User.Identity.Name)
            {
                bool deleteFlag = UserManager.DeleteUserAccount(name);
                if (deleteFlag)
                {
                    usuniecieKoszykaUzytkownika(User.Identity.Name);
                    FormsAuthentication.SignOut();
                    TempData["Message"] = "Twoje konto zostało usunięte";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "MojProfil");
                }
            }
            else
                return RedirectToAction("Index", "Home");
            
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
    }
}