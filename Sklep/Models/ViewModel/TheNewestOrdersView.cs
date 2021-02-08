using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class TheNewestOrdersView
    {
        public int OrderID { get; set; }
        public string UserName { get; set; }
        public string PriceB { get; set; }
        public string status { get; set; }
        public DateTime DateOrder { get; set; }
    }
}