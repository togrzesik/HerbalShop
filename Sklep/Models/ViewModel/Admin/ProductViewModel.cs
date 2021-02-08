using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sklep.Models.ViewModel.Admin
{
    public class ProductViewModel
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string CodeProduct { get; set; }
        public string CategoryName { get; set; }
        public int CategoryID { get; set; }
        public decimal PriceN { get; set; }
        public decimal OldPriceN { get; set; }
        public decimal PriceB { get; set; }
        public decimal VAT { get; set; }
        public string Description { get; set; }
        public string Specification { get; set; }
        public int Quantity { get; set; }
        public bool Recommended { get; set; }
        public bool IsShowed { get; set; }

    }
}