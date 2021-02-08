using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class OrderActualView
    {
        public int OrderID { get; set; }
        public string OrderShortDescription { get; set; }
        public string PriceB { get; set; }
        public string Status { get; set; }
        public OrderActualView()
        {

        }

        public OrderActualView(int OrderID, string OrderShortDescription, string PriceB, string Status)
        {
            this.OrderID = OrderID;
            this.OrderShortDescription = OrderShortDescription;
            this.PriceB = PriceB;
            this.Status = Status;
        }
    }
}