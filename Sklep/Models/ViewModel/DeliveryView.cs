using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class DeliveryView
    {
        public string DeliveryName {get;set;}
        public string DeliveryPriceN { get; set; }
        public decimal DeliveryVAT { get; set; }
        public string DeliveryPriceB { get; set; }
    }
}