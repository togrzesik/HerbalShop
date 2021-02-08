using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sklep.Models.ViewModel
{
    public class AdminDetailsOrder
    {
        public int OrderID { get; set; }
        public string ClientName { get; set; }
        public string ClientSurname { get; set; }
        public DateTime OrderDate { get; set; }
        public string InvoicePath { get; set; }
        public string OrderPath { get; set; }
        public string PriceN { get; set; }
        public string PriceB { get; set; }
        public string WhoModified { get; set; }
        public string LastTimeModified { get; set; }
        public string DeliverStreet { get; set; }
        public string DeliverNumOfHouse { get; set;}
        public string DeliverPostCode { get; set; }
        public string DeliverCity { get; set; }
        public string NameDelivery { get; set; }
        public string PriceNDelivery { get; set; }
        public string PriceBDelivery { get; set; }
        public string VatDelivery { get; set; }
        public string NamePayment { get; set; }
        public string PriceNPayment { get; set; }
        public string PriceBPayment { get; set; }
        public string VatPayment { get; set; }
        public string Status { get; set; }
        public List<SelectListItem> statusList = new List<SelectListItem>();
        public List<AdminDetailsOrderListProducts> ProductsList { get; set; }

        public AdminDetailsOrder() { }
        public AdminDetailsOrder(int orderID, string ClientName, string ClientSurname, DateTime OrderDate, string InvoicePath, string OrderPath,
            string PriceN, string PriceB,  string WhoModified, string LastTimeModified, string DeliverStreet, string DeliverNumOfHouse,
            string NameDelivery, string PriceNDelivery, string PriceBDelivery, string VatDelivery, string NamePayment, string PriceNPayment,
            string PriceBPayment, string VatPayment, string Status, List<AdminDetailsOrderListProducts> ProductsList)
        {
            this.OrderID = orderID;
            this.ClientName = ClientName;
            this.ClientSurname = ClientSurname;
            this.OrderDate = OrderDate;
            this.InvoicePath = InvoicePath;
            this.OrderPath = OrderPath;
            this.PriceN = PriceN;
            this.PriceB = PriceB;
            this.WhoModified = WhoModified;
            this.LastTimeModified = LastTimeModified;
            this.DeliverStreet = DeliverStreet;
            this.DeliverNumOfHouse = DeliverNumOfHouse;
            this.NameDelivery = NameDelivery;
            this.PriceNDelivery = PriceNDelivery;
            this.PriceBDelivery = PriceBDelivery;
            this.VatDelivery = VatDelivery;
            this.NamePayment = NamePayment;
            this.PriceNPayment = PriceNPayment;
            this.PriceBPayment = PriceBPayment;
            this.VatPayment = VatPayment;
            this.Status = Status;
            this.ProductsList = ProductsList;
        }
    }
}