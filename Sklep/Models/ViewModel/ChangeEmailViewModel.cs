using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class ChangeEmailViewModel
    {
        [Required(ErrorMessage = "Podaj e-mail")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "E-mail:")]
        public string Email { get; set; }
    }
}