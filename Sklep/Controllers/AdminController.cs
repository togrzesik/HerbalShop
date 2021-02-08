using Sklep.Models.db;
using Sklep.Models.EntityManager;
using Sklep.Models.ViewModel;
using Sklep.Models.ViewModel.Admin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Sklep.Controllers
{
    [Authorize]
    public class AdminController : BaseAdminController
    {        
        // GET: Admin
        public ActionResult Index()
        {
            
            List<TheNewestOrdersView> orderList = AdminManager.GetTheNewestOrders();
            StaticsView statics = new StaticsView();
            statics.NumberOfAllOrders = AdminManager.GetNumberOfAllOrders();
            statics.NumberOfClients = AdminManager.GetNumberOfClients();
            statics.NumberOfOrdersToRealize = AdminManager.GetNumberOfOrdersToRealize();
            statics.NumberOfProducts = AdminManager.GetNumberOfProducts();
            StaticsWithTheNewestOrdersView staticWithOrders = new StaticsWithTheNewestOrdersView();
            staticWithOrders.statics = statics;
            staticWithOrders.listOrders = orderList;
            return View(staticWithOrders);
        }
        
        public ActionResult ZamowieniaDoRealizacji()
        {
            return View(AdminManager.GetOrdersToRealize());
        }
        
        public ActionResult ZamowieniaWszystkie()
        {
            return View(AdminManager.GetAllOrders());
        }
        
        public ActionResult ZamowieniaSzczegoly(int orderID)
        {
            AdminDetailsOrder aas = AdminManager.GetDetailsOrder(orderID);
            ViewBag.statusList = aas.statusList;
            return View(AdminManager.GetDetailsOrder(orderID));
        }
        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EdytujZamowienie(AdminDetailsOrder order)
        {
            int statusSelected = int.Parse(Request["statusList"]);
            AdminManager.EditStatusOrder(order, statusSelected, User.Identity.Name);
            TempData["Message"] = "Status zamówienia został zmieniony";
            return RedirectToAction("Index");
        }


        
        
        public ActionResult Klienci()
        {
            if (TempData["Message"] != null)
            {
                ModelState.AddModelError("", TempData["Message"].ToString());

            }
            using (sklepEntities db = new sklepEntities())
            {
                var users = db.Users;
                return View(users.ToList());
            }
        }
        
        public ActionResult KlienciSzczegoly(int userID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Users users = db.Users.Where(x => x.UserID == userID).Single();
                return View(users);
            }
        }
       
        public ActionResult KlienciUsun(int userID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Users user = db.Users.Where(x => x.UserID == userID).Single();
                user.IsActive = "U";
                db.SaveChanges();
                TempData["Message"] = "Konto zostało usunięte";
                return RedirectToAction("Klienci");
            }
        }
        
        public ActionResult Komentarze()
        {
            if (TempData["Message"] != null)
            {
                ModelState.AddModelError("", TempData["Message"].ToString());

            }
            using (sklepEntities db = new sklepEntities())
            {
                var comments = db.Comments;
                List<CommentViewModel> listComments = new List<CommentViewModel>();
                foreach(Comments item in comments)
                {
                    CommentViewModel comment = new CommentViewModel();
                    comment.CommentID = item.CommentID;
                    comment.ProductID = item.ProductID;
                    comment.Date = item.Date;
                    comment.Description = item.Description;
                    string userLogin = db.Users.Where(x => x.UserID == item.UserID).Select(x => x.UserName).Single();
                    comment.User = userLogin;
                    listComments.Add(comment);
                }
                return View(listComments);
            }
        }

        public ActionResult KomentarzeSzczegoly(int commentID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Comments commentDB = db.Comments.Where(x => x.CommentID == commentID).Single();
                CommentViewModel comment = new CommentViewModel();
                comment.CommentID = commentDB.CommentID;
                comment.ProductID = commentDB.ProductID;
                comment.Date = commentDB.Date;
                comment.Description = commentDB.Description;
                string userLogin = db.Users.Where(x => x.UserID == commentDB.UserID).Select(x => x.UserName).Single();
                comment.User = userLogin;
                return View(comment);
            }
        }

        public ActionResult KomentarzeUsun(int commentID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Comments comment = db.Comments.Where(x => x.CommentID == commentID).Single();
                db.Comments.Remove(comment);
                db.SaveChanges();
                TempData["Message"] = "Komentarz został usunięty";
                return RedirectToAction("Komentarze");
            }
        }

        public ActionResult Produkty()                 // wyswietl wszystkie produkty
        {
            if (TempData["Message"] != null)
            {
                ModelState.AddModelError("", TempData["Message"].ToString());

            }
            return View(ProductManager.GetProducts());
        }

       
        public ActionResult ProduktySzczegoly(int productID)
        {      
            if (TempData["Message"] != null)
            {
                ModelState.AddModelError("", TempData["Message"].ToString());

            }
            ProductViewModel productDetails = ProductManager.GetDetailsProductAdmin(productID);
            return View(productDetails);
        }
        [HttpPost]
        public string ProduktyEdycja(int ProductId, string ProductName, string Codeproduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {
            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, Codeproduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
             {
                 return answer;
             }
            Debug.WriteLine("id " + Codeproduct);
            ProductViewModel product = new ProductViewModel
            {
                ProductID = ProductId,
                Name = ProductName,
                CodeProduct = Codeproduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (string.IsNullOrEmpty(product.Name))
            {
                TempData["Message"] = "Nazwa produktu jest za krótka";
                return "-1";
            }
            if (product.PriceN < 0)
            {
                TempData["Message"] = "Podano złą cenę produktu";
                return "-2";
            }

            if (product.VAT < 0 || product.VAT > 1)
            {
                TempData["Message"] = "Podano złą stawkę VAT";
                return "-2";

            }
            if (product.Quantity < 0)
            {
                TempData["Message"] = "Podano złą liczbę produktu";
                return "-2";
            }
            ProductManager.EditProduct(product);
            TempData["Message"] = "Zmiany zostały zapisane";
            return "";

        }

        public ActionResult ProduktyDodaj()
        {
            if (TempData["Message"] != null)
            {
                ModelState.AddModelError("", TempData["Message"].ToString());

            }
            List<SelectListItem> categoryList = new List<SelectListItem>();
            using (sklepEntities db = new sklepEntities())
            {
                var categories = db.Categories.Where(x => x.CategoryID != x.ParentCategoryID);
                categoryList.Add(new SelectListItem { Text = "Wybierz kategorie", Value = "0", Selected = true });
                foreach (Categories cate in categories)
                {
                    categoryList.Add(new SelectListItem { Text = cate.Name, Value = cate.CategoryID.ToString() });
                }

            }
            ViewBag.categoryList = categoryList;
            return View();
        }


        [HttpPost]
        public ActionResult UploadImages()
        {

            string nameCategoryAndProductID = Request.Params["productID"];
            string path = Server.MapPath("~/Images/" + nameCategoryAndProductID);
            string[] splitName = nameCategoryAndProductID.Split('/');
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (Request.Files.Count > 0)
            {
                try
                {
                    HttpFileCollectionBase files = Request.Files;

                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFileBase file = files[i];
                        string fname;

                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = "0" + i + "_" + file.FileName;
                        }

                        fname = Path.Combine(path, fname);
                        ProductManager.AddImage(int.Parse(splitName[1]), "~/Images/" + nameCategoryAndProductID + "/" + "0" + i + "_" + file.FileName);
                        file.SaveAs(fname);
                    }
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                ProductManager.AddImage(int.Parse(splitName[1]), "~/Images/Laptopy/134/00_brakZdjecia.jpg");
                return Json("No files selected.");
            }
        }


        public ActionResult ProcessorDetails()
        {
            return PartialView();
        }

        public ActionResult LaptopDetails()
        {
            return PartialView();
        }
        public ActionResult GraphicsCardDetails()
        {
            return PartialView();
        }

        public ActionResult MonitorDetails()
        {
            return PartialView();
        }
        public ActionResult RAMMemoryDetails()
        {
            return PartialView();
        }
        public ActionResult ComputerPCDetails()
        {
            return PartialView();
        }
        public ActionResult OperatingSystemDetails()
        {
            return PartialView();
        }
        public ActionResult AntivirusDetails()
        {
            return PartialView();
        }
        public ActionResult OfficeProgramDetails()
        {
            return PartialView();
        }
        public ActionResult MouseDetails()
        {
            return PartialView();
        }
        public ActionResult KeyboardsDetails()
        {
            return PartialView();
        }
        public ActionResult HeadphonesDetails()
        {
            return PartialView();
        }
        public ActionResult SpeakersDetails()
        {
            return PartialView();
        }
        public ActionResult ExternalHardDriveDetails()
        {
            return PartialView();
        }
        public ActionResult MotherboardsDetails()
        {
            return PartialView();
        }
        
        [HttpPost]
        public string AddLaptop(LaptopDetailsViewModel Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {

            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if(!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddLaptop(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }
        [HttpPost]
        public string AddProcessor(ProcessorDetailsViewModel Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {

            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddProcessor(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }
        [HttpPost]
        public string AddGraphicsCard(GraphicsCardViewModel Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {
            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddGraphicsCard(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }
        [HttpPost]
        public string AddMonitor(MonitorDetailsViewModel Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {
            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddMonitor(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }

        [HttpPost]
        public string AddMemoryRAM(MemoryRAMDetails Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {
            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddMemoryRAM(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }

        [HttpPost]
        public string AddOperatingSystem(OperatingSystemDetails Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {
            Debug.WriteLine("system");
            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddOperatingSystem(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }

        [HttpPost]
        public string AddAntivirus(AntivirusDetails Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {
            string answer = "";
            decimal parseOldPriceN; 
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddAntivirus(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }

        [HttpPost]
        public string AddMouse(MouseDetails Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {
            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddMouse(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }


        [HttpPost]
        public string AddKeyboard(KeyboardDetailsViewModel Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {
            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddKeyboard(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }

        [HttpPost]
        public string AddHeadphones(HeadphonesDetailsViewModel Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {
            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddHeadphones(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }

        [HttpPost]
        public string AddSpeakers(SpeakersDetailsViewModel Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {
            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddSpeakers(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }

        [HttpPost]
        public string AddExternalHardDrive(ExternalHardDriveDetailsViewmodel Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {
            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddExternalHardDrive(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }

        [HttpPost]
        public string AddMotherboards(MotherboardsDetailsViewModel Specyfication, string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed)
        {
            string answer = "";
            decimal parseOldPriceN;
            decimal parsePriceN;
            decimal parseVAT;
            answer = ProductManager.CheckDetailsProduct(ProductName, CodeProduct, CategoryID, OldPriceN, PriceN, VAT, Description, Quantity, Recommended, IsShowed, out parseOldPriceN, out parsePriceN, out parseVAT);
            if (!string.IsNullOrWhiteSpace(answer))
            {
                return answer;
            }

            ProductViewModel product = new ProductViewModel
            {
                Name = ProductName,
                CodeProduct = CodeProduct,
                CategoryID = CategoryID,
                OldPriceN = parseOldPriceN,
                PriceN = parsePriceN,
                VAT = parseVAT,
                Description = Description,
                Quantity = Quantity,
                Recommended = Recommended,
                IsShowed = IsShowed
            };
            if (ProductManager.CheckProductExistsName(ProductName))
                return "-1";
            ProductManager.AddMotherboards(Specyfication, product);
            TempData["Message"] = "Dodano produkt";
            return ProductManager.GetNameCategoryAndProductID(product.Name);
        }
        public ActionResult Statusy()
        {
            if (TempData["Message"] != null)
            {
                ModelState.AddModelError("", TempData["Message"].ToString());

            }
            using (sklepEntities db = new sklepEntities())
            {
                return View(db.Statuses.ToList());
            }
        }

        public ActionResult StatusySzczegoly(int statusID)
        {
            if (TempData["Message"] != null)
            {
                ModelState.AddModelError("", TempData["Message"].ToString());

            }
            using (sklepEntities db = new sklepEntities())
            {
                Statuses status = db.Statuses.Where(x => x.StatusID == statusID).Single();
                return View(status);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StatusyEdycja(Statuses status)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Statuses stat = db.Statuses.Where(x => x.StatusID == status.StatusID).Single();
                if (string.IsNullOrEmpty(status.Name))
                {
                    TempData["Message"] = "Podana nazwa jest za krótka";
                    return RedirectToAction("StatusySzczegoly", new { statusID = status.StatusID });
                }
                stat.Name = status.Name;
                db.SaveChanges();
                TempData["Message"] = "Zmiany zostały zapisane";
                return RedirectToAction("StatusySzczegoly", new { statusID = status.StatusID });
            }
        }
        
        public ActionResult StatusyDodaj()
        {
            if (TempData["Message"] != null)
            {
                ModelState.AddModelError("", TempData["Message"].ToString());

            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StatusyDodaj(Statuses status)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Statuses stat = new Statuses();
                if (string.IsNullOrEmpty(status.Name))
                {
                    TempData["Message"] = "Podana nazwa jest za krótka";
                    return RedirectToAction("StatusyDodaj");
                }
                if (db.Statuses.Where(x => x.Name == status.Name).Any())
                {
                    TempData["Message"] = "Status o podanej nazwie już istnieje";
                    return RedirectToAction("StatusyDodaj");
                }
                stat.Name = status.Name;
                db.Statuses.Add(stat);
                db.SaveChanges();
                TempData["Message"] = "Status został dodany";
                return RedirectToAction("Statusy");
            }
        }

        public ActionResult StatusyUsun(int statusID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Statuses status = db.Statuses.Where(x => x.StatusID == statusID).Single();
                db.Statuses.Remove(status);
                db.SaveChanges();
                TempData["Message"] = "Status został usunięty";
                return RedirectToAction("Statusy");
            }
        }

        public ActionResult Dostawy()
        {
            if (TempData["Message"] != null)
            {
                ModelState.AddModelError("", TempData["Message"].ToString());

            }
            return View(AdminManager.GetDeliveries());
        }

        public ActionResult DostawySzczegoly(int deliveryID)
        {
            if (TempData["Message"] != null)
            {
                ModelState.AddModelError("", TempData["Message"].ToString());

            }
            return View(AdminManager.GetDetailsDelivery(deliveryID));

        }

        public ActionResult DostawyEdycja(DeliveriesViewModel delivery)
        { 
            if (string.IsNullOrEmpty(delivery.Name))
            {
                TempData["Message"] = "Nie podano nazwy dostawy";
                return RedirectToAction("DostawyDodaj");
            }
            if (AdminManager.IsExistsDelivery(delivery.DeliveryID, delivery.Name))
            {
                TempData["Message"] = "Dostawa o podanej nazwie już istnieje";
                return RedirectToAction("DostawySzczegoly", new { deliveryID = delivery.DeliveryID });
            }
            if (delivery.PriceN < 0)
            {
                TempData["Message"] = "Podana cena jest błędna";
                return RedirectToAction("DostawySzczegoly", new { deliveryID = delivery.DeliveryID });
            }
            if (delivery.VAT < 0 || delivery.VAT > 1)
            {
                TempData["Message"] = "Podana stawka VAT jest błędna";
                return RedirectToAction("DostawySzczegoly", new { deliveryID = delivery.DeliveryID });
            }
            AdminManager.EditDelivery(delivery);
            TempData["Message"] = "Zmiany zostały zapisane";
            return RedirectToAction("DostawySzczegoly", new { deliveryID = delivery.DeliveryID });

        }
        public ActionResult DostawyDodaj()
        {
            if (TempData["Message"] != null)
            {
                ModelState.AddModelError("", TempData["Message"].ToString());
            }
            return View();
        }

        [HttpPost]
        public ActionResult DostawyDodaj(DeliveriesViewModel delivery)
        {
            if (string.IsNullOrEmpty(delivery.Name))
            {
                TempData["Message"] = "Nie podano nazwy dostawy";
                return RedirectToAction("DostawyDodaj");
            }
            if (AdminManager.IsExistsDelivery(delivery.Name))
            {
                TempData["Message"] = "Dostawa o podanej nazwie już istnieje";
                return RedirectToAction("DostawyDodaj");
            }
            if (delivery.PriceN < 0)
            {
                TempData["Message"] = "Podana cena jest błędna";
                return RedirectToAction("DostawyDodaj");
            }
            if (delivery.VAT < 0 || delivery.VAT > 1)
            {
                TempData["Message"] = "Podana stawka VAT jest błędna";
                return RedirectToAction("DostawyDodaj");
            }
            AdminManager.AddDelivery(delivery);
            TempData["Message"] = "Dostawa została dodana";
            return RedirectToAction("Dostawy");

        }

        public ActionResult DostawyUsun(int deliveryID)
        {
            AdminManager.RemoveDelivery(deliveryID);
            TempData["Message"] = "Dostawa została usunięta";
            return RedirectToAction("Dostawy");
        }   

        public static string GetPriceFromScraper(string pricesString, string checkedLink)
        {
            string price;
            if (string.IsNullOrWhiteSpace(pricesString))
                return "";
            string[] split = new string[] { "$#$#" };
            string[] splitPrices = pricesString.Split(split, StringSplitOptions.None);

            if (splitPrices.Length > 2)
            {
                price = splitPrices[0] + "???";
            }
            else
            {
                if (string.IsNullOrEmpty(checkedLink))
                    price = splitPrices[0] + "???";
                else
                    price = splitPrices[0];
            }
                
            return price;
        }
        public ActionResult Scraper()
        {
            List<ScraperViewModel> listShopScrap = new List<ScraperViewModel>();
            using (sklepEntities db = new sklepEntities())
            {
                
                var shopsScrap = db.Scrapers;
                foreach(Scrapers item in shopsScrap)
                {
                    ScraperViewModel shpScrap = new ScraperViewModel();
                    shpScrap.ProductID = item.ProductID;
                    shpScrap.XKomPrice = GetPriceFromScraper(item.XKomPrice, item.XKomLinkChecked);
                    shpScrap.AlsenPrice = GetPriceFromScraper(item.AlsenPrice, item.AlsentLinkChecked);
                    shpScrap.VobisPrice = GetPriceFromScraper(item.VobisPrice, item.VobistLinkChecked);
                    shpScrap.CeneoPrice = GetPriceFromScraper(item.CeneoPrice, item.CeneoLinkChecked);
                    shpScrap.LastUpdate = (DateTime)item.DateLastUpdate;
                    if(item.DifferencePrice != null)
                        shpScrap.DifferencePrice = ((decimal)item.DifferencePrice).ToString("C2");
                    listShopScrap.Add(shpScrap);

                }
            }
            return View(listShopScrap);
        }
        public async Task<ActionResult> WlaczScraper()
        {
            Thread a = new Thread(() => { ScraperManager.scrap(); });
            a.Start();
            a.Join();
            System.Diagnostics.Debug.WriteLine("po join ");
            return RedirectToAction("Index");

        }
        public static string[] GetLinks(string linksString)
        {

            if (string.IsNullOrWhiteSpace(linksString))
                return null;
            string[] split = new string[] { "$#$#" };
            string[] splitLinks = linksString.Split(split, StringSplitOptions.None);
            splitLinks = splitLinks.Take(splitLinks.Count()-1).ToArray();
            
            return splitLinks;
        }
        public static string ReplacePriceScraper(string price)
        {
            if (string.IsNullOrWhiteSpace(price))
                return "";
            return price.Replace("$#$#", "; ");
        }
        public ActionResult ScraperSzczegoly(int productID)
        {
            ScraperDetailsViewModel scraperDetails = new ScraperDetailsViewModel();
            using (sklepEntities db = new sklepEntities())
            {
                Scrapers scraper = db.Scrapers.Where(x => x.ProductID == productID).FirstOrDefault();
                if(scraper.DifferencePrice != null)
                    scraperDetails.DifferencePrice = ((decimal)scraper.DifferencePrice).ToString("C2");
                scraperDetails.LastUpdate = (DateTime)scraper.DateLastUpdate;
                scraperDetails.XKomPrice = ReplacePriceScraper(scraper.XKomPrice);
                scraperDetails.XKomLinks = GetLinks(scraper.XKomLink);
                scraperDetails.XKomLinkChecked = scraper.XKomLinkChecked;
                scraperDetails.VobisPrice = ReplacePriceScraper(scraper.VobisPrice);
                scraperDetails.VobisLinks= GetLinks(scraper.VobistLink);
                scraperDetails.VobisLinkChecked = scraper.VobistLinkChecked;
                scraperDetails.AlsenPrice = ReplacePriceScraper(scraper.AlsenPrice);
                scraperDetails.AlsenLinks = GetLinks(scraper.AlsentLink);
                scraperDetails.AlsenLinkChecked = scraper.AlsentLinkChecked;
                scraperDetails.CeneoPrice = ReplacePriceScraper(scraper.CeneoPrice);
                scraperDetails.CeneoLinks = GetLinks(scraper.CeneoLink);
                scraperDetails.CeneoLinkChecked = scraper.CeneoLinkChecked;
                scraperDetails.ProductID = productID;
            }
                return View(scraperDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ScraperEdycja(ScraperDetailsViewModel scraper)
        {
            System.Diagnostics.Debug.WriteLine("scrap " + scraper.ProductID + " " + scraper.XKomLinks + " \n" + scraper.XKomLinkChecked +" d");
            using (sklepEntities db = new sklepEntities())
            {
                Scrapers scraperDB = db.Scrapers.Where(x => x.ProductID == scraper.ProductID).First();
                scraperDB.XKomLinkChecked = scraper.XKomLinkChecked;
                scraperDB.VobistLinkChecked = scraper.VobisLinkChecked;
                scraperDB.AlsentLinkChecked = scraper.AlsenLinkChecked;
                scraperDB.CeneoLinkChecked = scraper.CeneoLinkChecked;
                db.SaveChanges();
            }
             return RedirectToAction("ScraperSzczegoly", new { productID= scraper.ProductID});
        }
    }
}