using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class StaticsWithTheNewestOrdersView
    {
        public List<TheNewestOrdersView> listOrders = new List<TheNewestOrdersView>();
        public StaticsView statics = new StaticsView();
    }
}