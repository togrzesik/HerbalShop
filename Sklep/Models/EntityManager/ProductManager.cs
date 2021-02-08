using Newtonsoft.Json.Linq;
using Sklep.Models.db;
using Sklep.Models.ViewModel;
using Sklep.Models.ViewModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sklep.Models.EntityManager
{
    public class ProductManager
    {
        public static bool CheckProductExistsName(string name)
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Products.Where(x => x.Name == name).Any();
            }
        }
        /* public static bool CheckProductExistsCodeProduct(string codeProduct)
         {
             using (sklepEntities db = new sklepEntities())
             {
                 var ds = db.Products.Where(x => x.CodeProduct == codeProduct);
                 foreach (var item in ds)
                     System.Diagnostics.Debug.WriteLine("szuk " + item.Name + " n " + codeProduct + " s" + item.CodeProduct);
                 return db.Products.Where(x => x.CodeProduct == codeProduct).Any();
             }
         } */
        public static bool CheckProductExistsID(int productID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Products.Where(x => x.ProductID == productID).Any();
            }
        }
        public static List<ProductViewModel> GetProducts()
        {
            using (sklepEntities db = new sklepEntities())
            {
                List<ProductViewModel> productList = new List<ProductViewModel>();
                var products = db.Products;
                foreach (Products item in products)
                {
                    string categoryName = db.Categories.Where(x => x.CategoryID == item.CategoryID).Select(x => x.Name).Single();
                    ProductViewModel prod = new ProductViewModel();
                    prod.ProductID = item.ProductID;
                    prod.Name = item.Name;
                    prod.CategoryName = categoryName;
                    prod.PriceN = item.PriceN;
                    prod.VAT = item.Vat;
                    prod.PriceB = (1 + item.Vat) * item.PriceN;
                    prod.Description = item.Description;
                    prod.Specification = item.Specification;
                    prod.Recommended = (bool)item.Recommended1;
                    prod.Quantity = item.Quantity;
                    productList.Add(prod);
                }
                return productList;
            }
        }
        public static List<ShoppingCartView> GetProductsToShoppingCart(string[] productsID, string[] quantity, ref decimal OrderPriceN, ref decimal OrderPriceB)
        {
            List<ShoppingCartView> productsList = new List<ShoppingCartView>();
            using (sklepEntities db = new sklepEntities())
            {
                for (int i = 0; i < productsID.Length; i++)
                {
                    int prID = Int32.Parse(productsID[i]);

                    Products pr = db.Products.Where(x => x.ProductID == prID).Single();                  // pobranie z bazy danych produktu o danym ID
                    ShoppingCartView shoppingCart = new ShoppingCartView();

                    shoppingCart.ProductID = pr.ProductID;                                               // przepisanie wartosci
                    shoppingCart.ProductName = pr.Name;
                    if (shoppingCart.ProductName.Length > 60)
                    {
                        shoppingCart.ProductName = string.Concat(shoppingCart.ProductName.Take(60)) + "...";
                    }
                    shoppingCart.ProductPriceN = pr.PriceN.ToString("C2");
                    shoppingCart.ProductPriceB = ((1M + pr.Vat) * pr.PriceN).ToString("C2");             // ToString("C2") oznacza - ustawienie zapisania ceny w odpowiedni sposob
                    shoppingCart.QuantityChoose = Int32.Parse(quantity[i]);
                    shoppingCart.TotalProductPriceB = ((1M + pr.Vat) * pr.PriceN * Int32.Parse(quantity[i])).ToString("C2");
                    shoppingCart.QuantityDB = ProductManager.GetQuantityProduct(pr.ProductID);

                    var productWithImage = from pro in db.Products                        // znalezienie zdjec danego produktu w bazie danych
                                           where pro.ProductID == prID
                                           join img in db.Images
                                           on pro.ProductID equals img.ProductID
                                           select new { pro.ProductID, img.Path };

                    var image = productWithImage.GroupBy(p => p.ProductID).Select(s => s.FirstOrDefault()).Single();      // wybranie pierwszego zdjecia produktu 
                    shoppingCart.ImagePath = image.Path;

                    OrderPriceN += (pr.PriceN * shoppingCart.QuantityChoose);                   // calkowita cena zamowienia (netto)
                    OrderPriceB += (1M + pr.Vat) * pr.PriceN * Int32.Parse(quantity[i]);        // calkowita cena zamowienia (brutto)

                    productsList.Add(shoppingCart);
                }
                return productsList;
            }
        }
        public static int GetIDProduct(string productName)
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Products.Where(x => x.Name == productName).Select(x => x.ProductID).First();
            }
        }
        public static string GetNameCategoryAndProductID(string productName)
        {
            using (sklepEntities db = new sklepEntities())
            {
                var name = db.Products.Where(x => x.Name == productName).Join(db.Categories, product => product.CategoryID, category => category.CategoryID,
                    (product, category) => new { product.ProductID, category.Name }).First();

                return name.Name + "/" + name.ProductID;
            }
        }
        public static ProductViewModel GetDetailsProductAdmin(int productID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products item = db.Products.Where(x => x.ProductID == productID).Single();
                string categoryName = db.Categories.Where(x => x.CategoryID == item.CategoryID).Select(x => x.Name).Single();


                ProductViewModel prod = new ProductViewModel();
                prod.ProductID = item.ProductID;
                prod.Name = item.Name;
                prod.CategoryName = categoryName;
                prod.CategoryID = item.CategoryID;
                if (item.OldPriceN != null)
                    prod.OldPriceN = (decimal)item.OldPriceN;

                prod.PriceN = item.PriceN;
                prod.VAT = item.Vat;
                prod.PriceB = (1 + item.Vat) * item.PriceN;
                prod.Description = item.Description;
                prod.Specification = item.Specification;
                prod.Recommended = item.Recommended1;
                prod.Quantity = item.Quantity;
                prod.IsShowed = item.IsShowed;
                prod.CodeProduct = item.CodeProduct;

                return prod;
            }
        }
        public static LaptopDetailsViewModel GetLaptopSpecyfication(int productID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                string specyfication = db.Products.Where(x => x.ProductID == productID).Select(x => x.Specification).FirstOrDefault();
                string[] splitSpec = specyfication.Split(new[] { "$%" }, StringSplitOptions.None);
                LaptopDetailsViewModel spec = new LaptopDetailsViewModel();
                spec.Processor = splitSpec[0];
                spec.HDDSize = splitSpec[1];
                spec.SSDSize = splitSpec[2];
                return spec;
            }
        }
        public static ProcessorDetailsViewModel GetProcessorSpecyfication(int productID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                string specyfication = db.Products.Where(x => x.ProductID == productID).Select(x => x.Specification).FirstOrDefault();
                string[] splitSpec = specyfication.Split(new[] { "$%" }, StringSplitOptions.None);
                ProcessorDetailsViewModel spec = new ProcessorDetailsViewModel();
                spec.ProcessorModel = splitSpec[0];
                spec.ProcessorGraphics = splitSpec[1];
                spec.Socket = splitSpec[2];
                return spec;
            }
        }

        public static void EditProduct(ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = db.Products.Where(x => x.ProductID == product.ProductID).SingleOrDefault();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.IsShowed = product.IsShowed;
                db.SaveChanges();

            }
        }


        public static void AddProduct(ProductViewModel product) ////////////////////// do usuniecia
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.Description = product.Description;
                prod.Specification = product.Specification;
                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;
                db.Products.Add(prod);
                db.SaveChanges();
            }
        }
        public static bool AddImage(int productID, string path)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Images image = new Images();
                image.ProductID = productID;
                image.Path = path;
                db.Images.Add(image);
                db.SaveChanges();
                return true;
            }
        }

        public static bool AddLaptop(LaptopDetailsViewModel Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Procesor\":" + "\"" + Specyfication.Processor + "\","
                + "\"Karta graficzna\":" + "\"" + Specyfication.GraphicCard + "\","
                + "\"Rozmiar pamięci RAM\":" + "\"" + Specyfication.RAMSize + "\","
                + "\"Rozmiar dysku twardego\":" + "\"" + Specyfication.HDDSize + "\","
                + "\"Rozmiar dysku SSD\":" + "\"" + Specyfication.SSDSize + "\","
                + "\"Napęd\":" + "\"" + Specyfication.OpticalDrive + "\","
                + "\"Rozdzielczość ekranu\":" + "\"" + Specyfication.DisplayResolution + "\","
                + "\"Przekątna ekranu\":" + "\"" + Specyfication.DisplaySize + "\","
                + "\"Kolor\":" + "\"" + Specyfication.Color + "\","
                + "\"Bateria\":" + "\"" + Specyfication.Battery + "\","
                + "\"Waga\":" + "\"" + Specyfication.Weight + "\","
                + "\"System operacyjny\":" + "\"" + Specyfication.OperatingSystem + "\","
                + "\"Inne\":" + "\"" + Specyfication.OtherSpec + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }

        public static bool AddProcessor(ProcessorDetailsViewModel Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Model\":" + "\"" + Specyfication.ProcessorModel + "\","
                + "\"Socket\":" + "\"" + Specyfication.Socket + "\","
                + "\"Taktowanie\":" + "\"" + Specyfication.Frequency + "\","
                + "\"Liczba rdzeni\":" + "\"" + Specyfication.AmountCores + "\","
                + "\"Liczba wątków\":" + "\"" + Specyfication.AmountThreads + "\","
                + "\"Karta graficzna\":" + "\"" + Specyfication.ProcessorGraphics + "\","
                + "\"Pamięć Cache\":" + "\"" + Specyfication.Cached + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }

        public static bool AddGraphicsCard(GraphicsCardViewModel Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Rozmair pamięci RAM\":" + "\"" + Specyfication.MemorySize + "\","
                + "\"Taktowanie pamięci\":" + "\"" + Specyfication.FrequencyMemory + "\","
                + "\"Rodzaj pamięci\":" + "\"" + Specyfication.KindOfMemory + "\","
                + "\"Szyna\":" + "\"" + Specyfication.MemoryBus + "\","
                + "\"Taktowanie rdzenia\":" + "\"" + Specyfication.FrequencyCore + "\","
                + "\"Wyjścia\":" + "\"" + Specyfication.Output + "\","
                + "\"Pobór prądu\":" + "\"" + Specyfication.PowerConsumption + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }
        /* public static bool CheckIsProductExists(string name) // czy produkt o takiej samej nazwie istnieje
         {
             using (sklepEntities db = new sklepEntities())
             {
                 return db.Products.Where(x => x.Name == name).Any();
             }
         }*/
        public static bool AddMonitor(MonitorDetailsViewModel Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Przekątna ekranu\":" + "\"" + Specyfication.ScreenSize + "\","
                + "\"Rozdzielczość\":" + "\"" + Specyfication.Resolution + "\","
                + "\"Format ekratnu\":" + "\"" + Specyfication.ScreenSize + "\","
                + "\"Powłoka matrycy\":" + "\"" + Specyfication.Layer + "\","
                + "\"Rodzaj matrycy\":" + "\"" + Specyfication.PanelType + "\","
                + "\"Pobór prądu\":" + "\"" + Specyfication.PowerConsumption + "\","
                + "\"Wyjście\":" + "\"" + Specyfication.Output + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }


        public static bool AddMemoryRAM(MemoryRAMDetails Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Rodzaj pamięci\":" + "\"" + Specyfication.TypeOfMemory + "\","
                + "\"Pojemność\":" + "\"" + Specyfication.Capacity + "\","
                + "\"Taktowanie\":" + "\"" + Specyfication.Frequency + "\","
                + "\"Opóźnienia\":" + "\"" + Specyfication.CycleLatency + "\","
                + "\"Napięcie\":" + "\"" + Specyfication.Voltage + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }


        public static bool AddOperatingSystem(OperatingSystemDetails Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Achitektura\":" + "\"" + Specyfication.Architecture + "\","
                + "\"Licencja\":" + "\"" + Specyfication.License + "\","
                + "\"Liczba użytkowników\":" + "\"" + Specyfication.NumOfUser + "\","
                + "\"Liczba stanowisk\":" + "\"" + Specyfication.NumOfComputer + "\","
                + "\"Język\":" + "\"" + Specyfication.Language + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }


        public static bool AddAntivirus(AntivirusDetails Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Achitektura\":" + "\"" + Specyfication.Architecture + "\","
                + "\"Licencja\":" + "\"" + Specyfication.License + "\","
                + "\"Liczba użytkowników\":" + "\"" + Specyfication.NumOfUser + "\","
                + "\"Liczba stanowisk\":" + "\"" + Specyfication.NumOfComputer + "\","
                + "\"Język\":" + "\"" + Specyfication.Language + "\","
                + "\"Platforma\":" + "\"" + Specyfication.Platform + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }


        public static bool AddMouse(MouseDetails Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Rodzaj\":" + "\"" + Specyfication.TypeOfMouse + "\","
                + "\"Sensor\":" + "\"" + Specyfication.Sensor + "\","
                + "\"Rozdzielczość\":" + "\"" + Specyfication.Resolution + "\","
                + "\"Interfejs\":" + "\"" + Specyfication.Interface + "\","
                + "\"Długość przewodu\":" + "\"" + Specyfication.LengthCable + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }

        public static bool AddKeyboard(KeyboardDetailsViewModel Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Rodzaj\":" + "\"" + Specyfication.TypeOfKeyboard + "\","
                + "\"Podświetlenie\":" + "\"" + Specyfication.Backlight + "\","
                + "\"Touchpad\":" + "\"" + Specyfication.Touchpad + "\","
                + "\"Kolor\":" + "\"" + Specyfication.Color + "\","
                + "\"Interfejs\":" + "\"" + Specyfication.Interface + "\","
                + "\"Długość przewodu\":" + "\"" + Specyfication.LengthCable + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }


        public static bool AddHeadphones(HeadphonesDetailsViewModel Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Rodzaj\":" + "\"" + Specyfication.TypeOfHeadphones + "\","
                + "\"Czułośc\":" + "\"" + Specyfication.Sensitivity + "\","
                + "\"Mikrofon\":" + "\"" + Specyfication.Microphone + "\","
                + "\"Kolor\":" + "\"" + Specyfication.Color + "\","
                + "\"Interfejs\":" + "\"" + Specyfication.Interface + "\","
                + "\"Długość przewodu\":" + "\"" + Specyfication.LengthCable + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }

        public static bool AddSpeakers(SpeakersDetailsViewModel Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Rodzaj\":" + "\"" + Specyfication.TypeOfSepakers + "\","
                + "\"Moc\":" + "\"" + Specyfication.Power + "\","
                + "\"Mikrofon\":" + "\"" + Specyfication.Microphone + "\","
                + "\"Kolor\":" + "\"" + Specyfication.Color + "\","
                + "\"Interfejs\":" + "\"" + Specyfication.Interface + "\","
                + "\"Długość przewodu\":" + "\"" + Specyfication.LengthCable + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }

        public static bool AddExternalHardDrive(ExternalHardDriveDetailsViewmodel Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Rodzaj\":" + "\"" + Specyfication.TypeOfDrive + "\","
                + "\"Pojemność\":" + "\"" + Specyfication.Capacity + "\","
                + "\"Rozmiar\":" + "\"" + Specyfication.Size + "\","
                + "\"Kolor\":" + "\"" + Specyfication.Color + "\","
                + "\"Interfejs\":" + "\"" + Specyfication.Interface + "\","
                + "\"Wodoszczelność\":" + "\"" + Specyfication.Waterproof + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }

        public static bool AddMotherboards(MotherboardsDetailsViewModel Specyfication, ProductViewModel product)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Products prod = new Products();
                prod.Name = product.Name;
                if (product.OldPriceN < 0)
                    prod.OldPriceN = null;
                else
                    prod.OldPriceN = product.OldPriceN;
                prod.PriceN = product.PriceN;
                prod.Vat = product.VAT;
                prod.CodeProduct = product.CodeProduct;
                prod.Description = product.Description;

                prod.Quantity = product.Quantity;
                prod.Recommended1 = product.Recommended;
                prod.CategoryID = product.CategoryID;
                prod.IsShowed = product.IsShowed;

                string spec = "{\"Standard\":" + "\"" + Specyfication.Standard + "\","
                + "\"Chipset\":" + "\"" + Specyfication.Chipset + "\","
                + "\"Socket\":" + "\"" + Specyfication.Socket + "\","
                + "\"Maksymalna ilość procesorów\":" + "\"" + Specyfication.MaksimumNumOfProcessors + "\","
                + "\"Rodzaj obsługiwanej pamięci RAM\":" + "\"" + Specyfication.KindOfMemoryRAM + "\","
                + "\"Ilość slotów pamięci RAM\":" + "\"" + Specyfication.NumOfSlotsMemoryRAM + "\","
                + "\"Maksymalna ilość pamięci RAM\":" + "\"" + Specyfication.MaksimumCapaticyMemoryRAM + "\","
                + "\"Zintegrowana karta graficzna\":" + "\"" + Specyfication.IntegratedGraphicsCard + "\","
                + "\"Chipset dźwiękowy\":" + "\"" + Specyfication.SoundChipset + "\","
                + "\"Zintegrowana karta sieciowa\":" + "\"" + Specyfication.IntegratedNetworkCard + "\","
                + "\"Wyjścia\":" + "\"" + Specyfication.Output + "\"}";
                prod.Specification = spec;
                db.Products.Add(prod);
                db.SaveChanges();
            }
            return true;
        }

        public static List<ProductRecommededView> GetProductsRecommended()                      // uzyskanie produktow polecanych
        {
            List<ProductRecommededView> products = new List<ProductRecommededView>();
            using (sklepEntities db = new sklepEntities())
            {
                var productRecommended = from pro in db.Products
                                         where pro.Recommended1 == true && pro.IsShowed == true
                                         join img in db.Images
                                         on pro.ProductID equals img.ProductID
                                         select new { pro.ProductID, pro.Name, pro.OldPriceN, pro.PriceN, pro.Vat, img.Path };

                var productsUnique = productRecommended.GroupBy(p => p.ProductID).Select(s => s.FirstOrDefault()).Take(8);   // zwraca niepowtarzajace sie krotki
                if (productsUnique.Count() < 1)             // gdy brak produktow polecanych
                {
                    return null;
                }

                foreach (var item in productsUnique)
                {
                    ProductRecommededView prd = new ProductRecommededView();
                    prd.ProductID = item.ProductID;
                    prd.Name = item.Name;
                    if (item.OldPriceN != null)
                        prd.OldPriceB = ((decimal)((1M + item.Vat) * item.OldPriceN)).ToString("C2");
                    prd.PriceB = ((1M + item.Vat) * item.PriceN).ToString("C2");
                    prd.ImagePath = item.Path;
                    products.Add(prd);


                }
            }
            return products;
        }


        public static List<ProductRecommededView> GetProductsBestseller()                      // uzyskanie produktow najczesciej kupowanych
        {
            List<ProductRecommededView> products = new List<ProductRecommededView>();
            using (sklepEntities db = new sklepEntities())
            {
                var productBestseller = db.ProductsOfOrders
                   .GroupBy(x => x.ProductID)
                   .Select(z => new
                   {
                       ProductID = z.Key,
                       Quantity = z.Sum(q => q.Quantity)
                   })
                   .OrderByDescending(z => z.Quantity)
                   .Take(5)
                   .Join(db.Images, prodID => prodID.ProductID, img => img.ProductID,
                      (prodID, img) => new { ProductID = prodID.ProductID, Quantity = prodID.Quantity, imagePath = img.Path })
                   .Join(db.Products, prodOrID => prodOrID.ProductID, prodID => prodID.ProductID,
                      (prodOrID, prodID) => new { prodID.IsShowed, prodOrID.ProductID, prodOrID.Quantity, prodOrID.imagePath, prodID.OldPriceN, prodID.PriceN, prodID.Name, prodID.Vat });


                var productsUnique = productBestseller.Where(x => x.IsShowed == true).GroupBy(p => p.ProductID).Select(s => s.FirstOrDefault());   // zwraca niepowtarzajace sie krotki
                if (productsUnique.Count() < 1)             // gdy brak produktow polecanych
                {
                    return null;
                }

                foreach (var item in productsUnique)
                {

                    ProductRecommededView prd = new ProductRecommededView();
                    prd.ProductID = item.ProductID;
                    prd.Name = item.Name;
                    if (item.OldPriceN != null)
                        prd.OldPriceB = ((decimal)((1M + item.Vat) * item.OldPriceN)).ToString("C2");
                    prd.PriceB = ((1M + item.Vat) * item.PriceN).ToString("C2");
                    prd.ImagePath = item.imagePath;
                    products.Add(prd);
                }
            }
            return products;
        }

        public static int GetQuantityProduct(int productID)
        {
            int quantity = 0;
            using (sklepEntities db = new sklepEntities())
            {
                Products product = db.Products.Where(x => x.ProductID == productID).SingleOrDefault();
                quantity = product.Quantity;
            }
            return quantity;
        }


        public static List<ProductCategoryView> GetProductsFromCategory(int categoryID)
        {
            List<ProductCategoryView> listProducts = new List<ProductCategoryView>();
            using (sklepEntities db = new sklepEntities())
            {
                bool isExistsCategory = db.Categories.Where(x => x.CategoryID == categoryID).Any();
                if (!isExistsCategory)
                {
                    return null;    // gdy nie istnieje dana kategoria
                }
                var products = from cat in db.Categories
                               where cat.CategoryID == categoryID
                               join pr in db.Products
                               on cat.CategoryID equals pr.CategoryID
                               join img in db.Images
                               on pr.ProductID equals img.ProductID
                               select new { pr.IsShowed, pr.ProductID, pr.Name, pr.PriceN, pr.Vat, pr.Specification, img.Path };

                if (products.Count() < 1)
                {
                    return null;   // gdy w kategorii nie ma produktow
                }
                var productsUnique = products.Where(x => x.IsShowed == true).GroupBy(p => p.ProductID).Select(s => s.FirstOrDefault());

                foreach (var item in productsUnique)
                {
                    ProductCategoryView prd = new ProductCategoryView();
                    prd.ProductID = item.ProductID;
                    prd.Name = item.Name;
                    prd.PriceN = item.PriceN.ToString("C2");
                    prd.PriceB = ((1M + item.Vat) * item.PriceN).ToString("C2");
                    prd.ImagePath = item.Path;
                    prd.Specification = item.Specification;
                    listProducts.Add(prd);
                }


            }
            return listProducts;
        }


        public static List<ProductCategoryView> GetProductsFromCategoryMinMax(int categoryID, int min, int max, int sort, out int maxToSelect)
        {
            List<ProductCategoryView> listProducts = new List<ProductCategoryView>();
            maxToSelect = 0;

            using (sklepEntities db = new sklepEntities())
            {
                bool isExistsCategory = db.Categories.Where(x => x.CategoryID == categoryID).Any();
                if (!isExistsCategory)
                {
                    return null;    // gdy nie istnieje dana kategoria
                }
                var products = from pr in db.Products
                               where (((1M + pr.Vat) * pr.PriceN) >= min) && (((1M + pr.Vat) * pr.PriceN) <= max) && pr.CategoryID == categoryID
                               join img in db.Images
                               on pr.ProductID equals img.ProductID
                               select new { pr.IsShowed, pr.ProductID, pr.Name, pr.OldPriceN, pr.PriceN, pr.Vat, pr.Specification, img.Path };

                if (products.Count() < 1)
                {
                    return null;   // gdy w kategorii nie ma produktow
                }
                var productsUnique = products.Where(x => x.IsShowed == true).GroupBy(p => p.ProductID).Select(s => s.FirstOrDefault());

                switch (sort)
                {
                    case 0:
                        productsUnique = productsUnique.OrderBy(x => x.Name);
                        break;
                    case 1:
                        productsUnique = productsUnique.OrderByDescending(x => x.Name);
                        break;
                    case 2:
                        productsUnique = productsUnique.OrderBy(x => x.PriceN);
                        break;
                    case 3:
                        productsUnique = productsUnique.OrderByDescending(x => x.PriceN);
                        break;
                }

                foreach (var item in productsUnique)
                {
                    ProductCategoryView prd = new ProductCategoryView();
                    prd.ProductID = item.ProductID;
                    prd.Name = item.Name;
                    prd.PriceN = item.PriceN.ToString("C2");
                    decimal priceB = (1M + item.Vat) * item.PriceN;
                    if (item.OldPriceN != null)
                        prd.OldPriceB = ((decimal)((1M + item.Vat) * item.OldPriceN)).ToString("C2");
                    prd.PriceB = priceB.ToString("C2");
                    prd.ImagePath = item.Path;
                    prd.Specification = item.Specification;
                    prd.specJ = JObject.Parse(item.Specification);
                    listProducts.Add(prd);

                    if (maxToSelect < priceB)
                    {
                        maxToSelect = (int)priceB;
                    }
                }


            }
            return listProducts;
        }


        public static List<ProductCategoryView> GetProductFromSearch(string productName, int categoryID, int min, int max, int sort, out int maxToSelect)
        {
            List<ProductCategoryView> listProducts = new List<ProductCategoryView>();
            // productName = productName.ToLower();
            maxToSelect = 0;
            using (sklepEntities db = new sklepEntities())
            {
                if (categoryID == -1) // gdy wybrano wyszukiwanie we wszystkich kategoriach
                {
                    var allProducts = from pr in db.Products
                                      where (((1M + pr.Vat) * pr.PriceN) >= min) && (((1M + pr.Vat) * pr.PriceN) <= max)
                                      join img in db.Images
                                      on pr.ProductID equals img.ProductID
                                      select new { pr.IsShowed, pr.ProductID, pr.Name, pr.OldPriceN, pr.PriceN, pr.Vat, pr.Specification, img.Path };


                    if (allProducts.Count() < 1)
                    {
                        return null;   // gdy brak szukanych produktow
                    }


                    var productsUnique = allProducts.Where(x => x.IsShowed == true).GroupBy(p => p.ProductID).Select(s => s.FirstOrDefault());

                    switch (sort) // sortowanie
                    {
                        case 0:
                            productsUnique = productsUnique.OrderBy(x => x.Name);
                            break;
                        case 1:
                            productsUnique = productsUnique.OrderByDescending(x => x.Name);
                            break;
                        case 2:
                            productsUnique = productsUnique.OrderBy(x => x.PriceN);
                            break;
                        case 3:
                            productsUnique = productsUnique.OrderByDescending(x => x.PriceN);
                            break;
                    }
                    foreach (var item in productsUnique)
                    {
                     //   if (productName.Split(' ').All(((item.Name).ToLower()).Contains))
                     //   {
                            ProductCategoryView prd = new ProductCategoryView();
                            prd.ProductID = item.ProductID;
                            prd.Name = item.Name;
                            prd.PriceN = item.PriceN.ToString("C2");
                            decimal priceB = (1M + item.Vat) * item.PriceN;
                            prd.PriceB = priceB.ToString("C2");
                            if (item.OldPriceN != null)
                                prd.OldPriceB = ((decimal)((1M + item.Vat) * item.OldPriceN)).ToString("C2");
                            prd.ImagePath = item.Path;
                            prd.Specification = item.Specification;
                            prd.specJ = JObject.Parse(item.Specification);
                            listProducts.Add(prd);


                            if (maxToSelect < priceB)
                            {
                                maxToSelect = (int)priceB;
                            }
                     //   }
                    }


                    return listProducts;
                }
                else  // gdy wybrano dana kategorie w wyszukiwarce
                {
                    if (!CategoryManager.isRootCategory(categoryID)) // gdy wybrano podkategorie 
                    {

                        var products = from pr in db.Products
                                       where (((1M + pr.Vat) * pr.PriceN) >= min) && (((1M + pr.Vat) * pr.PriceN) <= max) && pr.CategoryID == categoryID
                                       join img in db.Images
                                       on pr.ProductID equals img.ProductID
                                       select new { pr.IsShowed, pr.ProductID, pr.Name, pr.OldPriceN, pr.PriceN, pr.Vat, pr.Specification, img.Path };

                        if (products.Count() < 1)
                        {
                            return null;   // gdy brak szukanych produktow
                        }
                        var productsUnique = products.Where(x => x.IsShowed == true).GroupBy(p => p.ProductID).Select(s => s.FirstOrDefault());


                        foreach (var item in productsUnique)
                        {
                         //   if (productName.Split(' ').All(((item.Name).ToLower()).Contains))
                         //   {
                                ProductCategoryView prd = new ProductCategoryView();
                                prd.ProductID = item.ProductID;
                                prd.Name = item.Name;
                                prd.PriceN = item.PriceN.ToString("C2");
                                decimal priceB = (1M + item.Vat) * item.PriceN;
                                prd.PriceB = priceB.ToString("C2");
                                if (item.OldPriceN != null)
                                    prd.OldPriceB = ((decimal)((1M + item.Vat) * item.OldPriceN)).ToString("C2");
                                prd.ImagePath = item.Path;
                                prd.Specification = item.Specification;
                                prd.specJ = JObject.Parse(item.Specification);
                                listProducts.Add(prd);

                                if (maxToSelect < priceB)
                                {
                                    maxToSelect = (int)priceB;
                                }
                         //   }
                        }
                    }
                    else
                    {
                        var categories = db.Categories.Where(x => x.ParentCategoryID == categoryID && x.CategoryID != x.ParentCategoryID);

                        foreach (Categories category in categories)
                        {
                            var products = from pr in db.Products
                                           where (((1M + pr.Vat) * pr.PriceN) >= min) && (((1M + pr.Vat) * pr.PriceN) <= max)
                                           join img in db.Images
                                           on pr.ProductID equals img.ProductID
                                           join cat in categories
                                           on pr.CategoryID equals cat.CategoryID

                                           select new { pr.IsShowed, pr.ProductID, pr.Name, pr.OldPriceN, pr.PriceN, pr.Vat, pr.Specification, img.Path };

                            if (products.Count() < 1)
                            {
                                return null;   // gdy brak szukanych produktow
                            }
                            var productsUnique = products.Where(x => x.IsShowed == true).GroupBy(p => p.ProductID).Select(s => s.FirstOrDefault());


                            foreach (var item in productsUnique)
                            {
                             //   if (productName.Split(' ').All(((item.Name).ToLower()).Contains))
                             //   {
                                    ProductCategoryView prd = new ProductCategoryView();
                                    prd.ProductID = item.ProductID;
                                    prd.Name = item.Name;
                                    prd.PriceN = item.PriceN.ToString("C2");
                                    decimal priceB = (1M + item.Vat) * item.PriceN;
                                    prd.PriceB = priceB.ToString("C2");
                                    if (item.OldPriceN != null)
                                        prd.OldPriceB = ((decimal)((1M + item.Vat) * item.OldPriceN)).ToString("C2");
                                    prd.ImagePath = item.Path;
                                    prd.Specification = item.Specification;
                                    prd.specJ = JObject.Parse(item.Specification);
                                    listProducts.Add(prd);

                                    if (maxToSelect < priceB)
                                    {
                                        maxToSelect = (int)priceB;
                                    }
                             //   }
                            }

                        }
                    }

                    listProducts = listProducts.GroupBy(p => p.ProductID).Select(s => s.FirstOrDefault()).ToList();
                    switch (sort)
                    {
                        case 0:
                            listProducts = listProducts.OrderBy(x => x.Name).ToList();
                            break;
                        case 1:
                            listProducts = listProducts.OrderByDescending(x => x.Name).ToList();
                            break;
                        case 2:
                            listProducts = listProducts.OrderBy(x => x.PriceN).ToList();
                            break;
                        case 3:
                            listProducts = listProducts.OrderByDescending(x => x.PriceN).ToList();
                            break;
                    }

                    return listProducts;
                }

            }

        }



        public static List<ProductsSearchWithPromptViewModel> GetProductFromSearchWithSuggestions(string productName, int categoryID)
        {
            List<ProductsSearchWithPromptViewModel> returnProducts = new List<ProductsSearchWithPromptViewModel>();
            productName = productName.ToLower();
            using (sklepEntities db = new sklepEntities())
            {
                if (categoryID == -1) // gdy wybrano wyszukiwanie we wszystkich kategoriach
                {
                    int counter = 1;
                    var allProducts = db.Products;
                    foreach (var item in allProducts)
                    {
                        if (counter > 8)
                            break;
                        if (productName.Split(' ').All(((item.Name).ToLower()).Contains))
                        {
                            ProductsSearchWithPromptViewModel pr = new ProductsSearchWithPromptViewModel();
                            pr.ProductID = item.ProductID;
                            pr.ProductName = item.Name;
                            pr.PriceB = ((1 + item.Vat) * item.PriceN).ToString("C2");
                            returnProducts.Add(pr);
                            counter++;
                        }
                    }
                    if (counter == 1)
                    {
                        ProductsSearchWithPromptViewModel pr = new ProductsSearchWithPromptViewModel();
                        pr.ProductID = -1;
                        pr.ProductName = "Brak przedmiotów o podanej nazwie";
                        pr.PriceB = "-1";
                        returnProducts.Add(pr);
                    }
                    return returnProducts;
                }
                else  // gdy wybrano dana kategorie w wyszukiwarce
                {
                    if (!CategoryManager.isRootCategory(categoryID)) // gdy wybrano podkategorie 
                    {
                        int counter = 1;
                        var productsInCategory = db.Products.Join(db.Categories, prCat => prCat.CategoryID, catID => catID.CategoryID,
                               (prCat, catID) => new { prCat.Name, prCat.ProductID, prCat.PriceN, prCat.Vat, catID.CategoryID }); // wszystkie produkty z kategorii

                        foreach (var item in productsInCategory)
                        {
                            // System.Diagnostics.Debug.WriteLine("cala lista: " + item.Name);
                            if (counter > 8)
                                break;
                            if (productName.Split(' ').All(((item.Name).ToLower()).Contains))
                            {
                                ProductsSearchWithPromptViewModel pr = new ProductsSearchWithPromptViewModel();
                                pr.ProductID = item.ProductID;
                                pr.ProductName = item.Name;
                                pr.PriceB = ((1 + item.Vat) * item.PriceN).ToString("C2");
                                returnProducts.Add(pr);
                                counter++;
                            }
                        }
                        if (counter == 1)
                        {
                            ProductsSearchWithPromptViewModel pr = new ProductsSearchWithPromptViewModel();
                            pr.ProductID = -1;
                            pr.ProductName = "Brak przedmiotów o podanej nazwie";
                            pr.PriceB = "-1";
                            returnProducts.Add(pr);
                        }
                        return returnProducts;
                    }
                    else
                    {

                        var categories = db.Categories.Where(x => x.ParentCategoryID == categoryID && x.CategoryID != x.ParentCategoryID);

                        int counter = 1;
                        foreach (Categories category in categories)
                        {
                            var productsInCategory = db.Products.Join(db.Categories, prCat => prCat.CategoryID, catID => catID.CategoryID,
                                (prCat, catID) => new { prCat.Name, prCat.ProductID, prCat.PriceN, prCat.Vat, catID.CategoryID }).Where(x => x.CategoryID == category.CategoryID); // wszystkie produkty z kategorii

                            foreach (var item in productsInCategory)
                            {
                                if (counter > 8)
                                    break;
                                if (productName.Split(' ').All(((item.Name).ToLower()).Contains))
                                {
                                    ProductsSearchWithPromptViewModel pr = new ProductsSearchWithPromptViewModel();
                                    pr.ProductID = item.ProductID;
                                    pr.ProductName = item.Name;
                                    pr.PriceB = ((1 + item.Vat) * item.PriceN).ToString("C2");
                                    returnProducts.Add(pr);
                                    counter++;
                                }
                            }
                        }
                        if (counter == 1)
                        {
                            ProductsSearchWithPromptViewModel pr = new ProductsSearchWithPromptViewModel();
                            pr.ProductID = -1;
                            pr.ProductName = "Brak przedmiotów o podanej nazwie";
                            pr.PriceB = "-1";
                            returnProducts.Add(pr);
                        }
                    }



                    return returnProducts;
                }

            }
        }


        public static ProductView GetDetailsProduct(int productID)
        {
            ProductView product = new ProductView();
            using (sklepEntities db = new sklepEntities())
            {
                Products productDB = db.Products.Where(x => x.ProductID == productID).Single();
                product.ProductID = productDB.ProductID;
                product.Name = productDB.Name;
                product.PriceN = productDB.PriceN.ToString("C2");
                product.PriceB = (productDB.PriceN + (productDB.PriceN * productDB.Vat)).ToString("C2");
                if (productDB.OldPriceN != null)
                    product.OldPriceB = ((decimal)((1M + productDB.Vat) * productDB.OldPriceN)).ToString("C2");
                product.Specification = productDB.Specification;
                product.Description = productDB.Description;
                product.CodeProduct = productDB.CodeProduct;
                product.CategoryID = productDB.CategoryID;
                product.Quantity = productDB.Quantity;
                product.ImagesPath = new List<string>();
                var images = db.Images.Where(y => y.ProductID == productID);
                foreach (Images item in images)
                {
                    product.ImagesPath.Add(item.Path);
                }

                product.specJ = JObject.Parse(productDB.Specification);
            }
            return product;
        }



        public static string CheckDetailsProduct(string ProductName, string CodeProduct, int CategoryID, string OldPriceN, string PriceN, string VAT, string Description, int Quantity, bool Recommended, bool IsShowed, out decimal parseOldPriceN, out decimal parsePriceN, out decimal parseVAT)
        {
            string answer = "";
            parseOldPriceN = 0;
            parsePriceN = 0;
            parseVAT = 0;

            if (ProductName.Length < 3)
            {
                answer += "Nazwa produktu musi liczyć minimum 3 znaki";
            }

            try
            {
                parsePriceN = decimal.Parse(PriceN.Replace('.', ','));
                if (parsePriceN < 0)
                    answer += "Cena nie może być ujemna";

                if (OldPriceN != "" && OldPriceN != "NaN")
                {
                    try
                    {

                        parseOldPriceN = decimal.Parse(OldPriceN.Replace('.', ','));
                        if (parseOldPriceN < 0)
                            answer += "Cena nie może być ujemna";
                        //  if (parsePriceN > parseOldPriceN)
                        //    answer += "Nowa cena nie może być niższa lub równa od starej ceny";
                    }
                    catch (Exception e)
                    {
                        parseOldPriceN = -1;
                        answer += "Podano zły format starej ceny";
                    }
                }
                else
                {
                    parseOldPriceN = -2;
                }
            }
            catch (Exception e)
            {
                parsePriceN = -1;
                answer += "Podano zły format nowej ceny";
            }

            try
            {
                parseVAT = decimal.Parse(VAT.Replace('.', ','));
                if (parseVAT < 0)
                    answer += "VAT nie może być ujemny";
            }
            catch (Exception e)
            {
                parseVAT = -1;
                answer += "Podano zły format VAT";
            }
            if (Quantity < 0)
            {
                answer += "Ilość nie może być ujemna";
            }
            return answer;
        }
    }

}
