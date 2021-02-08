using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class PriceWithLinkScraper
    {
        public string Price { get; set; }
        public string Link { get; set; }

        public PriceWithLinkScraper()
        {
            Price = "";
            Link = "";
        }
    }
}