using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel.Admin
{
    public class ScraperDetailsViewModel
    {
        public int ProductID { get; set; }
        public string XKomPrice { get; set; }
        public string [] XKomLinks { get; set; }
        public string XKomLinkChecked { get; set; }
        public string VobisPrice { get; set; }
        public string [] VobisLinks { get; set; }
        public string VobisLinkChecked { get; set; }
        public string AlsenPrice { get; set; }
        public string [] AlsenLinks { get; set; }
        public string AlsenLinkChecked { get; set; }
        public string CeneoPrice { get; set; }
        public string [] CeneoLinks { get; set; }
        public string CeneoLinkChecked { get; set; }
        public DateTime LastUpdate { get; set; }
        public string DifferencePrice { get; set; }
    }
}