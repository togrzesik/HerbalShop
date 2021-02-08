using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class ProductRecommededView
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string OldPriceB { get; set; }
        public string PriceB { get; set; }
        
        public string ImagePath { get; set; }
    }
}