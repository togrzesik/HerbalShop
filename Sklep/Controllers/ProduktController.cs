using Newtonsoft.Json.Linq;
using PagedList;
using Sklep.Models;
using Sklep.Models.EntityManager;
using Sklep.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;

namespace Sklep.Controllers
{
    public class ProduktController : BaseController
    {
        public ActionResult ProduktyZKategorii(int categoryID, int? page, int? minV, int? maxV, string sortV)
        {
            int min, max;
            if (Request["minPole"] == null && minV == null)  // pierwsze wejscie na strone bez podania parametrow
                min = 0;
            else if (Request["minPole"] == null)            // przechodzenie po kolejnych stronach
                min = minV.Value;
            else                                            // filtrowanie
                {
                    min = Int32.Parse(Request["minPole"]);
                    page = 1;
                }
            if (Request["maxPole"] == null && maxV == null)
                max = 99999;
            else if (Request["maxPole"] == null)
                max = maxV.Value;
            else
                max = Int32.Parse(Request["maxPole"]);

            int sortV1 = Int32.Parse(String.IsNullOrEmpty(sortV) ? "0" : sortV);
            System.Diagnostics.Debug.WriteLine("szuk " + categoryID + " min " + min + " max" + max + " sort" + sortV1);
            int maxToSelect;
            List <ProductCategoryView> listProducts = ProductManager.GetProductsFromCategoryMinMax(categoryID, min, max, sortV1, out maxToSelect);
            if (listProducts == null)
            {
                ViewBag.Ilosc = 0;
                return View(); 
            }
            var list = listProducts;
            var pageNumber = page ?? 1;
            int pageSize = 10;
            var onePageOfItem = list.ToPagedList(pageNumber, pageSize);
            ViewBag.sortV = sortV1;
            ViewBag.min = min;
            ViewBag.max = max;
            if (max == 99999)
            { 
                ViewBag.max = maxToSelect;
            }
            ViewBag.page = 1; 
            ViewBag.categoryID = categoryID;
            List<CategoryView> listCategories = CategoryManager.GetCategoriesNames(categoryID);
            ViewData["categories"] = listCategories;
            return View(onePageOfItem);
        }
     
        [WebMethod]
        public JsonResult SzukajZPodopowiedziami(string productName, int? categoryID)
        {     
            if (string.IsNullOrWhiteSpace(productName))
                return null;
            if (categoryID == null)
                categoryID = -1;
            System.Diagnostics.Debug.WriteLine("szukajka: " + productName + " cat: " + categoryID);
            List<ProductsSearchWithPromptViewModel> listProducts = ProductManager.GetProductFromSearchWithSuggestions(productName, (int)categoryID);
            return Json(listProducts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Szukaj(string productName, int?categoryID, int? page, int? minV, int? maxV, string sortV, string codeProduct)
        {
            if(Request["searchProduct"] != null)
            {
                productName = Request["searchProduct"];
            }
            System.Diagnostics.Debug.WriteLine("co to " + productName);

            if (String.IsNullOrEmpty(Request["kategoria" ]) && categoryID == null)
                categoryID = -1;
            else if (!String.IsNullOrEmpty(Request["kategoria"]))
            {
                categoryID = Int32.Parse(Request["kategoria"]);
            }
            
            int min, max;
            if (Request["minPole"] == null && minV == null)
                min = 0;
            else if (Request["minPole"] == null)
                min = minV.Value;
            else
            {
                min = Int32.Parse(Request["minPole"]) * 10;
                page = 1;
            }
            if (Request["maxPole"] == null && maxV == null)
                max = 99999;
            else if (Request["maxPole"] == null)
                max = maxV.Value;
            else
                max = Int32.Parse(Request["maxPole"])* 10;

            int sortV1 = Int32.Parse(String.IsNullOrEmpty(sortV) ? "0" : sortV);
            ViewBag.sortV = sortV1;
            ViewBag.min = min;
            ViewBag.max = max;
            int maxToSelect;
            System.Diagnostics.Debug.WriteLine("wart sortV1" + sortV1 + " min " + min + " max " + max + " cat " + categoryID.Value + " name " + productName);
            List<ProductCategoryView> listProducts = ProductManager.GetProductFromSearch(productName, categoryID.Value, min, max, sortV1, out maxToSelect);
            if (listProducts == null)
            {
                ViewBag.Ilosc = 0;
                return View();
            }

            if (max == 99999)
            {
                ViewBag.max = maxToSelect;
            }
            var list = listProducts;
            var pageNumber = page ?? 1;
            int pageSize = 10;
            var onePageOfItem = list.ToPagedList(pageNumber, pageSize);
            ViewBag.productName = productName;
            ViewBag.page = 1;
            ViewBag.categoryID = categoryID.Value;
            List<CategoryView> listCategories = CategoryManager.GetCategoriesNamesForSearchName(categoryID.Value, productName);
            foreach (CategoryView item in listCategories)
                System.Diagnostics.Debug.WriteLine(" " + item.Name);
            ViewData["categories"] = listCategories;
            return View(onePageOfItem);
        }

        public ActionResult SzczegolyProduktu(int productID)
        {
            ProductView product = ProductManager.GetDetailsProduct(productID);
            ViewBag.Quantity = product.Quantity;
            if (TempData["Message"] != null)
            {
                ModelState.AddModelError("", TempData["Message"].ToString());
                return View(product);
            }
            return View(product);
        }
    }
}