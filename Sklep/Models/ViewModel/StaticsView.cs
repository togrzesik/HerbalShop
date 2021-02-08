using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class StaticsView
    {

        public int NumberOfClients { get; set; }
        public int NumberOfAllOrders { get; set; }
        public int NumberOfOrdersToRealize { get; set; }
        public int NumberOfProducts { get; set; }
    }
}