using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class ShoppingCartView
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductPriceN { get; set; }
        public string ProductPriceB { get; set; }
        public string TotalProductPriceB { get; set; }
        public int QuantityChoose { get; set; }
        public int QuantityDB { get; set; }
        public string ImagePath { get; set; }       
    }
}