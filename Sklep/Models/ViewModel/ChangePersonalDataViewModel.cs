using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class ChangePersonalDataViewModel
    {
        [Required(ErrorMessage = "Podaj Imię")]
        [Display(Name = "Imię:")]
        public string Name { get; set; }

       // [Required(ErrorMessage = "Podaj Nazwisko")]
        [Display(Name = "Nazwisko:")]
        public string Surname { get; set; }

        
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Numer telefonu (opcjonalne):")]
        public int? PhoneNumber { get; set; }
    }
}