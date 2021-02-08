using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel.Admin
{
    public class DeliveriesViewModel
    {
        public int DeliveryID { get; set; }
        public string Name { get; set; }
        public decimal PriceN { get; set; }
        public decimal VAT { get; set; }
        public decimal PriceB { get; set; }
        public DeliveriesViewModel()
        {

        }
        public DeliveriesViewModel(int DeliveryID, string Name, decimal PriceN, decimal VAT, decimal PriceB)
        {
            this.DeliveryID = DeliveryID;
            this.Name = Name;
            this.PriceN = PriceN;
            this.VAT = VAT;
            this.PriceB = PriceB;
        }
    }
}