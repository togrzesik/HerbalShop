using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class UserLoginViewModel
    {
        
        [Display(Name = "Login:")]
        public string Login { get; set; }

       
        [Display(Name = "Hasło:")]
        public string Password { get; set; }
    }
}