using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class OrderProductView
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductPriceN { get; set; }
        public string ProductPriceB { get; set; }
        public string TotalProductPriceB { get; set; }
        public int QuantityChoose { get; set; }
        public string ImagePath { get; set; }
        public OrderProductView() { }

        public OrderProductView(int ProductID, string ProductName, string ProductPriceN, string ProductPriceB, string TotalProductPriceB, int QuantityChoose, 
            string ImagePath)
        {
            this.ProductID = ProductID;
            this.ProductName = ProductName;
            this.ProductPriceN = ProductPriceN;
            this.ProductPriceB = ProductPriceB;
            this.TotalProductPriceB = TotalProductPriceB;
            this.QuantityChoose = QuantityChoose;
            this.ImagePath = ImagePath;
        }
    }
}