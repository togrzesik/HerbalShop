using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Sklep.Models.EntityManager
{
    public class EmailService  
    {
        private static readonly string _password = "bwyqzkwpqztrpfpy";
        private static readonly string _emailStore = "tomek061998@gmail.com";

        private static SmtpClient initSmtpClient()
        {
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 20000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(_emailStore, _password);
            return client;
        }

        public static void sendConfirmAccount(string name, string email, string link) {
            SmtpClient client = initSmtpClient();

            MailMessage mm = new MailMessage(_emailStore, email, "Link aktywacyjny", "Witaj " + name + "," +
                "\nAby aktywować konto w sklepie, kliknij w poniższy link\n" + link +  "\nSklep internetowy");
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);

        }

        public static void sendChangeEmail(string name, string email, string link)
        {
            SmtpClient client = initSmtpClient();

            MailMessage mm = new MailMessage(_emailStore, email, "Zmiana adresu e-mail", "Witaj " + name + "," +
                "\nTwój adres e-mail został zmieniony, kliknij w poniższy link, aby potwierdzić zmianę\n" + link + "\nSklep internetowy");
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
        }

        public static void sendQuestion(string email, string content)
        {
            SmtpClient client = initSmtpClient();

            MailMessage mm = new MailMessage(_emailStore, _emailStore, "Wiadomość od klienta", "Adres e-mail klienta: " + email
                + "\nTreść: " + content);
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
        }
    }
}