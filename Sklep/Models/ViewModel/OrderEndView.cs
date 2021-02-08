using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class OrderEndView
    {
        public int OrderID { get; set; }
        public string OrderShortDescription { get; set; }
        public string PriceB { get; set; }
        public DateTime date { get; set; }
        public OrderEndView()
        {

        }

        public OrderEndView(int OrderID, string OrderShortDescription, string PriceB, DateTime date)
        {
            this.OrderID = OrderID;
            this.OrderShortDescription = OrderShortDescription;
            this.PriceB = PriceB;
            this.date = date;
        }
    }
}