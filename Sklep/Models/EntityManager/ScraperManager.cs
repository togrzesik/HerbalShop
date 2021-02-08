using Sklep.Models.db;
using Sklep.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Sklep.Models.EntityManager
{
    public class ScraperManager
    {
        public async static void scrap()
        {
            ScraperManager scraperManager = new ScraperManager();
            using (sklepEntities db = new sklepEntities())
            {

                var products = from pr in db.Products
                               join scraper in db.Scrapers on pr.ProductID equals scraper.ProductID into joi
                               from scraper in joi.DefaultIfEmpty()
                               where pr.CodeProduct != null 
                               select new
                               {
                                   ProductID = pr.ProductID,
                                   Name = pr.Name,
                                   CodeProduct = pr.CodeProduct,
                                   PriceN = pr.PriceN,
                                   VAT = pr.Vat,
                                   XKomLinkChecked = scraper == null ? String.Empty : scraper.XKomLinkChecked,
                                   VobistLinkChecked = scraper == null ? String.Empty : scraper.VobistLinkChecked,
                                   AlsentLinkChecked = scraper == null ? String.Empty : scraper.AlsentLinkChecked,
                                   CeneoLinkChecked = scraper == null ? String.Empty : scraper.CeneoLinkChecked,
                                   scraperID = scraper == null ? 0 : scraper.Id
                               };

                foreach (var item in products)
                {
                    if(string.IsNullOrWhiteSpace(item.CodeProduct))
                    {
                        continue;
                    }

                    string xkomLink ="",  vobisLink ="",  alsenLink="",  ceneoLink="";
                    string[] splitProductName = (item.Name).Split(' ');
                    PriceWithLinkScraper xkom = new PriceWithLinkScraper();
                    PriceWithLinkScraper vobis = new PriceWithLinkScraper();
                    PriceWithLinkScraper alsen = new PriceWithLinkScraper();
                    PriceWithLinkScraper ceneo = new PriceWithLinkScraper();
                    Thread xKomThread, vobisThread, alsenThread, ceneoThread;
                      if (string.IsNullOrWhiteSpace(item.XKomLinkChecked))
                         {
                            xKomThread = new Thread(() => { xkom = scraperManager.runXkom(item.ProductID, item.CodeProduct); });
                             xKomThread.Start();
                             xKomThread.Join();
                             xkomLink = xkom.Link;
                         }
                         else
                         {
                             xKomThread = new Thread(() => { xkom = scraperManager.runXkomChecked(item.ProductID, item.CodeProduct, item.XKomLinkChecked); });
                             xKomThread.Start();
                             xKomThread.Join();
                             xkomLink = item.XKomLinkChecked;
                         }
                    if (string.IsNullOrWhiteSpace(item.VobistLinkChecked))
                    { 
                        vobisThread = new Thread(() => { vobis = scraperManager.runVobis(item.ProductID, item.CodeProduct); });
                        vobisThread.Start();
                        vobisThread.Join();
                        vobisLink = vobis.Link;
                      
                    }
                    else
                    {
                        vobisThread = new Thread(() => { vobis = scraperManager.runVobisChecked(item.ProductID, item.CodeProduct, item.VobistLinkChecked); });
                        vobisThread.Start();
                        vobisThread.Join();
                        vobisLink = item.VobistLinkChecked;
                    }

                    if (string.IsNullOrWhiteSpace(item.AlsentLinkChecked))
                    {
                        alsenThread = new Thread(() => { alsen = scraperManager.runAlsen(item.ProductID, item.CodeProduct); });
                        alsenThread.Start();
                        alsenThread.Join();
                        alsenLink = alsen.Link;
                    }
                    else
                    {
                        alsenThread = new Thread(() => { alsen = scraperManager.runAlsenChecked(item.ProductID, item.CodeProduct, item.AlsentLinkChecked); });
                        alsenThread.Start();
                        alsenThread.Join();
                        alsenLink = item.AlsentLinkChecked;
                    }
                    if (string.IsNullOrWhiteSpace(item.CeneoLinkChecked))
                    {
                        ceneoThread = new Thread(() => { ceneo = scraperManager.runCeneo(item.ProductID, item.CodeProduct, splitProductName[0]); });
                        ceneoThread.Start();
                        ceneoThread.Join();
                        ceneoLink = ceneo.Link;
                    }
                    else
                    {
                        ceneoThread = new Thread(() => { ceneo = scraperManager.runCeneoChecked(item.ProductID, splitProductName[0], item.CeneoLinkChecked); });
                        ceneoThread.Start();
                        ceneoThread.Join();
                        ceneoLink = item.CeneoLinkChecked;
                    }
                  
                   
                    List<string> prices = new List<string>();
                    if (xkomLink == item.XKomLinkChecked && !string.IsNullOrWhiteSpace(item.XKomLinkChecked))
                        prices.Add(xkom.Price);
                    if (alsenLink == item.AlsentLinkChecked && !string.IsNullOrWhiteSpace(item.AlsentLinkChecked))
                        prices.Add(alsen.Price);
                    if (vobisLink == item.VobistLinkChecked && !string.IsNullOrWhiteSpace(item.VobistLinkChecked))
                        prices.Add(vobis.Price);
                    if (ceneoLink == item.CeneoLinkChecked && !string.IsNullOrWhiteSpace(item.CeneoLinkChecked))
                        prices.Add(ceneo.Price);
                    decimal differencePrice = -9999999;
                    if (prices.Count>0)
                         differencePrice = scraperManager.countDifferece(prices, (1 + item.VAT) * item.PriceN);
                    scraperManager.saveData(item.ProductID, xkom.Price, xkomLink, vobis.Price, vobisLink, alsen.Price, alsenLink, ceneo.Price, ceneoLink, differencePrice);

                }

            }

        }
        public PriceWithLinkScraper runXkom(int productID, string codeProduct)
        {
            PriceWithLinkScraper priceWithLinkScraper = new PriceWithLinkScraper();
            priceWithLinkScraper.Price = "";
            priceWithLinkScraper.Link = "";
            System.Diagnostics.Debug.WriteLine("xkom przed " + productID);
            string python = @"C:\Users\Dawid\AppData\Local\Programs\Python\Python36-32\python.exe";
            string myPythonApp = @"C:\Users\Dawid\Documents\GitHub\SklepInternetowy\Scraper\xkom.py";

            if (codeProduct[0] == ' ')                                      // usuniecie spacji na poczatku
                codeProduct = codeProduct.Substring(1);

            if (codeProduct[codeProduct.Length - 1] == ' ')                 // usuniecie spacji na koncu
                codeProduct = codeProduct.Substring(0, codeProduct.Length - 1);

            codeProduct = codeProduct.Replace(" ", "$#$#");
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.RedirectStandardOutput = true;
            myProcessStartInfo.Arguments = myPythonApp + " " + codeProduct;

            Process myProcess = new Process();
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.Start();

            char[] splitter = { '\n' };
            StreamReader myStreamReader = myProcess.StandardOutput;
            string[] myString = myStreamReader.ReadToEnd().Split(splitter);
            string[] spllit = new string[] { "&&&" };
            
            System.Diagnostics.Debug.WriteLine("xkom " + productID + " " + myString);
            foreach (string s in myString)
            {
                if (!String.IsNullOrWhiteSpace(s) || s.Length > 2)
                {
                    string[] spli = s.Split(spllit, StringSplitOptions.None);
                    string[] splitLink = spli[0].Split(new[] { "link: " }, StringSplitOptions.None);
                    string[] splitPrice = spli[1].Split(new[] { "cena: " }, StringSplitOptions.None);

                    if (string.IsNullOrWhiteSpace(splitLink[1]))
                    {
                        continue;
                    }
                    priceWithLinkScraper.Price = splitPrice[1];
                    priceWithLinkScraper.Link = splitLink[1];
                }


            }
            myProcess.WaitForExit();
            myProcess.Close();
            return priceWithLinkScraper;
        }
        public PriceWithLinkScraper runXkomChecked(int productID, string codeProduct, string linkProduct)
        {
            PriceWithLinkScraper priceWithLinkScraper = new PriceWithLinkScraper();
            priceWithLinkScraper.Price = "";
            priceWithLinkScraper.Link = "";
            string python = @"C:\Users\Dawid\AppData\Local\Programs\Python\Python36-32\python.exe";
            string myPythonApp = @"C:\Users\Dawid\Documents\GitHub\SklepInternetowy\Scraper\xkomChecked.py";

            if (codeProduct[0] == ' ')                                      // usuniecie spacji na poczatku
                codeProduct = codeProduct.Substring(1);

            if (codeProduct[codeProduct.Length - 1] == ' ')                 // usuniecie spacji na koncu
                codeProduct = codeProduct.Substring(0, codeProduct.Length - 1);

            codeProduct = codeProduct.Replace(" ", "$#$#");
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.RedirectStandardOutput = true;
            myProcessStartInfo.Arguments = myPythonApp + " " + codeProduct + " " + linkProduct;

            Process myProcess = new Process();
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.Start();
        
            char[] splitter = { '\n' };
            StreamReader myStreamReader = myProcess.StandardOutput;
            string[] myString = myStreamReader.ReadToEnd().Split(splitter);

            foreach (string s in myString)
            {
                if (!String.IsNullOrWhiteSpace(s) || s.Length > 2)
                {
                    string[] splitPrice = s.Split(new[] { "cena: " }, StringSplitOptions.None);
                    priceWithLinkScraper.Price = splitPrice[1];
                }


            }
            myProcess.WaitForExit();
            myProcess.Close();
            return priceWithLinkScraper;
        }

        public PriceWithLinkScraper runVobis(int productID, string codeProduct)
        {
            PriceWithLinkScraper priceWithLinkScraper = new PriceWithLinkScraper();
            priceWithLinkScraper.Price = "";
            priceWithLinkScraper.Link = "";
            string python = @"C:\Users\Dawid\AppData\Local\Programs\Python\Python36-32\python.exe";
            string myPythonApp = @"C:\Users\Dawid\Documents\GitHub\SklepInternetowy\Scraper\vobis.py";

            if (codeProduct[0] == ' ')                                      // usuniecie spacji na poczatku
                codeProduct = codeProduct.Substring(1);

            if (codeProduct[codeProduct.Length - 1] == ' ')                 // usuniecie spacji na koncu
                codeProduct = codeProduct.Substring(0, codeProduct.Length - 1);

            codeProduct = codeProduct.Replace(" ", "$#$#");
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.RedirectStandardOutput = true;
            myProcessStartInfo.Arguments = myPythonApp + " " + codeProduct;

            Process myProcess = new Process();
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.Start();

            char[] splitter = { '\n' };
            StreamReader myStreamReader = myProcess.StandardOutput;
            string[] myString = myStreamReader.ReadToEnd().Split(splitter);
          
           
            string[] spllit = new string[] { "&&&" };
            foreach (string s in myString)
            {
                if (!String.IsNullOrWhiteSpace(s) || s.Length > 2)
                {
                
                    string[] spli = s.Split(spllit, StringSplitOptions.None);

                    string[] splitLink = spli[0].Split(new[] { "link: " }, StringSplitOptions.None);
                    string[] splitPrice = spli[1].Split(new[] { "cena: " }, StringSplitOptions.None);

                    if (string.IsNullOrWhiteSpace(splitLink[1]))
                    {
                        continue;
                    }
                    priceWithLinkScraper.Price = splitPrice[1];
                    priceWithLinkScraper.Link = splitLink[1];
                    System.Diagnostics.Debug.WriteLine("vobis " + productID + " " + s + " link " + priceWithLinkScraper.Link );
                }


            }
            myProcess.WaitForExit();
            myProcess.Close();
            return priceWithLinkScraper;
        }

        public PriceWithLinkScraper runVobisChecked(int productID, string codeProduct, string linkProduct)
        {
            PriceWithLinkScraper priceWithLinkScraper = new PriceWithLinkScraper();
            priceWithLinkScraper.Price = "";
            priceWithLinkScraper.Link = "";
            string python = @"C:\Users\Dawid\AppData\Local\Programs\Python\Python36-32\python.exe";
            string myPythonApp = @"C:\Users\Dawid\Documents\GitHub\SklepInternetowy\Scraper\vobisChecked.py";

            if (codeProduct[0] == ' ')                                      // usuniecie spacji na poczatku
                codeProduct = codeProduct.Substring(1);

            if (codeProduct[codeProduct.Length - 1] == ' ')                 // usuniecie spacji na koncu
                codeProduct = codeProduct.Substring(0, codeProduct.Length - 1);

            codeProduct = codeProduct.Replace(" ", "$#$#");
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.RedirectStandardOutput = true;
            myProcessStartInfo.Arguments = myPythonApp + " " + codeProduct + " " + linkProduct;

            Process myProcess = new Process();
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.Start();
            
            char[] splitter = { '\n' };
            StreamReader myStreamReader = myProcess.StandardOutput;
            string[] myString = myStreamReader.ReadToEnd().Split(splitter);

            foreach (string s in myString)
            {
                if (!String.IsNullOrWhiteSpace(s) || s.Length > 2)
                {
                    string[] splitPrice = s.Split(new[] { "cena: " }, StringSplitOptions.None);
                    priceWithLinkScraper.Price = splitPrice[1];
                }
                

            }
            myProcess.WaitForExit();
            myProcess.Close();
            return priceWithLinkScraper;
        }

        public  PriceWithLinkScraper runAlsen(int productID, string codeProduct)
        {
            PriceWithLinkScraper priceWithLinkScraper = new PriceWithLinkScraper();
            priceWithLinkScraper.Price = "";
            priceWithLinkScraper.Link = "";
            string python = @"C:\Users\Dawid\AppData\Local\Programs\Python\Python36-32\python.exe";
            string myPythonApp = @"C:\Users\Dawid\Documents\GitHub\SklepInternetowy\Scraper\alsen.py";

            if (codeProduct[0] == ' ')                                      // usuniecie spacji na poczatku
                codeProduct = codeProduct.Substring(1);

            if (codeProduct[codeProduct.Length - 1] == ' ')                 // usuniecie spacji na koncu
                codeProduct = codeProduct.Substring(0, codeProduct.Length - 1);

            codeProduct = codeProduct.Replace(" ", "$#$#");
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.RedirectStandardOutput = true;
            myProcessStartInfo.Arguments = myPythonApp + " " + codeProduct;

            Process myProcess = new Process();
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.Start();

            char[] splitter = { '\n' };
            StreamReader myStreamReader = myProcess.StandardOutput;
            string[] myString = myStreamReader.ReadToEnd().Split(splitter);
           
            string[] spllit = new string[] { "&&&" };

            foreach (string s in myString)
            {
                if (!String.IsNullOrWhiteSpace(s) || s.Length > 2)
                {

                    string[] spli = s.Split(spllit, StringSplitOptions.None);
                    string[] splitLink = spli[0].Split(new[] { "link: " }, StringSplitOptions.None);
                    string[] splitPrice = spli[1].Split(new[] { "cena: " }, StringSplitOptions.None);

                    if (string.IsNullOrWhiteSpace(splitLink[1]))
                    {
                        continue;
                    }
                    priceWithLinkScraper.Price = splitPrice[1];
                    priceWithLinkScraper.Link = splitLink[1];
                    System.Diagnostics.Debug.WriteLine("vobis " + productID + " " + s + " link " + priceWithLinkScraper.Link);
                }


            }
            myProcess.WaitForExit();
            myProcess.Close();
            return priceWithLinkScraper;
        }

        public  PriceWithLinkScraper runAlsenChecked(int productID, string codeProduct, string linkProduct)
        {
            PriceWithLinkScraper priceWithLinkScraper = new PriceWithLinkScraper();
            priceWithLinkScraper.Price = "";
            priceWithLinkScraper.Link = "";
            string python = @"C:\Users\Dawid\AppData\Local\Programs\Python\Python36-32\python.exe";
            string myPythonApp = @"C:\Users\Dawid\Documents\GitHub\SklepInternetowy\Scraper\alsenChecked.py";

            if (codeProduct[0] == ' ')                                      // usuniecie spacji na poczatku
                codeProduct = codeProduct.Substring(1);

            if (codeProduct[codeProduct.Length - 1] == ' ')                 // usuniecie spacji na koncu
                codeProduct = codeProduct.Substring(0, codeProduct.Length - 1);

            codeProduct = codeProduct.Replace(" ", "$#$#");
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.RedirectStandardOutput = true;
            myProcessStartInfo.Arguments = myPythonApp + " " + codeProduct + " " + linkProduct;

            Process myProcess = new Process();
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.Start();
            
            char[] splitter = { '\n' };
            StreamReader myStreamReader = myProcess.StandardOutput;
            string[] myString = myStreamReader.ReadToEnd().Split(splitter);

            foreach (string s in myString)
            {
                if (!String.IsNullOrWhiteSpace(s) || s.Length > 2)
                {
                    string[] splitPrice = s.Split(new[] { "cena: " }, StringSplitOptions.None);
                    priceWithLinkScraper.Price = splitPrice[1];
                }


            }
            myProcess.WaitForExit();
            myProcess.Close();
            return priceWithLinkScraper;
        }


        public PriceWithLinkScraper runCeneo(int productID, string codeProduct, string productName)
        {
            PriceWithLinkScraper priceWithLinkScraper = new PriceWithLinkScraper();
            priceWithLinkScraper.Price = "";
            priceWithLinkScraper.Link = "";
            string python = @"C:\Users\Dawid\AppData\Local\Programs\Python\Python36-32\python.exe";
            string myPythonApp = @"C:\Users\Dawid\Documents\GitHub\SklepInternetowy\Scraper\ceneo.py";

            if (codeProduct[0] == ' ')                                      // usuniecie spacji na poczatku
                codeProduct = codeProduct.Substring(1);

            if (codeProduct[codeProduct.Length - 1] == ' ')                 // usuniecie spacji na koncu
                codeProduct = codeProduct.Substring(0, codeProduct.Length - 1);

            codeProduct = codeProduct.Replace(" ", "$#$#");
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.RedirectStandardOutput = true;
            myProcessStartInfo.Arguments = myPythonApp + " " + codeProduct + " " + productName;

            Process myProcess = new Process();
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.Start();

            char[] splitter = { '\n' };
            StreamReader myStreamReader = myProcess.StandardOutput;
            string[] myString = myStreamReader.ReadToEnd().Split(splitter);
            
            string[] spllit = new string[] { "&&&" };
            foreach (string s in myString)
            {
                if (!String.IsNullOrWhiteSpace(s) || s.Length > 2)
                {

                    string[] spli = s.Split(spllit, StringSplitOptions.None);
                    string[] splitLink = spli[0].Split(new[] { "link: " }, StringSplitOptions.None);
                    string[] splitPrice = spli[1].Split(new[] { "cena: " }, StringSplitOptions.None);

                    if (string.IsNullOrWhiteSpace(splitLink[1]))
                    {
                        continue;
                    }
                    priceWithLinkScraper.Price = splitPrice[1];
                    priceWithLinkScraper.Link = splitLink[1];
                }


            }
            myProcess.WaitForExit();
            myProcess.Close();
            return priceWithLinkScraper;
        }

        public PriceWithLinkScraper runCeneoChecked(int productID, string nameProduct, string linkProduct)
        {
            PriceWithLinkScraper priceWithLinkScraper = new PriceWithLinkScraper();
            priceWithLinkScraper.Price = "";
            priceWithLinkScraper.Link = "";
            string python = @"C:\Users\Dawid\AppData\Local\Programs\Python\Python36-32\python.exe";
            string myPythonApp = @"C:\Users\Dawid\Documents\GitHub\SklepInternetowy\Scraper\ceneoChecked.py";

            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo(python);
            myProcessStartInfo.UseShellExecute = false;
            myProcessStartInfo.RedirectStandardOutput = true;
            myProcessStartInfo.Arguments = myPythonApp + " " + nameProduct + " " + linkProduct;

            Process myProcess = new Process();
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.Start();
           
            char[] splitter = { '\n' };
            StreamReader myStreamReader = myProcess.StandardOutput;
            string[] myString = myStreamReader.ReadToEnd().Split(splitter);

            foreach (string s in myString)
            {
                if (!String.IsNullOrWhiteSpace(s) || s.Length > 2)
                {
                    string[] splitPrice = s.Split(new[] { "cena: " }, StringSplitOptions.None);
                    priceWithLinkScraper.Price = splitPrice[1];
                }

            }
            myProcess.WaitForExit();
            myProcess.Close();
            return priceWithLinkScraper;
        }
        public bool isOnePrice(string pricesDB, ref List<decimal> listPrices)  // sprawdzenie czy cena jest jedna, jezeli tak, zwrocenie jej wartosci
        {
            if (string.IsNullOrWhiteSpace(pricesDB))
            {
                listPrices.Add(999999);
                return false;
            }

            string[] split = new string[] { "$#$#" };
            pricesDB += "byleco";

            string[] splitPrices = pricesDB.Split(split, StringSplitOptions.None);
            if (splitPrices.Length > 2)
            {
                listPrices.Add(999999);
                return false;
            }
            else if (splitPrices.Length < 2)
            {
                listPrices.Add(999999);
                return true;
            }
            else
            {
                listPrices.Add(decimal.Parse(splitPrices[0]));
                return true;
            }
        }


        public void countDifferencePrice()
        {

            using (sklepEntities db = new sklepEntities())
            {
                var productsScraped = db.Scrapers;
                foreach (Scrapers item in productsScraped)
                {
                    Products product = db.Products.Where(x => x.ProductID == item.ProductID).SingleOrDefault();

                    decimal priceProduct = (1 + product.Vat) * product.PriceN;

                    List<decimal> listPrices = new List<decimal>();

                    if (isOnePrice(item.XKomPrice, ref listPrices) && isOnePrice(item.VobisPrice, ref listPrices)
                        && isOnePrice(item.AlsenPrice, ref listPrices) && isOnePrice(item.CeneoPrice, ref listPrices))
                    {
                        decimal minPrice = listPrices.Min();
                        decimal differencePrice = priceProduct - minPrice;
                        item.DifferencePrice = differencePrice;
                    }
                    else
                    {
                        item.DifferencePrice = null;
                    }


                }

                db.SaveChanges();
            }
        }

        public void saveData(int productID, string xkomPrice, string xkomLink, string vobisPrice, string vobisLink, string alsenPrice, string alsenLink, string ceneoPrice, string ceneoLink, decimal differencePrice)
        {

            using (sklepEntities db = new sklepEntities())
            {
                bool isExistProduct = db.Scrapers.Where(x => x.ProductID == productID).Any();
                if (isExistProduct)
                {
                    Scrapers productScrap = db.Scrapers.Where(x => x.ProductID == productID).Single();
                    productScrap.XKomPrice = xkomPrice;
                    productScrap.XKomLink = xkomLink;
                    productScrap.VobisPrice = vobisPrice;
                    productScrap.VobistLink = vobisLink;
                    productScrap.AlsenPrice = alsenPrice;
                    productScrap.AlsentLink = alsenLink;
                    productScrap.CeneoPrice = ceneoPrice;
                    productScrap.CeneoLink = ceneoLink;
                    productScrap.DateLastUpdate = DateTime.Now;
                    if (differencePrice == -9999999)
                        productScrap.DifferencePrice = null;
                    else
                        productScrap.DifferencePrice = differencePrice;
                    db.SaveChanges();
                }
                else
                {
                    Scrapers productScrap = new Scrapers();
                    productScrap.XKomPrice = xkomPrice;
                    productScrap.XKomLink = xkomLink;
                    productScrap.VobisPrice = vobisPrice;
                    productScrap.VobistLink = vobisLink;
                    productScrap.AlsenPrice = alsenPrice;
                    productScrap.AlsentLink = alsenLink;
                    productScrap.CeneoPrice = ceneoPrice;
                    productScrap.CeneoLink = ceneoLink;
                    productScrap.ProductID = productID;
                    productScrap.DateLastUpdate = DateTime.Now;
                    if (differencePrice == -9999999)
                        productScrap.DifferencePrice = null;
                    else
                        productScrap.DifferencePrice = differencePrice;
                    db.Scrapers.Add(productScrap);
                    db.SaveChanges();
                }
            }
        }
        public static decimal priceDecimalFromString(string price)
        {

            string[] split = new string[] { "$#$#" };
            string[] splitPrices = price.Split(split, StringSplitOptions.None);
            if (splitPrices.Length == 2)
            {
                return decimal.Parse(splitPrices[0]);
            }
            return 9999999;
        }
        public decimal countDifferece(List<string> prices, decimal priceProduct)
        {
            List<decimal> decimalPrices = new List<decimal>();
            foreach (string item in prices)
            {
                decimal tempPrice = priceDecimalFromString(item);
                decimalPrices.Add(tempPrice);
            }
            if (decimalPrices.Count < 1)
                return -1;
            decimal minPrice = decimalPrices.Min();
            if (minPrice == 9999999)
                return -1;
            else
                return priceProduct - minPrice;
        }
    }
}

