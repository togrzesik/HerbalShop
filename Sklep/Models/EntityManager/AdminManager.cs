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
    public class AdminManager
    {
        public static int GetNumberOfClients()                // uzyskanie liczby klientow z bazy danych
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Users.Count();
            }
        }

        public static int GetNumberOfAllOrders()                // uzyskanie liczby wszystkich zamowien z bazy danych
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Orders.Count();
            }
        }

        public static int GetNumberOfOrdersToRealize()                // uzyskanie liczby zamowien do realizacji z bazy danych
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Orders.Where(x => x.Status != "Zakończono").Count();
            }
        }

        public static int GetNumberOfProducts()                // uzyskanie liczby produktow z bazy danych
        {
            using (sklepEntities db = new sklepEntities())
            {
                return db.Products.Count();
            }
        }

        public static List<TheNewestOrdersView> GetTheNewestOrders()   // uzyskanie najnowszych zamowien
        {
            List<TheNewestOrdersView> ordersList = new List<TheNewestOrdersView>();
            using (sklepEntities db = new sklepEntities())
            {
                var orders = db.Orders.OrderByDescending(x => x.OrderID).Take(5);
                foreach (var item in orders)
                {
                    
                        TheNewestOrdersView newestOrder = new TheNewestOrdersView();
                        newestOrder.OrderID = item.OrderID;
                        Users user = db.Users.Where(x => x.UserID == item.UserID).Single();
                        newestOrder.UserName = user.UserName;
                        newestOrder.PriceB = ((decimal)item.PriceB).ToString("C2");
                        newestOrder.status = item.Status;
                        ordersList.Add(newestOrder);
                }
            }
            return ordersList;
        }

        public static AdminDetailsOrder GetDetailsOrder(int orderID)     // uzyskanie szczegolow zamowienia
        {
            using (sklepEntities db = new sklepEntities())
            {
                AdminDetailsOrder detailsOrder = new AdminDetailsOrder();
                Orders order = db.Orders.Where(x => x.OrderID == orderID).Single();
                Users client = db.Users.Where(x => x.UserID == order.UserID).Single();
                var products = db.ProductsOfOrders.Where(x => x.NumOfOrderID == orderID);
                Users whoModified = new Users();
                if (order.WhoModified != null)
                    whoModified = db.Users.Where(x => x.UserID == order.WhoModified).Single();

                List<AdminDetailsOrderListProducts> productsList = new List<AdminDetailsOrderListProducts>();
                foreach (ProductsOfOrders item in products)
                {
                    string imagePath = db.Images.Where(x => x.ProductID == item.ProductID).Select(x => x.Path).First();
                    string productName = db.Products.Where(x => x.ProductID == item.ProductID).Select(x => x.Name).Single();
                    AdminDetailsOrderListProducts prod = new AdminDetailsOrderListProducts(item.ProductID, productName, item.PriceN.ToString("C2"), item.Vat.ToString(),
                        ((1 + item.Vat) * item.PriceN).ToString("C2"), ((decimal)((1 + item.Vat) * item.PriceN * item.Quantity)).ToString("C2"),
                        (int)item.Quantity, imagePath);
                    productsList.Add(prod);
                }

                detailsOrder.OrderID = orderID;
                detailsOrder.ClientName = client.Name;
                detailsOrder.ClientSurname = client.Surname;
                detailsOrder.OrderDate = order.Date;
                detailsOrder.InvoicePath = String.IsNullOrEmpty(order.InvoicePath) ? "Brak" : order.InvoicePath;
                detailsOrder.OrderPath = String.IsNullOrEmpty(order.OrderPath) ? "Brak" : order.OrderPath;
                detailsOrder.PriceN = ((decimal)order.PriceN).ToString("C2");
                detailsOrder.PriceB = ((decimal)order.PriceB).ToString("C2");
                if (String.IsNullOrEmpty(whoModified.UserName))
                    detailsOrder.WhoModified = "Brak aktualizacji";
                else
                    detailsOrder.WhoModified = whoModified.UserName + " " + whoModified.Surname;
                detailsOrder.LastTimeModified = String.IsNullOrEmpty(order.LastTimeModified.ToString()) ? "Brak aktualizacji" : order.LastTimeModified.ToString();
                detailsOrder.DeliverStreet = order.DeliverStreet;
                detailsOrder.DeliverNumOfHouse = order.DeliverNumOfHouse;
                detailsOrder.DeliverPostCode = order.DeliverPostCode;
                detailsOrder.DeliverCity = order.DeliverCity;
                detailsOrder.NameDelivery = order.NameDelivery;
                detailsOrder.PriceNDelivery = ((decimal)order.PriceNDelivery).ToString("C2");
                detailsOrder.PriceBDelivery = ((decimal)((1 + order.VATDelivery) * order.PriceNDelivery)).ToString("C2");
                detailsOrder.VatDelivery = order.VATDelivery.ToString();
                if (String.IsNullOrEmpty(order.NamePayment))
                {
                    detailsOrder.NamePayment = "Nie wybrano";
                    detailsOrder.PriceNPayment = "Brak";
                    detailsOrder.PriceBPayment = "Brak";
                    detailsOrder.VatPayment = "Brak";
                }
                else
                {
                    detailsOrder.NamePayment = order.NamePayment;
                    detailsOrder.PriceNPayment = ((decimal)(order.PriceNPayment)).ToString("C2");
                    detailsOrder.PriceBPayment = ((decimal)((1 + order.VatPayment) * order.PriceNPayment)).ToString("C2");
                    detailsOrder.VatPayment = order.VatPayment.ToString();
                }
                detailsOrder.Status = order.Status;
                detailsOrder.ProductsList = productsList;

                List<SelectListItem> statusList = new List<SelectListItem>();
                var statuses = db.Statuses;
                foreach (Statuses item in statuses)
                {
                    if (item.Name == order.Status)
                    {

                        statusList.Add(new SelectListItem { Text = item.Name, Value = item.StatusID.ToString(), Selected = true });
                    }
                    else
                        statusList.Add(new SelectListItem { Text = item.Name, Value = item.StatusID.ToString(), Selected = false });
                }
                detailsOrder.statusList = statusList;
                return detailsOrder;
            }
        }
        public static bool EditStatusOrder(AdminDetailsOrder order, int statusID, string WhoMidified)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Orders orderDB = db.Orders.Where(x => x.OrderID == order.OrderID).Single();
                Statuses status = db.Statuses.Where(x => x.StatusID == statusID).Single();
                orderDB.Status = status.Name;
            //    int userID = db.Users.Where(x => x.Name == WhoMidified).Select(x => x.UserID).Single();
            //    orderDB.WhoModified = userID;
            //    orderDB.LastTimeModified = DateTime.Now;
                db.SaveChanges();
                if (status.Name == "Zakończono")
                {
                    OrderManager orderManager = new OrderManager();
                    orderManager.GenereateInvoicePdf(order.OrderID);
                }
            }
            
            
            return true;
        }

        public static List<TheNewestOrdersView> GetAllOrders()
        {
            List<TheNewestOrdersView> orderList = new List<TheNewestOrdersView>();
            using (sklepEntities db = new sklepEntities())
            {
                var orders = db.Orders;
                foreach (Orders item in orders)
                {
                    
                        TheNewestOrdersView ord = new TheNewestOrdersView();
                        ord.OrderID = item.OrderID;
                        System.Diagnostics.Debug.WriteLine("id order " + item.OrderID + " price " + item.PriceB);
                        ord.PriceB = ((decimal)item.PriceB).ToString("C2");
                        ord.status = item.Status;
                        ord.DateOrder = item.Date;
                        Users user = db.Users.Where(x => x.UserID == item.UserID).Single();
                        ord.UserName = user.UserName;
                        orderList.Add(ord);
                }
            }
            return orderList;
        }

        public static List<TheNewestOrdersView> GetOrdersToRealize()
        {
            List<TheNewestOrdersView> orderList = new List<TheNewestOrdersView>();
            using (sklepEntities db = new sklepEntities())
            {
                var orders = db.Orders.Where(x => x.Status != "Zakończono");
                foreach (Orders item in orders)
                {
                    if (item.Status != "Utworzono")
                    {
                        TheNewestOrdersView ord = new TheNewestOrdersView();
                        ord.OrderID = item.OrderID;
                        ord.PriceB = ((decimal)item.PriceB).ToString("C2");
                        ord.status = item.Status;
                        ord.DateOrder = item.Date;
                        Users user = db.Users.Where(x => x.UserID == item.UserID).Single();
                        ord.UserName = user.UserName;
                        orderList.Add(ord);
                    }
                }
            }
            return orderList;
        }

        public static List<DeliveriesViewModel> GetDeliveries()
        {
            List<DeliveriesViewModel> deliveriesList = new List<DeliveriesViewModel>();
            using (sklepEntities db = new sklepEntities())
            {
                var deliveries = db.Deliveries;

                foreach (Deliveries item in deliveries)
                {
                    DeliveriesViewModel deliver = new DeliveriesViewModel(
                        item.DeliveryID,
                        item.Name,
                        item.PriceN,
                        item.Vat,
                        decimal.Round(((1 + item.Vat) * item.PriceN), 2, MidpointRounding.AwayFromZero)
                        );
                    deliveriesList.Add(deliver);
                }
            }
            return deliveriesList;
        }
        public static DeliveriesViewModel GetDetailsDelivery(int deliveryID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Deliveries deliveryDB = db.Deliveries.Where(x => x.DeliveryID == deliveryID).Single();
                DeliveriesViewModel delivery = new DeliveriesViewModel(
                    deliveryDB.DeliveryID,
                    deliveryDB.Name,
                    deliveryDB.PriceN,
                    deliveryDB.Vat,
                    decimal.Round(((1 + deliveryDB.Vat) * deliveryDB.PriceN), 2, MidpointRounding.AwayFromZero));
                return delivery;
            }
        }
        public static bool EditDelivery(DeliveriesViewModel delivery)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Deliveries deliveryDB = db.Deliveries.Where(x => x.DeliveryID == delivery.DeliveryID).Single();
                deliveryDB.Name = delivery.Name;
                deliveryDB.PriceN = delivery.PriceN;
                deliveryDB.Vat = delivery.VAT;
                db.SaveChanges();
                return true;
            }
        }
        public static bool AddDelivery(DeliveriesViewModel delivery)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Deliveries deliveryDB = new Deliveries();
                deliveryDB.Name = delivery.Name;
                deliveryDB.PriceN = delivery.PriceN;
                deliveryDB.Vat = delivery.VAT;
                db.Deliveries.Add(deliveryDB);
                db.SaveChanges();
                return true;
            }
        }
        public static bool RemoveDelivery(int deliveryID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Deliveries delivery = db.Deliveries.Where(x => x.DeliveryID == deliveryID).Single();
                db.Deliveries.Remove(delivery);
                db.SaveChanges();
                return true;
            }
        }
        public static bool IsExistsDelivery( string name)
        {
            using (sklepEntities db = new sklepEntities())
            {
                    return db.Deliveries.Where(x => x.Name == name).Any();
            }
        }
        public static bool IsExistsDelivery(int deliverID, string name)
        {
            using (sklepEntities db = new sklepEntities())
            {
                string deliverDB = db.Deliveries.Where(x => x.DeliveryID == deliverID).FirstOrDefault().Name;
                if (name == deliverDB)
                    return false;
                else
                    return db.Deliveries.Where(x => x.Name == name).Any();
            }
        }
    }
}