using Sklep.Models.db;
using Sklep.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sklep.Models.EntityManager
{
    public class UserManager
    {
        public static void AddUser(UserRegisterViewModel URV, string link, string code)              // dodanie uzytkownika
        {
            using (sklepEntities db = new sklepEntities())
            {
                Users user = new Users();
                user.UserName = URV.Login;
                user.Email = URV.Email;
                string mySalt = BCrypt.Net.BCrypt.GenerateSalt();
                string myHash = BCrypt.Net.BCrypt.HashPassword(URV.Password, mySalt);
                user.Password = myHash;
                user.IsActive = "N";
                user.Role = "Uzytkownik";
                user.Link = code;
                EmailService.sendConfirmAccount(user.UserName, user.Email, link);
                db.Users.Add(user);
                db.SaveChanges();
            }

        }

        public static bool DeleteUserAccount(string login)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Users user = db.Users.Where(u => u.UserName == login).Single();
                user.IsActive = "U";
                db.SaveChanges();
                return true;
            }
        }
        public static bool CheckLoginExists(string login)    // sprawdzenie czy login istnieje w bazie danych
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Users.Where(u => u.UserName.Equals(login)).Any();
            }
        }

        public static string CheckUserIsAdmin(string login)
        {
            using (sklepEntities db = new sklepEntities())
            {
                if (CheckLoginExists(login))
                    return db.Users.Where(u => u.UserName.Equals(login)).Select(x => x.Role=="Admin").Single().ToString();
                else
                    return "False";
            }
        }

        public static bool CheckActiveAccountAsync(string login)
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Users.Where(u => u.UserName.Equals(login) && u.IsActive == "A").Any();
            }
        }

        public static bool CheckEmailExists(string email)    // sprawdzenie czy email istnieje w bazie danych
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Users.Where(u => u.Email.Equals(email)).Any();
            }
        }

        public static string GenerateLink(string login, string email)                // generowanie linku
        {
           
            string link = sha256(login + "%2fz&51%(f" + email + DateTime.Now);
            return link;
        }

        static string sha256(string randomString)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString), 0, Encoding.UTF8.GetByteCount(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        public static bool VerifyPassword(string login, string passwordView)            // weryfikacja hasla do konta uzytkownika
        {
            bool doesPasswordMatch = false;
            using (sklepEntities db = new sklepEntities())
            {
                string passwordDB = db.Users.Where(x => x.UserName == login).Select(x => x.Password).Single();
                doesPasswordMatch = BCrypt.Net.BCrypt.Verify(passwordView, passwordDB);
            }
            return doesPasswordMatch;
        }


        public static bool ActiveAccount(string login, string code)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Users user = db.Users.Where(x => x.UserName == login).Single();
                if (code != user.Link)
                    return false;
                user.IsActive = "A";
                string mySalt = BCrypt.Net.BCrypt.GenerateSalt();
                string myHash = BCrypt.Net.BCrypt.HashPassword((DateTime.Now).ToString(), mySalt);
                user.Link = myHash;
                db.SaveChanges();
                return true;
            }
        }


        public static ChangeDataInMyProfileViewModel GetAllDataToView(string login)
        {
            ChangeDataInMyProfileViewModel changeDataInMyProfileViewModel = new ChangeDataInMyProfileViewModel();
            changeDataInMyProfileViewModel.changeEmailViewModel = new ChangeEmailViewModel();
            changeDataInMyProfileViewModel.changePersonalDataViewModel = new ChangePersonalDataViewModel();
            changeDataInMyProfileViewModel.deliveryAddressViewModel = new DeliveryAddressViewModel();

            using (sklepEntities db = new sklepEntities())
            {
                Users user = db.Users.Where(x => x.UserName == login).Single();
                changeDataInMyProfileViewModel.changePersonalDataViewModel.Name = user.Name;
                changeDataInMyProfileViewModel.changePersonalDataViewModel.Surname = user.Surname;
                if (user.PhoneNumber != null)
                    changeDataInMyProfileViewModel.changePersonalDataViewModel.PhoneNumber = user.PhoneNumber;
                changeDataInMyProfileViewModel.deliveryAddressViewModel.City = user.City;
                changeDataInMyProfileViewModel.deliveryAddressViewModel.NumOfHouse = user.NumOfHouse;
                changeDataInMyProfileViewModel.deliveryAddressViewModel.PostCode = user.PostCode;
                changeDataInMyProfileViewModel.deliveryAddressViewModel.Street = user.Street;
                changeDataInMyProfileViewModel.changeEmailViewModel.Email = user.Email;
                return changeDataInMyProfileViewModel;
            }
        }
        public static ChangePersonalDataViewModel GetPersonalData(string login)   // uzyskanie danych adresowych uzytkownika
        {
            ChangePersonalDataViewModel changePersonalDataViewModel = new ChangePersonalDataViewModel();

            using (sklepEntities db = new sklepEntities())
            {
              
                Users user = db.Users.Where(x => x.UserName == login).Single();
                changePersonalDataViewModel.Name = user.Name;
                changePersonalDataViewModel.Surname = user.Surname;
                if(user.PhoneNumber != null)
                    changePersonalDataViewModel.PhoneNumber = (int)user.PhoneNumber;
                return changePersonalDataViewModel;
            }
        }

        public static DeliveryAddressViewModel GetUserAddress(string login)   // uzyskanie danych adresowych uzytkownika
        {
            DeliveryAddressViewModel deliveryAddressViewModel = new DeliveryAddressViewModel();

            using (sklepEntities db = new sklepEntities())
            {
                Users addressDB = db.Users.Where(x => x.UserName == login).Single();
                deliveryAddressViewModel.NumOfHouse = addressDB.NumOfHouse;
                deliveryAddressViewModel.PostCode = addressDB.PostCode;
                deliveryAddressViewModel.Street = addressDB.Street;
                deliveryAddressViewModel.City = addressDB.City;
                deliveryAddressViewModel.Name = addressDB.Name;
                deliveryAddressViewModel.Surname = addressDB.Surname;
                return deliveryAddressViewModel;
            }
        }

        public static int GetUserID(string login)    // uzyskanie ID uzytkownika
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Users.Where(x => x.UserName == login).Select(x => x.UserID).Single();
            }
        }
        public static string GetUserLogin(int userID)    // uzyskanie loginu uzytkownika
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Users.Where(x => x.UserID == userID).Select(x => x.UserName).Single();
            }
        }
        public static string GetUserEmail(string login)    // uzyskanie adresu email uzytkownika
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Users.Where(x => x.UserName == login).Select(x => x.Email).Single();
            }
        }

        public static bool ChangePersonalData(string login, string name, string surname, int? phoneNumber)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Users user = db.Users.Where(x => x.UserName == login).Single();
                user.Name = name;
                user.Surname = surname;
                if (phoneNumber != null)
                    user.PhoneNumber = phoneNumber;
                else
                    user.PhoneNumber = null;
                db.SaveChanges();
                return true;
            }
          
        }
        public static bool ChangeUserEmail(string login, string email, string link, string code)
        {
            if (CheckEmailExists(email))
            {
                return false;
            }
            var chEmail = new System.Net.Mail.MailAddress(email);
            if (chEmail.Address != email)
            {
                return false;
            }
            using (sklepEntities db = new sklepEntities())
            {
                Users user = db.Users.Where(x => x.UserName == login).Single();
                user.Email = email;
                user.IsActive = "N";
                user.Link = code;
                EmailService.sendChangeEmail(user.UserName, user.Email, link);
                db.SaveChanges();
                return true;
            }
        }

        public static bool changeUserPassword(string login, string activePassword, string newPassword)
        {
            if (!VerifyPassword(login, activePassword)) // podane aktualne haslo nie zgadza sie
            {
                return false;
            }

            using (sklepEntities db = new sklepEntities())
            {
                Users user = db.Users.Where(x => x.UserName == login).Single();
                string mySalt = BCrypt.Net.BCrypt.GenerateSalt();
                string myHash = BCrypt.Net.BCrypt.HashPassword(newPassword, mySalt);
                user.Password = myHash;
                db.SaveChanges();
                return true;
            }

        }

        public static bool changeDeliveryAddress(string login, DeliveryAddressViewModel deliveryAddressViewModel)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Users user = db.Users.Where(x => x.UserName == login).Single();
                user.NumOfHouse = deliveryAddressViewModel.NumOfHouse;
                user.Street = deliveryAddressViewModel.Street;
                user.PostCode = deliveryAddressViewModel.PostCode;
                user.City = deliveryAddressViewModel.City;
                db.SaveChanges();
                return true;
            }
        }
    }
}