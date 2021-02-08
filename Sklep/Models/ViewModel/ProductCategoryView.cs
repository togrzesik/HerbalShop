using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class ProductCategoryView
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string PriceN { get; set; }
        public string OldPriceB { get; set; }
        public string CodeProduct { get; set; }
        public string PriceB { get; set; }
        public string ImagePath { get; set; }
        public string Specification { get; set; }
        public JObject specJ { get; set; }
    }
}