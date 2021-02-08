using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class PayUForm
    {
        public Dictionary<string, string> PayUConfiguration { get; set; }
        public List<OrderProductView> listProduct { get; set; }
    }
}