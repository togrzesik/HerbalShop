using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class UserRegisterViewModel
    {

        [Display(Name = "Login:")]
        public string Login { get; set; }

        [Display(Name = "Hasło:")]
        public string Password { get; set; }

        [Display(Name = "Powtórz hasło:")]
        public string PasswordRepeat { get; set; }

        [Display(Name = "E-mail:")]
        public string Email { get; set; }
        
        public bool Statute { get; set; }
    }
}