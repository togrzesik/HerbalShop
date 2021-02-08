using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayPal.Api;
using Sklep.Models;
using Sklep.Models.ViewModel;
using Sklep.Models.db;

namespace Sklep.Controllers
{
    [Authorize]
    public class PaypalController : BaseController
    {
       
        public PayPal.Api.Payment payment;
      
        private Payment CreatePayment2(string redirectUrl, int? orderID)
        {
            var details = new Details();
            var amount = new Amount();
            decimal subtotal = 0M;
            List<Item> itemsList = new List<Item>();
            using (sklepEntities db = new sklepEntities())
            {
                if (!db.Orders.Where(x => x.OrderID == orderID).Any())
                    return null;
                Orders order = db.Orders.Where(x => x.OrderID == orderID).Single();
                var products = db.ProductsOfOrders.Where(x => x.NumOfOrderID == orderID);
                foreach (ProductsOfOrders itemLoop in products)
                {
                    Item item = new Item();
                    item.name = db.Products.Where(x => x.ProductID == itemLoop.ProductID).Select(x => x.Name).Single();
                    item.currency = "PLN";
                    item.price = ((1 + itemLoop.Vat) * itemLoop.PriceN).ToString("0.00");
                    subtotal += (decimal)((1 + itemLoop.Vat) * itemLoop.PriceN * itemLoop.Quantity);
                    item.price = item.price.Replace(",", ".");
                    item.quantity = itemLoop.Quantity.ToString();
                    itemsList.Add(item);
                }
                Payments payment = db.Payments.Where(x => x.Name == "PayPal").Single();
                details.tax = ((1 + payment.VAT) * payment.PriceN).ToString("0.00");
                details.tax = details.tax.Replace(",", ".");
                details.shipping = ((decimal)((1 + order.VATDelivery) * order.PriceNDelivery)).ToString("0.00");
                details.shipping = details.shipping.Replace(",", ".");

                details.subtotal = subtotal.ToString("0.00");
                details.subtotal = details.subtotal.Replace(",", ".");
                amount.currency = "PLN";
                amount.total = ((decimal)(order.PriceB + (1 + payment.VAT) * payment.PriceN)).ToString("0.00");
                amount.total = amount.total.Replace(",", ".");
                amount.details = details;

            }

            ItemList itemList = new ItemList();
            itemList.items = itemsList;

            var payer = new Payer() { payment_method = "paypal" };

            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "/Paypal/FailureView",
                return_url = redirectUrl + "/Paypal/SuccessView?orderID=" + orderID
            };

            System.Diagnostics.Debug.WriteLine("cancel " + redirectUrl);
            var transactionList = new List<Transaction>();

            transactionList.Add(new Transaction()
            {
                description = "Zakupy w sklepie internetowym",
                invoice_number = "numer",
                amount = amount,
                item_list = itemList
            });

            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            var apiContext = Configuration.GetAPIContext();

            return this.payment.Create(apiContext);
        }

        public ActionResult PaymentWithPaypal1(int orderID)
        {
            string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority ;
            var payment = CreatePayment2(baseURI, orderID);

            return Redirect(payment.GetApprovalUrl());
        }

        public ActionResult FailureView()
        {
            // TODO: Handle cancelled payment
            return View();
        }

        public ActionResult SuccessView(int orderID, string paymentId, string token, string PayerID)
        {
            using (sklepEntities db = new sklepEntities())
            {
                Orders order = db.Orders.Where(x => x.OrderID == orderID).Single();
                order.Status = "Wpłacono";
                db.SaveChanges();
            }
            return View();
        }


        private static RedirectUrls GetReturnUrls(string baseUrl, string intent)
        {
            var returnUrl = intent == "sale" ? "/Home/PaymentSuccessful" : "/Home/AuthorizeSuccessful";
            return new RedirectUrls()
            {
                cancel_url = baseUrl + "/Home/PaymentCancelled",
                return_url = baseUrl + returnUrl
            };
        }

        public static Payment ExecutePayment(string paymentId, string payerId)
        {
            var apiContext = Configuration.GetAPIContext();

            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            var payment = new Payment() { id = paymentId };

            var executedPayment = payment.Execute(apiContext, paymentExecution);

            return executedPayment;
        }
    }

}