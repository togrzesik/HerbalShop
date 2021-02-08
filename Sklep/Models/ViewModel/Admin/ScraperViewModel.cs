using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel.Admin
{
    public class ScraperViewModel
    {
        public int ProductID { get; set; }
        public string XKomPrice { get; set; }
        public string VobisPrice { get; set; }
        public string AlsenPrice { get; set; }
        public string CeneoPrice { get; set; }
        public DateTime LastUpdate { get; set; }
        public string DifferencePrice { get; set; }
    }
}