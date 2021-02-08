using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.ViewModel
{
    public class ProductWithCategoryView
    {
        public List<CategoryView> listCategories { get; set; }
        public List<ProductCategoryView> listProducts { get; set; }

        public ProductWithCategoryView()
        {
            this.listCategories = new List<CategoryView>();
            this.listProducts = new List<ProductCategoryView>();
        }
    }
}