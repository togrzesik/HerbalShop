using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class SummaryOrderViewModel
    {
        public List<OrderProductView> orderProductList { get; set; }
        public string TotalOrderPriceN { get; set; }
        public string TotalOrderPriceB { get; set; }
        public string NameDelivery { get; set; }
        public string DeliveryPriceB { get; set; }
        public string lastTimeModified { get; set; }
        public string Status { get; set; }
        public string InvoicePath { get; set; }
        public string OrderPath { get; set; }
        public int OrderID { get; set; }
        public string Street { get; set; }
        public string NumOfHouse { get; set; }
        public string PostCode { get; set; }
        public string City { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; } 
    }
}