using Sklep.Models;
using Sklep.Models.db;
using Sklep.Models.EntityManager;
using Sklep.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Sklep.Controllers
{
    public class LogowanieController : BaseController
    {
        [HttpPost]
        [AllowAnonymous]
        public string Rejestracja(UserRegisterViewModel user)
        {
                bool isLoginExist = UserManager.CheckLoginExists(user.Login);
                if (isLoginExist)
                {
                    return "Login jest już zajęty";          // poinformowanie uzytkownika o zajetym loginie
                }
                if (user.Login.Length < 5)
                {
                    return "Login musi liczyć minimum 4 znaki";          // poinformowanie uzytkownika o zbyt krotkim loginie
                }
                bool isEmailExsits = UserManager.CheckEmailExists(user.Email);
                if (isEmailExsits)
                {
                    return "Adres e-mail jest już zajęty";   // poinformowanie uzytkownika o zajetym adresie email
                }
                var email = new System.Net.Mail.MailAddress(user.Email);
                if (email.Address != user.Email)
                {
                    return "Adres e-mail podano w nieprawidłowej formie";   // poinformowanie uzytkownika o nieprawidłowej formie adresu email
                }

                if (user.Password.Length < 8 || user.Password.Length > 200)
                {
                    return "Hasło musi liczyć od 8 do 200 znaków";          // poinformowanie uzytkownika o zbyt krotkim lub za dlugim hasle
                }
                Regex rgx = new Regex("^(?=.*[0-9])(?=.*[A-Z])(?=.*[a-z])(?=\\S+$).{8,200}$");
                if (!rgx.IsMatch(user.Password))
                {
                    return "Hasło musi mieć jedną wielką, jedną małą literę oraz cyfrę";   // poinformowanie uzytkownika o niepoprawnym formacie hasla
                }
                if (user.Password != user.PasswordRepeat)
                {
                    return "Hasła nie są takie same";          // poinformowanie uzytkownika o niepasujacych haslach
                }
                if(user.Statute == false)
                {
                return "Nie zaakceptowano regulaminu";
                }
                var code = UserManager.GenerateLink(user.Login, user.Email);
                string callbackUrl = Url.Action(
                   "Potwierdzenie", "Logowanie",
                   new { userId = user.Login, code = code },
                   Request.Url.Scheme);
                UserManager.AddUser(user, callbackUrl, code);                         // dodanie uzytkownika do bazy danych
                return "OK";
                
        }
        
        [AllowAnonymous]
        public ActionResult Potwierdzenie(string userId, string code)
        {
            bool isLoginExist = UserManager.CheckLoginExists(userId);
            
            if (!isLoginExist)
            {
                ViewBag.Status = "Nieprawidłowy link";         
                return View();
            }
            bool active = UserManager.ActiveAccount(userId, code);
            if (!active)   
            {
                ViewBag.Status = "Nieprawidłowy link";
                return View();
            }
            ViewBag.isConfirm = 1;
            ViewBag.Status = "Konto jest aktywne";
            return View();
        }
        
        [HttpPost]
        [AllowAnonymous]
        public string Logowanie(UserLoginViewModel user)
        {
            if (!UserManager.CheckActiveAccountAsync(user.Login))
            {
                return "ERROR";
            }

            if (!UserManager.VerifyPassword(user.Login, user.Password))
            {
                return "ERROR";
            }

            FormsAuthentication.SetAuthCookie(user.Login, false);
            PrzypisanieKoszykaDoUzytkownika(user.Login);
            return "OK";
        }

        [Authorize]
        public void PrzypisanieKoszykaDoUzytkownika(string login)
        {
            HttpCookie cookies;
            if (HttpContext.Request.Cookies.Get("koszyk") == null)                  // spradzenie czy uzytkownik posiada ciasteczko "koszyk"
            {
                return; // brak koszyka
            }
            if (Request.Cookies["koszyk"]["p"] == null)          // jezeli nie ma produktow w ciasteczku to dodajemy produkt
            {
                cookies = Request.Cookies["koszyk"];
                cookies.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookies);
                return;
            }

            string cookiesID = Request.Cookies["koszyk"].Value.ToString();           // pobranie indywidualnego numeru z ciasteczka
            string[] productsID = Request.Cookies["koszyk"]["p"].Split(',');         // pobranie ID produktow z ciasteczka
            string[] quantity = Request.Cookies["koszyk"]["q"].Split(',');           // pobranie ilosci produktow z ciasteczka
            System.Diagnostics.Debug.WriteLine("Userprzypisanie " + login);
            cookies = Request.Cookies["koszyk"];
            cookies.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookies);
            cookies = new HttpCookie("koszyk", login);                           // utworzenie ciasteczka o takiej samej nazwie, ktore podmieni stare ciasteczko
            cookies.Expires.AddDays(1);
            Response.Cookies.Add(cookies);


            for (int i = 0; i < productsID.Length; i++)
            {
                cookies.Values.Add("p", productsID[i]);                              // przepisujemy ze starego ciasteczka do nowego ciasteczka produkty  
                cookies.Values.Add("q", quantity[i]);
            }
            HttpContext.Response.SetCookie(cookies);
            cookiesID = Request.Cookies["koszyk"].Value.ToString();
            return;
        }
        [Authorize]
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

        [Authorize]
        public ActionResult Wylogowanie()
        {
            usuniecieKoszykaUzytkownika(User.Identity.Name);
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}