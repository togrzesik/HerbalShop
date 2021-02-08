using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class ProductView
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string PriceN { get; set; }
        public string OldPriceB { get; set; }
        public string PriceB { get; set; }
        public List<string> ImagesPath { get; set; }
        public string Specification { get; set; }
        public string Description { get; set; }
        public string CodeProduct { get; set; }
        public int CategoryID { get; set; }
        public int Quantity { get; set; }
        public int QuantityChoose { get; set; }
        public JObject specJ { get; set; }
    }
}