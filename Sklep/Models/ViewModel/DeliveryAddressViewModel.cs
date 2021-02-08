using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class DeliveryAddressViewModel
    {
        [Required(ErrorMessage = "Podaj ulicę do wysyłki")]
        [Display(Name = "Ulica:")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Podaj numer domu/lokalu")]
        [Display(Name = "Numer domu/lokalu:")]
        public string NumOfHouse { get; set; }

        [Required(ErrorMessage = "Podaj kod pocztowy")]
        [Display(Name = "Kod pocztowy:")]
        public string PostCode { get; set; }

        [Required(ErrorMessage = "Podaj miejscowość")]
        [Display(Name = "Miejscowość:")]
        public string City { get; set; }

        [Display(Name = "Imię:")]
        public string Name { get; set; }
     
        [Display(Name = "Nazwisko:")]
        public string Surname { get; set; }
        public string Delivery;
    }
}