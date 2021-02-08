using iTextSharp.text;
using iTextSharp.text.pdf;
using Sklep.Models.db;
using Sklep.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Sklep.Models.EntityManager
{
    public class OrderManager
    {
        private static string GetShortDescriptionOrder(int orderID)                                  // uzyskanie skroconego opisu zamowienia
        {
            string description = "";
            using (sklepEntities db = new sklepEntities())
            {
                var productsID = db.ProductsOfOrders.Where(x => x.NumOfOrderID == orderID).Select(x => x.ProductID);
                int counter = 0;
                foreach(int item in productsID)
                {
                    if (counter != 0)
                        description = description + "/";
                    string nameProduct = db.Products.Where(x => x.ProductID == item).Select(x => x.Name).Single();
                    description = description + nameProduct;
                    counter++;
                }
            }
            if (description.Length > 30)
            {
                description = string.Concat(description.Take(30)) + "...";
            }
            else
            {
                
                while (description.Length < 30)
                    description += " ";
            }
            return description;
        }
        public static List<OrderActualView> GetActualOrders(string login)
        {
            List<OrderActualView> ordersList = new List<OrderActualView>();
            using (sklepEntities db = new sklepEntities())
            {
                int userID = db.Users.Where(x => x.UserName == login).Select(x => x.UserID).Single();
                var orders = db.Orders.Where(x => x.UserID == userID && x.Status != "Zakończono");
                foreach (Orders item in orders)
                {
                    OrderActualView order = new OrderActualView(item.OrderID, GetShortDescriptionOrder(item.OrderID), ((decimal)item.PriceB).ToString("C2"), item.Status);
                    ordersList.Add(order);
                }
            }
            return ordersList; 
        }

        public static List<OrderProductView> GetOrderProducts(int orderID)  
        {
            List<OrderProductView> orderProductList = new List<OrderProductView>();
            using (sklepEntities db = new sklepEntities())
            {
                var products = db.ProductsOfOrders.Where(x => x.NumOfOrderID == orderID);
                foreach(ProductsOfOrders item in products)
                {
                    Products pr = db.Products.Where(x => x.ProductID == item.ProductID).Single();
                    string ImagePath = db.Images.Where(x => x.ProductID == item.ProductID).Select(x => x.Path).First();
                    OrderProductView product = new OrderProductView(item.ProductID, pr.Name, item.PriceN.ToString("C2"), ((1 + item.Vat) * item.PriceN).ToString("C2"), ((decimal)(((1M + item.Vat) * item.PriceN) * item.Quantity)).ToString("C2"), (int)item.Quantity, ImagePath);
                    orderProductList.Add(product);
                }
            }
            return orderProductList;
        }

        public static OrderDetailsView GetOrderDetails(string login, int orderID)
        {
            OrderDetailsView orderDetails = new OrderDetailsView();
            orderDetails.orderProductList = GetOrderProducts(orderID);
            using (sklepEntities db = new sklepEntities())
            {
                Users user = db.Users.Where(x => x.UserName == login).Single();
                orderDetails.OrderID = orderID;

                Orders order = db.Orders.Where(x => x.OrderID == orderID).Single();
                if (order.UserID != user.UserID)                                             // gdy uzytkonik chce wyswietlic czyjes zamowienie
                    return null;
                orderDetails.TotalOrderPriceN = ((decimal)order.PriceN).ToString("C2");
                orderDetails.TotalOrderPriceB = ((decimal)order.PriceB).ToString("C2");
                orderDetails.Status = order.Status;
                orderDetails.NameDelivery = order.NameDelivery;
                orderDetails.DeliveryPriceB = ((decimal)((1M + order.VATDelivery) * order.PriceNDelivery)).ToString("C2");
                if (order.LastTimeModified == null)
                    orderDetails.lastTimeModified = "Brak";
                else
                    orderDetails.lastTimeModified = "Ostatnia aktualizacja: " + ((DateTime)order.LastTimeModified).ToString();
                orderDetails.OrderPath = order.OrderPath;
                if (order.InvoicePath == null)
                    orderDetails.InvoicePath = "0";                        // gdy brak faktury
                else
                    orderDetails.InvoicePath = order.InvoicePath;
                orderDetails.Street = order.DeliverStreet;
                orderDetails.NumOfHouse = order.DeliverNumOfHouse;
                orderDetails.PostCode = order.DeliverPostCode;
                orderDetails.City = order.DeliverCity;
            }
            return orderDetails;
        }

        public static List<OrderEndView> GetHistoryOrders(string login)
        {
            List<OrderEndView> orderShortList = new List<OrderEndView>();
            using (sklepEntities db = new sklepEntities())
            {
                int userID = db.Users.Where(x => x.UserName == login).Select(x => x.UserID).Single();
                var orders = db.Orders.Where(x => x.UserID == userID && x.Status == "Zakończono");
                foreach (Orders item in orders)
                {
                    OrderEndView order = new OrderEndView(item.OrderID, GetShortDescriptionOrder(item.OrderID), ((decimal)item.PriceB).ToString("C2"), item.Date);
                    orderShortList.Add(order);
                }
            }
            return orderShortList;
        }

        public static bool SubmitOrder(string [] productsID, string [] quantity, string login, out string productName, out int productQuantity, out int orderID) // zlozenie zamowienia
        {
            using (sklepEntities db = new sklepEntities())
            {
                List<Products> listProducts = new List<Products>();
                for (int i = 0; i < productsID.Length; i++)
                {
                    int productID = Int32.Parse(productsID[i]);
                    Products product = db.Products.Where(x => x.ProductID == productID).Single();
                    if (product.Quantity < Int32.Parse(quantity[i]) || Int32.Parse(quantity[i]) <= 0)     // sprawdzenie czy liczba wybranego produktu jest mniejsza niz w bazie danych
                    {
                      
                        productName = product.Name;
                        productQuantity = product.Quantity;
                        orderID = 0;
                        return false;//RedirectToAction("Index", "Koszyk");
                    }
                }
                DateTime date = DateTime.Now;
                Orders order = new Orders();
                int userID = db.Users.Where(x => x.UserName == login).Select(x => x.UserID).Single();
                order.UserID = userID;
                order.Date = date;
                order.Status = "Utworzono";
                db.Orders.Add(order); // utworzenie wstepnego zamowienia
                db.SaveChanges();

                order = (from or in db.Orders   // pobranie utworzonego zamowienia, w celu uzyskania OrderID
                         where
                         or.Date.Day == date.Day &&
                         or.Date.Month == date.Month &&
                         or.Date.Year == date.Year &&
                         or.Date.Hour == date.Hour &&
                         or.Date.Minute == date.Minute &&
                         or.Date.Second == date.Second
                         select or).Single();

                decimal priceN = 0M;
                decimal priceB = 0M;
                for (int i = 0; i < productsID.Length; i++)   // kazdy produkt dodawany jest do tabeli ProductsOfOrders
                {
                    int productID = Int32.Parse(productsID[i]);
                    Products product = db.Products.Where(x => x.ProductID == productID).Single();
                    ProductsOfOrders productOfOrder = new ProductsOfOrders();
                    productOfOrder.NumOfOrderID = order.OrderID;
                    productOfOrder.ProductID = productID;
                    productOfOrder.PriceN = product.PriceN;
                    productOfOrder.Vat = product.Vat;
                    productOfOrder.Quantity = Int32.Parse(quantity[i]);
                    db.ProductsOfOrders.Add(productOfOrder);
                    priceN = priceN + product.PriceN * Int32.Parse(quantity[i]);
                    priceB = priceB + (1M + product.Vat) * product.PriceN * Int32.Parse(quantity[i]);
                    product.Quantity -= Int32.Parse(quantity[i]); // zmiejszana jest ilosc dostepnych sztuk danego produktu
                }

                order.PriceN = priceN;
                order.PriceB = priceB;
                db.SaveChanges();
                productName = "";
                productQuantity = 0;
                orderID = order.OrderID;
                return true;
            }
        }
        public static List<SelectListItem> GetDeliveriesList()
        {
            using (sklepEntities db = new sklepEntities())
            {
                List<SelectListItem> deliveriesList = new List<SelectListItem>();
                var deliveries = db.Deliveries;
                foreach (Deliveries item in deliveries)  // wpisanie danych zwiazanych z dostawa do listy
                {
                    string name = item.Name;
                    string priceB = ((1M + item.Vat) * item.PriceN).ToString("C2");
                    deliveriesList.Add(new SelectListItem { Text = name + " " + priceB, Value = item.DeliveryID.ToString() });
                }
                return deliveriesList;
            }
        }
        public static SummaryOrderViewModel GetOrderToSummary(DeliveryAddressViewModel deliveryAddressViewModel, int deliveryID, string login, out int status)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Deliveries delivery = db.Deliveries.Where(x => x.DeliveryID == deliveryID).Single();
                Users user = db.Users.Where(x => x.UserName == login).Single();
                int orderID = db.Orders.Where(x => x.UserID == user.UserID)
                                       .OrderByDescending(x => x.OrderID)
                                       .Select(x => x.OrderID)
                                       .First();

                Orders order = db.Orders.Where(x => x.OrderID == orderID).Single();
                var productsOfOrder = db.ProductsOfOrders.Where(x => x.NumOfOrderID == orderID);
                order.PriceN = order.PriceN + delivery.PriceN;
                order.PriceB = order.PriceB + (1M + delivery.Vat) * delivery.PriceN;
                order.NameDelivery = delivery.Name;
                order.PriceNDelivery = delivery.PriceN;
                order.VATDelivery = delivery.Vat;
                order.DeliverCity = deliveryAddressViewModel.City;
                order.DeliverNumOfHouse = deliveryAddressViewModel.NumOfHouse;
                order.DeliverPostCode = deliveryAddressViewModel.PostCode;
                order.DeliverStreet = deliveryAddressViewModel.Street;
                order.Name = deliveryAddressViewModel.Name;
                order.Surname = deliveryAddressViewModel.Surname;
               

                SummaryOrderViewModel orderDetails = new SummaryOrderViewModel();
                orderDetails.orderProductList = new List<OrderProductView>();
                List<ShoppingCartView> productsList = new List<ShoppingCartView>();
                foreach (ProductsOfOrders item in productsOfOrder)
                {
                    OrderProductView orderProductView = new OrderProductView();
                    orderProductView.ProductID = item.ProductID;
                    orderProductView.ProductName = db.Products.Where(x => x.ProductID == item.ProductID).Select(x => x.Name).Single();
                    orderProductView.ProductPriceN = item.PriceN.ToString("C2");
                    orderProductView.ProductPriceB = ((1M + item.Vat) * item.PriceN).ToString("C2");             // ToString("C2") oznacza - ustawienie zapisania ceny w odpowiedni sposob
                    orderProductView.QuantityChoose = (int)item.Quantity;
                    orderProductView.TotalProductPriceB = ((1M + item.Vat) * item.PriceN * (int)item.Quantity).ToString("C2");
          
                    var productWithImage = from pro in db.Products                        // znalezienie zdjec danego produktu w bazie danych
                                           where pro.ProductID == item.ProductID
                                           join img in db.Images
                                           on pro.ProductID equals img.ProductID
                                           select new { pro.ProductID, img.Path };
                    var image = productWithImage.GroupBy(p => p.ProductID).Select(s => s.FirstOrDefault()).Single();      // wybranie pierwszego zdjecia produktu 
                   
                    orderProductView.ImagePath = image.Path;
                    orderDetails.orderProductList.Add(orderProductView);
                }
                orderDetails.TotalOrderPriceN = ((decimal)order.PriceN).ToString("C2");
                orderDetails.TotalOrderPriceB = ((decimal)order.PriceB).ToString("C2");
                orderDetails.NameDelivery = delivery.Name;
                orderDetails.DeliveryPriceB = ((1M + delivery.Vat) * delivery.PriceN).ToString("C2");
                orderDetails.Name = order.Name;
                orderDetails.Surname = order.Surname;
                orderDetails.Street = order.DeliverStreet;
                orderDetails.NumOfHouse = order.DeliverNumOfHouse;
                orderDetails.PostCode = order.DeliverPostCode;
                orderDetails.City = order.DeliverCity;
                orderDetails.OrderID = order.OrderID;
                if (delivery.DeliveryID <= 1)                                                        // czy przedplata
                {
                    order.Status = "Oczekiwanie na wpłatę";
                    db.SaveChanges();
                    status = 1;
                    GenereateOrderPdf(orderID);
                    return orderDetails;
                }
                else                                                                                                    // czy za pobraniem
                {
                    order.Status = "Oczekiwanie na wysyłkę";
                    db.SaveChanges();
                    status = 0;
                    GenereateOrderPdf(orderID);
                    return orderDetails;
                }
            }
        }

        private static void GenereateOrderPdf(int orderID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Orders order = db.Orders.Where(x => x.OrderID == orderID).Single();
                Users user = db.Users.Where(x => x.UserID == order.UserID).Single();
                var products = db.ProductsOfOrders.Where(x => x.NumOfOrderID == orderID);

                FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Zamowienia\") + "Z_" + orderID + "_" + user.UserName + ".pdf", FileMode.Create, FileAccess.Write, FileShare.None);
                Document doc = new Document();
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();
                var bigFont = FontFactory.GetFont(BaseFont.COURIER, BaseFont.CP1257, 18, Font.BOLD);
                Font header = new Font(Font.FontFamily.TIMES_ROMAN, 15f, Font.BOLD, BaseColor.BLACK);

           /*     string ARIALUNI_TFF = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ARIALUNI.TTF"); // polskie znaki
                BaseFont bf = BaseFont.CreateFont(ARIALUNI_TFF, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                Font f = new Font(bf, 12, Font.NORMAL); // czcionka z polskimi znakami
                Font forTable = new Font(bf, 10, Font.NORMAL); */

                Paragraph para = new Paragraph("Sklep internetowy\nZamówienie nr(" + orderID + ")\n" + order.Date, header);
                para.Alignment = Element.ALIGN_CENTER;
                doc.Add(para);
                Phrase phraseShop = new Phrase("Dane sklepu\nTelefon: 601 601 601\nAdres e-mail: sklep@sklep.pl\nAdres: ul. Krakowska 30\nKod pocztowy: 33-100\nTarnów\n\n\n");
                doc.Add(phraseShop);
                Phrase phraseCustomer = new Phrase("Dane klienta\nImię: " + user.Name + "\nNazwisko: " + user.Surname +
                    "\nAdres e-mail: " + user.Email + "\nUlica: " + user.Street + "\nNumer domu\\lokalu: " + user.NumOfHouse +
                    "\nKod pocztowy: " + user.PostCode + "\nMiasto: " + user.City + "\n");
                doc.Add(phraseCustomer);
                PdfPTable table = new PdfPTable(6);

                table.AddCell("Lp.");
                table.AddCell("Nazwa");
                table.AddCell("Cena (netto)");
                table.AddCell("Cena (brutto)");
                table.AddCell("Liczba");
                table.AddCell("Razem");
                int licznik = 1;
                foreach (ProductsOfOrders item in products)
                {
                    Phrase phraseLicznik = new Phrase();
                    phraseLicznik.Add(
                        new Chunk(licznik.ToString())
                    );
                    table.AddCell(phraseLicznik);
                    licznik++;

                    string productName = db.Products.Where(x => x.ProductID == item.ProductID).Select(x => x.Name).Single();
                    Phrase phraseName = new Phrase();
                    phraseName.Add(
                        new Chunk(productName)
                    );
                    table.AddCell(phraseName);

                    Phrase phrase = new Phrase();
                    phrase.Add(
                        new Chunk(item.PriceN.ToString("C2"))
                    );
                    table.AddCell(phrase);

                    Phrase phrase1 = new Phrase();
                    phrase1.Add(
                        new Chunk(((1M + item.Vat) * item.PriceN).ToString("C2"))
                    );
                    table.AddCell(phrase1);
                    Phrase phraseQuantity = new Phrase();
                    phraseQuantity.Add(
                        new Chunk(item.Quantity.ToString())
                    );
                    table.AddCell(phraseQuantity);

                    Phrase phrase2 = new Phrase();
                    phrase2.Add(
                        new Chunk(((1M + item.Vat) * item.PriceN * (int)item.Quantity).ToString("C2"))
                    );
                    table.AddCell(phrase2);

                }
                doc.Add(table);

                Phrase deliver = new Phrase("\nWybrana dostawa: " + order.NameDelivery + "\nKoszt dostawy (netto): " + ((decimal)order.PriceNDelivery).ToString("C2") +
                    "\nStawka VAT dostawy: " + order.VATDelivery + "\nKoszt dostawy (brutto): " + ((decimal)((1M + order.VATDelivery) * order.PriceNDelivery)).ToString("C2"));
                doc.Add(deliver);

                if (string.IsNullOrEmpty(order.NamePayment))
                {

                }
                else
                {
                    Phrase payment = new Phrase("\nWybrana płatność: " + order.NamePayment + "\nKoszt płatności (netto): " + ((decimal)order.PriceNPayment).ToString("C2") +
                        "\nStawka VAT płatności: " + order.VatPayment + "\nKoszt płatności (brutto): " + ((decimal)((1M + order.VatPayment) * order.PriceNPayment)).ToString("C2"));
                    doc.Add(payment);
                }

                Phrase sumOfOrder = new Phrase("\n\n\nŁącznie (netto): " + ((decimal)order.PriceN).ToString("C2") + "\nŁącznie (brutto): " + ((decimal)order.PriceB).ToString("C2"));
                doc.Add(sumOfOrder);
                doc.Close();

            }
        }



        public void GenereateInvoicePdf(int orderID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Orders order = db.Orders.Where(x => x.OrderID == orderID).Single();
                Users user = db.Users.Where(x => x.UserID == order.UserID).Single();
                var products = db.ProductsOfOrders.Where(x => x.NumOfOrderID == orderID);

                FileStream fs = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Faktury\") + "FV_" + orderID + "_" + user.UserName + ".pdf", FileMode.Create, FileAccess.Write, FileShare.None);
                Document doc = new Document();
                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();
                var bigFont = FontFactory.GetFont(BaseFont.COURIER, BaseFont.CP1257, 18, Font.BOLD);
                Font header = new Font(Font.FontFamily.TIMES_ROMAN, 15f, Font.BOLD, BaseColor.BLACK);

                string ARIALUNI_TFF = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ARIALUNI.TTF"); // polskie znaki
                BaseFont bf = BaseFont.CreateFont(ARIALUNI_TFF, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                Font f = new Font(bf, 12, Font.NORMAL); // czcionka z polskimi znakami
                Font forTable = new Font(bf, 10, Font.NORMAL);

                Paragraph para = new Paragraph("Sklep internetowy\nFaktura VAT Nr: " + orderID + "\n Data wystawienia " + order.LastTimeModified, header);
                para.Alignment = Element.ALIGN_CENTER;
                doc.Add(para);
                Phrase phraseShop = new Phrase("Sprzedawca:\nTelefon: 601 601 601\nAdres e-mail: sklep@sklep.pl\nAdres: ul. Krakowska 30\nKod pocztowy: 33-100\nTarnów\n\n\n", f);
                doc.Add(phraseShop);
                Phrase phraseCustomer = new Phrase("Nabywca:\nImię: " + user.Name + "\nNazwisko: " + user.Surname +
                    "\nAdres e-mail: " + user.Email + "\nUlica: " + user.Street + "\nNumer domu\\lokalu: " + user.NumOfHouse +
                    "\nKod pocztowy: " + user.PostCode + "\nMiasto: " + user.City + "\n", f);
                doc.Add(phraseCustomer);
                Phrase dateOrder = new Phrase("Data wykonania płatności: " + order.Date + "\n", f);
                doc.Add(dateOrder);
            
                PdfPTable table = new PdfPTable(8);

                table.AddCell("Lp.");
                table.AddCell("Nazwa");
                table.AddCell("Cena (netto)");
                table.AddCell("VAT %");
                table.AddCell("Kwota VAT");
                table.AddCell("Cena (brutto)");
                table.AddCell("Liczba");
                table.AddCell("Razem (brutto)");
                int licznik = 1;
                foreach (ProductsOfOrders item in products)
                {
                    Phrase phraseLicznik = new Phrase();
                    phraseLicznik.Add(
                        new Chunk(licznik.ToString(), forTable)
                    );
                    table.AddCell(phraseLicznik);
                    licznik++;

                    string productName = db.Products.Where(x => x.ProductID == item.ProductID).Select(x => x.Name).Single();
                    Phrase phraseName = new Phrase();
                    phraseName.Add(
                        new Chunk(productName, forTable)
                    );
                    table.AddCell(phraseName);

                    Phrase phrase = new Phrase();
                    phrase.Add(
                        new Chunk(item.PriceN.ToString("C2"), forTable)
                    );
                    table.AddCell(phrase);
                    Phrase VAT = new Phrase();
                    VAT.Add(
                        new Chunk((item.Vat * 100).ToString() + "%", forTable)
                    );
                    table.AddCell(VAT);
                    Phrase VATPrice = new Phrase();
                    VATPrice.Add(
                        new Chunk((item.Vat*item.PriceN).ToString("C2"), forTable)
                    );
                    table.AddCell(VATPrice);


                    Phrase phrase1 = new Phrase();
                    phrase1.Add(
                        new Chunk(((1M + item.Vat) * item.PriceN).ToString("C2"), forTable)
                    );
                    table.AddCell(phrase1);
                    Phrase phraseQuantity = new Phrase();
                    phraseQuantity.Add(
                        new Chunk(item.Quantity.ToString(), forTable)
                    );
                    table.AddCell(phraseQuantity);

                    Phrase phrase2 = new Phrase();
                    phrase2.Add(
                        new Chunk(((1M + item.Vat) * item.PriceN * (int)item.Quantity).ToString("C2"), forTable)
                    );
                    table.AddCell(phrase2);

                }
                doc.Add(table);

               

                if (string.IsNullOrEmpty(order.NamePayment))
                {

                }
                else
                {
                    Phrase payment = new Phrase("\nWybrana płatność: " + order.NamePayment + "\nKoszt płatności (netto): " + ((decimal)order.PriceNPayment).ToString("C2") +
                        "\nStawka VAT płatności: " + order.VatPayment + "\nKoszt płatności (brutto): " + ((decimal)((1M + order.VatPayment) * order.PriceNPayment)).ToString("C2"), f);
                    doc.Add(payment);
                }

                Phrase sumOfOrder = new Phrase("\n\n\nŁącznie (netto): " + ((decimal)order.PriceN).ToString("C2") + "\nŁącznie (brutto): " + ((decimal)order.PriceB).ToString("C2"), f);
                doc.Add(sumOfOrder);
                doc.Close();

            }
        }

        public static PayUForm GetPayUFrom(OrderDetailsView orderDetails)
        {
            PayUForm payUForm = new PayUForm();
            payUForm.listProduct = new List<OrderProductView>(orderDetails.orderProductList);
            OrderProductView ord = new OrderProductView();
            ord.ProductName = orderDetails.NameDelivery;
            ord.ProductPriceB = orderDetails.DeliveryPriceB;
            ord.QuantityChoose = 1;
            payUForm.listProduct.Add(ord);
            Dictionary<string, string> dictionaryPayU = new Dictionary<string, string>();
            dictionaryPayU.Add("customerIp", "127.0.0.1");
            dictionaryPayU.Add("merchantPosId", "145227");
            dictionaryPayU.Add("description", "Sklep internetowy");
            String totalPrice = orderDetails.TotalOrderPriceB.Replace(" ", "").Replace("zł", "");
            Decimal tPrice = decimal.Parse(totalPrice);
            tPrice = tPrice * 100;
            dictionaryPayU.Add("totalAmount", ((int)tPrice).ToString());
            dictionaryPayU.Add("currencyCode", "PLN");
            dictionaryPayU.Add("notifyUrl", "http://localhost:44300/Home/testPay");
            dictionaryPayU.Add("continueUrl", "http://localhost:44300/Zamowienia/PayUSuccess"); // po udanej transakcji
            for (int i = 0; i < payUForm.listProduct.Count; i++)
            {
                dictionaryPayU.Add("products[" + i + "].name", payUForm.listProduct[i].ProductName);
                String itemPrice = (payUForm.listProduct[i].ProductPriceB).Replace(" ", "").Replace("zł", "");
                Decimal itPrice = decimal.Parse(itemPrice);
                itPrice = itPrice * 100;
                dictionaryPayU.Add("products[" + i + "].unitPrice", ((int)itPrice).ToString());
                dictionaryPayU.Add("products[" + i + "].quantity", payUForm.listProduct[i].QuantityChoose.ToString());
            }

            dictionaryPayU = dictionaryPayU.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            string content = "";
            foreach (var item in dictionaryPayU)
            {
                content = content + item.Key + "=" + Uri.EscapeDataString(item.Value).Replace("%20", "+") + "&";

            }

            content = content + "13a980d4f851f3d9a1cfc792fb1f5e50";
            string hash = sha256(content);
            dictionaryPayU.Add("Signature", "sender=145227;algorithm=SHA-256;signature=" + hash);
            payUForm.PayUConfiguration = dictionaryPayU;
            return payUForm;
        }

        static string sha256(string randomString)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString), 0, Encoding.UTF8.GetByteCount(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}