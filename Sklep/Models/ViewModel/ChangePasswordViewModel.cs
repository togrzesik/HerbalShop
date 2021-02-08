using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class ChangePasswordViewModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Aktualne hasło:")]
        public string ActualPassword { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Nowe hasło:")]
        public string NewPassword { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Powtórz hasło:")]
        public string NewPassword1 { get; set; }
    }
}