using Sklep.Models.db;
using Sklep.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sklep.Models.EntityManager
{
    public class CategoryManager
    {
        public static bool CheckCategoryExists(int categoryID)    // sprawdzenie czy kategoria istnieje w bazie danych
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Categories.Where(u => u.CategoryID.Equals(categoryID)).Any();
            }
        }

        public static List<CategoryView> GetCategoriesNames(int categoryID)
        {
            List<CategoryView> listCategories = new List<CategoryView>();
            if (categoryID ==  -1)
            {
                using (sklepEntities db = new sklepEntities())
                {
                    var categories = db.Categories;
                    foreach (Categories item in categories)
                    {
                        CategoryView tempCategory = new CategoryView();
                        tempCategory.CategoryID = item.CategoryID;
                        tempCategory.Name = item.Name;
                        tempCategory.ParentCategoryID = item.ParentCategoryID;
                        listCategories.Add(tempCategory);
                    }
                    return listCategories;
                }
            }
            else
            {
                using (sklepEntities db = new sklepEntities())
                {
                    Categories category = db.Categories.Where(x => x.CategoryID == categoryID).Single();
                    int parentCategoryID = category.ParentCategoryID;

                    var categories = db.Categories.Where(x => x.ParentCategoryID == parentCategoryID && x.CategoryID != parentCategoryID);
                    foreach (Categories item in categories)
                    {
                        CategoryView tempCategory = new CategoryView();
                        tempCategory.CategoryID = item.CategoryID;
                        tempCategory.Name = item.Name;
                        tempCategory.ParentCategoryID = item.ParentCategoryID;
                        listCategories.Add(tempCategory);
                    }
                }

                return listCategories;
            }
        }


        public static List<CategoryView> GetCategoriesNamesForSearchName(int categoryID, string productName)
        {
            List<CategoryView> listCategories = new List<CategoryView>();
            if (categoryID == -1)
            {
                using (sklepEntities db = new sklepEntities())
                {
                    var tempCategories = from pr in db.Products
                                  where pr.Name.Contains(productName)
                                  join cat in db.Categories
                                  on pr.CategoryID equals cat.CategoryID
                                  select new { cat.CategoryID, cat.Name, cat.ParentCategoryID };
                    var categories = tempCategories.GroupBy(p => p.CategoryID).Select(s => s.FirstOrDefault());

                    foreach (var item in categories)
                    {
                        CategoryView tempCategory = new CategoryView();
                        tempCategory.CategoryID = item.CategoryID;
                        tempCategory.Name = item.Name;
                        tempCategory.ParentCategoryID = item.ParentCategoryID;
                        listCategories.Add(tempCategory);
                    }
                    return listCategories;
                }
            }
            else
            {
                using (sklepEntities db = new sklepEntities())
                {
                    Categories category = db.Categories.Where(x => x.CategoryID == categoryID).Single();
                    int parentCategoryID = category.ParentCategoryID;
                    var categories = db.Categories.Where(x => x.ParentCategoryID == parentCategoryID && x.CategoryID != parentCategoryID);

                    var tempCategories = from pr in db.Products
                                         where pr.Name.Contains(productName)
                                         join cat in categories
                                         on pr.CategoryID equals cat.CategoryID
                                         select new { cat.CategoryID, cat.Name, cat.ParentCategoryID };
                    var tempCategories1 = tempCategories.GroupBy(p => p.CategoryID).Select(s => s.FirstOrDefault());

                    foreach (var item in tempCategories1)
                    {
                        CategoryView tempCategory = new CategoryView();
                        tempCategory.CategoryID = item.CategoryID;
                        tempCategory.Name = item.Name;
                        tempCategory.ParentCategoryID = item.ParentCategoryID;
                        listCategories.Add(tempCategory);
                    }
                }

                return listCategories;
            }
        }

        public static bool isRootCategory(int categoryID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Categories.Where(x => x.CategoryID == categoryID && x.ParentCategoryID == categoryID).Any();
            }
        }

        public static List<Categories> GetCategoriesToLayout()
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Categories.ToList();
            }  
        }
    }
}