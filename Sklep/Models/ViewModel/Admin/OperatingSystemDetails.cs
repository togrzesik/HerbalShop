using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel.Admin
{
    public class OperatingSystemDetails
    {
        public string Architecture { get; set; }
        public string License { get; set; }
        public string NumOfUser { get; set; }
        public string NumOfComputer { get; set; }
        public string Language { get; set; }
    }
}