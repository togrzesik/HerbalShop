using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class CategoryView
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public int ParentCategoryID { get; set; }
    }
}