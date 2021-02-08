using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class ContactView
    {
        [Required(ErrorMessage = "Podaj adres e-mail")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Twój adres e-mail:")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O co chesz zapytać?")]
        [Display(Name = "Treść wiadomości:")]
        public string Content { get; set; }

    }
}